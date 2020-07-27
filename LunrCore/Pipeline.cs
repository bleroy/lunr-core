using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Lunr
{
    /// <summary>
    /// Pipelines maintain an ordered list of functions to be applied to all
    /// tokens in documents entering the search index and queries being ran against
    /// the index.
    /// 
    /// An instance of Index created with default parameters will contain a
    /// pipeline with a stop word filter and an English language stemmer. Extra
    /// functions can be added before or after either of these functions or these
    /// default functions can be removed.
    /// 
    /// When run the pipeline will call each function in turn, passing a token, the
    /// index of that token in the original list of all tokens and finally a list of
    /// all the original tokens.
    /// 
    /// The output of functions in the pipeline will be passed to the next function
    /// in the pipeline. To exclude a token from entering the index the function
    /// should return undefined, the rest of the pipeline will not be called with
    /// this token.
    /// 
    /// For serialization of pipelines to work, all functions used in an instance of
    /// a pipeline should be registered with Pipeline. Registered functions can
    /// then be loaded. If trying to load a serialized pipeline that uses functions
    /// that are not registered an error will be thrown.
    /// 
    /// If not planning on serializing the pipeline then registering pipeline functions
    /// is not necessary.
    /// </summary>
    public class Pipeline
    {
        /// <summary>
        /// A pipeline function maps Token to Token. A Token contains the token
        /// string as well as all known metadata. A pipeline function can mutate the token string
        /// or mutate (or add) metadata for a given token.
        ///
        /// A pipeline function can indicate that the passed token should be discarded by returning
        /// null, undefined or an empty string. This token will not be passed to any downstream pipeline
        /// functions and will not be added to the index.
        ///
        /// Multiple tokens can be returned by returning an array of tokens. Each token will be passed
        /// to any downstream pipeline functions and all will returned tokens will be added to the index.
        ///
        /// Any number of pipeline functions may be chained together using a Pipeline.
        /// </summary>
        /// <param name="token">A token to process from the document being processed.</param>
        /// <param name="i">The index of this token in the complete list of tokens for this document or field.</param>
        /// <param name="tokens">The complete set of tokens for this document or field.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A list of processed tokens.</returns>
        public delegate IAsyncEnumerable<Token> Function(
            Token token,
            int i,
            IAsyncEnumerable<Token> tokens,
            CancellationToken cancellationToken);

        /// <summary>
        /// Builds a `Pipeline.Function` from a simple `Action&lt;Token&gt;`.
        /// </summary>
        /// <param name="fun">The function to wrap as a `Pipeline.Function`.</param>
        /// <returns>The `Pipeline.Function`.</returns>
        public static Function BuildFunction(Action<Token> fun)
        {
            return (Token token, int i, IAsyncEnumerable<Token> tokens, CancellationToken cancellationToken) =>
            {
                fun(token);
                return (new[] { token }).ToAsyncEnumerable(cancellationToken);
            };
        }

        private readonly IList<Function> _process;
        private readonly IList<string> _processFunctionNames = Array.Empty<string>();

        /// <summary>
        /// Creates a new pipeline.
        /// </summary>
        /// <param name="functionRegistry">The function registry, necessary to load pipelines from their serialized form.</param>
        /// <param name="functions">A list of functions to build the pipeline process from.</param>
        public Pipeline(IDictionary<string, Function> functionRegistry, params Function[] functions)
        {
            RegisteredFunctions = functionRegistry;
            _process = new List<Function>(functions);
        }

        /// <summary>
        /// Creates a new pipeline.
        /// </summary>
        public Pipeline()
        {
            RegisteredFunctions = new Dictionary<string, Function>();
            _process = new List<Function>();
        }

        /// <summary>
        /// Creates a new pipeline from function names, even if the function registry is not yet known.
        /// Functions will be resolved later.
        /// </summary>
        /// <param name="functionNames">The pipeline description asd a list of function names.</param>
        public Pipeline(IEnumerable<string> functionNames) : this()
            => _processFunctionNames = functionNames.ToList();

        public IDictionary<string, Function> RegisteredFunctions { get; }

        /// <summary>
        /// Gets the list of functions in the pipeline, rehydrating it from the registry if necessary.
        /// </summary>
        public IList<Function> Process
        {
            get
            {
                if (!_process.Any() && _processFunctionNames.Any())
                {
                    Load(_processFunctionNames);
                    _processFunctionNames.Clear();
                }

                return _process;
            }
        }

        /// <summary>
        /// Registers a function with the pipeline.
        /// 
        /// Functions that are used in the pipeline should be registered if the pipeline
        /// needs to be serialised, or a serialised pipeline needs to be loaded.
        /// 
        /// Registering a function does not add it to a pipeline, functions must still be
        /// added to instances of the pipeline for them to be used when running a pipeline.
        /// </summary>
        /// <param name="fn">The function to check for.</param>
        /// <param name="label">The label to register this function with.</param>
        /// <returns>The pipeline.</returns>
        public Pipeline RegisterFunction(Function fn, string label)
        {
            if (RegisteredFunctions.ContainsKey(label))
            {
                throw new InvalidOperationException($"A pipeline function with the name {label} already exists.");
            }

            RegisteredFunctions[label] = fn;
            return this;
        }

        /// <summary>
        /// Loads a previously serialized pipeline.
        ///
        /// All functions to be loaded must already be registered with the pipeline.
        /// If any function from the serialized data has not been registered, then an
        /// exception will be thrown.
        /// </summary>
        /// <param name="functions">The serialised pipeline to load.</param>
        /// <returns>The pipeline.</returns>
        public Pipeline Load(IEnumerable<string> functions)
        {
            if (_process.Any()) throw new InvalidOperationException("This pipeline has already been loaded.");
            foreach (string function in functions)
            {
                if (RegisteredFunctions.TryGetValue(function, out Function registeredFunction))
                {
                    _process.Add(registeredFunction);
                }
                else throw new InvalidOperationException($"Can't find a pipeline function registered with the name {function}.");
            }
            return this;
        }

        /// <summary>
        /// Returns a representation of the pipeline ready for serialisation.
        /// </summary>
        /// <remarks>In lunr.js, this method is calles "toJSON" which is a bit of a misnomer, so I renamed it "Save" because it's the reverse of "Load".</remarks>
        /// <returns>The list of name of the functions forming the pipeline process.</returns>
        public IEnumerable<string> Save()
            => Process.Select(fn => RegisteredFunctions.First(kvp => kvp.Value == fn).Key);

        /// <summary>
        /// Adds new functions to the end of the pipeline.
        /// </summary>
        /// <param name="functions">Any number of functions to add to the pipeline.</param>
        /// <returns>The pipeline.</returns>
        public Pipeline Add(params Function[] functions)
        {
            foreach (Function function in functions)
            {
                Process.Add(function);
            }
            return this;
        }

        /// <summary>
        /// Adds a single function after a function that already exists in the pipeline.
        /// </summary>
        /// <param name="existingFunction">The function after which to add the new function.</param>
        /// <param name="newFunction">The function to add.</param>
        /// <returns>The pipeline.</returns>
        public Pipeline After(Function existingFunction, Function newFunction)
        {
            int position = Process.IndexOf(existingFunction);
            if (position == -1) throw new InvalidOperationException($"Can't find {nameof(existingFunction)}.");
            Process.Insert(position + 1, newFunction);
            return this;
        }

        /// <summary>
        /// Adds a single function before a function that already exists in the pipeline.
        /// </summary>
        /// <param name="existingFunction">The function before which to add the new function.</param>
        /// <param name="newFunction">The function to add.</param>
        /// <returns>The pipeline.</returns>
        public Pipeline Before(Function existingFunction, Function newFunction)
        {
            int position = Process.IndexOf(existingFunction);
            if (position == -1) throw new InvalidOperationException($"Can't find {nameof(existingFunction)}.");
            Process.Insert(position, newFunction);
            return this;
        }

        /// <summary>
        /// Removes a function from the pipeline.
        /// </summary>
        /// <param name="existingFunction">The function to remove from the pipeline.</param>
        /// <returns>The pipeline.</returns>
        public Pipeline Remove(Function existingFunction)
        {
            int position = Process.IndexOf(existingFunction);
            if (position == -1) return this;
            Process.RemoveAt(position);
            return this;
        }

        /// <summary>
        /// Runs the current list of functions that make up the pipeline against the passed tokens.
        /// </summary>
        /// <param name="tokens">The tokens to run through the pipeline.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The processed tokens.</returns>
        public async IAsyncEnumerable<Token> Run(
            IAsyncEnumerable<Token> tokens,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (Function function in Process)
            {
                tokens = RunStep(function, tokens, cancellationToken);
            }
            await foreach (Token token in tokens)
            {
                yield return token;
            }
        }

        /// <summary>
        /// Convenience method for passing a string through a pipeline and getting
        /// strings out. This method takes care of wrapping the passed string in a
        /// token and mapping the resulting tokens back to strings.
        /// </summary>
        /// <param name="str">The string to pass through the pipeline.</param>
        /// <param name="metadata">Some metadata to pass along.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The processed list of string tokens.</returns>
        public async IAsyncEnumerable<string> RunString(
            string str,
            TokenMetadata metadata,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (string s in Run(
                new[] { new Token(str, metadata) }.ToAsyncEnumerable(cancellationToken),
                cancellationToken)
                .Select(t => t.String, cancellationToken))
            {
                yield return s;
            }
        }

        /// <summary>
        /// Convenience method for passing a string through a pipeline and getting
        /// strings out. This method takes care of wrapping the passed string in a
        /// token and mapping the resulting tokens back to strings.
        /// </summary>
        /// <param name="str">The string to pass through the pipeline.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The processed list of string tokens.</returns>
        public IAsyncEnumerable<string> RunString(string str, CancellationToken cancellationToken)
            => RunString(str, new TokenMetadata(), cancellationToken);

        /// <summary>
        /// Resets the pipeline by removing any existing processors.
        /// </summary>
        public void Reset()
        {
            Process.Clear();
        }

        /// <summary>
        /// Runs a single step of the pipeline on each of the tokens, and enumerates the results as a single async
        /// enumeration. This is like an async SelectMany.
        /// </summary>
        /// <param name="step">The pipeline function to run on each token.</param>
        /// <param name="tokens">The tokens to process.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The flat processed list of tokens.</returns>
        private static async IAsyncEnumerable<Token> RunStep(
            Function step,
            IAsyncEnumerable<Token> tokens,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            int i = 0;
            await foreach (Token token in tokens)
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                await foreach (Token processedToken in step(token, i++, tokens, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested) yield break;
                    if (processedToken.String != "") yield return processedToken;
                }
            }
        }
    }
}
