using Analytics_BE.Application.Interfaces;

namespace Analytics_BE.Infrastructure.Services
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
