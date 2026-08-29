using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Constants.Subscription;
using NatournaServer.Constants.User;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Authentication;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Interfaces.Tenancy;
using NatournaServer.Models.Api.Requests.Organization;
using NatournaServer.Models.Api.Response.Login;
using NatournaServer.Models.Api.Response.Organization;
using NatournaServer.Models.Configurations;
using NatournaServer.Models.Entities;
using Microsoft.Extensions.Options;

namespace NatournaServer.Services.Api
{
    public class OrganizationApiManager : IOrganizationApiManager
    {
        private readonly IOrganizationContextManager _organizationContextManager;
        private readonly ISubscriptionContextManager _subscriptionContextManager;
        private readonly IBuildingContextManager _buildingContextManager;
        private readonly ICompoundContextManager _compoundContextManager;
        private readonly IApartmentContextManager _apartmentContextManager;
        private readonly IUserContextManager _userContextManager;
        private readonly IRoleContextManager _roleContextManager;
        private readonly IPasswordHashingService _passwordHashingService;
        private readonly IJwtAuthenticationService _jwtService;
        private readonly IAuditService _auditService;
        private readonly ITenantContext _tenantContext;
        private readonly RegistrationConfiguration _registrationSettings;
        private readonly JwtConfiguration _jwtSettings;
        private readonly ILogger<OrganizationApiManager> _logger;

        private const decimal DefaultPricePerBuilding = 7m;

        public OrganizationApiManager(
            IOrganizationContextManager organizationContextManager,
            ISubscriptionContextManager subscriptionContextManager,
            IBuildingContextManager buildingContextManager,
            ICompoundContextManager compoundContextManager,
            IApartmentContextManager apartmentContextManager,
            IUserContextManager userContextManager,
            IRoleContextManager roleContextManager,
            IPasswordHashingService passwordHashingService,
            IJwtAuthenticationService jwtService,
            IAuditService auditService,
            ITenantContext tenantContext,
            IOptions<RegistrationConfiguration> registrationSettings,
            IOptions<JwtConfiguration> jwtSettings,
            ILogger<OrganizationApiManager> logger)
        {
            _organizationContextManager = organizationContextManager;
            _subscriptionContextManager = subscriptionContextManager;
            _buildingContextManager = buildingContextManager;
            _compoundContextManager = compoundContextManager;
            _apartmentContextManager = apartmentContextManager;
            _userContextManager = userContextManager;
            _roleContextManager = roleContextManager;
            _passwordHashingService = passwordHashingService;
            _jwtService = jwtService;
            _auditService = auditService;
            _tenantContext = tenantContext;
            _registrationSettings = registrationSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<OrganizationResponse?> GetMyOrganizationAsync()
        {
            int? organizationId = _tenantContext.OrganizationId;

            if (organizationId == null)
            {
                return null;
            }

            OrganizationEntity? organization = await _organizationContextManager.GetByIdAsync(organizationId.Value);

            if (organization == null)
            {
                return null;
            }

            return await MapToResponseAsync(organization);
        }

        public async Task<OrganizationResponse?> UpdateSettingsAsync(UpdateOrganizationSettingsRequest request)
        {
            int? organizationId = _tenantContext.OrganizationId;

            if (organizationId == null)
            {
                return null;
            }

            OrganizationEntity? existing = await _organizationContextManager.GetByIdAsync(organizationId.Value);

            if (existing == null)
            {
                return null;
            }

            var oldValues = new { existing.Name, existing.LbpExchangeRate };

            OrganizationEntity? updated = await _organizationContextManager.UpdateAsync(organizationId.Value, request.Name, request.LbpExchangeRate);

            if (updated == null)
            {
                return null;
            }

            await _auditService.LogAsync(LogAction.Update, "Organization", updated.Id, oldValues, new { updated.Name, updated.LbpExchangeRate });

            return await MapToResponseAsync(updated);
        }

        public async Task<LoginResponse> RegisterAsync(RegisterOrganizationRequest request)
        {
            if (!_registrationSettings.Enabled)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Organization.RegistrationDisabled();
                _logger.LogWarning("[{ErrorCode}] Registration attempt while disabled", ErrorCodes.ORGANIZATION_REGISTRATION_DISABLED_ERROR);
                throw new ApiException(ErrorCodes.ORGANIZATION_REGISTRATION_DISABLED_ERROR, userMessage, technicalDetails, statusCode: 404);
            }

            try
            {
                _logger.LogInformation("Registering organization '{OrganizationName}' with {BuildingCount} buildings", request.OrganizationName, request.Buildings.Count);

                UserEntity? existingUser = await _userContextManager.GetByEmailAsync(request.AdminEmail);
                if (existingUser != null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Organization.EmailTaken(request.AdminEmail);
                    _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.ORGANIZATION_EMAIL_TAKEN_ERROR, userMessage);
                    throw new ApiException(ErrorCodes.ORGANIZATION_EMAIL_TAKEN_ERROR, userMessage, technicalDetails, statusCode: 409);
                }

                RoleEntity? adminRole = await _roleContextManager.GetByNameAsync(RoleNames.Admin);
                if (adminRole == null)
                {
                    (string userMessage, string technicalDetails) = ErrorMessageBuilder.Organization.RegisterFailed(request.OrganizationName);
                    _logger.LogError("[{ErrorCode}] Admin role is missing", ErrorCodes.ORGANIZATION_REGISTER_ERROR);
                    throw new ApiException(ErrorCodes.ORGANIZATION_REGISTER_ERROR, userMessage, technicalDetails, statusCode: 500);
                }

                // 1. Organization + trial subscription
                OrganizationEntity organization = await _organizationContextManager.CreateAsync(new OrganizationEntity(request.OrganizationName));
                await _subscriptionContextManager.CreateAsync(new SubscriptionEntity(organization.Id, SubscriptionStatus.Trial, DefaultPricePerBuilding));

                // 2. Admin user (registration is anonymous, so the org must be set explicitly)
                string passwordHash = _passwordHashingService.HashPassword(request.AdminPassword);
                UserEntity adminUser = new(0, request.AdminEmail, passwordHash, request.AdminPhoneNumber ?? string.Empty, adminRole.Id)
                {
                    OrganizationId = organization.Id
                };
                adminUser = await _userContextManager.CreateAsync(adminUser);

                // 3. Compound - a single-building customer gets a compound named after the building
                string compoundName = !string.IsNullOrWhiteSpace(request.CompoundName)
                    ? request.CompoundName!
                    : request.Buildings.Count == 1 ? request.Buildings[0].Name : request.OrganizationName;

                int totalApartments = request.Buildings.Sum(b => b.Floors * b.ApartmentsPerFloor);

                CompoundEntity compound = new(0, compoundName, request.Address ?? string.Empty, totalApartments)
                {
                    OrganizationId = organization.Id
                };
                compound = await _compoundContextManager.CreateAsync(compound);

                // 4. Buildings (+ optional empty apartments, floors numbered from 0)
                foreach (RegisterBuildingRequest buildingRequest in request.Buildings)
                {
                    BuildingEntity building = new(0, buildingRequest.Name, buildingRequest.Floors * buildingRequest.ApartmentsPerFloor, buildingRequest.Floors, compound.Id)
                    {
                        OrganizationId = organization.Id
                    };
                    building = await _buildingContextManager.CreateAsync(building);

                    for (int floor = 0; floor < buildingRequest.Floors; floor++)
                    {
                        for (int slot = 1; slot <= buildingRequest.ApartmentsPerFloor; slot++)
                        {
                            ApartmentEntity apartment = new(0, $"F{floor}-{slot}", floor, true, building.Id)
                            {
                                OrganizationId = organization.Id
                            };
                            await _apartmentContextManager.CreateAsync(apartment);
                        }
                    }
                }

                await _auditService.LogAsync(LogAction.Create, "Organization", organization.Id, null,
                    new { organization.Name, Buildings = request.Buildings.Count, Apartments = totalApartments });

                _logger.LogInformation("Successfully registered organization {OrganizationId} '{Name}'", organization.Id, organization.Name);

                // 5. Auto-login
                string token = _jwtService.GenerateToken(adminUser.Email, adminUser.Id.ToString(), RoleNames.Admin, organization.Id);

                return new LoginResponse
                {
                    Token = token,
                    Username = adminUser.Email,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                    OrganizationName = organization.Name
                };
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Organization.RegisterFailed(request.OrganizationName);
                _logger.LogError(ex, "[{ErrorCode}] {ErrorMessage}", ErrorCodes.ORGANIZATION_REGISTER_ERROR, userMessage);
                throw new ApiException(ErrorCodes.ORGANIZATION_REGISTER_ERROR, userMessage, technicalDetails, ex, statusCode: 500);
            }
        }

        private async Task<OrganizationResponse> MapToResponseAsync(OrganizationEntity organization)
        {
            SubscriptionEntity? subscription = await _subscriptionContextManager.GetByOrganizationIdAsync(organization.Id);

            SubscriptionResponse? subscriptionResponse = null;

            if (subscription != null)
            {
                // Buildings are tenant-scoped by the global query filter, so this counts the caller's org only
                List<BuildingEntity> buildings = await _buildingContextManager.GetAllAsync();

                subscriptionResponse = new SubscriptionResponse
                {
                    Status = subscription.Status.ToString(),
                    PricePerBuilding = subscription.PricePerBuilding,
                    BuildingCount = buildings.Count,
                    MonthlyCost = subscription.PricePerBuilding * buildings.Count,
                    StartDate = subscription.StartDate
                };
            }

            return new OrganizationResponse
            {
                Id = organization.Id,
                Name = organization.Name,
                LbpExchangeRate = organization.LbpExchangeRate,
                IsActive = organization.IsActive,
                Subscription = subscriptionResponse
            };
        }
    }
}
