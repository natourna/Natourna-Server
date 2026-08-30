namespace NatournaServer.Models.Entities
{
    /// <summary>Implemented by every organization-owned entity; OrganizationId is stamped on insert and scoped by a global query filter.</summary>
    public interface ITenantEntity
    {
        int OrganizationId { get; set; }
    }
}
