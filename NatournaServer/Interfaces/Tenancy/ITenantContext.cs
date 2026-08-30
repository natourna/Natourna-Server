namespace NatournaServer.Interfaces.Tenancy
{
    /// <summary>Organization of the current request; null outside an authenticated request (login, health checks, seeding).</summary>
    public interface ITenantContext
    {
        int? OrganizationId { get; }
    }
}
