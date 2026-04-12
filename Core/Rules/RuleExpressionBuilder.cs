using System.Linq.Expressions;
using Core.Entities;
using Core.Enums;

namespace Core.Rules
{
    /// <summary>
    /// Builds Expression&lt;Func&lt;User, bool&gt;&gt; from DynamicGroupingRule / DynamicPermissionRule trees.
    /// The expression is EF Core-translatable, so filtering happens in the database.
    /// </summary>
    public static class RuleExpressionBuilder
    {
        private static readonly ParameterExpression UserParam = Expression.Parameter(typeof(User), "u");

        /// <summary>
        /// Build a predicate expression from a rule tree (handles Simple, AND, OR recursively).
        /// </summary>
        public static Expression<Func<User, bool>> Build<T>(BaseDynamicRule<T> rule) where T : class
        {
            var body = BuildExpression(rule);
            return Expression.Lambda<Func<User, bool>>(body, UserParam);
        }

        private static Expression BuildExpression<T>(BaseDynamicRule<T> rule) where T : class
        {
            if (!rule.IsActive)
                return Expression.Constant(false);

            if (rule.RuleType == RuleType.Simple)
                return BuildSimpleExpression(rule);

            return BuildCompositeExpression(rule);
        }

        private static Expression BuildCompositeExpression<T>(BaseDynamicRule<T> rule) where T : class
        {
            var activeChildren = rule.ChildRules?
                .Cast<BaseDynamicRule<T>>()
                .Where(c => c.IsActive)
                .ToList();

            if (activeChildren == null || activeChildren.Count == 0)
                return Expression.Constant(false);

            Expression combined = BuildExpression(activeChildren[0]);

            for (int i = 1; i < activeChildren.Count; i++)
            {
                var childExpr = BuildExpression(activeChildren[i]);
                combined = rule.RuleType == RuleType.And
                    ? Expression.AndAlso(combined, childExpr)
                    : Expression.OrElse(combined, childExpr);
            }

            return combined;
        }

        private static Expression BuildSimpleExpression<T>(BaseDynamicRule<T> rule) where T : class
        {
            var fieldExpr = ResolveFieldExpression(rule.Field);
            if (fieldExpr == null)
                return Expression.Constant(false);

            // Coalesce nullable string to empty string: (field ?? "")
            var nullSafe = CoalesceToEmpty(fieldExpr);
            var value = rule.Value ?? string.Empty;

            return rule.Operator switch
            {
                RuleOperator.Equals             => EqualsIgnoreCase(nullSafe, value),
                RuleOperator.NotEquals           => Expression.Not(EqualsIgnoreCase(nullSafe, value)),
                RuleOperator.Contains            => CallStringMethod(nullSafe, "Contains", value),
                RuleOperator.StartsWith          => CallStringMethod(nullSafe, "StartsWith", value),
                RuleOperator.EndsWith            => CallStringMethod(nullSafe, "EndsWith", value),
                RuleOperator.In                  => BuildInExpression(nullSafe, value),
                RuleOperator.NotIn               => Expression.Not(BuildInExpression(nullSafe, value)),
                RuleOperator.GreaterThan         => StringCompare(nullSafe, value, ExpressionType.GreaterThan),
                RuleOperator.LessThan            => StringCompare(nullSafe, value, ExpressionType.LessThan),
                RuleOperator.GreaterThanOrEqual  => StringCompare(nullSafe, value, ExpressionType.GreaterThanOrEqual),
                RuleOperator.LessThanOrEqual     => StringCompare(nullSafe, value, ExpressionType.LessThanOrEqual),
                _                                => Expression.Constant(false)
            };
        }

        /// <summary>
        /// Maps "User.Role.Name" → u.Role.Name  (as Expression)
        /// </summary>
        private static Expression? ResolveFieldExpression(string field)
        {
            var parts = field.Split('.');
            Expression current = UserParam;

            foreach (var part in parts)
            {
                // Skip "User" root — our parameter is already typed as User
                if (current.Type.Name == part)
                    continue;

                var prop = current.Type.GetProperty(part);
                if (prop == null)
                    return null;

                current = Expression.Property(current, prop);
            }

            // If we ended on a non-string, convert to string for comparison
            if (current.Type != typeof(string))
            {
                // For nullable types or enums, call .ToString() — EF translates this
                if (current.Type == typeof(DateTime?) || current.Type == typeof(DateTime))
                {
                    // Cast to object first for nullable, then ToString
                    current = Expression.Call(current, current.Type.GetMethod("ToString", Type.EmptyTypes)!);
                }
                else
                {
                    current = Expression.Call(current, "ToString", null);
                }
            }

            return current;
        }

        // (field ?? "")
        private static Expression CoalesceToEmpty(Expression expr)
        {
            if (expr.Type == typeof(string))
                return Expression.Coalesce(expr, Expression.Constant(string.Empty));
            return expr;
        }

        // field.ToLower() == value.ToLower()
        private static Expression EqualsIgnoreCase(Expression field, string value)
        {
            var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var fieldLower = Expression.Call(field, toLower);
            var valueLower = Expression.Constant(value.ToLower());
            return Expression.Equal(fieldLower, valueLower);
        }

        // field.ToLower().Contains/StartsWith/EndsWith(value.ToLower())
        private static Expression CallStringMethod(Expression field, string methodName, string value)
        {
            var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var method = typeof(string).GetMethod(methodName, [typeof(string)])!;
            var fieldLower = Expression.Call(field, toLower);
            var valueLower = Expression.Constant(value.ToLower());
            return Expression.Call(fieldLower, method, valueLower);
        }

        // new[] { "a", "b", "c" }.Contains(field.ToLower())
        private static Expression BuildInExpression(Expression field, string csv)
        {
            var values = csv.Split(',').Select(v => v.Trim().ToLower()).ToArray();
            var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var fieldLower = Expression.Call(field, toLower);
            var arrayExpr = Expression.Constant(values);
            var containsMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(string));
            return Expression.Call(null, containsMethod, arrayExpr, fieldLower);
        }

        // string.Compare(field, value) > 0  (etc.)
        private static Expression StringCompare(Expression field, string value, ExpressionType comparison)
        {
            var compareMethod = typeof(string).GetMethod("Compare", [typeof(string), typeof(string)])!;
            var call = Expression.Call(null, compareMethod, field, Expression.Constant(value));
            return Expression.MakeBinary(comparison, call, Expression.Constant(0));
        }
    }
}
