using System.Collections.Generic;
using System.Linq;

namespace Lunr
{
    /// <summary>
    /// A registry of named pipeline functions.
    /// </summary>
    public class PipelineFunctionRegistry : Dictionary<string, Pipeline.Function>
    {
        public PipelineFunctionRegistry() : base() { }

        public PipelineFunctionRegistry(IDictionary<string, Pipeline.Function> dictionary) : base(dictionary) { }

        public PipelineFunctionRegistry(params (string name, Pipeline.Function function)[] functions)
            : base(functions.ToDictionary(kv => kv.name, kv => kv.function)) { }
    }
}
