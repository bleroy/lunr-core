using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Idx = Lunr.Index;

namespace Lunr.Globalization.fr
{
	public static class Index
	{
		public static async Task<Idx> Build(
			Func<Builder, Task>? config = null!,
			Tokenizer? tokenizer = null!,
			PipelineFunctionRegistry? registry = null!,
			IEnumerable<string>? indexingPipeline = null!,
			IEnumerable<string>? searchPipeline = null!,
			params Field[] fields)
		{
			Pipeline.Function trimmerFunction = new FrenchTrimmer().FilterFunction;
			Pipeline.Function filterFunction = new FrenchStopWordFilter().FilterFunction;
			Pipeline.Function stemmerFunction = new FrenchStemmer().StemmerFunction;

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

			var builder = new Builder(
				indexingPipeline: idxPipeline,
				searchPipeline: srchPipeline,
				tokenizer: tokenizer ?? new Tokenizer(),
				fields: fields);

			if (config != null)
			{
				await config(builder);
			}

			return builder.Build();
		}
	}
}