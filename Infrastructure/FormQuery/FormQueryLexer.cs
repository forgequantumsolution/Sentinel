using Application.FormQuery;

namespace Infrastructure.FormQuery
{
    /// <summary>
    /// Tokenizer for form-SQL queries. Converts raw SQL string into a stream of tokens.
    /// </summary>
    public class FormQueryLexer
    {
        private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["SELECT"] = TokenType.Select,
            ["DISTINCT"] = TokenType.Distinct,
            ["FROM"] = TokenType.From,
            ["WHERE"] = TokenType.Where,
            ["AND"] = TokenType.And,
            ["OR"] = TokenType.Or,
            ["NOT"] = TokenType.Not,
            ["JOIN"] = TokenType.Join,
            ["INNER"] = TokenType.Inner,
            ["LEFT"] = TokenType.Left,
            ["RIGHT"] = TokenType.Right,
            ["OUTER"] = TokenType.Outer,
            ["ON"] = TokenType.On,
            ["AS"] = TokenType.As,
            ["ORDER"] = TokenType.Order,
            ["BY"] = TokenType.By,
            ["GROUP"] = TokenType.Group,
            ["HAVING"] = TokenType.Having,
            ["LIMIT"] = TokenType.Limit,
            ["OFFSET"] = TokenType.Offset,
            ["ASC"] = TokenType.Asc,
            ["DESC"] = TokenType.Desc,
            ["IS"] = TokenType.Is,
            ["NULL"] = TokenType.Null,
            ["IN"] = TokenType.In,
            ["LIKE"] = TokenType.Like,
            ["BETWEEN"] = TokenType.Between,
            ["COUNT"] = TokenType.Count,
            ["SUM"] = TokenType.Sum,
            ["AVG"] = TokenType.Avg,
            ["MIN"] = TokenType.Min,
            ["MAX"] = TokenType.Max,
            ["CAST"] = TokenType.Cast,
            ["TRUE"] = TokenType.True,
            ["FALSE"] = TokenType.False,
            ["TOP"] = TokenType.Top,
            ["CASE"] = TokenType.Case,
            ["WHEN"] = TokenType.When,
            ["THEN"] = TokenType.Then,
            ["ELSE"] = TokenType.Else,
            ["END"] = TokenType.End,
            ["BEGIN"] = TokenType.Begin,
            ["IF"] = TokenType.If,
        };

        private readonly string _input;
        private int _pos;

        public FormQueryLexer(string input)
        {
            _input = input;
            _pos = 0;
        }

        public List<Token> Tokenize()
        {
            var tokens = new List<Token>();

            while (_pos < _input.Length)
            {
                SkipWhitespace();
                if (_pos >= _input.Length) break;

                var ch = _input[_pos];

                // Single-line comment: --
                if (ch == '-' && Peek(1) == '-')
                {
                    while (_pos < _input.Length && _input[_pos] != '\n') _pos++;
                    continue;
                }

                Token? token = ch switch
                {
                    ',' => MakeToken(TokenType.Comma, ","),
                    '.' => MakeToken(TokenType.Dot, "."),
                    '*' => MakeToken(TokenType.Star, "*"),
                    '(' => MakeToken(TokenType.LParen, "("),
                    ')' => MakeToken(TokenType.RParen, ")"),
                    ';' => MakeToken(TokenType.Semicolon, ";"),
                    '"' => ReadQuotedIdentifier(),
                    '[' => ReadBracketedIdentifier(),
                    '\'' => ReadStringLiteral(),
                    _ => null
                };

                if (token != null)
                {
                    tokens.Add(token);
                    continue;
                }

                // Operators
                if (ch == '=' ) { tokens.Add(MakeToken(TokenType.Eq, "=")); continue; }
                if (ch == '<' && Peek(1) == '>') { tokens.Add(MakeToken(TokenType.Neq, "<>", 2)); continue; }
                if (ch == '!' && Peek(1) == '=') { tokens.Add(MakeToken(TokenType.Neq, "!=", 2)); continue; }
                if (ch == '<' && Peek(1) == '=') { tokens.Add(MakeToken(TokenType.Lte, "<=", 2)); continue; }
                if (ch == '>' && Peek(1) == '=') { tokens.Add(MakeToken(TokenType.Gte, ">=", 2)); continue; }
                if (ch == '<') { tokens.Add(MakeToken(TokenType.Lt, "<")); continue; }
                if (ch == '>') { tokens.Add(MakeToken(TokenType.Gt, ">")); continue; }

                // Numbers — also catches bare ISO date / datetime literals (YYYY-MM-DD[ HH:MM[:SS[.fff]]])
                // and emits them as StringLiteral so the translator can ::DATE-cast them like quoted dates.
                if (char.IsDigit(ch))
                {
                    if (TryReadIsoDateLiteral() is { } dateToken)
                    {
                        tokens.Add(dateToken);
                        continue;
                    }
                    tokens.Add(ReadNumber());
                    continue;
                }

                // Identifiers / Keywords
                if (char.IsLetter(ch) || ch == '_')
                {
                    tokens.Add(ReadIdentifierOrKeyword());
                    continue;
                }

                throw new FormQueryException($"Unexpected character '{ch}' at position {_pos}");
            }

            tokens.Add(new Token { Type = TokenType.Eof, Value = "", Position = _pos });
            return tokens;
        }

        private void SkipWhitespace()
        {
            while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
                _pos++;
        }

        private char Peek(int offset = 0)
        {
            var idx = _pos + offset;
            return idx < _input.Length ? _input[idx] : '\0';
        }

        private Token MakeToken(TokenType type, string value, int advance = 1)
        {
            var token = new Token { Type = type, Value = value, Position = _pos };
            _pos += advance;
            return token;
        }

        private Token ReadBracketedIdentifier()
        {
            var start = _pos;
            _pos++; // skip opening [
            var sb = new System.Text.StringBuilder();
            while (_pos < _input.Length)
            {
                if (_input[_pos] == ']')
                {
                    _pos++; // skip closing ]
                    return new Token { Type = TokenType.QuotedIdentifier, Value = sb.ToString(), Position = start };
                }
                sb.Append(_input[_pos]);
                _pos++;
            }
            throw new FormQueryException($"Unterminated bracketed identifier starting at position {start}");
        }

        private Token ReadQuotedIdentifier()
        {
            var start = _pos;
            _pos++; // skip opening "
            var sb = new System.Text.StringBuilder();
            while (_pos < _input.Length)
            {
                if (_input[_pos] == '"')
                {
                    // Escaped quote ""
                    if (Peek(1) == '"')
                    {
                        sb.Append('"');
                        _pos += 2;
                    }
                    else
                    {
                        _pos++; // skip closing "
                        return new Token { Type = TokenType.QuotedIdentifier, Value = sb.ToString(), Position = start };
                    }
                }
                else
                {
                    sb.Append(_input[_pos]);
                    _pos++;
                }
            }
            throw new FormQueryException($"Unterminated quoted identifier starting at position {start}");
        }

        private Token ReadStringLiteral()
        {
            var start = _pos;
            _pos++; // skip opening '
            var sb = new System.Text.StringBuilder();
            while (_pos < _input.Length)
            {
                if (_input[_pos] == '\'')
                {
                    // Escaped quote ''
                    if (Peek(1) == '\'')
                    {
                        sb.Append('\'');
                        _pos += 2;
                    }
                    else
                    {
                        _pos++; // skip closing '
                        return new Token { Type = TokenType.StringLiteral, Value = sb.ToString(), Position = start };
                    }
                }
                else
                {
                    sb.Append(_input[_pos]);
                    _pos++;
                }
            }
            throw new FormQueryException($"Unterminated string literal starting at position {start}");
        }

        private Token ReadNumber()
        {
            var start = _pos;
            while (_pos < _input.Length && (char.IsDigit(_input[_pos]) || _input[_pos] == '.'))
                _pos++;
            return new Token { Type = TokenType.Number, Value = _input[start.._pos], Position = start };
        }

        /// <summary>
        /// Tries to read an unquoted ISO-style date or datetime literal starting at the current position:
        ///   YYYY-MM-DD
        ///   YYYY-MM-DD HH:MM
        ///   YYYY-MM-DDTHH:MM
        ///   YYYY-MM-DD HH:MM:SS[.fff]
        /// Emits a StringLiteral token so the translator's date-string handling auto-casts it to ::DATE.
        /// Returns null (without consuming any input) if the pattern doesn't match.
        /// </summary>
        private Token? TryReadIsoDateLiteral()
        {
            // YYYY-MM-DD requires at least 10 chars
            if (_pos + 10 > _input.Length) return null;

            for (var i = 0; i < 4; i++) if (!char.IsDigit(_input[_pos + i])) return null;
            if (_input[_pos + 4] != '-') return null;
            if (!char.IsDigit(_input[_pos + 5]) || !char.IsDigit(_input[_pos + 6])) return null;
            if (_input[_pos + 7] != '-') return null;
            if (!char.IsDigit(_input[_pos + 8]) || !char.IsDigit(_input[_pos + 9])) return null;

            var start = _pos;
            var end = _pos + 10;

            // Optional time portion: separator (T or space) + HH:MM[:SS[.fff]]
            if (end + 6 <= _input.Length
                && (_input[end] == 'T' || _input[end] == ' ')
                && char.IsDigit(_input[end + 1]) && char.IsDigit(_input[end + 2])
                && _input[end + 3] == ':'
                && char.IsDigit(_input[end + 4]) && char.IsDigit(_input[end + 5]))
            {
                end += 6; // consumed " HH:MM"

                // Optional :SS
                if (end + 3 <= _input.Length
                    && _input[end] == ':'
                    && char.IsDigit(_input[end + 1]) && char.IsDigit(_input[end + 2]))
                {
                    end += 3;

                    // Optional fractional seconds .fff
                    if (end < _input.Length && _input[end] == '.')
                    {
                        var fracEnd = end + 1;
                        while (fracEnd < _input.Length && char.IsDigit(_input[fracEnd])) fracEnd++;
                        if (fracEnd > end + 1) end = fracEnd;
                    }
                }
            }

            _pos = end;
            return new Token { Type = TokenType.StringLiteral, Value = _input[start..end], Position = start };
        }

        private Token ReadIdentifierOrKeyword()
        {
            var start = _pos;
            while (_pos < _input.Length && (char.IsLetterOrDigit(_input[_pos]) || _input[_pos] == '_'))
                _pos++;
            var value = _input[start.._pos];

            if (Keywords.TryGetValue(value, out var keywordType))
                return new Token { Type = keywordType, Value = value, Position = start };

            return new Token { Type = TokenType.Identifier, Value = value, Position = start };
        }
    }

    public class FormQueryException : Exception
    {
        public FormQueryException(string message) : base(message) { }
    }
}
