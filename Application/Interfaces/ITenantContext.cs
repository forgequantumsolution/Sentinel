namespace Analytics_BE.Application.Interfaces
{
    public interface ITenantContext
    {
        Guid? OrganizationId { get; }
        void SetOrganizationId(Guid? organizationId);
    }
}
