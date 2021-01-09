using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lunr;
using Lunr.Globalization.it;

namespace LunrCoreLmdb.Globalization.it
{
    public static class LmdbIndex
    {
        /// <summary>
        /// A convenience function for configuring and constructing
        /// a new lunr DelegatedIndex.
        /// 
        /// An `LmdbBuilder` instance is created and the pipeline setup
        /// with a trimmer, stop word filter and stemmer.
        /// 
        /// This builder object is yielded to the configuration function
        /// that is passed as a parameter, allowing the list of fields
        /// and other builder parameters to be customized.
        /// 
        /// All documents _must_ be added within the passed config function.
        /// </summary>
        /// <example>
        /// var idx = Index.Build(async builder =>
        /// {
        ///      builder
        ///         .AddField("title")
        ///         .AddField("body");
        /// 
        ///      builder.ReferenceField = "id";
        /// 
        ///      foreach(Document doc in documents)
        ///      {
        ///          builder.add(doc);
        ///      }
        /// });
        /// </example>
        /// <param name="path">The directory path to the LMDB database used to store this index.</param>
        /// <param name="trimmer">An optional trimmer. Default is a regex-based word trimmer.</param>
        /// <param name="stopWordFilter">An optional stopword filter. Default is English.</param>
        /// <param name="stemmer">An optional stemmer. Default is English.</param>
        /// <param name="config">A Configuration function.</param>
        /// <param name="tokenizer">An optional tokenizer. Default is a ToString() based splitter. </param>
        /// <param name="registry">An optional pipeline function registry. Default filters through the specific trimmer, stopword filter, and stemmer.</param>
        /// <param name="indexingPipeline">An optional indexing pipeline. Default filters through the specific trimmer, stopword filter, and stemmer. </param>
        /// <param name="searchPipeline">An optional search pipeline. Default filters through the stemmer.</param>
        /// <param name="cancellationToken">An optional cancellation token. Default is equivalent to CancellationToken.None.</param>
        /// <param name="fields">The fields for this builder.</param>
        /// <returns>The delegated index.</returns>
        public static async Task<DelegatedIndex> Build(
            string path,
            Func<LmdbBuilder, Task>? config = null!,
            Tokenizer? tokenizer = null!,
            PipelineFunctionRegistry? registry = null!,
            IEnumerable<string>? indexingPipeline = null!,
            IEnumerable<string>? searchPipeline = null!,
            CancellationToken cancellationToken = default,
            params Field[] fields)
        {
            Pipeline.Function trimmerFunction = new ItalianTrimmer().FilterFunction;
            Pipeline.Function filterFunction = new ItalianStopWordFilter().FilterFunction;
            Pipeline.Function stemmerFunction = new ItalianStemmer().StemmerFunction;
            registry ??= new PipelineFunctionRegistry();
            registry.Add("trimmer", trimmerFunction);
            registry.Add("stopWordFilter", filterFunction);
            registry.Add("stemmer", stemmerFunction);

            Pipeline idxPipeline = indexingPipeline is null
                ? new Pipeline(registry, trimmerFunction, filterFunction, stemmerFunction)
                : new Pipeline(registry, indexingPipeline.Select(function => registry[function]).ToArray());
            Pipeline srchPipeline = searchPipeline is null
                ? new Pipeline(registry, stemmerFunction)
                : new Pipeline(registry, searchPipeline.Select(function => registry[function]).ToArray());

            var builder = new LmdbBuilder(
                indexingPipeline: idxPipeline,
                searchPipeline: srchPipeline,
                tokenizer: tokenizer ?? new Tokenizer(),
                fields: fields);

            if (config != null)
            {
                await config(builder);
            }

            return builder.Build(path, cancellationToken);
        }
    }
}
