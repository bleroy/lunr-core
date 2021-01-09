﻿using System;

namespace Lunr.Globalization.fr
{
	public sealed class FrenchStopWordFilter : StopWordFilterBase
	{
		private const string Data =
			@"ai aie aient aies ait as au aura aurai auraient aurais aurait auras aurez auriez aurions aurons auront aux avaient avais avait avec avez aviez avions avons ayant ayez ayons c ce ceci celà ces cet cette d dans de des du elle en es est et eu eue eues eurent eus eusse eussent eusses eussiez eussions eut eux eûmes eût eûtes furent fus fusse fussent fusses fussiez fussions fut fûmes fût fûtes ici il ils j je l la le les leur leurs lui m ma mais me mes moi mon même n ne nos notre nous on ont ou par pas pour qu que quel quelle quelles quels qui s sa sans se sera serai seraient serais serait seras serez seriez serions serons seront ses soi soient sois soit sommes son sont soyez soyons suis sur t ta te tes toi ton tu un une vos votre vous y à étaient étais était étant étiez étions été étée étées étés êtes";

		private static readonly ISet<string> WordList =
			new Set<string>(Data.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries));

		protected override ISet<string> StopWords => WordList;
	}
}