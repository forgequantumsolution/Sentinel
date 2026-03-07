using Application.Interfaces;

namespace Infrastructure.Services
{
    public class TenantContext : ITenantContext
    {
        public Guid? OrganizationId { get; private set; }

        public void SetOrganizationId(Guid? organizationId)
        {
            OrganizationId = organizationId;
        }
    }
}
