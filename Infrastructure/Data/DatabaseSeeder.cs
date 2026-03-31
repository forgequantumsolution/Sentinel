using Core.Entities;
using Core.Enums;
using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAllAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            try
            {
                await SeedSqlFileAsync(context, passwordHasher);
                await SeedDefaultOrganizationAsync(context);
                await SeedDefaultRoleAndUserAsync(context, passwordHasher);
                await SeedDefaultFoldersAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during seeding: {ex.Message}");
                throw;
            }
        }

        private static async Task SeedSqlFileAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            var sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedData.sql");

            if (!File.Exists(sqlFilePath))
            {
                var projectDir = Directory.GetCurrentDirectory();
                sqlFilePath = Path.Combine(projectDir, "..", "Infrastructure", "Data", "SeedData.sql");
            }

            if (!File.Exists(sqlFilePath))
                sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "Data", "SeedData.sql");

            if (!File.Exists(sqlFilePath))
                sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData.sql");

            if (File.Exists(sqlFilePath))
            {
                var sql = await File.ReadAllTextAsync(sqlFilePath);

                var adminHash = passwordHasher.HashPassword("Admin123!");
                var superAdminHash = passwordHasher.HashPassword("S@cur3Sup3r!2026#Xz");
                sql = sql.Replace("'AQAAAAIAAYagAAAAEJrO6yvXm5H9p0V1Z2W3X4Y5Z6A7B8C9D0E1F2G3H4I5J6K7L8M9N0O1P2Q3R=='", $"'{adminHash}'");
                sql = sql.Replace("'__SUPER_ADMIN_HASH__'", $"'{superAdminHash}'");

                await context.Database.ExecuteSqlRawAsync(sql);
            }
            else
            {
                Console.WriteLine($"Warning: Seed SQL file not found. Skipping SQL seed.");
            }
        }

        private static async Task SeedDefaultOrganizationAsync(AppDbContext context)
        {
            var exists = await context.Organizations
                .IgnoreQueryFilters()
                .AnyAsync(o => o.Code == "DEFAULT");

            if (exists) return;

            var org = new Organization
            {
                Name = "Default Organization",
                Code = "DEFAULT",
                Description = "System default organization",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Organizations.Add(org);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded default organization: {org.Id}");
        }

        private static async Task SeedDefaultRoleAndUserAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            var defaultOrg = await context.Organizations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Code == "DEFAULT");

            if (defaultOrg == null) return;

            // Seed sys-admin role
            var role = await context.Roles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Name == "sys-admin");

            if (role == null)
            {
                role = new Role
                {
                    Name = "sys-admin",
                    Description = "System Administrator",
                    IsDefault = false,
                    OrganizationId = defaultOrg.Id,
                    CreatedAt = DateTime.UtcNow
                };
                context.Roles.Add(role);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded default role: sys-admin");
            }

            // Seed default admin user
            var adminExists = await context.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == "admin@system.local");

            if (adminExists) return;

            var admin = new User
            {
                Name = "System Admin",
                Email = "admin@system.local",
                PasswordHash = passwordHasher.HashPassword("Admin123!"),
                RoleId = role.Id,
                OrganizationId = defaultOrg.Id,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded default admin user: {admin.Email}");
        }

        private static async Task SeedDefaultFoldersAsync(AppDbContext context)
        {
            var defaultOrg = await context.Organizations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Code == "DEFAULT");

            if (defaultOrg == null) return;

            var rootFolders = new[]
            {
                new { Name = "Organization", Route = "/", Icon = "org", Sort = 0 },
            };

            foreach (var folder in rootFolders)
            {
                var exists = await context.ActionObjects
                    .IgnoreQueryFilters()
                    .AnyAsync(ao => ao.Route == folder.Route && ao.ObjectType == ObjectType.Folder);

                if (exists) continue;

                context.ActionObjects.Add(new ActionObject
                {
                    Name = folder.Name,
                    Route = folder.Route,
                    Icon = folder.Icon,
                    SortOrder = folder.Sort,
                    ObjectType = ObjectType.Folder,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();

            Console.WriteLine("Seeded default root folders.");
        }
    }
}
