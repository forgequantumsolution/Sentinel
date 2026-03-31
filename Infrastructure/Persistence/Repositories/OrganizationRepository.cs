using Core.Entities;
using Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly AppDbContext _context;

        public OrganizationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Organization?> GetByIdAsync(Guid id)
        {
            return await _context.Organizations
                .Include(o => o.ParentOrganization)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Organization?> GetByCodeAsync(string code)
        {
            return await _context.Organizations.FirstOrDefaultAsync(o => o.Code == code);
        }

        public async Task<List<Organization>> GetAllAsync()
        {
            return await _context.Organizations
                .Include(o => o.ParentOrganization)
                .ToListAsync();
        }

        public async Task AddAsync(Organization organization)
        {
            await _context.Organizations.AddAsync(organization);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Organization organization)
        {
            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization != null)
            {
                organization.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
