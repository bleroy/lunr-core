using Lunr;
using System.Linq;
using Xunit;

namespace LunrCoreTests
{
    public class QueryLexerTests
    {
        [Fact]
        public void SingleTermProducesOneLexeme()
        {
            QueryLexer lexer = Lex("foo");
            Assert.Single(lexer.Lexemes);

            Lexeme lexeme = lexer.Lexemes.First();
            Assert.Equal(LexemeType.Term, lexeme.Type);
            Assert.Equal("foo", lexeme.Value);
            Assert.Equal(0, lexeme.Start);
            Assert.Equal(3, lexeme.End);
        }

        [Fact]
        public void SingleTermWithHyphenProducesTwoLexemes()
        {
            QueryLexer lexer = Lex("foo-bar");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme fooLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Term, fooLexeme.Type);
            Assert.Equal("foo", fooLexeme.Value);
            Assert.Equal(0, fooLexeme.Start);
            Assert.Equal(3, fooLexeme.End);

            Lexeme barLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.Term, barLexeme.Type);
            Assert.Equal("bar", barLexeme.Value);
            Assert.Equal(4, barLexeme.Start);
            Assert.Equal(7, barLexeme.End);
        }

        [Fact]
        public void TermEscapeCharProducesOneLexeme()
        {
            QueryLexer lexer = Lex(@"foo\:bar");
            Assert.Single(lexer.Lexemes);

            Lexeme lexeme = lexer.Lexemes.First();
            Assert.Equal(LexemeType.Term, lexeme.Type);
            Assert.Equal("foo:bar", lexeme.Value);
            Assert.Equal(0, lexeme.Start);
            Assert.Equal(8, lexeme.End);
        }

        [Fact]
        public void MultipleTermsProduceTwoLexemes()
        {
            QueryLexer lexer = Lex("foo bar");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme fooLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Term, fooLexeme.Type);
            Assert.Equal("foo", fooLexeme.Value);
            Assert.Equal(0, fooLexeme.Start);
            Assert.Equal(3, fooLexeme.End);

            Lexeme barLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.Term, barLexeme.Type);
            Assert.Equal("bar", barLexeme.Value);
            Assert.Equal(4, barLexeme.Start);
            Assert.Equal(7, barLexeme.End);
        }

        [Fact]
        public void MultipleTermsWithPresence()
        {
            QueryLexer lexer = Lex("+foo +bar");
            Assert.Equal(4, lexer.Lexemes.Count);

            Lexeme fooPresenceLexeme = lexer.Lexemes[0];
            Lexeme fooLexeme = lexer.Lexemes[1];

            Assert.Equal(LexemeType.Presence, fooPresenceLexeme.Type);
            Assert.Equal(LexemeType.Term, fooLexeme.Type);
            Assert.Equal("+", fooPresenceLexeme.Value);
            Assert.Equal("foo", fooLexeme.Value);

            Lexeme barPresenceLexeme = lexer.Lexemes[2];
            Lexeme barLexeme = lexer.Lexemes[3];

            Assert.Equal(LexemeType.Presence, barPresenceLexeme.Type);
            Assert.Equal(LexemeType.Term, barLexeme.Type);
            Assert.Equal("+", barPresenceLexeme.Value);
            Assert.Equal("bar", barLexeme.Value);
        }

        [Fact]
        public void MultipleTermsWithPresenceAndFuzz()
        {
            QueryLexer lexer = Lex("+foo~1 +bar");
            Assert.Equal(5, lexer.Lexemes.Count);

            Lexeme fooPresenceLexeme = lexer.Lexemes[0];
            Lexeme fooLexeme = lexer.Lexemes[1];
            Lexeme fooFuzzLexeme = lexer.Lexemes[2];

            Assert.Equal(LexemeType.Presence, fooPresenceLexeme.Type);
            Assert.Equal(LexemeType.Term, fooLexeme.Type);
            Assert.Equal(LexemeType.EditDistance, fooFuzzLexeme.Type);
            Assert.Equal("+", fooPresenceLexeme.Value);
            Assert.Equal("foo", fooLexeme.Value);
            Assert.Equal("1", fooFuzzLexeme.Value);

            Lexeme barPresenceLexeme = lexer.Lexemes[3];
            Lexeme barLexeme = lexer.Lexemes[4];
            Assert.Equal(LexemeType.Presence, barPresenceLexeme.Type);
            Assert.Equal(LexemeType.Term, barLexeme.Type);
            Assert.Equal("+", barPresenceLexeme.Value);
            Assert.Equal("bar", barLexeme.Value);
        }

        [Fact]
        public void MultipleSpacesBetweenTerms()
        {
            QueryLexer lexer = Lex("foo    bar");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme fooLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Term, fooLexeme.Type);
            Assert.Equal("foo", fooLexeme.Value);
            Assert.Equal(0, fooLexeme.Start);
            Assert.Equal(3, fooLexeme.End);

            Lexeme barLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.Term, barLexeme.Type);
            Assert.Equal("bar", barLexeme.Value);
            Assert.Equal(7, barLexeme.Start);
            Assert.Equal(10, barLexeme.End);
        }

        [Fact]
        public void TermWithField()
        {
            QueryLexer lexer = Lex("title:foo");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme fieldLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Field, fieldLexeme.Type);
            Assert.Equal("title", fieldLexeme.Value);
            Assert.Equal(0, fieldLexeme.Start);
            Assert.Equal(5, fieldLexeme.End);

            Lexeme termLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.Term, termLexeme.Type);
            Assert.Equal("foo", termLexeme.Value);
            Assert.Equal(6, termLexeme.Start);
            Assert.Equal(9, termLexeme.End);
        }

        [Fact]
        public void TermWithFieldWithEscapeChar()
        {
            QueryLexer lexer = Lex(@"ti\:tle:foo");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme fieldLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Field, fieldLexeme.Type);
            Assert.Equal("ti:tle", fieldLexeme.Value);
            Assert.Equal(0, fieldLexeme.Start);
            Assert.Equal(7, fieldLexeme.End);

            Lexeme termLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.Term, termLexeme.Type);
            Assert.Equal("foo", termLexeme.Value);
            Assert.Equal(8, termLexeme.Start);
            Assert.Equal(11, termLexeme.End);
        }

        [Fact]
        public void TermWithPresenceRequired()
        {
            QueryLexer lexer = Lex("+foo");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme presenceLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Presence, presenceLexeme.Type);
            Assert.Equal("+", presenceLexeme.Value);
            Assert.Equal(0, presenceLexeme.Start);
            Assert.Equal(1, presenceLexeme.End);

            Lexeme termLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.Term, termLexeme.Type);
            Assert.Equal("foo", termLexeme.Value);
            Assert.Equal(1, termLexeme.Start);
            Assert.Equal(4, termLexeme.End);
        }

        [Fact]
        public void TermWithFieldWithPresenceRequired()
        {
            QueryLexer lexer = Lex("+title:foo");
            Assert.Equal(3, lexer.Lexemes.Count);

            Lexeme presenceLexeme = lexer.Lexemes[0];
            Lexeme fieldLexeme = lexer.Lexemes[1];
            Lexeme termLexeme = lexer.Lexemes[2];

            Assert.Equal(LexemeType.Presence, presenceLexeme.Type);
            Assert.Equal("+", presenceLexeme.Value);
            Assert.Equal(0, presenceLexeme.Start);
            Assert.Equal(1, presenceLexeme.End);

            Assert.Equal(LexemeType.Field, fieldLexeme.Type);
            Assert.Equal("title", fieldLexeme.Value);
            Assert.Equal(1, fieldLexeme.Start);
            Assert.Equal(6, fieldLexeme.End);

            Assert.Equal(LexemeType.Term, termLexeme.Type);
            Assert.Equal("foo", termLexeme.Value);
            Assert.Equal(7, termLexeme.Start);
            Assert.Equal(10, termLexeme.End);
        }

        [Fact]
        public void TermWithPresenceProhibited()
        {
            QueryLexer lexer = Lex("-foo");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme presenceLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Presence, presenceLexeme.Type);
            Assert.Equal("-", presenceLexeme.Value);
            Assert.Equal(0, presenceLexeme.Start);
            Assert.Equal(1, presenceLexeme.End);

            Lexeme termLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.Term, termLexeme.Type);
            Assert.Equal("foo", termLexeme.Value);
            Assert.Equal(1, termLexeme.Start);
            Assert.Equal(4, termLexeme.End);
        }

        [Fact]
        public void TermWithEditDistance()
        {
            QueryLexer lexer = Lex("foo~2");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme termLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Term, termLexeme.Type);
            Assert.Equal("foo", termLexeme.Value);
            Assert.Equal(0, termLexeme.Start);
            Assert.Equal(3, termLexeme.End);

            Lexeme editDistanceLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.EditDistance, editDistanceLexeme.Type);
            Assert.Equal("2", editDistanceLexeme.Value);
            Assert.Equal(4, editDistanceLexeme.Start);
            Assert.Equal(5, editDistanceLexeme.End);
        }

        [Fact]
        public void TermWithBoost()
        {
            QueryLexer lexer = Lex("foo^10");
            Assert.Equal(2, lexer.Lexemes.Count);

            Lexeme termLexeme = lexer.Lexemes[0];
            Assert.Equal(LexemeType.Term, termLexeme.Type);
            Assert.Equal("foo", termLexeme.Value);
            Assert.Equal(0, termLexeme.Start);
            Assert.Equal(3, termLexeme.End);

            Lexeme boostLexeme = lexer.Lexemes[1];
            Assert.Equal(LexemeType.Boost, boostLexeme.Type);
            Assert.Equal("10", boostLexeme.Value);
            Assert.Equal(4, boostLexeme.Start);
            Assert.Equal(6, boostLexeme.End);
        }

        [Fact]
        public void TermWithFieldBoostAndEditDistancez()
        {
            QueryLexer lexer = Lex("title:foo^10~5");
            Assert.Equal(4, lexer.Lexemes.Count);

            Lexeme fieldLexeme = lexer.Lexemes[0];
            Lexeme termLexeme = lexer.Lexemes[1];
            Lexeme boostLexeme = lexer.Lexemes[2];
            Lexeme editDistanceLexeme = lexer.Lexemes[3];

            Assert.Equal(LexemeType.Field, fieldLexeme.Type);
            Assert.Equal("title", fieldLexeme.Value);
            Assert.Equal(0, fieldLexeme.Start);
            Assert.Equal(5, fieldLexeme.End);

            Assert.Equal(LexemeType.Term, termLexeme.Type);
            Assert.Equal("foo", termLexeme.Value);
            Assert.Equal(6, termLexeme.Start);
            Assert.Equal(9, termLexeme.End);

            Assert.Equal(LexemeType.Boost, boostLexeme.Type);
            Assert.Equal("10", boostLexeme.Value);
            Assert.Equal(10, boostLexeme.Start);
            Assert.Equal(12, boostLexeme.End);

            Assert.Equal(LexemeType.EditDistance, editDistanceLexeme.Type);
            Assert.Equal("5", editDistanceLexeme.Value);
            Assert.Equal(13, editDistanceLexeme.Start);
            Assert.Equal(14, editDistanceLexeme.End);
        }

        private QueryLexer Lex(string str)
        {
            var lexer = new QueryLexer(str);
            lexer.Run();
            return lexer;
        }
    }
}
