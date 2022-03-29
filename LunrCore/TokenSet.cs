using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lunr
{
    /// <summary>
    /// A token set is used to store the unique list of all tokens
    /// within an index. Token sets are also used to represent an
    /// incoming query to the index, this query token set and index
    /// token set are then intersected to find which tokens to look
    /// up in the inverted index.
    ///
    /// A token set can hold multiple tokens, as in the case of the
    /// index token set, or it can hold a single token as in the
    /// case of a simple query token set.
    ///
    /// Additionally token sets are used to perform wildcard matching.
    /// Leading, contained and trailing wildcards are supported, and
    /// from this edit distance matching can also be provided.
    ///
    /// Token sets are implemented as a minimal finite state automaton,
    /// where both common prefixes and suffixes are shared between tokens.
    /// This helps to reduce the space used for storing the token set.
    /// </summary>
    public class TokenSet
    {
        private readonly TokenSetIdProvider _idProvider;
        private string? _str;

        public TokenSet() : this(TokenSetIdProvider.Instance) { }

        public TokenSet(TokenSetIdProvider idProvider)
        {
            _idProvider = idProvider;
            Id = _idProvider.Next();
        }

        public bool IsFinal { get; set; }
        public IDictionary<char, TokenSet> Edges { get; }
            = new Dictionary<char, TokenSet>();
        public int Id { get; }

        /// <summary>
        /// Creates a TokenSet instance from the given sorted array of words.
        /// </summary>
        /// <param name="arr">A sorted array of strings to create the set from.</param>
        /// <param name="idProvider">An optional token set id provider.</param>
        /// <returns>A token set.</returns>
        public static TokenSet FromArray(
            IEnumerable<string> arr,
            TokenSetIdProvider? idProvider = null)
        {
            var builder = new Builder(idProvider ?? TokenSetIdProvider.Instance);

            foreach (string s in arr)
            {
                builder.Insert(s);
            }

            builder.Finish();
            return builder.Root;
        }

        /// <summary>
        /// Creates a token set from a query clause.
        /// </summary>
        /// <param name="clause">A single clause.</param>
        /// <param name="idProvider">An optional token set id provider.</param>
        /// <returns>The token set.</returns>
        public static TokenSet FromClause(
            Clause clause,
            TokenSetIdProvider? idProvider = null)
        {
            idProvider ??= TokenSetIdProvider.Instance;
            return clause.EditDistance > 0
                           ? FromFuzzyString(clause.Term, clause.EditDistance, idProvider)
                           : FromString(clause.Term, idProvider);
        }

        /// <summary>
        /// Creates a token set representing a single string with a specified
        /// edit distance.
        /// 
        /// Insertions, deletions, substitutions and transpositions are each
        /// treated as an edit distance of 1.
        /// 
        /// Increasing the allowed edit distance will have a dramatic impact
        /// on the performance of both creating and intersecting these TokenSets.
        /// It is advised to keep the edit distance less than 3.
        /// </summary>
        /// <param name="str">The string to create the token set from.</param>
        /// <param name="editDistance">The allowed edit distance to match.</param>
        /// <param name="idProvider">An optional token set id provider.</param>
        public static TokenSet FromFuzzyString(
            string str,
            int editDistance,
            TokenSetIdProvider? idProvider = null)
        {
            idProvider ??= TokenSetIdProvider.Instance;
            var root = new TokenSet(idProvider);

            var stack = new Stack<(TokenSet node, int editsRemaining, string str)>();
            stack.Push((
                node: root,
                editsRemaining: editDistance,
                str));

            while (stack.Any())
            {
                (TokenSet frameNode, int frameEditsRemaining, string frameStr) = stack.Pop();

                // no edit
                if (frameStr.Length > 0)
                {
                    char ch = frameStr[0];

                    TokenSet noEditNode;
                    if (frameNode.Edges.TryGetValue(ch, out TokenSet? existingChNode))
                    {
                        noEditNode = existingChNode;
                    }
                    else
                    {
                        noEditNode = new TokenSet(idProvider);
                        frameNode.Edges.Add(ch, noEditNode);
                    }

                    if (frameStr.Length == 1)
                    {
                        noEditNode.IsFinal = true;
                    }

                    stack.Push((
                        node: noEditNode,
                        editsRemaining: frameEditsRemaining,
                        str: frameStr.Substring(1)));
                }

                if (frameEditsRemaining == 0) continue;

                // insertion
                TokenSet insertionNode;
                if (frameNode.Edges.TryGetValue(Query.Wildcard, out TokenSet? wildcardNode))
                {
                    insertionNode = wildcardNode;
                }
                else
                {
                    insertionNode = new TokenSet(idProvider);
                    frameNode.Edges.Add(Query.Wildcard, insertionNode);
                }

                if (frameStr.Length == 0)
                {
                    insertionNode.IsFinal = true;
                }

                stack.Push((
                    node: insertionNode,
                    editsRemaining: frameEditsRemaining - 1,
                    str: frameStr));

                // deletion
                // Can only do a deletion if we have enough edits remaining
                // and if there are characters left to delete in the string.
                if (frameStr.Length > 1)
                {
                    stack.Push((
                        node: frameNode,
                        editsRemaining: frameEditsRemaining - 1,
                        str: frameStr.Substring(1)));
                }

                // deletion
                // Just removing the last character from the string.
                if (frameStr.Length == 1)
                {
                    frameNode.IsFinal = true;
                }

                // substitution
                // Can only do a substitution if we have enough edits remaining
                // and if there are characters left to substitute.
                if (frameStr.Length >= 1)
                {
                    TokenSet substitutionNode;
                    if (frameNode.Edges.TryGetValue(Query.Wildcard, out TokenSet? substitutionWildcardNode))
                    {
                        substitutionNode = substitutionWildcardNode;
                    }
                    else
                    {
                        substitutionNode = new TokenSet(idProvider);
                        frameNode.Edges.Add(Query.Wildcard, substitutionNode);
                    }

                    if (frameStr.Length == 1)
                    {
                        substitutionNode.IsFinal = true;
                    }

                    stack.Push((
                        node: substitutionNode,
                        editsRemaining: frameEditsRemaining - 1,
                        str: frameStr.Substring(1)));
                }

                // transposition
                // Can only do a transposition if there are edits remaining
                // and there are enough characters to transpose.
                if (frameStr.Length > 1)
                {
                    char chA = frameStr[0];
                    char chB = frameStr[1];
                    TokenSet transposeNode;

                    if (frameNode.Edges.TryGetValue(chB, out TokenSet? chBNode))
                    {
                        transposeNode = chBNode;
                    }
                    else
                    {
                        transposeNode = new TokenSet(idProvider);
                        frameNode.Edges.Add(chB, transposeNode);
                    }

                    if (frameStr.Length == 1) // Note: I'm pretty sure this can't happen, just porting the js version closely.
                    {
                        transposeNode.IsFinal = true;
                    }

                    stack.Push((
                        node: transposeNode,
                        editsRemaining: frameEditsRemaining - 1,
                        str: chA + frameStr.Substring(2)));
                }
            }

            return root;
        }

        /// <summary>
        /// Creates a TokenSet from a string.
        /// 
        /// The string may contain one or more wildcard characters(*)
        /// that will allow wildcard matching when intersecting with
        /// another TokenSet.
        /// </summary>
        /// <param name="str">The string to create a TokenSet from.</param>
        /// <param name="idProvider">An optional token set id provider.</param>
        /// <returns>The token set.</returns>
        public static TokenSet FromString(
            string str,
            TokenSetIdProvider? idProvider = null)
        {
            idProvider ??= TokenSetIdProvider.Instance;
            var root = new TokenSet(idProvider);
            TokenSet node = root;

            // Iterates through all characters within the passed string
            // appending a node for each character.
            //
            // When a wildcard character is found then a self
            // referencing edge is introduced to continually match
            // any number of any characters.
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                bool isFinal = i == str.Length - 1;

                if (ch == Query.Wildcard)
                {
                    node.Edges.Add(ch, node);
                    node.IsFinal = isFinal;
                }
                else
                {
                    var next = new TokenSet(idProvider) { IsFinal = isFinal };

                    node.Edges.Add(ch, next);
                    node = next;
                }
            }

            return root;
        }

        /// <summary>
        /// Tests if the token set is empty.
        /// </summary>
        /// <returns>True if not empty.</returns>
        public bool Any()
        {
            var stack = new Stack<(string, TokenSet)>();
            stack.Push(("", this));

            while (stack.Any())
            {
                (string prefix, TokenSet node) = stack.Pop();

                if (node.IsFinal)
                {
                    return true;
                }

                foreach ((char edgeKey, TokenSet edge) in node.Edges)
                {
                    stack.Push((prefix + edgeKey, edge));
                }
            }

            return false;
        }

        /// <summary>
        /// Converts this TokenSet into an enumeration of strings
        /// contained within the TokenSet.
        ///
        /// This is not intended to be used on a TokenSet that
        /// contains wildcards, in these cases the results are
        /// undefined and are likely to cause an infinite loop.
        /// </summary>
        /// <returns>The list of strings in the token set.</returns>
        public IEnumerable<string> ToEnumeration()
        {
            var stack = new Stack<(string prefix, TokenSet node)>();
            stack.Push(("", this));

            while (stack.Any())
            {
                (string prefix, TokenSet node) = stack.Pop();

                if (node.IsFinal)
                {
                    yield return prefix;
                }

                foreach ((char edgeKey, TokenSet edge) in node.Edges)
                {
                    stack.Push((prefix + edgeKey, edge));
                }
            }
        }

        /// <summary>
        /// Returns a new TokenSet that is the intersection of
        /// this TokenSet and the passed TokenSet.
        /// 
        /// This intersection will take into account any wildcards
        /// contained within the TokenSet.
        /// </summary>
        /// <param name="other">Another TokenSet to intersect with.</param>
        /// <returns>The intersection of the sets.</returns>
        public TokenSet Intersect(TokenSet other)
        {
            var output = new TokenSet(_idProvider);

            Stack<(TokenSet qNode, TokenSet output, TokenSet node)> stack = new ();
            stack.Push((other, output, this));

            while (stack.Any())
            {
                (TokenSet frameQNode, TokenSet frameOutput, TokenSet frameNode) = stack.Pop();

                foreach ((char qEdge, TokenSet qNode) in frameQNode.Edges)
                {
                    foreach ((char nEdge, TokenSet node) in frameNode.Edges)
                    {
                        if (nEdge == qEdge || qEdge == '*')
                        {
                            bool isFinal = node.IsFinal && qNode.IsFinal;

                            if (frameOutput.Edges.TryGetValue(nEdge, out TokenSet? next))
                            {
                                // an edge already exists for this character
                                // no need to create a new node, just set the finality
                                // bit unless this node is already final
                                next.IsFinal = next.IsFinal || isFinal;
                            }
                            else
                            {
                                // no edge exists yet, must create one
                                // set the finality bit and insert it
                                // into the output
                                next = new TokenSet(_idProvider) { IsFinal = isFinal };
                                frameOutput.Edges.Add(nEdge, next);
                            }

                            stack.Push((qNode, next, node));
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Generates a string representation of a TokenSet.
        ///
        /// This is intended to allow TokenSets to be used as keys
        /// in objects, largely to aid the construction and minimization
        /// of a TokenSet.As such it is not designed to be a human
        /// friendly representation of the TokenSet.
        /// </summary>
        /// <returns>The string representation of the TokenSet.</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(_str)) return _str!;

            var str = new ValueStringBuilder(128);
            
            str.Append(IsFinal ? "1" : "0");

            foreach ((char label, TokenSet node) in Edges.OrderBy(static k => k.Key.ToString(), StringComparer.Ordinal))
            {
                str.Append(label);
                str.AppendSpanFormattable(node.Id);
            }

            return str.ToString();
        }

        public sealed class Builder
        {
            private string _previousWord = "";
            private readonly List<(TokenSet parent, char ch, TokenSet child)> _uncheckedNodes = new ();
            private readonly Dictionary<string, TokenSet> _minimizedNodes = new ();
            private readonly TokenSetIdProvider _idProvider;

            public Builder(TokenSetIdProvider? idProvider = null)
            {
                _idProvider = idProvider ?? TokenSetIdProvider.Instance;
                Root = new TokenSet(_idProvider);
            }

            public TokenSet Root { get; }

            public void Insert(string word)
            {
                int commonPrefix = 0;

                if (StringComparer.Ordinal.Compare(word, _previousWord) < 0)
                    throw new InvalidOperationException("Out of order word insertion.");

                for (int i = 0; i < word.Length && i < _previousWord.Length; i ++)
                {
                    if (word[i] != _previousWord[i]) break;
                    commonPrefix++;
                }

                Minimize(commonPrefix);

                TokenSet node = _uncheckedNodes.LastOrDefault().child ?? Root;

                for (int i = commonPrefix; i < word.Length; i++)
                {
                    var nextNode = new TokenSet(_idProvider);
                    char ch = word[i];

                    node.Edges.Add(ch, nextNode);

                    _uncheckedNodes.Add((node, ch, nextNode));

                    node = nextNode;
                }

                node.IsFinal = true;
                _previousWord = word;
            }

            public void Finish() => Minimize(0);

            public void Minimize(int downTo)
            {
                for (int i = _uncheckedNodes.Count - 1; i >= downTo; i--)
                {
                    (TokenSet parent, char ch, TokenSet child) = _uncheckedNodes[i];
                    string childKey = child.ToString();

                    if (_minimizedNodes.ContainsKey(childKey))
                    {
                        parent.Edges[ch] = _minimizedNodes[childKey];
                    }
                    else
                    {
                        // Cache the key for this node since we know it can't change anymore.
                        child._str = childKey;

                        _minimizedNodes.Add(childKey, child);
                    }

                    _uncheckedNodes.RemoveAt(_uncheckedNodes.Count - 1);
                }
            }
        }
    }
}
