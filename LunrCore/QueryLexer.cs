using System;
using System.Collections.Generic;
using System.Text;

namespace Lunr
{
    public class QueryLexer
    {
        /// <summary>
        /// A lex is a function that takes a lexer and returns the next lex.
        /// </summary>
        /// <param name="lexer">The lexer.</param>
        /// <returns>The next lex</returns>
        private delegate Lex Lex(QueryLexer lexer);

        private readonly string _str;
        private readonly int _length;
        private readonly IList<int> _escapeCharPositions = new List<int>();
        private int _pos = 0;
        private int _start = 0;

        public QueryLexer(string str)
        {
            _str = str;
            _length = str.Length;
        }

        public IList<Lexeme> Lexemes { get; } = new List<Lexeme>();

        private int Width => _pos - _start;
        private bool HasMore => _pos < _length;

        public void Run()
        {
            Lex state = LexText;

            while (state != LexPastEOS)
            {
                state = state(this);
            }
        }

        private static Lex LexPastEOS(QueryLexer lexer) => throw new InvalidOperationException("End of string should never be called");

        private static Lex LexField(QueryLexer lexer)
        {
            lexer.Backup();
            lexer.Emit(LexemeType.Field);
            lexer.Ignore();
            return LexText;
        }

        private static Lex LexTerm(QueryLexer lexer)
        {
            if (lexer.Width > 1)
            {
                lexer.Backup();
                lexer.Emit(LexemeType.Term);
            }

            lexer.Ignore();

            if (lexer.HasMore) return LexText;

            return LexPastEOS;
        }

        private static Lex LexEditDistance(QueryLexer lexer)
        {
            lexer.Ignore();
            lexer.AcceptDigitRun();
            lexer.Emit(LexemeType.EditDistance);
            return LexText;
        }

        private static Lex LexBoost(QueryLexer lexer)
        {
            lexer.Ignore();
            lexer.AcceptDigitRun();
            lexer.Emit(LexemeType.Boost);
            return LexText;
        }

        private static Lex LexEOS(QueryLexer lexer)
        {
            if (lexer.Width > 0)
            {
                lexer.Emit(LexemeType.Term);
            }
            return LexPastEOS;
        }

        private static Lex LexText(QueryLexer lexer)
        {
            while(true)
            {
                (bool EOS, char ch) = lexer.Next();

                if (EOS) return LexEOS;

                // Escape character is '\'
                if (ch == '\\')
                {
                    lexer.EscapeCharacter();
                    continue;
                }

                if (ch == ':') return LexField;

                if (ch == '~')
                {
                    lexer.Backup();
                    if (lexer.Width > 0) lexer.Emit(LexemeType.Term);
                    return LexEditDistance;
                }

                if (ch == '^')
                {
                    lexer.Backup();
                    if (lexer.Width > 0) lexer.Emit(LexemeType.Term);
                    return LexBoost;
                }

                // "+" indicates term presence is required
                // checking for length to ensure that only
                // leading "+" are considered
                if (ch == '+' && lexer.Width == 1)
                {
                    lexer.Emit(LexemeType.Presence);
                    return LexText;
                }

                // "-" indicates term presence is prohibited
                // checking for length to ensure that only
                // leading "-" are considered
                if (ch == '-' && lexer.Width == 1)
                {
                    lexer.Emit(LexemeType.Presence);
                    return LexText;
                }

                if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsSeparator(ch))
                {
                    return LexTerm;
                }
            }
        }

        private string SliceString()
        {
            var subSlices = new StringBuilder();
            int sliceStart = _start;
            
            foreach (int escapeCharposition in _escapeCharPositions)
            {
                int sliceEnd = escapeCharposition;
                subSlices.Append(_str.Substring(sliceStart, sliceEnd - sliceStart));
                sliceStart = sliceEnd + 1;
            }

            subSlices.Append(_str.Substring(sliceStart, _pos - sliceStart));
            _escapeCharPositions.Clear();

            return subSlices.ToString();
        }

        private void Emit(LexemeType type)
        {
            Lexemes.Add(new Lexeme(type, SliceString(), _start, _pos));
            _start = _pos;
        }

        private void EscapeCharacter()
        {
            _escapeCharPositions.Add(_pos - 1);
            _pos++;
        }

        private (bool EOS, char nextChar) Next()
            => _pos >= _length ? (true, char.MinValue) : (false, _str[_pos++]);

        private void Ignore()
        {
            if (_start == _pos) _pos++;
            _start = _pos;
        }

        private void Backup() => _pos--;

        private void AcceptDigitRun()
        {
            char ch;
            bool EOS;

            do (EOS, ch) = Next();
            while (char.IsDigit(ch));

            if (!EOS) Backup();
        }
    }
}
