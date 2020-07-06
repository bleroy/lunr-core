using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lunr
{
    public abstract class StopWordFilterBase
    {
        public abstract ISet<string> StopWords { get; }


        private async IAsyncEnumerable<Token> StopFilterImplementation(
            Token token,
            int i,
            IAsyncEnumerable<Token>
            tokens,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || StopWords.Contains(token.String)) yield break;
            yield return await Task.FromResult(token);
        }

        public Pipeline.Function FilterFunction => StopFilterImplementation;
    }
}
