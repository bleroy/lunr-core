using System.Collections.Generic;
using System.Threading;

namespace Lunr
{
    public abstract class StemmerBase
    {
        protected abstract string Stem(string w);

        private IAsyncEnumerable<Token> StemWrapper(
            Token token,
            int i,
            IAsyncEnumerable<Token>
            tokens,
            CancellationToken cancellationToken)
        {
            return new Token[] { new Token(Stem(token.String)) }.ToAsyncEnumerable(cancellationToken);
        }

        public Pipeline.Function StemmerFunction => StemWrapper;
    }
}
