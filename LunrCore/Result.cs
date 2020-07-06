namespace Lunr
{
    /// <summary>
    /// A result contains details of a document matching a search query.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Builds a new result.
        /// </summary>
        /// <param name="documentReference">The reference of the document this result represents.</param>
        /// <param name="score">A number between 0 and 1 representing how similar this document is to the query.</param>
        /// <param name="matchData">Contains metadata about this match including which term(s) caused the match.</param>
        public Result(string documentReference, double score, MatchData matchData)
        {
            DocumentReference = documentReference;
            Score = score;
            MatchData = matchData;
        }

        /// <summary>
        /// The reference of the document this result represents.
        /// </summary>
        public string DocumentReference { get; }

        /// <summary>
        /// A number between 0 and 1 representing how similar this document is to the query.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Contains metadata about this match including which term(s) caused the match.
        /// </summary>
        public MatchData MatchData { get; }
    }
}
