namespace NatournaServer.Interfaces.Tenancy
{
    /// <summary>
    /// Exposes the organization the current request belongs to.
    /// Null when there is no authenticated request (login, health checks,
    /// startup seeding) - the context's query filters are permissive in that case.
    /// </summary>
    public interface ITenantContext
    {
        int? OrganizationId { get; }
    }
}
