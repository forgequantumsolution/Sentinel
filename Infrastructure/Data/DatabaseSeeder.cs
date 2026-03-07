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
                var sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedData.sql");
                
                // Fallback for development if BaseDirectory doesn't work as expected
                if (!File.Exists(sqlFilePath))
                {
                    var projectDir = Directory.GetCurrentDirectory();
                    sqlFilePath = Path.Combine(projectDir, "..", "Infrastructure", "Data", "SeedData.sql");
                }

                if (!File.Exists(sqlFilePath))
                {
                    // Second fallback for direct project run
                    sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Infrastructure", "Data", "SeedData.sql");
                }

                if (!File.Exists(sqlFilePath))
                {
                     // Final fallback: try just the current directory path for the Infrastructure project
                     sqlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SeedData.sql");
                }

                if (File.Exists(sqlFilePath))
                {
                    var sql = await File.ReadAllTextAsync(sqlFilePath);
                    
                    // Generate hash for default admin password and replace placeholder
                    var adminHash = passwordHasher.HashPassword("Admin123!");
                    sql = sql.Replace("'AQAAAAIAAYagAAAAEJrO6yvXm5H9p0V1Z2W3X4Y5Z6A7B8C9D0E1F2G3H4I5J6K7L8M9N0O1P2Q3R=='", $"'{adminHash}'");

                    await context.Database.ExecuteSqlRawAsync(sql);
                }
                else
                {
                    Console.WriteLine($"Warning: Seed SQL file not found at {sqlFilePath}. Skipping SQL seed.");
                    // Optional: Fallback to manual seeding if file is missing
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during SQL seeding: {ex.Message}");
                throw;
            }
        }
    }
}
