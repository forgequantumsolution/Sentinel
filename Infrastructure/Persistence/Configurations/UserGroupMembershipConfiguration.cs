using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserGroupMembershipConfiguration : IEntityTypeConfiguration<UserGroupMembership>
{
    public void Configure(EntityTypeBuilder<UserGroupMembership> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_UserGroupMemberships");

        builder.HasOne(m => m.User)
              .WithMany()
              .HasForeignKey(m => m.UserId);

        builder.HasOne(m => m.UserGroup)
              .WithMany()
              .HasForeignKey(m => m.UserGroupId);
    }
}
