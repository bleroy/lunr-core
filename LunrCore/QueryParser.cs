using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lunr
{
    public class QueryParser
    {
        /// <summary>
        /// A state is a function that takes a parser and returns the next state.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <returns>The next state.</returns>
        private delegate State State(QueryParser parser);

        private readonly QueryLexer _lexer;
        private Clause _currentClause = Clause.Empty;
        private int _lexemeIndex = 0;
        private IList<Lexeme> _lexemes = Array.Empty<Lexeme>();

        public QueryParser(string str, Query query, CultureInfo? culture = null!)
        {
            _lexer = new QueryLexer(str);
            Query = query;
            Culture = culture ?? CultureInfo.CurrentCulture;
        }

        public Query Query { get; }

        public CultureInfo Culture { get; }

        public Query Parse()
        {
            _lexer.Run();
            _lexemes = _lexer.Lexemes;

            State state = ParseClause;

            while (state != PastEOS)
            {
                state = state(this);
            }

            return Query;
        }

        private Lexeme PeekLexeme()
            => _lexemeIndex < _lexemes.Count ? _lexemes[_lexemeIndex] : Lexeme.Empty;

        private Lexeme ConsumeLexeme()
            => _lexemeIndex < _lexemes.Count ? _lexemes[_lexemeIndex++] : Lexeme.Empty;

        private void NextClause()
        {
            Query.AddClause(_currentClause);
            _currentClause = Clause.Empty;
        }

        private static State PastEOS(QueryParser parser) => throw new InvalidOperationException("Can't parse past end of string.");

        private static State ParseClause(QueryParser parser)
        {
            Lexeme lexeme = parser.PeekLexeme();

            if (lexeme == Lexeme.Empty) return PastEOS;

            switch(lexeme.Type)
            {
                case LexemeType.Presence: return ParsePresence;
                case LexemeType.Field: return ParseField;
                case LexemeType.Term: return ParseTerm;
                default:
                    string errorMessage =
                        lexeme.Value.Length >= 1 ?
                        $"Expected either a field or a term, found {lexeme.Type} with value '{lexeme.Value}'." :
                        $"Expected either a field or a term, found {lexeme.Type}.";
                    throw new QueryParserException(errorMessage, lexeme.Start, lexeme.End);
            }
        }

        private static State ParsePresence(QueryParser parser)
        {
            Lexeme lexeme = parser.ConsumeLexeme();

            if (lexeme == Lexeme.Empty) return PastEOS;

            parser._currentClause = parser._currentClause.WithPresence(lexeme.Value switch
            {
                "-" => QueryPresence.Prohibited,
                "+" => QueryPresence.Required,
                _ => throw new QueryParserException($"Unrecognized presence operator '{lexeme.Value}' at [{lexeme.Start}, {lexeme.End}].", lexeme.Start, lexeme.End),
            });
            Lexeme nextLexeme = parser.PeekLexeme();

            if (nextLexeme == Lexeme.Empty)
                throw new QueryParserException($"Expecting term or field at [{nextLexeme.Start}, {nextLexeme.End}], found nothing.", nextLexeme.Start, nextLexeme.End);

            return nextLexeme.Type switch
            {
                LexemeType.Field => ParseField,
                LexemeType.Term => ParseTerm,
                _ => throw new QueryParserException($"Expecting term or field at [{nextLexeme.Start}, {nextLexeme.End}], found '{nextLexeme.Type}'.", nextLexeme.Start, nextLexeme.End),
            };
        }

        private static State ParseField(QueryParser parser)
        {
            Lexeme lexeme = parser.ConsumeLexeme();

            if (lexeme == Lexeme.Empty) return PastEOS;

            if (!parser.Query.AllFields.Any(field => field.Name == lexeme.Value))
            {
                throw new QueryParserException(
                    $"Unrecognized field '{lexeme.Value}'. Available fields are: [{String.Join(", ", parser.Query.AllFields.Select(f => $"'{f.Name}'"))}].",
                    lexeme.Start,
                    lexeme.End);
            }

            parser._currentClause = parser._currentClause
                .WithFields(parser.Query.AllFields.First(f => f.Name == lexeme.Value));

            Lexeme nextLexeme = parser.PeekLexeme();

            if (nextLexeme == Lexeme.Empty)
            {
                throw new QueryParserException(
                    $"Expected term, found nothing at [{nextLexeme.Start}, {nextLexeme.End}].",
                    nextLexeme.Start,
                    nextLexeme.End);
            }

            return nextLexeme.Type switch
            {
                LexemeType.Term => ParseTerm,
                _ => throw new QueryParserException(
                    $"Expected term, found '{nextLexeme.Type}'.",
                    nextLexeme.Start,
                    nextLexeme.End)
            };
        }

        private static State ParseTerm(QueryParser parser)
        {
            Lexeme lexeme = parser.ConsumeLexeme();

            if (lexeme == Lexeme.Empty) return PastEOS;

            parser._currentClause = parser._currentClause
                .WithTerm(lexeme.Value.ToLower(parser.Culture));

            if (lexeme.Value.IndexOf(Query.Wildcard) != -1)
            {
                parser._currentClause = parser._currentClause.WithUsePipeline(false);
            }

            Lexeme nextLexeme = parser.PeekLexeme();

            if (nextLexeme == Lexeme.Empty)
            {
                parser.NextClause();
                return PastEOS;
            }

            return NextState(parser, nextLexeme);
        }

        private static State ParseEditDistance(QueryParser parser)
        {
            Lexeme lexeme = parser.ConsumeLexeme();

            if (lexeme == Lexeme.Empty) return PastEOS;

            if (!int.TryParse(lexeme.Value, out int editDistance))
            {
                throw new QueryParserException($"Edit distance must be numeric. Couldn't parse '{lexeme.Value}'.",
                    lexeme.Start,
                    lexeme.End);
            }

            parser._currentClause = parser._currentClause.WithEditDistance(editDistance);

            Lexeme nextLexeme = parser.PeekLexeme();

            if (nextLexeme == Lexeme.Empty)
            {
                parser.NextClause();
                return PastEOS;
            }

            return NextState(parser, nextLexeme);
        }

        private static State ParseBoost(QueryParser parser)
        {
            Lexeme lexeme = parser.ConsumeLexeme();

            if (lexeme == Lexeme.Empty) return PastEOS;

            if (!double.TryParse(lexeme.Value, out double boost))
            {
                throw new QueryParserException($"Edit distance must be numeric. Couldn't parse '{lexeme.Value}'.",
                    lexeme.Start,
                    lexeme.End);
            }

            parser._currentClause = parser._currentClause.WithBoost(boost);

            Lexeme nextLexeme = parser.PeekLexeme();

            if (nextLexeme == Lexeme.Empty)
            {
                parser.NextClause();
                return PastEOS;
            }

            return NextState(parser, nextLexeme);
        }

        private static State NextState(QueryParser parser, Lexeme nextLexeme)
        {
            switch (nextLexeme.Type)
            {
                case LexemeType.Term:
                    parser.NextClause();
                    return ParseTerm;
                case LexemeType.Field:
                    parser.NextClause();
                    return ParseField;
                case LexemeType.EditDistance:
                    return ParseEditDistance;
                case LexemeType.Boost:
                    return ParseBoost;
                case LexemeType.Presence:
                    parser.NextClause();
                    return ParsePresence;
                default:
                    throw new QueryParserException(
                        $"Unexpected lexeme type '{nextLexeme.Type}'.",
                        nextLexeme.Start,
                        nextLexeme.End);
            }
        }
    }
}
