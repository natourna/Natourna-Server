namespace NatournaServer.Models.Entities
{
    /// <summary>
    /// Implemented by every entity that belongs to a single organization.
    /// The context applies a global query filter on OrganizationId and stamps
    /// it automatically on insert (see NatournaServerContext).
    /// </summary>
    public interface ITenantEntity
    {
        int OrganizationId { get; set; }
    }
}
