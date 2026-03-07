using Core.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Services;

namespace Infrastructure.Extensions
{
    public static class UserExtension
    {
        public static async Task<IEnumerable<UserGroup>> GetGroupsAsync(this User user)
        {
            var userService = Provider.Get<IUserService>();
            var context = Provider.Get<AppDbContext>();
            
            // Fetch all user groups that have dynamic grouping rules
            var groupsWithRules = await context.UserGroups
                .Include(g => g.DynamicGroupingRules)
                .Where(g => g.DynamicGroupingRules.Any(r => r.IsActive))
                .ToListAsync();

            var matchingGroups = new List<UserGroup>();

            foreach (var group in groupsWithRules)
            {
                if (await group.UserBelongsToGroupAsync(user))
                {
                    matchingGroups.Add(group);
                }
            }

            return matchingGroups.Distinct().ToList();
        }
    }
}
