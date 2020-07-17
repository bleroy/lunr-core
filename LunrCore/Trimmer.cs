using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
            yield return await Task.FromResult(token.Clone(Trim));
        }

        public Pipeline.Function FilterFunction => TrimImplementation;
    }

    public class Trimmer : TrimmerBase
    {
        private static readonly Regex _trimStartExpression = new Regex(@"^\W+");
        private static readonly Regex _trimEndExpression = new Regex(@"\W+$");

        public override string Trim(string s)
            => _trimEndExpression.Replace(_trimStartExpression.Replace(s, ""), "");
    }
}
