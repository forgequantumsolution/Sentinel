using Application.Common.Pagination;
using Core.Entities;
using Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;

        public DepartmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Department?> GetByIdAsync(Guid id)
        {
            return await _context.Departments
                .Include(d => d.ParentDepartment)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Department?> GetByNameAsync(string name)
        {
            return await _context.Departments.FirstOrDefaultAsync(d => d.Name == name);
        }

        public async Task<PagedResult<Department>> GetAllAsync(PageRequest pageRequest)
        {
            var query = _context.Departments
                .Include(d => d.ParentDepartment)
                .OrderBy(d => d.Name);

            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageRequest.Skip).Take(pageRequest.PageSize).ToListAsync();

            return new PagedResult<Department>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task AddAsync(Department department)
        {
            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                department.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
