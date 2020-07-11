using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Lunr
{
    /// <summary>
    /// A set of internal simple extensions to work with asynchronous enumerables
    /// without importing System.Linq.Async.
    /// </summary>
    public static class AsyncEnumerableExtensions
    {
        /// <summary>
        /// Applies the provided selector on each item in the source enumeration,
        /// then enumrates the flattened list of list of results.
        /// </summary>
        /// <param name="source">The source enumeration.</param>
        /// <param name="selector">The selector to apply on each item in the enumeration.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The flattened enumeration of results from applying the selector.</returns>
        public static async IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, int, CancellationToken, IAsyncEnumerable<TResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            int i = 0;
            await foreach(TSource sourceItem in source)
            {
                await foreach(TResult resultItem in selector(sourceItem, i++, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested) yield break;
                    yield return resultItem;
                }
            }
        }

        /// <summary>
        /// Applies the provided selector on each item in the source enumeration,
        /// then enumrates the flattened list of list of results.
        /// </summary>
        /// <param name="source">The source enumeration.</param>
        /// <param name="selector">The selector to apply on each item in the enumeration.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The flattened enumeration of results from applying the selector.</returns>
        public static async IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, CancellationToken, IAsyncEnumerable<TResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (TSource sourceItem in source)
            {
                await foreach (TResult resultItem in selector(sourceItem, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested) yield break;
                    yield return resultItem;
                }
            }
        }

        /// <summary>
        /// Applies the provided selector on each item in the source enumeration,
        /// then enumrates the flattened list of list of results.
        /// </summary>
        /// <param name="source">The source enumeration.</param>
        /// <param name="selector">The selector to apply on each item in the enumeration.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The flattened enumeration of results from applying the selector.</returns>
        public static async IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, IAsyncEnumerable<TResult>> selector,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (TSource sourceItem in source)
            {
                await foreach (TResult resultItem in selector(sourceItem))
                {
                    if (cancellationToken.IsCancellationRequested) yield break;
                    yield return resultItem;
                }
            }
        }

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
            await foreach (TSource sourceItem in source)
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
                yield return await Task.FromResult(item);
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
            CancellationToken cancellationToken)
        {
            var result = new List<T>();
            await foreach (T item in source)
            {
                if (cancellationToken.IsCancellationRequested) return result;
                result.Add(item);
            }
            return result;
        }

        public static async IAsyncEnumerable<T> Empty<T>()
        {
            yield break;
        }
    }
}
