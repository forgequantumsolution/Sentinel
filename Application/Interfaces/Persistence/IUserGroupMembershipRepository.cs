using Core.Entities;
using Application.Common.Pagination;

namespace Application.Interfaces.Persistence
{
    public interface IUserGroupMembershipRepository
    {
        Task<PagedResult<UserGroupMembership>> GetByGroupIdAsync(Guid userGroupId, PageRequest pageRequest);
        Task<List<UserGroupMembership>> GetByUserIdAsync(Guid userId);
    }
}
