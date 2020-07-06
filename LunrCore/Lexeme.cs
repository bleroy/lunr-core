namespace Lunr
{
    public class Lexeme
    {
        public static readonly Lexeme Empty
            = new Lexeme(LexemeType.Empty, "", 0, 0);

        public Lexeme(LexemeType type, string value, int start, int end)
        {
            Type = type;
            Value = value;
            Start = start;
            End = end;
        }

        public LexemeType Type { get; }
        public string Value { get; }
        public int Start { get; }
        public int End { get; }
    }
}
