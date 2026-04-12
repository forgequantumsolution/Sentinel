using Application.Common.Pagination;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IGroupMembershipService
    {
        Task<PagedResult<UserGroupMembershipDto>> GetGroupMembersAsync(Guid userGroupId, PageRequest pageRequest);
        Task<List<UserGroupMembershipDto>> GetUserGroupsAsync(Guid userId);
    }
}
