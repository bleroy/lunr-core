using System;

namespace Lunr.Multi
{
    public sealed class JapaneseStopWordFilter : StopWordFilterBase
    {
        private const string Data = @"これ それ あれ この その あの ここ そこ あそこ こちら どこ だれ なに なん 何 私 貴方 貴方方 我々 私達 あの人 あのかた 彼女 彼 です あります おります います は が の に を で え から まで より も どの と し それで しかし";
        private static readonly ISet<string> WordList = new Set<string>(Data.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        protected override ISet<string> StopWords => WordList;
    }
}