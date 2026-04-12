using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.Interfaces;
using Application.Interfaces.Persistence;
using Application.Common.Pagination;

namespace Infrastructure.Persistence.Repositories
{
    public class UserGroupMembershipRepository : IUserGroupMembershipRepository
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenantContext;

        public UserGroupMembershipRepository(AppDbContext context, ITenantContext tenantContext)
        {
            _context = context;
            _tenantContext = tenantContext;
        }

        public async Task<PagedResult<UserGroupMembership>> GetByGroupIdAsync(Guid userGroupId, PageRequest pageRequest)
        {
            var query = _context.UserGroupMemberships
                .Include(m => m.User)
                .Where(m => m.UserGroupId == userGroupId);

            query = ApplyTenantFilter(query);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.User!.Name)
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            return new PagedResult<UserGroupMembership>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<List<UserGroupMembership>> GetByUserIdAsync(Guid userId)
        {
            var query = _context.UserGroupMemberships
                .Include(m => m.UserGroup)
                .Where(m => m.UserId == userId);

            query = ApplyTenantFilter(query);

            return await query.ToListAsync();
        }

        private IQueryable<UserGroupMembership> ApplyTenantFilter(IQueryable<UserGroupMembership> query)
        {
            var orgId = _tenantContext.OrganizationId;
            if (orgId != null)
            {
                query = query.Where(m => m.OrganizationId == orgId);
            }
            return query;
        }
    }
}
