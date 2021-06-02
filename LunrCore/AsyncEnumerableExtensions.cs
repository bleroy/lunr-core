using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lunr
{
    /// <summary>
    /// A set of internal simple extensions to work with asynchronous enumerable
    /// without importing System.Linq.Async.
    /// </summary>
    public static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Applies the provided selector on each item in the source enumeration.
        /// </summary>
        /// <param name="source">The source enumeration.</param>
        /// <param name="selector">The selector to apply on each item in the enumeration.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The async enumeration of results from applying the selector.</returns>
        public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, TResult> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (TSource sourceItem in source.WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                yield return selector(sourceItem);
            }
        }

        /// <summary>
        /// Creates an async enumerable from a regular enumerable.
        /// </summary>
        /// <param name="source">The enumerable.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The async enumerable.</returns>
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
            this IEnumerable<T> source,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach(T item in source)
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                yield return await Task.FromResult(item).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Builds a list from an async enumerable.
        /// This enumerates the whole thing, so use with caution,
        /// there's probably a reason why that was async enumerable.
        /// </summary>
        /// <param name="source">The async enumerable.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        public static async Task<IList<T>> ToList<T>(
            this IAsyncEnumerable<T> source,
            CancellationToken? cancellationToken = null)
        {
            var result = new List<T>();
            await foreach (T item in source)
            {
                if (cancellationToken?.IsCancellationRequested ?? false)
                {
                    return result;
                }
                result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Tests if an async enumerable has any elements satisfying a condition.
        /// </summary>
        /// <param name="source">The enumerable.</param>
        /// <param name="predicate">
        /// An optional predicate that an element of the enumerable must satisfy.
        /// If this is not provided, any element will do.
        /// </param>
        /// <returns>True if any element satisfy the condition.</returns>
        public static async Task<bool> Any<T>(
            this IAsyncEnumerable<T> source,
            Func<T, bool>? predicate = null)
        {
            await foreach(T item in source)
            {
                if (predicate is null || predicate(item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// An empty async enumerable.
        /// </summary>
        /// <returns>An empty async enumerable of the specified type.</returns>
        public static async IAsyncEnumerable<T> Empty<T>()
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }
    }
}
