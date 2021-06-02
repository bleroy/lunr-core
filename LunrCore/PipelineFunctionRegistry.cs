using System.Collections.Generic;
using System.Linq;

namespace Lunr
{
    /// <summary>
    /// A registry of named pipeline functions.
    /// </summary>
    public sealed class PipelineFunctionRegistry : Dictionary<string, Pipeline.Function>
    {
        public PipelineFunctionRegistry()
        { }

        public PipelineFunctionRegistry(IDictionary<string, Pipeline.Function> dictionary) : base(dictionary) { }

        public PipelineFunctionRegistry(params (string name, Pipeline.Function function)[] functions)
            : base(functions.ToDictionary(kv => kv.name, kv => kv.function)) { }
    }
}
