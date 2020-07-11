using Lunr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LunrCoreTests
{
    public static class TestHelpers
    {
        public async static Task<string[]> BasicallyRun(
            this Pipeline.Function fun,
            string token,
            int i = 0,
            string[]? tokens = null!)
        {
            var result = new List<string>();
            var cancellationToken = new CancellationToken();
            await foreach(Token t in fun(
                new Token(token),
                i,
                tokens is null ?
                    AsyncEnumerableExtensions.Empty<Token>() :
                    tokens
                        .Select(s => new Token(s))
                        .ToAsyncEnumerable(cancellationToken),
                cancellationToken))
            {
                result.Add(t.String);
            }
            return result.ToArray();
        }

        public static Pipeline.Function ToPipelineFunction(this Func<Token, Token> fun)
            => (
                Token token,
                int i,
                IAsyncEnumerable<Token> tokens,
                CancellationToken cancellationToken)
                => new Token[] { fun(token) }.ToAsyncEnumerable(cancellationToken);

        public static Pipeline.Function ToPipelineFunction(this Action<int> action)
            => (
                Token token,
                int i,
                IAsyncEnumerable<Token> tokens,
                CancellationToken cancellationToken)
                =>
                {
                    action(i);
                    return new Token[] { token }.ToAsyncEnumerable(cancellationToken);
                };
    }
}
