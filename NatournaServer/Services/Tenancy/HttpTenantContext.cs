using NatournaServer.Authentication;
using NatournaServer.Interfaces.Tenancy;

namespace NatournaServer.Services.Tenancy
{
    /// <summary>
    /// Resolves the current organization from the "orgId" claim of the
    /// authenticated JWT. Returns null outside an authenticated request.
    /// </summary>
    public class HttpTenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? OrganizationId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User.FindFirst(CustomClaimTypes.OrganizationId)?.Value;

                if (!string.IsNullOrEmpty(claim) && int.TryParse(claim, out int organizationId))
                {
                    return organizationId;
                }

                return null;
            }
        }
    }
}
