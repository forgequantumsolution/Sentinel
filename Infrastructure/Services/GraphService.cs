using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Analytics_BE.Infrastructure.Persistence;
using Application.Interfaces.Services;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class GraphService : IGraphService
    {
        private readonly AppDbContext _dbContext;

        public GraphService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GraphData> GetGraphDataAsync(GraphConfigRequest config)
        {
            // 1. Validate Target Table
            var propertyInfo = _dbContext.GetType().GetProperty(config.TableName);
            if (propertyInfo == null)
                throw new ArgumentException($"Table '{config.TableName}' does not exist on the database context.");

            // Get DbSet as IQueryable
            var query = propertyInfo.GetValue(_dbContext) as IQueryable;
            if (query == null)
                throw new InvalidOperationException($"Could not get IQueryable for '{config.TableName}'.");

            // 2. Apply Filters
            if (config.Filters != null && config.Filters.Count > 0)
            {
                foreach (var filter in config.Filters)
                {
                    string parameterKey = "@0";
                    string clause = $"{filter.Column} {filter.Operator} {parameterKey}";
                    // Dynamic linq will map @0 to filter.Value
                    query = query.Where(clause, filter.Value);
                }
            }

            // 3. Handle Functions like DISTINCT (Just return distinctly the targeted groupby column)
            if (!string.IsNullOrWhiteSpace(config.AggregateFunction) && config.AggregateFunction.ToUpper() == "DISTINCT")
            {
                if (string.IsNullOrWhiteSpace(config.GroupByColumn))
                    throw new ArgumentException("GroupByColumn is required to find distinct values.");

                var distinctQuery = query.Select(config.GroupByColumn).Distinct();
                var distinctValues = await distinctQuery.ToDynamicListAsync();

                return new GraphData
                {
                    Labels = distinctValues.Cast<object?>().Select(v => v?.ToString() ?? "Unknown").ToList(),
                    Datasets = new List<GraphDataset>
                    {
                        new GraphDataset
                        {
                            Label = $"Distinct {config.GroupByColumn}",
                            Data = distinctValues.Cast<object?>().ToList(),
                            Type = config.ChartType
                        }
                    }
                };
            }

            // 4. GROUP BY & Standard Aggregation (COUNT, SUM, AVG, MIN, MAX)
            if (string.IsNullOrWhiteSpace(config.GroupByColumn))
                throw new ArgumentException("GroupByColumn is required for aggregations.");

            // Using System.Linq.Dynamic.Core for GroupBy
            var groupedQuery = query.GroupBy(config.GroupByColumn);
            
            string aggFunc = config.AggregateFunction?.ToUpper() ?? "COUNT";
            string aggExpression = string.Empty;

            if (aggFunc == "COUNT")
            {
                aggExpression = "new (Key as Label, Count() as Value)";
            }
            else if (aggFunc == "SUM" || aggFunc == "AVG" || aggFunc == "MAX" || aggFunc == "MIN")
            {
                if (string.IsNullOrWhiteSpace(config.AggregateColumn))
                    throw new ArgumentException($"AggregateColumn is required when using {aggFunc}.");
                
                // Capitalize first letter (e.g., Sum, Avg, Max, Min) as expected by dynamic linq
                string properCaseFunc = aggFunc.Substring(0, 1).ToUpper() + aggFunc.Substring(1).ToLower();
                aggExpression = $"new (Key as Label, {properCaseFunc}({config.AggregateColumn}) as Value)";
            }
            else
            {
                throw new ArgumentException($"Unsupported aggregate function: {config.AggregateFunction}"); // DistinctCount could be handled separately if needed
            }

            var selectedQuery = groupedQuery.Select(aggExpression);
            var resultsList = await selectedQuery.ToDynamicListAsync();

            var labels = new List<string>();
            var dataValues = new List<object?>();

            foreach (dynamic item in resultsList)
            {
                labels.Add(item.Label?.ToString() ?? "Unknown");
                dataValues.Add(item.Value);
            }

            return new GraphData
            {
                Labels = labels,
                Datasets = new List<GraphDataset>
                {
                    new GraphDataset
                    {
                        Label = $"{aggFunc} of {config.AggregateColumn ?? "*"} by {config.GroupByColumn}",
                        Data = dataValues,
                        Type = config.ChartType
                    }
                }
            };
        }
    }
}
