using System;

namespace Lunr
{
    public class FieldReference
    {
        public const char Joiner = '/';

        private string? _stringValue;

        public FieldReference(string documentReference, string fieldName, string? stringValue = null!)
        {
            DocumentReference = documentReference;
            FieldName = fieldName;
            _stringValue = stringValue;
        }

        public string DocumentReference { get; }
        public string FieldName { get; }

        public static FieldReference FromString(string s)
        {
            int n = s.IndexOf(Joiner);

            if (n == -1) throw new InvalidOperationException($"Malformed field reference string: \"{s}\".");

            return new FieldReference(s.Substring(n + 1), s.Substring(0, n), s);
        }

        public override string ToString()
            => _stringValue ??= FieldName + Joiner + DocumentReference;

        public override bool Equals(object obj)
            => obj is FieldReference otherRef
                && otherRef.FieldName == FieldName
                && otherRef.DocumentReference == DocumentReference;

        public override int GetHashCode() => (DocumentReference, FieldName).GetHashCode();
    }
}
