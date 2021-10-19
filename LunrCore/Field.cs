using System;
using System.Diagnostics;

namespace Lunr
{
    /// <summary>
    /// A field of indeterminate type.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class Field
    {
        protected Field(string name, double boost = 1)
        {
            if (name is "") throw new InvalidOperationException("Can't create a field with an empty name.");
            if (name.IndexOf('/') != -1) throw new InvalidOperationException($"Can't create a field with a '/' character in its name \"{name}\".");

            Name = name;
            Boost = boost;
        }

        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Boost applied to all terms within this field.
        /// </summary>
        public double Boost { get; }

        public abstract object? ExtractValue(Document doc);

        private string DebuggerDisplay => Boost != 1 ? $"{Name} x{Boost}" : Name;
    }

    /// <summary>
    /// Represents an index field.
    /// </summary>
    public sealed class Field<T> : Field
    {
        public Field(string name, double boost = 1, Func<Document, T>? extractor = null) : base(name, boost) =>
            Extractor = extractor ?? (doc => (T)doc[name]);
        
        /// <summary>
        /// Function to extract a field from a document.
        /// </summary>
        public Func<Document, T> Extractor { get; }

        public override object? ExtractValue(Document doc)
            => Extractor(doc);

    }
}
