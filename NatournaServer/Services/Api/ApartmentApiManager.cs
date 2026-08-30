using NatournaServer.Constants.Error;
using NatournaServer.Constants.Log;
using NatournaServer.Exceptions;
using NatournaServer.Interfaces.Api;
using NatournaServer.Interfaces.Context;
using NatournaServer.Interfaces.Services;
using NatournaServer.Models.Api.Requests.Apartment;
using NatournaServer.Models.Api.Requests.Paging;
using NatournaServer.Models.Api.Response.Paging;
using NatournaServer.Models.Api.Response.Apartment;
using NatournaServer.Models.Entities;

namespace NatournaServer.Services.Api
{
    public class ApartmentApiManager : IApartmentApiManager
    {
        private readonly IApartmentContextManager _contextManager;
        private readonly IBuildingContextManager _buildingContextManager;
        private readonly IPaymentContextManager _paymentContextManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<ApartmentApiManager> _logger;

        public ApartmentApiManager(IApartmentContextManager contextManager, IBuildingContextManager buildingContextManager, IPaymentContextManager paymentContextManager, IAuditService auditService, ILogger<ApartmentApiManager> logger)
        {
            _contextManager = contextManager;
            _buildingContextManager = buildingContextManager;
            _paymentContextManager = paymentContextManager;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PagedResponse<ApartmentResponse>> GetPagedApartmentsAsync(PagedQuery query, int? buildingId = null, bool? isActive = null, string? search = null)
        {
            var (items, totalCount) = await _contextManager.GetPagedAsync(query.Page, query.PageSize, buildingId, isActive, search);

            return new PagedResponse<ApartmentResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ApartmentResponse?> GetApartmentByIdAsync(int id)
        {
            ApartmentEntity? apartment = await _contextManager.GetByIdAsync(id);
            return apartment == null ? null : MapToResponse(apartment);
        }

        public async Task<List<ApartmentResponse>> GetApartmentsByBuildingIdAsync(int buildingId)
        {
            List<ApartmentEntity> apartments = await _contextManager.GetByBuildingIdAsync(buildingId);
            return apartments.Select(MapToResponse).ToList();
        }

        public async Task<ApartmentResponse> CreateApartmentAsync(ApartmentRequest apartment)
        {
            await EnsureBuildingExistsAsync(apartment.BuildingId);

            ApartmentEntity created = await _contextManager.CreateAsync(MapToEntity(apartment));

            await _auditService.LogAsync(LogAction.Create, "Apartment", created.Id, null, new { created.BuildingId, created.ApartmentInfo, created.IsActive });

            return MapToResponse(created);
        }

        public async Task<ApartmentResponse?> UpdateApartmentAsync(int id, ApartmentRequest apartment)
        {
            ApartmentEntity? existing = await _contextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            await EnsureBuildingExistsAsync(apartment.BuildingId);

            var oldValues = new
            {
                existing.BuildingId,
                existing.ApartmentInfo,
                existing.IsActive
            };

            ApartmentEntity? updated = await _contextManager.UpdateAsync(id, MapToEntity(apartment));

            if (updated != null)
            {
                await _auditService.LogAsync(LogAction.Update, "Apartment", id, oldValues, new { updated.BuildingId, updated.ApartmentInfo, updated.IsActive });

                return MapToResponse(updated);
            }

            return null;
        }

        public async Task<bool> DeleteApartmentAsync(int id)
        {
            ApartmentEntity? existing = await _contextManager.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            // The Apartment->Payments FK is Restrict; fail with a clear 409 instead of a raw database error
            if (await _paymentContextManager.AnyAsync(apartmentId: id))
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Reference.InUse("Apartment", id, "payments");
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.APARTMENT_HAS_PAYMENTS_ERROR, userMessage);
                throw new ApiException(ErrorCodes.APARTMENT_HAS_PAYMENTS_ERROR, userMessage, technicalDetails, statusCode: 409);
            }

            await _auditService.LogAsync(LogAction.Delete, "Apartment", id, new { existing.BuildingId, existing.ApartmentInfo }, null);

            return await _contextManager.DeleteAsync(id);
        }

        private async Task EnsureBuildingExistsAsync(int buildingId)
        {
            BuildingEntity? building = await _buildingContextManager.GetByIdAsync(buildingId);

            if (building == null)
            {
                (string userMessage, string technicalDetails) = ErrorMessageBuilder.Reference.NotFound("Building", buildingId);
                _logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BUILDING_NOT_FOUND_ERROR, userMessage);
                throw new ApiException(ErrorCodes.BUILDING_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
            }
        }

        public async Task<ApartmentResponse?> SetApartmentActiveAsync(int id, bool isActive)
        {
            ApartmentEntity? existing = await _contextManager.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            ApartmentEntity? result = await _contextManager.SetActiveAsync(id, isActive);

            if (result != null)
            {
                LogAction action = isActive ? LogAction.ActivateUser : LogAction.DeactivateUser;
                await _auditService.LogAsync(action, "Apartment", id, new { existing.IsActive }, new { IsActive = isActive });

                return MapToResponse(result);
            }

            return null;
        }

        private static ApartmentEntity MapToEntity(ApartmentRequest request)
        {
            return new ApartmentEntity(0, request.ApartmentInfo, request.Floor, request.IsActive, request.BuildingId)
            {
                Owner = request.Owner,
                Tenant = request.Tenant
            };
        }

        private static ApartmentResponse MapToResponse(ApartmentEntity apartment)
        {
            return new ApartmentResponse
            {
                Id = apartment.Id,
                ApartmentInfo = apartment.ApartmentInfo,
                Owner = apartment.Owner,
                Tenant = apartment.Tenant,
                IsActive = apartment.IsActive,
                Floor = apartment.Floor,
                BuildingId = apartment.BuildingId,
                BuildingName = apartment.Building?.Name,
                CreatedAt = apartment.CreatedAt,
                UpdatedAt = apartment.UpdatedAt
            };
        }
    }
}