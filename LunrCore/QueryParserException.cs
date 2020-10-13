using System;

namespace Lunr
{
    public sealed class QueryParserException : Exception
    {
        public QueryParserException(string message, int start, int end) : base(message)
        {
            Start = start;
            End = end;
        }

        public int Start { get; }
        public int End { get; }
    }
}
