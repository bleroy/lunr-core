using Lunr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LunrCoreTests
{
    public class PipelineTests
    {
        [Fact]
        public void AddFunctionToPipeline()
        {
            var pipeline = new Pipeline();
            pipeline.Add(Noop);

            Assert.Single(pipeline.Process);
        }

        [Fact]
        public void AddMultipleFunctionsToPipeline()
        {
            var pipeline = new Pipeline();
            pipeline.Add(Noop, Noop);

            Assert.Equal(2, pipeline.Process.Count);
        }

        [Fact]
        public void RemoveExistingFunctionFromPipeline()
        {
            var pipeline = new Pipeline();
            pipeline.Add(Noop);

            Assert.Single(pipeline.Process);

            pipeline.Remove(Noop);
            Assert.Empty(pipeline.Process);
        }

        [Fact]
        public void RemoveNonExistingFunctionFromPipeline()
        {
            var pipeline = new Pipeline();
            pipeline.Add(Noop);

            Assert.Single(pipeline.Process);

            pipeline.Remove(NopNop);
            Assert.Single(pipeline.Process);
        }

        [Fact]
        public void BeforeExistingFunction()
        {
            var pipeline = new Pipeline();
            pipeline.Add(Noop);
            pipeline.Before(Noop, NopNop);

            Assert.Equal(
                new Pipeline.Function[] { NopNop, Noop },
                pipeline.Process);
        }

        [Fact]
        public void BeforeNonExistingFunction()
        {
            var pipeline = new Pipeline();
            Assert.Throws<InvalidOperationException>(() =>
            {
                pipeline.Before(Noop, NopNop);
            });

            Assert.Empty(pipeline.Process);
        }

        [Fact]
        public void AfterExistingFunction()
        {
            var pipeline = new Pipeline();
            pipeline.Add(Noop);
            pipeline.After(Noop, NopNop);

            Assert.Equal(
                new Pipeline.Function[] { Noop, NopNop },
                pipeline.Process);
        }

        [Fact]
        public void AfterNonExistingFunction()
        {
            var pipeline = new Pipeline();
            Assert.Throws<InvalidOperationException>(() =>
            {
                pipeline.After(Noop, NopNop);
            });

            Assert.Empty(pipeline.Process);
        }

        [Fact]
        public async Task RunCallsEachFunctionForEachToken()
        {
            int count1 = 0;
            int count2 = 0;

            Func<Token, Token> fn1 = t => { count1++; return t; };
            Func<Token, Token> fn2 = t => { count2++; return t; };

            var pipeline = new Pipeline();
            pipeline.Add(fn1.ToPipelineFunction(), fn2.ToPipelineFunction());

            var result = new List<string>();
            var cancellationToken = new CancellationToken();
            await foreach(Token resultToken in pipeline.Run(
                new[] { "1", "2", "3" }.Select(t => new Token(t)).ToAsyncEnumerable(cancellationToken),
                cancellationToken))
            {
                result.Add(resultToken.String);
            }

            Assert.Equal(3, count1);
            Assert.Equal(3, count2);
            Assert.Equal(new[] { "1", "2", "3" }, result);
        }

        [Fact]
        public async Task RunPassesTokenToPipelineFunction()
        {
            Func<Token, Token> fn = t =>
            {
                Assert.Equal("foo", t);
                return new Token(t.String + "bar");
            };

            var pipeline = new Pipeline();
            pipeline.Add(fn.ToPipelineFunction());

            var cancellationToken = new CancellationToken();
            await foreach (string resultToken in pipeline.RunString(
                "foo",
                cancellationToken))
            {
                Assert.Equal("foobar", resultToken);
            }
        }

        [Fact]
        public async Task RunPassesIndexToPipelineFunction()
        {
            bool hasRun = false;

            Action<int> action = i =>
            {
                Assert.Equal(0, i);
                hasRun = true;
            };

            var pipeline = new Pipeline();
            pipeline.Add(action.ToPipelineFunction());

            var cancellationToken = new CancellationToken();
            await foreach (string resultToken in pipeline.RunString(
                "foo",
                cancellationToken))
            {
                Assert.Equal("foo", resultToken);
            }
            Assert.True(hasRun);
        }

        [Fact]
        public async Task RunPassesEntireTokenArrayToPipelineFunction()
        {
            _hasRun = false;

            var pipeline = new Pipeline();
            pipeline.Add(ChecksTokenArray);

            var cancellationToken = new CancellationToken();
            await foreach (string resultToken in pipeline.Run(
                new[] { new Token("foo") }.ToAsyncEnumerable(cancellationToken),
                cancellationToken))
            {
                Assert.False(true, "Function fn is supposed to eat tokens.");
            }
            Assert.True(_hasRun);
        }

        [Fact]
        public async Task RunPassesOutputOfOneFunctionAsInputToTheNext()
        {
            Func<Token, Token> fn1 = t =>
            {
                Assert.Equal("foo", t);
                return new Token(t.String.ToUpperInvariant());
            };

            Func<Token, Token> fn2 = t =>
            {
                Assert.Equal("FOO", t);
                return new Token(t + t);
            };

            var pipeline = new Pipeline();
            pipeline.Add(fn1.ToPipelineFunction());
            pipeline.Add(fn2.ToPipelineFunction());

            var cancellationToken = new CancellationToken();
            await foreach (string resultToken in pipeline.RunString(
                "foo",
                new Dictionary<string, object>(),
                cancellationToken))
            {
                Assert.Equal("FOOFOO", resultToken);
            }
        }

        [Fact]
        public async Task FiltersOutNullAndEmptyStrings()
        {
            var tokens = new List<Token>();

            Func<Token, Token> fn = t =>
            {
                tokens.Add(t);
                return t;
            };

            var pipeline = new Pipeline();
            pipeline.Add(NullAndEmptyFun);
            pipeline.Add(fn.ToPipelineFunction());

            var cancellationToken = new CancellationToken();
            var output = new List<string>();
            await foreach (Token resultToken in pipeline.Run(
                new[] { "a", "b", "c", "d", "foo", "bar", "baz" }
                    .Select(s => new Token(s))
                    .ToAsyncEnumerable(cancellationToken),
                cancellationToken))
            {
                output.Add(resultToken.String);
            }

            Assert.Equal(new[] { "b", "d" }, tokens.Select(t => t.String));
            Assert.Equal(new[] { "b", "d" }, output);
        }

        [Fact]
        public async Task ExpandingTokensPassedToOutput()
        {
            var pipeline = new Pipeline();
            pipeline.Add(ExpandTokenFun);

            var cancellationToken = new CancellationToken();
            IList<string> output = await pipeline.RunString(
                "foo",
                cancellationToken)
                .ToList(cancellationToken);

            Assert.Equal(new[] { "foo", "FOO" }, output);
        }

        [Fact]
        public async Task ExpandingTokensPassedToTheNextFunction()
        {
            var received = new List<string>();

            Func<Token, Token> fn = t =>
            {
                received.Add(t);
                return t;
            };

            var pipeline = new Pipeline();
            pipeline.Add(ExpandTokenFun);
            pipeline.Add(fn.ToPipelineFunction());

            var cancellationToken = new CancellationToken();
            await pipeline.RunString("foo", cancellationToken)
                .ToList(cancellationToken);

            Assert.Equal(new[] { "foo", "FOO" }, received);
        }

        [Fact]
        public void SaveReturnsAnArrayOfRegisteredFunctionLabels()
        {
            var pipeline = new Pipeline();
            pipeline.RegisterFunction(Noop, "fn");

            pipeline.Add(Noop);

            Assert.Equal(new[] { "fn" }, pipeline.Save());
        }

        [Fact]
        public void RegisterFunctionAddsToTheRegistry()
        {
            var pipeline = new Pipeline();
            pipeline.RegisterFunction(Noop, "fn");

            pipeline.Add(Noop);

            Assert.Equal(Noop, pipeline.RegisteredFunctions["fn"]);
        }

        [Fact]
        public void LoadWithRegisteredFunctions()
        {
            var pipeline = new Pipeline();
            pipeline.RegisterFunction(Noop, "fn1");
            pipeline.RegisterFunction(NopNop, "fn2");

            string[] serializedPipeline = new[] { "fn1", "fn2", "fn1" };

            pipeline.Load(serializedPipeline);

            Assert.Equal(
                new Pipeline.Function[] { Noop, NopNop, Noop },
                pipeline.Process);
        }

        [Fact]
        public void LoadWithUnRegisteredFunctions()
        {
            var pipeline = new Pipeline();

            Assert.Throws<InvalidOperationException>(() =>
            {
                pipeline.Load(new[] { "nope" });
            });
        }

        [Fact]
        public void ResetEmptiesTheStack()
        {
            var pipeline = new Pipeline();
            pipeline.Add(Noop);

            Assert.Single(pipeline.Process);

            pipeline.Reset();

            Assert.Empty(pipeline.Process);
        }

        private bool _hasRun = false;

        private async IAsyncEnumerable<Token> ChecksTokenArray(
            Token _,
            int __,
            IAsyncEnumerable<Token> tokens,
            [EnumeratorCancellation] CancellationToken ___)
        {
            await foreach (Token resultToken in tokens)
            {
                Assert.Equal("foo", resultToken.String);
                _hasRun = true;
            }
            yield break;
        }

        private async IAsyncEnumerable<Token> Noop(
            Token token,
            int i,
            IAsyncEnumerable<Token> tokens,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }

        private async IAsyncEnumerable<Token> NopNop(
            Token token,
            int i,
            IAsyncEnumerable<Token> tokens,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield break;
        }

        private async IAsyncEnumerable<Token> NullAndEmptyFun(
            Token token,
            int i,
            IAsyncEnumerable<Token> tokens,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            if (i == 4) yield break;
            else if (i == 5) yield return new Token("");
            else if (i % 2 != 0) yield return token;
            else yield break;
        }

        private async IAsyncEnumerable<Token> ExpandTokenFun(
            Token token,
            int i,
            IAsyncEnumerable<Token> tokens,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            yield return token;
            yield return new Token(token.String.ToUpperInvariant());
        }
    }
}
