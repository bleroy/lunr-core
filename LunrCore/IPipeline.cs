using System.Collections.Generic;
using System.Threading;

namespace Lunr
{
    /// <summary>
    /// Pipelines maintain an ordered list of functions to be applied to all
    /// tokens in documents entering the search index and queries being ran against
    /// the index.
    /// 
    /// When run the pipeline will call each function in turn, passing a token, the
    /// index of that token in the original list of all tokens and finally a list of
    /// all the original tokens.
    /// 
    /// The output of functions in the pipeline will be passed to the next function
    /// in the pipeline. To exclude a token from entering the index the function
    /// should return undefined, the rest of the pipeline will not be called with
    /// this token.
    /// </summary>
    public interface IPipeline
    {
        /// <summary>
        /// Adds new functions to the end of the pipeline.
        /// </summary>
        /// <param name="functions">Any number of functions to add to the pipeline.</param>
        /// <returns>The pipeline.</returns>
        Pipeline Add(params Pipeline.Function[] functions);

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
        Pipeline RegisterFunction(Pipeline.Function fn, string label);

        /// <summary>
        /// Runs the current list of functions that make up the pipeline against the passed tokens.
        /// </summary>
        /// <param name="tokens">The tokens to run through the pipeline.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The processed tokens.</returns>
        IAsyncEnumerable<Token> Run(IAsyncEnumerable<Token> tokens, CancellationToken cancellationToken);

        /// <summary>
        /// Convenience method for passing a string through a pipeline and getting
        /// strings out. This method takes care of wrapping the passed string in a
        /// token and mapping the resulting tokens back to strings.
        /// </summary>
        /// <param name="str">The string to pass through the pipeline.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The processed list of string tokens.</returns>
        IAsyncEnumerable<string> RunString(string str, CancellationToken cancellationToken);

        /// <summary>
        /// Convenience method for passing a string through a pipeline and getting
        /// strings out. This method takes care of wrapping the passed string in a
        /// token and mapping the resulting tokens back to strings.
        /// </summary>
        /// <param name="str">The string to pass through the pipeline.</param>
        /// <param name="metadata">Some metadata to pass along.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The processed list of string tokens.</returns>
        IAsyncEnumerable<string> RunString(string str, IDictionary<string, object> metadata, CancellationToken cancellationToken);
    }
}