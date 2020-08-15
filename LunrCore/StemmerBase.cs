using System.Collections.Generic;
using System.Threading;

namespace Lunr
{
    public abstract class StemmerBase
    {
        public abstract string Stem(string w);

        private IAsyncEnumerable<Token> StemWrapper(
            Token token,
            int i,
            IAsyncEnumerable<Token>
            tokens,
            CancellationToken cancellationToken)
        {
            return new Token[] { token.Clone(Stem) }.ToAsyncEnumerable(cancellationToken);
        }

        public Pipeline.Function StemmerFunction => StemWrapper;
    }
}
