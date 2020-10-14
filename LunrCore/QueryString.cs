namespace Lunr
{
    /// <summary>
    /// Although lunr provides the ability to create queries using `Query`, it also provides a simple
    /// query language which itself is parsed into an instance of lunr.Query.
    ///
    /// For programmatically building queries it is advised to directly use `Query`, the query language
    /// is best used for human entered text rather than program generated text.
    ///
    /// At its simplest queries can just be a single term, e.g. `hello`, multiple terms are also supported
    /// and will be combined with OR, e.g `hello world` will match documents that contain either 'hello'
    /// or 'world', though those that contain both will rank higher in the results.
    ///
    /// Wildcards can be included in terms to match one or more unspecified characters, these wildcards can
    /// be inserted anywhere within the term, and more than one wildcard can exist in a single term.Adding
    /// wildcards will increase the number of documents that will be found but can also have a negative
    /// impact on query performance, especially with wildcards at the beginning of a term.
    ///
    /// Terms can be restricted to specific fields, e.g. `title:hello`, only documents with the term
    /// hello in the title field will match this query.Using a field not present in the index will lead
    /// to an error being thrown.
    ///
    /// Modifiers can also be added to terms, lunr supports edit distance and boost modifiers on terms.A term
    /// boost will make documents matching that term score higher, e.g. `foo^5`. Edit distance is also supported
    /// to provide fuzzy matching, e.g. 'hello~2' will match documents with hello with an edit distance of 2.
    /// Avoid large values for edit distance to improve query performance.
    ///
    /// Each term also supports a presence modifier.By default a term's presence in document is optional, however
    /// this can be changed to either required or prohibited. For a term's presence to be required in a document the
    /// term should be prefixed with a '+', e.g. `+foo bar` is a search for documents that must contain 'foo' and
    /// optionally contain 'bar'. Conversely a leading '-' sets the terms presence to prohibited, i.e.it must not
    /// appear in a document, e.g. `-foo bar` is a search for documents that do not contain 'foo' but may contain 'bar'.
    ///
    /// To escape special characters the backslash character '\' can be used, this allows searches to include
    /// characters that would normally be considered modifiers, e.g. `foo\~2` will search for a term "foo~2" instead
    /// of attempting to apply a boost of 2 to the search term "foo".
    /// </summary>
    /// <example>Simple single term query: "hello"</example>
    /// <example>Multiple term query: "hello world"</example>
    /// <example>Term scoped to a field: "title:hello"</example>
    /// <example>Term with a boost of 10: "hello^10"</example>
    /// <example>Term with an edit distance of 2: "hello~2"</example>
    /// <example>Terms with presence modifiers: "-foo +bar baz"</example>
    public sealed class QueryString
    {
        /// <summary>
        /// Constructs a query string.
        /// </summary>
        /// <param name="value">The string value of the query string.</param>
        public QueryString(string value) => Value = value;

        /// <summary>
        /// The string value of the query string.
        /// </summary>
        public string Value { get; }

        public static implicit operator string(QueryString qs) => qs.Value;
        public static implicit operator QueryString(string s) => new QueryString(s);
    }
}
