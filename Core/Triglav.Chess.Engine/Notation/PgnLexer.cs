using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Triglav.Chess.Engine.Notation
{
    internal enum PgnTokenKind
    {
        Open,
        Close,
        Quoted,
        Word,
        Number,
        Dot,
        End,
    }
    internal sealed record PgnToken(PgnTokenKind Kind, String Value, Int32 Line, Int32 Column);

    internal sealed class PgnLexer
    {
        private readonly String text;
        private readonly List<PgnDiagnostic> warnings;
        private Int32 index;
        private Int32 line = 1;
        private Int32 column = 1;

        internal PgnLexer(String text, List<PgnDiagnostic> warnings)
        {
            this.text = text.TrimStart('\uFEFF').Replace("\r\n", "\n").Replace('\r', '\n');
            this.warnings = warnings;
        }

        private Char Current => this.index < this.text.Length ? this.text[this.index] : '\0';

        private Char Take()
        {
            Char value = this.text[this.index++];
            if (value == '\n')
            {
                this.line++;
                this.column = 1;
            }
            else
            {
                this.column++;
            }
            return value;
        }

        internal PgnToken Next()
        {
            while (this.index < this.text.Length)
            {
                if (Char.IsWhiteSpace(this.Current))
                {
                    this.Take();
                    continue;
                }
                if (this.Current is not '{' and not ';')
                {
                    break;
                }
                Int32 startLine = this.line;
                Int32 startColumn = this.column;
                Char start = this.Take();
                while (this.index < this.text.Length && (start == '{' ? this.Current != '}' : this.Current != '\n'))
                {
                    if (start == '{' && this.Current == '{')
                    {
                        throw new PgnException(PgnDiagnosticCode.Syntax, this.line, this.column, token: "{");
                    }
                    this.Take();
                }
                if (start == '{')
                {
                    if (this.index == this.text.Length)
                    {
                        throw new PgnException(PgnDiagnosticCode.Syntax, startLine, startColumn, token: "{");
                    }
                    this.Take();
                }
                if (!this.warnings.Any(static warning => warning.Code == PgnDiagnosticCode.CommentsDiscarded))
                {
                    this.warnings.Add(new PgnDiagnostic(PgnDiagnosticCode.CommentsDiscarded, startLine, startColumn));
                }
            }
            Int32 line = this.line;
            Int32 column = this.column;
            if (this.index == this.text.Length)
            {
                return new PgnToken(PgnTokenKind.End, String.Empty, line, column);
            }
            Char first = this.Take();
            if (first is '[' or ']' or '.')
            {
                return new PgnToken(first == '[' ? PgnTokenKind.Open : first == ']' ? PgnTokenKind.Close : PgnTokenKind.Dot, first.ToString(), line, column);
            }
            if (first == '"')
            {
                StringBuilder value = new();
                while (this.index < this.text.Length && this.Current != '"')
                {
                    Char character = this.Take();
                    if (Char.IsControl(character))
                    {
                        throw new PgnException(PgnDiagnosticCode.Syntax, line, column);
                    }
                    if (character == '\\')
                    {
                        if (this.Current is not '"' and not '\\')
                        {
                            throw new PgnException(PgnDiagnosticCode.Syntax, this.line, this.column);
                        }
                        character = this.Take();
                    }
                    value.Append(character);
                    if (value.Length > 255)
                    {
                        throw new PgnException(PgnDiagnosticCode.LimitExceeded, line, column);
                    }
                }
                if (this.index == this.text.Length)
                {
                    throw new PgnException(PgnDiagnosticCode.Syntax, line, column);
                }
                this.Take();
                return new PgnToken(PgnTokenKind.Quoted, value.ToString(), line, column);
            }
            if (first is '(' or ')' or '$' or '%' or '<' or '>' or '}')
            {
                throw new PgnException(PgnDiagnosticCode.UnsupportedConstruct, line, column, token: first.ToString());
            }
            StringBuilder word = new();
            word.Append(first);
            while (this.index < this.text.Length && !Char.IsWhiteSpace(this.Current) && !"[].{};\"()$%<>".Contains(this.Current))
            {
                word.Append(this.Take());
                if (word.Length > 255)
                {
                    throw new PgnException(PgnDiagnosticCode.LimitExceeded, line, column);
                }
            }
            String token = word.ToString();
            return new PgnToken(token.All(static character => character is >= '0' and <= '9') ? PgnTokenKind.Number : PgnTokenKind.Word, token, line, column);
        }
    }
}
