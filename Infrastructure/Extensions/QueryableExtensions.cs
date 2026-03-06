using Application.Common.Filtering;
using Application.Common.Sorting;
using Core.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, CompositeFilterDescriptor filter)
    {
        var expression = FilterExpressionBuilder.Build<T>(filter);

        return query.Where(expression);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, IEnumerable<SortDescriptor>? sorts)
    {
        if (sorts == null || !sorts.Any())
            return query;

        IOrderedQueryable<T>? orderedQuery = null;

        foreach (var sort in sorts)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, sort.Field);
            var lambda = Expression.Lambda(property, parameter);

            string method = sort.Dir == SortDirection.Desc
                ? "OrderByDescending"
                : "OrderBy";

            if (orderedQuery == null)
            {
                orderedQuery = (IOrderedQueryable<T>)typeof(Queryable)
                    .GetMethods()
                    .First(m => m.Name == method && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.Type)
                    .Invoke(null, new object[] { query, lambda })!;
            }
            else
            {
                method = sort.Dir == SortDirection.Desc
                    ? "ThenByDescending"
                    : "ThenBy";

                orderedQuery = (IOrderedQueryable<T>)typeof(Queryable)
                    .GetMethods()
                    .First(m => m.Name == method && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.Type)
                    .Invoke(null, new object[] { orderedQuery, lambda })!;
            }
        }

        return orderedQuery ?? query;
    }
}