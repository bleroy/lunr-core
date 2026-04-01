using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lunr
{
    public abstract class TrimmerBase
    {
        public abstract string Trim(string s);

        private async IAsyncEnumerable<Token> TrimImplementation(
            Token token,
            int i,
            IAsyncEnumerable<Token>
            tokens,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) yield break;
            yield return await new ValueTask<Token>(token.Clone(Trim));
        }

        public Pipeline.Function FilterFunction => TrimImplementation;
    }

    public sealed class Trimmer : TrimmerBase
    {
        public override string Trim(string s)
        {
            int start = 0;

            while (start < s.Length && !char.IsLetterOrDigit(s[start]) && s[start] != '_')
            {
                start++;
            }

            int end = s.Length - 1;

            while (end >= start && !char.IsLetterOrDigit(s[end]) && s[end] != '_')
            {
                end--;
            }

            if (start == 0 && end == s.Length - 1)
            {
                return s;
            }

            return s.Substring(start, end - start + 1);
        }
    }
}
