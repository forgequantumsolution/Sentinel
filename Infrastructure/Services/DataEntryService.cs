using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Persistence;
using Application.Interfaces.Services;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class DataEntryService : IDataEntryService
    {
        private readonly AppDbContext _dbContext;

        public DataEntryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> InsertDataAsync(DataEntryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TableName))
                throw new ArgumentException("Table name is required.");

            if (request.Data == null || !request.Data.Any())
                throw new ArgumentException("No data provided to insert.");

            // Basic validation - check if the table corresponds to an entity map in DbContext
            // (A fully dynamic app might skip this if writing directly to tables EF doesn't know about,
            // but for security it's best to validate against known tables if possible).
            var propertyInfo = _dbContext.GetType().GetProperty(request.TableName);
            if (propertyInfo == null)
            {
                // To allow completely generic dynamically generated tables via frontend logic, 
                // you would remove this check or use a separate "AllowedTables" white-list logic here.
                throw new ArgumentException($"Table '{request.TableName}' is not configured in the application context.");
            }

            var connection = _dbContext.Database.GetDbConnection();
            bool wasClosed = connection.State == ConnectionState.Closed;

            if (wasClosed)
                await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();
            try
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                var columns = new List<string>();
                var parameters = new List<string>();

                int i = 0;
                foreach (var kvp in request.Data)
                {
                    // Quote columns to prevent SQL injection and support case-sensitive names in PG
                    columns.Add($"\"{kvp.Key}\"");
                    
                    string paramName = $"@p{i}";
                    parameters.Add(paramName);

                    var param = command.CreateParameter();
                    param.ParameterName = paramName;
                    
                    // Npgsql converts mapping nicely or we define DBNull
                    param.Value = kvp.Value ?? DBNull.Value; 
                    
                    command.Parameters.Add(param);
                    i++;
                }

                // Generates string: INSERT INTO "Sales" ("Amount", "Region") VALUES (@p0, @p1)
                string colsString = string.Join(", ", columns);
                string paramsString = string.Join(", ", parameters);
                
                command.CommandText = $"INSERT INTO \"{request.TableName}\" ({colsString}) VALUES ({paramsString})";

                int rowsAffected = await command.ExecuteNonQueryAsync();

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                if (wasClosed)
                    await connection.CloseAsync();
            }
        }
    }
}
