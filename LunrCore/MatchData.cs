using System.Collections.Generic;
using System.Linq;

namespace Lunr
{
    /// <summary>
    /// Contains and collects metadata about a matching document.
    /// A single instance of `MatchData` is returned as part of every index result.
    /// </summary>
    /// <param name="term">The term this match data is associated with.</param>
    /// <param name="field">The field in which the term was found.</param>
    /// <param name="metadata">The metadata recorded about this term in this field.</param>
    public class MatchData
    {
        public static readonly MatchData Empty = new MatchData("", "", new Dictionary<string, IList<object>>());

        public MatchData(
            string term,
            string field,
            IDictionary<string, IList<object>> metadata)
        {
            Term = term;
            Field = field;

            // Cloning the metadata to prevent the original being mutated during match data combination.
            // Metadata is kept in an array within the inverted index.
            var clonedMetadata = new Dictionary<string, IList<object>>(capacity: metadata.Count);

            foreach((string key, IEnumerable<object> value) in metadata)
            {
                clonedMetadata.Add(key, new List<object>(value));
            }

            Metadata = new Dictionary<string, IDictionary<string, IDictionary<string, IList<object>>>>
            {
                {
                    term,
                    new Dictionary<string, IDictionary<string, IList<object>>>
                    {
                        {
                            field,
                            clonedMetadata
                        }
                    }
                }
            };
        }

        /// <summary>
        /// The term this match data is associated with.
        /// </summary>
        public string Term { get; }

        /// <summary>
        /// The field in which the term was found.
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// A cloned collection of metadata associated with this document.
        /// </summary>
        public IDictionary<string, IDictionary<string, IDictionary<string, IList<object>>>> Metadata { get; }

        /// <summary>
        /// An instance of `MatchData` will be created for every term that matches a
        /// document. However only one instance is required in an index result. This
        /// method combines metadata from another instance of `MatchData` with this
        /// object's metadata.
        /// </summary>
        /// <param name="otherMatchData">Another instance of match data to merge with this one.</param>
        public void Combine(MatchData otherMatchData)
        {
            IEnumerable<string> terms = otherMatchData.Metadata.Keys;

            foreach(string term in terms)
            {
                IEnumerable<string> fields = otherMatchData.Metadata[term].Keys;
                if (!Metadata.ContainsKey(term))
                {
                    Metadata.Add(term, new Dictionary<string, IDictionary<string, IList<object>>>());
                }
                IDictionary<string, IDictionary<string, IList<object>>> thisTermEntry = Metadata[term];
                foreach (string field in fields)
                {
                    IEnumerable<string> keys = otherMatchData.Metadata[term][field].Keys;
                    if (!thisTermEntry.ContainsKey(field))
                    {
                        thisTermEntry.Add(field, new Dictionary<string, IList<object>>(capacity: otherMatchData.Metadata[term][field].Keys.Count));
                    }
                    IDictionary<string, IList<object>> thisFieldEntry = thisTermEntry[field];
                    foreach(string key in keys)
                    {
                        IList<object> otherData = otherMatchData.Metadata[term][field][key];
                        if (!thisFieldEntry.ContainsKey(key))
                        {
                            thisFieldEntry.Add(key, new List<object>(otherData));
                        }
                        else
                        {
                            thisFieldEntry[key] = thisFieldEntry[key].Concat(otherData).ToList();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add metadata for a term/field pair to this instance of match data.
        /// </summary>
        /// <param name="term">The term this match data is associated with.</param>
        /// <param name="field">The field in which the term was found.</param>
        /// <param name="metadata">The metadata recorded about this term in this field.</param>
        public void Add(string term, string field, IDictionary<string, IList<object>> metadata)
        {
            if (!Metadata.ContainsKey(term))
            {
                Metadata.Add(term, new Dictionary<string, IDictionary<string, IList<object>>>
                {
                    {
                        field,
                        metadata
                    }
                });
                return;
            }

            IDictionary<string, IDictionary<string, IList<object>>> termMetadata = Metadata[term];
            if (!termMetadata.ContainsKey(field))
            {
                termMetadata.Add(field, metadata);
                return;
            }

            foreach(string key in metadata.Keys)
            {
                IDictionary<string, IList<object>> fieldMetadata = termMetadata[field];
                if (fieldMetadata.ContainsKey(key))
                {
                    fieldMetadata[key] = fieldMetadata[key].Concat(metadata[key]).ToList();
                }
                else
                {
                    fieldMetadata[key] = metadata[key];
                }
            }
        }
    }
}
