using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Application.Common.Filtering;

namespace Core.Filtering;

public static class FilterExpressionBuilder
{
public static Expression<Func<T, bool>> Build<T>(CompositeFilterDescriptor filter)
{
    var parameter = Expression.Parameter(typeof(T), "x");

    var body = BuildGroup<T>(parameter, filter);

    return Expression.Lambda<Func<T, bool>>(body, parameter);
}

private static Expression BuildGroup<T>(ParameterExpression param, CompositeFilterDescriptor group)
{
    Expression? result = null;

    foreach (var filter in group.Filters)
    {
        Expression current;

        if (filter is FilterDescriptor descriptor)
        {
            current = BuildFilter(param, descriptor);
        }
        else
        {
            var composite = filter as CompositeFilterDescriptor;
            current = BuildGroup<T>(param, composite!);
        }

        if (result == null)
            result = current;
        else
        {
            result = group.Logic == "and"
                ? Expression.AndAlso(result, current)
                : Expression.OrElse(result, current);
        }
    }

    return result!;
}

private static Expression BuildFilter(ParameterExpression param, FilterDescriptor filter)
{
    var property = Expression.Property(param, filter.Field);
    var constant = Expression.Constant(filter.Value);

    switch (filter.Operator)
    {
        case FilterOperator.Eq:
            return Expression.Equal(property, constant);

        case FilterOperator.Neq:
            return Expression.NotEqual(property, constant);

        case FilterOperator.Gt:
            return Expression.GreaterThan(property, constant);

        case FilterOperator.Gte:
            return Expression.GreaterThanOrEqual(property, constant);

        case FilterOperator.Lt:
            return Expression.LessThan(property, constant);

        case FilterOperator.Lte:
            return Expression.LessThanOrEqual(property, constant);

        case FilterOperator.Contains:
            return Expression.Call(property, "Contains", null, constant);

        case FilterOperator.StartsWith:
            return Expression.Call(property, "StartsWith", null, constant);

        case FilterOperator.EndsWith:
            return Expression.Call(property, "EndsWith", null, constant);

        case FilterOperator.IsNull:
            return Expression.Equal(property, Expression.Constant(null));

        case FilterOperator.IsNotNull:
            return Expression.NotEqual(property, Expression.Constant(null));

        default:
            throw new NotSupportedException($"Operator {filter.Operator} not supported");
    }
}
}