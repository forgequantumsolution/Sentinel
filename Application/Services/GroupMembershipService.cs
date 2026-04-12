using Core.Entities;
using Application.DTOs;
using Application.Common.Pagination;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class GroupMembershipService : IGroupMembershipService
    {
        private readonly IUserGroupMembershipRepository _membershipRepository;

        public GroupMembershipService(IUserGroupMembershipRepository membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

        public async Task<PagedResult<UserGroupMembershipDto>> GetGroupMembersAsync(Guid userGroupId, PageRequest pageRequest)
        {
            var pagedResult = await _membershipRepository.GetByGroupIdAsync(userGroupId, pageRequest);

            return new PagedResult<UserGroupMembershipDto>
            {
                Items = pagedResult.Items.Select(MapToDto),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
        }

        public async Task<List<UserGroupMembershipDto>> GetUserGroupsAsync(Guid userId)
        {
            var memberships = await _membershipRepository.GetByUserIdAsync(userId);
            return memberships.Select(MapToDto).ToList();
        }

        private static UserGroupMembershipDto MapToDto(UserGroupMembership m)
        {
            return new UserGroupMembershipDto
            {
                UserId = m.UserId,
                UserGroupId = m.UserGroupId,
                UserName = m.User?.Name,
                UserEmail = m.User?.Email,
                GroupName = m.UserGroup?.Name,
                RuleId = m.RuleId
            };
        }
    }
}
