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
        public static readonly MatchData Empty = new MatchData("", "", new FieldMatchMetadata());

        public MatchData(
            string term,
            string field,
            FieldMatchMetadata metadata)
        {
            Term = term;
            Field = field;

            // Cloning the metadata to prevent the original being mutated during match data combination.
            // Metadata is kept in an array within the inverted index.
            var clonedMetadata = new FieldMatchMetadata(capacity: metadata.Count);

            foreach((string key, IEnumerable<object?> value) in metadata)
            {
                clonedMetadata.Add(key, new List<object?>(value));
            }

            Posting = new InvertedIndexEntry
            {
                {
                    term,
                    new FieldMatches
                    {
                        { field, clonedMetadata }
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
        public InvertedIndexEntry Posting { get; }

        /// <summary>
        /// An instance of `MatchData` will be created for every term that matches a
        /// document. However only one instance is required in an index result. This
        /// method combines metadata from another instance of `MatchData` with this
        /// object's metadata.
        /// </summary>
        /// <param name="otherMatchData">Another instance of match data to merge with this one.</param>
        public void Combine(MatchData otherMatchData)
        {
            IEnumerable<string> terms = otherMatchData.Posting.Keys;

            foreach(string term in terms)
            {
                IEnumerable<string> fields = otherMatchData.Posting[term].Keys;
                if (!Posting.ContainsKey(term))
                {
                    Posting.Add(term, new FieldMatches());
                }
                Dictionary<string, FieldMatchMetadata> thisTermEntry = Posting[term];
                foreach (string field in fields)
                {
                    IEnumerable<string> keys = otherMatchData.Posting[term][field].Keys;
                    if (!thisTermEntry.ContainsKey(field))
                    {
                        thisTermEntry.Add(field, new FieldMatchMetadata(capacity: otherMatchData.Posting[term][field].Keys.Count));
                    }
                    FieldMatchMetadata thisFieldEntry = thisTermEntry[field];
                    foreach(string key in keys)
                    {
                        IList<object?> otherData = otherMatchData.Posting[term][field][key];
                        if (!thisFieldEntry.ContainsKey(key))
                        {
                            thisFieldEntry.Add(key, new List<object?>(otherData));
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
        public void Add(string term, string field, FieldMatchMetadata metadata)
        {
            if (!Posting.ContainsKey(term))
            {
                Posting.Add(term, new FieldMatches
                {
                    {
                        field,
                        metadata
                    }
                });
                return;
            }

            FieldMatches termMetadata = Posting[term];
            if (!termMetadata.ContainsKey(field))
            {
                termMetadata.Add(field, metadata);
                return;
            }

            foreach(string key in metadata.Keys)
            {
                FieldMatchMetadata fieldMetadata = termMetadata[field];
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
