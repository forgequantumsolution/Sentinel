using Application.FormQuery;

namespace Infrastructure.FormQuery
{
    /// <summary>
    /// Recursive descent parser that converts tokens into an AST (FormQuery).
    /// Supports: SELECT [TOP n], FROM, JOIN, WHERE, GROUP BY, HAVING, ORDER BY, LIMIT, OFFSET, CASE.
    /// </summary>
    public class FormQueryParser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        public FormQueryParser(List<Token> tokens)
        {
            _tokens = tokens;
            _pos = 0;
        }

        private Token Current => _pos < _tokens.Count ? _tokens[_pos] : _tokens[^1];
        private Token Peek(int offset = 1) => (_pos + offset) < _tokens.Count ? _tokens[_pos + offset] : _tokens[^1];

        private Token Advance()
        {
            var t = Current;
            _pos++;
            return t;
        }

        private Token Expect(TokenType type)
        {
            if (Current.Type != type)
                throw new FormQueryException($"Expected {type} but got {Current.Type} ('{Current.Value}') at position {Current.Position}");
            return Advance();
        }

        private bool Match(TokenType type)
        {
            if (Current.Type == type) { Advance(); return true; }
            return false;
        }

        private bool Check(TokenType type) => Current.Type == type;

        // ── Entry point ──

        public Application.FormQuery.FormQuery Parse()
        {
            var (selectClause, topN) = ParseSelectClause();
            var query = new Application.FormQuery.FormQuery
            {
                Select = selectClause,
                From = ParseFromClause(),
                Limit = topN
            };

            // JOINs
            while (IsJoinKeyword())
            {
                query.Joins.Add(ParseJoinClause());
            }

            // WHERE
            if (Match(TokenType.Where))
            {
                query.Where = ParseExpression();
            }

            // GROUP BY
            if (Check(TokenType.Group) && Peek().Type == TokenType.By)
            {
                Advance(); Advance(); // GROUP BY
                query.GroupBy = new List<Expression> { ParseExpression() };
                while (Match(TokenType.Comma))
                    query.GroupBy.Add(ParseExpression());
            }

            // HAVING
            if (Match(TokenType.Having))
            {
                query.Having = ParseExpression();
            }

            // ORDER BY
            if (Check(TokenType.Order) && Peek().Type == TokenType.By)
            {
                Advance(); Advance(); // ORDER BY
                query.OrderBy = new List<OrderByItem>();
                query.OrderBy.Add(ParseOrderByItem());
                while (Match(TokenType.Comma))
                    query.OrderBy.Add(ParseOrderByItem());
            }

            // LIMIT (explicit LIMIT overrides TOP if both present)
            if (Match(TokenType.Limit))
            {
                query.Limit = int.Parse(Expect(TokenType.Number).Value);
            }
            // If TOP was specified but no LIMIT, topN is already set

            // OFFSET
            if (Match(TokenType.Offset))
            {
                query.Offset = int.Parse(Expect(TokenType.Number).Value);
            }

            // Optional semicolon
            Match(TokenType.Semicolon);

            return query;
        }

        // ── Clause parsers ──

        private (SelectClause clause, int? topN) ParseSelectClause()
        {
            Expect(TokenType.Select);
            var clause = new SelectClause();
            int? topN = null;

            clause.Distinct = Match(TokenType.Distinct);

            // SELECT TOP n ...
            if (Match(TokenType.Top))
            {
                topN = int.Parse(Expect(TokenType.Number).Value);
            }

            clause.Items.Add(ParseSelectItem());
            while (Match(TokenType.Comma))
                clause.Items.Add(ParseSelectItem());
            return (clause, topN);
        }

        private SelectItem ParseSelectItem()
        {
            var item = new SelectItem { Expr = ParseExpression() };
            if (Match(TokenType.As))
            {
                item.Alias = ParseIdentifierName();
            }
            else if (Current.Type == TokenType.Identifier || Current.Type == TokenType.QuotedIdentifier)
            {
                // Implicit alias (no AS keyword) — but only if next token isn't FROM/WHERE/etc.
                if (!IsClauseKeyword())
                {
                    item.Alias = ParseIdentifierName();
                }
            }
            return item;
        }

        private FromClause ParseFromClause()
        {
            Expect(TokenType.From);
            var clause = new FromClause { FormName = ParseIdentifierName() };
            if (Match(TokenType.As))
            {
                clause.Alias = ParseIdentifierName();
            }
            else if ((Current.Type == TokenType.Identifier || Current.Type == TokenType.QuotedIdentifier) && !IsClauseKeyword())
            {
                clause.Alias = ParseIdentifierName();
            }
            return clause;
        }

        private JoinClause ParseJoinClause()
        {
            var joinType = JoinType.Inner;

            if (Match(TokenType.Left)) { joinType = JoinType.Left; Match(TokenType.Outer); }
            else if (Match(TokenType.Right)) { joinType = JoinType.Right; Match(TokenType.Outer); }
            else if (Match(TokenType.Inner)) { /* default */ }

            Expect(TokenType.Join);

            var clause = new JoinClause
            {
                Type = joinType,
                FormName = ParseIdentifierName()
            };

            // Alias (with or without AS keyword)
            if (Match(TokenType.As))
            {
                clause.Alias = ParseIdentifierName();
            }
            else if (Current.Type == TokenType.Identifier || Current.Type == TokenType.QuotedIdentifier)
            {
                if (!Check(TokenType.On))
                    clause.Alias = ParseIdentifierName();
            }

            Expect(TokenType.On);
            clause.On = ParseExpression();

            return clause;
        }

        private OrderByItem ParseOrderByItem()
        {
            var item = new OrderByItem { Expr = ParseExpression() };
            if (Match(TokenType.Desc)) item.Descending = true;
            else Match(TokenType.Asc); // consume optional ASC
            return item;
        }

        // ── Expression parser (precedence climbing) ──

        private Expression ParseExpression() => ParseOr();

        private Expression ParseOr()
        {
            var left = ParseAnd();
            while (Match(TokenType.Or))
            {
                var right = ParseAnd();
                left = new BinaryExpr { Left = left, Operator = "OR", Right = right };
            }
            return left;
        }

        private Expression ParseAnd()
        {
            var left = ParseNot();
            while (Match(TokenType.And))
            {
                var right = ParseNot();
                left = new BinaryExpr { Left = left, Operator = "AND", Right = right };
            }
            return left;
        }

        private Expression ParseNot()
        {
            if (Match(TokenType.Not))
                return new UnaryExpr { Operator = "NOT", Operand = ParseNot() };
            return ParseComparison();
        }

        private Expression ParseComparison()
        {
            var left = ParsePrimary();

            // IS [NOT] NULL
            if (Check(TokenType.Is))
            {
                Advance();
                bool not = Match(TokenType.Not);
                Expect(TokenType.Null);
                return new IsNullExpr { Expr = left, Not = not };
            }

            // [NOT] IN (...)
            bool notIn = false;
            if (Check(TokenType.Not) && Peek().Type == TokenType.In)
            {
                Advance(); notIn = true;
            }
            if (Check(TokenType.In))
            {
                Advance();
                Expect(TokenType.LParen);
                var values = new List<Expression> { ParsePrimary() };
                while (Match(TokenType.Comma))
                    values.Add(ParsePrimary());
                Expect(TokenType.RParen);
                return new InExpr { Expr = left, Values = values, Not = notIn };
            }

            // [NOT] LIKE
            bool notLike = false;
            if (Check(TokenType.Not) && Peek().Type == TokenType.Like)
            {
                Advance(); notLike = true;
            }
            if (Check(TokenType.Like))
            {
                Advance();
                var pattern = ParsePrimary();
                var expr = new BinaryExpr { Left = left, Operator = "LIKE", Right = pattern };
                if (notLike) return new UnaryExpr { Operator = "NOT", Operand = expr };
                return expr;
            }

            // [NOT] BETWEEN ... AND ...
            bool notBetween = false;
            if (Check(TokenType.Not) && Peek().Type == TokenType.Between)
            {
                Advance(); notBetween = true;
            }
            if (Check(TokenType.Between))
            {
                Advance();
                var low = ParsePrimary();
                Expect(TokenType.And);
                var high = ParsePrimary();
                return new BetweenExpr { Expr = left, Low = low, High = high, Not = notBetween };
            }

            // Comparison operators: =, !=, <>, <, >, <=, >=
            string? op = Current.Type switch
            {
                TokenType.Eq => "=",
                TokenType.Neq => "<>",
                TokenType.Lt => "<",
                TokenType.Gt => ">",
                TokenType.Lte => "<=",
                TokenType.Gte => ">=",
                _ => null
            };

            if (op != null)
            {
                Advance();
                var right = ParsePrimary();
                return new BinaryExpr { Left = left, Operator = op, Right = right };
            }

            return left;
        }

        private Expression ParsePrimary()
        {
            // Parenthesized expression
            if (Check(TokenType.LParen))
            {
                Advance();
                var expr = ParseExpression();
                Expect(TokenType.RParen);
                return expr;
            }

            // NULL literal
            if (Match(TokenType.Null))
                return new LiteralExpr { Value = null, Type = LiteralType.Null };

            // Boolean literals
            if (Match(TokenType.True))
                return new LiteralExpr { Value = true, Type = LiteralType.Boolean };
            if (Match(TokenType.False))
                return new LiteralExpr { Value = false, Type = LiteralType.Boolean };

            // String literal
            if (Check(TokenType.StringLiteral))
                return new LiteralExpr { Value = Advance().Value, Type = LiteralType.String };

            // Number literal
            if (Check(TokenType.Number))
            {
                var val = Advance().Value;
                if (val.Contains('.'))
                    return new LiteralExpr { Value = decimal.Parse(val), Type = LiteralType.Number };
                return new LiteralExpr { Value = long.Parse(val), Type = LiteralType.Number };
            }

            // Star (*)
            if (Check(TokenType.Star))
            {
                Advance();
                return new StarExpr();
            }

            // CASE, BEGIN, or IF expression
            if (Check(TokenType.Case) || Check(TokenType.Begin) || Check(TokenType.If))
            {
                return ParseCaseExpression();
            }

            // CAST(expr AS type)
            if (Check(TokenType.Cast))
            {
                Advance();
                Expect(TokenType.LParen);
                var expr = ParseExpression();
                Expect(TokenType.As);
                var targetType = ParseIdentifierName();
                Expect(TokenType.RParen);
                return new CastExpr { Expr = expr, TargetType = targetType };
            }

            // Aggregate functions: COUNT, SUM, AVG, MIN, MAX
            if (IsAggregateFunction())
            {
                var funcName = Advance().Value.ToUpperInvariant();
                Expect(TokenType.LParen);
                bool distinct = Match(TokenType.Distinct);
                var args = new List<Expression>();
                if (!Check(TokenType.RParen))
                {
                    args.Add(ParseExpression());
                    while (Match(TokenType.Comma))
                        args.Add(ParseExpression());
                }
                Expect(TokenType.RParen);
                return new FunctionExpr { Name = funcName, Args = args, Distinct = distinct };
            }

            // Identifier (possibly qualified: alias.field, alias.*, or function call)
            if (Current.Type == TokenType.Identifier || Current.Type == TokenType.QuotedIdentifier)
            {
                var name = ParseIdentifierName();

                // General function call: identifier(args...)
                if (Check(TokenType.LParen))
                {
                    Advance(); // consume (
                    var args = new List<Expression>();
                    if (!Check(TokenType.RParen))
                    {
                        args.Add(ParseExpression());
                        while (Match(TokenType.Comma))
                            args.Add(ParseExpression());
                    }
                    Expect(TokenType.RParen);
                    return new FunctionExpr { Name = name.ToUpperInvariant(), Args = args };
                }

                // Check for alias.field or alias.*
                if (Match(TokenType.Dot))
                {
                    if (Check(TokenType.Star))
                    {
                        Advance();
                        return new StarExpr { TableAlias = name };
                    }
                    var fieldName = ParseMultiWordName();
                    return new FieldRefExpr { TableAlias = name, FieldName = fieldName };
                }

                // Bare field ref — may be multi-word (e.g., "Brand Name")
                var extendedName = name;
                while (Current.Type == TokenType.Identifier)
                    extendedName += " " + Advance().Value;
                return new FieldRefExpr { FieldName = extendedName };
            }

            throw new FormQueryException($"Unexpected token {Current.Type} ('{Current.Value}') at position {Current.Position}");
        }

        // ── CASE expression ──

        /// <summary>
        /// Parses both forms:
        ///   Searched: CASE WHEN cond THEN result [WHEN ...] [ELSE default] END
        ///   Simple:   CASE expr WHEN val THEN result [WHEN ...] [ELSE default] END
        /// </summary>
        private CaseExpr ParseCaseExpression()
        {
            // Accept CASE, BEGIN, or IF as the opening keyword
            if (Check(TokenType.Case) || Check(TokenType.Begin) || Check(TokenType.If))
                Advance();
            else
                throw new FormQueryException($"Expected CASE, BEGIN, or IF but got {Current.Type} ('{Current.Value}') at position {Current.Position}");

            var caseExpr = new CaseExpr();

            // Determine if this is a simple CASE (has operand) or searched CASE (starts with WHEN)
            if (!Check(TokenType.When))
            {
                caseExpr.Operand = ParseExpression();
            }

            // Parse WHEN ... THEN ... clauses
            while (Match(TokenType.When))
            {
                var condition = ParseExpression();
                Expect(TokenType.Then);
                var result = ParseExpression();
                caseExpr.Whens.Add(new CaseWhen { Condition = condition, Result = result });
            }

            if (caseExpr.Whens.Count == 0)
                throw new FormQueryException($"CASE expression requires at least one WHEN clause at position {Current.Position}");

            // Optional ELSE
            if (Match(TokenType.Else))
            {
                caseExpr.Else = ParseExpression();
            }

            Expect(TokenType.End);

            return caseExpr;
        }

        // ── Helpers ──

        private string ParseIdentifierName()
        {
            if (Current.Type == TokenType.QuotedIdentifier || Current.Type == TokenType.Identifier)
                return Advance().Value;
            throw new FormQueryException($"Expected identifier but got {Current.Type} ('{Current.Value}') at position {Current.Position}");
        }

        /// <summary>
        /// Parses a field name that may contain spaces (e.g., "Brand Name").
        /// Consumes the first identifier, then keeps consuming consecutive bare identifiers.
        /// Stops at keywords, operators, EOF, etc. Quoted identifiers are returned as-is.
        /// </summary>
        private string ParseMultiWordName()
        {
            var name = ParseIdentifierName();
            while (Current.Type == TokenType.Identifier)
                name += " " + Advance().Value;
            return name;
        }

        private bool IsAggregateFunction() =>
            Current.Type is TokenType.Count or TokenType.Sum or TokenType.Avg or TokenType.Min or TokenType.Max;

        private bool IsJoinKeyword() =>
            Current.Type is TokenType.Join or TokenType.Inner or TokenType.Left or TokenType.Right;

        private bool IsClauseKeyword() =>
            Current.Type is TokenType.From or TokenType.Where or TokenType.Join or TokenType.Inner
                or TokenType.Left or TokenType.Right or TokenType.On or TokenType.Group
                or TokenType.Having or TokenType.Order or TokenType.Limit or TokenType.Offset
                or TokenType.Eof or TokenType.Comma or TokenType.RParen;
    }
}
