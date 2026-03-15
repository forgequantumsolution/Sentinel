namespace Application.FormQuery
{
    // ── Token types ──

    public enum TokenType
    {
        // Literals
        Identifier, QuotedIdentifier, StringLiteral, Number,
        // Symbols
        Comma, Dot, Star, LParen, RParen, Semicolon,
        // Operators
        Eq, Neq, Lt, Gt, Lte, Gte,
        // Keywords
        Select, Distinct, From, Where, And, Or, Not,
        Join, Inner, Left, Right, Outer, On,
        As, Order, By, Group, Having,
        Limit, Offset, Asc, Desc,
        Is, Null, In, Like, Between,
        Count, Sum, Avg, Min, Max,
        Cast, True, False,
        Top, Case, When, Then, Else, End, Begin, If,
        // End
        Eof
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; } = string.Empty;
        public int Position { get; set; }
    }

    // ── AST Nodes ──

    public class FormQuery
    {
        public SelectClause Select { get; set; } = new();
        public FromClause From { get; set; } = new();
        public List<JoinClause> Joins { get; set; } = new();
        public Expression? Where { get; set; }
        public List<Expression>? GroupBy { get; set; }
        public Expression? Having { get; set; }
        public List<OrderByItem>? OrderBy { get; set; }
        public int? Limit { get; set; }
        public int? Offset { get; set; }
    }

    public class SelectClause
    {
        public bool Distinct { get; set; }
        public List<SelectItem> Items { get; set; } = new();
    }

    public class SelectItem
    {
        public Expression Expr { get; set; } = null!;
        public string? Alias { get; set; }
    }

    public class FromClause
    {
        public string FormName { get; set; } = string.Empty;
        public string? Alias { get; set; }
    }

    public class JoinClause
    {
        public JoinType Type { get; set; }
        public string FormName { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
        public Expression On { get; set; } = null!;
    }

    public enum JoinType { Inner, Left, Right }

    public class OrderByItem
    {
        public Expression Expr { get; set; } = null!;
        public bool Descending { get; set; }
    }

    // ── Expressions ──

    public abstract class Expression { }

    public class FieldRefExpr : Expression
    {
        public string? TableAlias { get; set; }
        public string FieldName { get; set; } = string.Empty;
    }

    public class StarExpr : Expression
    {
        public string? TableAlias { get; set; }
    }

    public class LiteralExpr : Expression
    {
        public object? Value { get; set; }
        public LiteralType Type { get; set; }
    }

    public enum LiteralType { String, Number, Boolean, Null }

    public class BinaryExpr : Expression
    {
        public Expression Left { get; set; } = null!;
        public string Operator { get; set; } = string.Empty;
        public Expression Right { get; set; } = null!;
    }

    public class UnaryExpr : Expression
    {
        public string Operator { get; set; } = string.Empty;
        public Expression Operand { get; set; } = null!;
    }

    public class FunctionExpr : Expression
    {
        public string Name { get; set; } = string.Empty;
        public List<Expression> Args { get; set; } = new();
        public bool Distinct { get; set; }
    }

    public class InExpr : Expression
    {
        public Expression Expr { get; set; } = null!;
        public List<Expression> Values { get; set; } = new();
        public bool Not { get; set; }
    }

    public class BetweenExpr : Expression
    {
        public Expression Expr { get; set; } = null!;
        public Expression Low { get; set; } = null!;
        public Expression High { get; set; } = null!;
        public bool Not { get; set; }
    }

    public class IsNullExpr : Expression
    {
        public Expression Expr { get; set; } = null!;
        public bool Not { get; set; }
    }

    public class CastExpr : Expression
    {
        public Expression Expr { get; set; } = null!;
        public string TargetType { get; set; } = string.Empty;
    }

    /// <summary>
    /// CASE WHEN cond1 THEN result1 [WHEN cond2 THEN result2 ...] [ELSE default] END
    /// Also supports: CASE expr WHEN val1 THEN result1 ... END (simple form)
    /// </summary>
    public class CaseExpr : Expression
    {
        /// <summary>
        /// Optional operand for simple CASE (CASE expr WHEN ...). Null for searched CASE (CASE WHEN cond ...).
        /// </summary>
        public Expression? Operand { get; set; }
        public List<CaseWhen> Whens { get; set; } = new();
        public Expression? Else { get; set; }
    }

    public class CaseWhen
    {
        public Expression Condition { get; set; } = null!;
        public Expression Result { get; set; } = null!;
    }

    // ── Schema cache models ──

    public class FormSchema
    {
        public Guid FormId { get; set; }
        public string FormName { get; set; } = string.Empty;
        public Dictionary<string, FieldSchema> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public class FieldSchema
    {
        public Guid FieldDefinitionId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
    }
}
