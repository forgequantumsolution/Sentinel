namespace Analytics_BE.Core.Enums
{
    public enum RuleOperator
    {
        Equals,
        NotEquals,
        Contains,
        StartsWith,
        EndsWith,
        In,
        NotIn,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        NA
    }

    public enum RuleType
    {
        Simple,
        And,
        Or
    }
}
