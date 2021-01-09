namespace Lunr.Globalization.it
{
	public sealed class ItalianStemmer : StemmerBase
	{
		private int I_p1;
		private int I_p2;
		private int I_pV;
		private readonly SnowballProgram sbp;

		public ItalianStemmer()
		{
			sbp = new SnowballProgram();
			I_p2 = 0;
			I_p1 = 0;
			I_pV = 0;
		}

		public override string Stem(string w)
		{
			sbp.SetCurrent(w);
			StemImpl();
			return sbp.GetCurrent();
		}

		private bool habr1(string c1, string c2, int v_1)
		{
			if (sbp.EqualsSegment(1, c1))
			{
				sbp.ket = sbp.cursor;
				if (sbp.InGrouping(g_v, 97, 249))
				{
					sbp.SliceFrom(c2);
					sbp.cursor = v_1;
					return true;
				}
			}

			return false;
		}

		private void r_prelude()
		{
			var v_1 = sbp.cursor;
			while (true)
			{
				sbp.bra = sbp.cursor;
				var among_var = sbp.FindAmong(a_0, 7);
				if (among_var != 0) /* !! */
				{
					sbp.ket = sbp.cursor;
					switch (among_var)
					{
						case 1:
							sbp.SliceFrom(@"à");
							continue;
						case 2:
							sbp.SliceFrom(@"è");
							continue;
						case 3:
							sbp.SliceFrom(@"ì");
							continue;
						case 4:
							sbp.SliceFrom(@"ò");
							continue;
						case 5:
							sbp.SliceFrom(@"ù");
							continue;
						case 6:
							sbp.SliceFrom("qU");
							continue;
						case 7:
							if (sbp.cursor >= sbp.limit)
							{
								break;
							}

							sbp.cursor++;
							continue;
					}
				}

				break;
			}

			sbp.cursor = v_1;
			while (true)
			{
				var v_2 = sbp.cursor;
				while (true)
				{
					var v_3 = sbp.cursor;
					if (sbp.InGrouping(g_v, 97, 249))
					{
						sbp.bra = sbp.cursor;
						var v_4 = sbp.cursor;
						if (habr1("u", "U", v_3))
						{
							break;
						}

						sbp.cursor = v_4;
						if (habr1("i", "I", v_3))
						{
							break;
						}
					}

					sbp.cursor = v_3;
					if (sbp.cursor >= sbp.limit)
					{
						sbp.cursor = v_2;
						return;
					}

					sbp.cursor++;
				}
			}
		}

		private bool habr2(int v_1)
		{
			sbp.cursor = v_1;
			if (!sbp.InGrouping(g_v, 97, 249))
			{
				return false;
			}

			while (!sbp.OutGrouping(g_v, 97, 249))
			{
				if (sbp.cursor >= sbp.limit)
				{
					return false;
				}

				sbp.cursor++;
			}

			return true;
		}

		private bool habr3()
		{
			if (sbp.InGrouping(g_v, 97, 249))
			{
				var v_1 = sbp.cursor;
				if (sbp.OutGrouping(g_v, 97, 249))
				{
					while (!sbp.InGrouping(g_v, 97, 249))
					{
						if (sbp.cursor >= sbp.limit)
						{
							return habr2(v_1);
						}

						sbp.cursor++;
					}

					return true;
				}

				return habr2(v_1);
			}

			return false;
		}

		private void habr4()
		{
			var v_1 = sbp.cursor;
			if (!habr3())
			{
				sbp.cursor = v_1;
				if (!sbp.OutGrouping(g_v, 97, 249))
				{
					return;
				}

				var v_2 = sbp.cursor;
				if (sbp.OutGrouping(g_v, 97, 249))
				{
					while (!sbp.InGrouping(g_v, 97, 249))
					{
						if (sbp.cursor >= sbp.limit)
						{
							sbp.cursor = v_2;
							if (sbp.InGrouping(g_v, 97, 249) &&
							    sbp.cursor < sbp.limit)
							{
								sbp.cursor++;
							}

							return;
						}

						sbp.cursor++;
					}

					I_pV = sbp.cursor;
					return;
				}

				sbp.cursor = v_2;
				if (!sbp.InGrouping(g_v, 97, 249) || sbp.cursor >= sbp.limit)
				{
					return;
				}

				sbp.cursor++;
			}

			I_pV = sbp.cursor;
		}

		private bool habr5()
		{
			while (!sbp.InGrouping(g_v, 97, 249))
			{
				if (sbp.cursor >= sbp.limit)
				{
					return false;
				}

				sbp.cursor++;
			}

			while (!sbp.OutGrouping(g_v, 97, 249))
			{
				if (sbp.cursor >= sbp.limit)
				{
					return false;
				}

				sbp.cursor++;
			}

			return true;
		}

		private void r_mark_regions()
		{
			var v_1 = sbp.cursor;
			I_pV = sbp.limit;
			I_p1 = I_pV;
			I_p2 = I_pV;
			habr4();
			sbp.cursor = v_1;
			if (habr5())
			{
				I_p1 = sbp.cursor;
				if (habr5())
				{
					I_p2 = sbp.cursor;
				}
			}
		}

		private void r_postlude()
		{
			while (true)
			{
				sbp.bra = sbp.cursor;
				var among_var = sbp.FindAmong(a_1, 3);
				if (among_var == 0) /* !! */
				{
					break;
				}

				sbp.ket = sbp.cursor;
				switch (among_var)
				{
					case 1:
						sbp.SliceFrom("i");
						break;
					case 2:
						sbp.SliceFrom("u");
						break;
					case 3:
						if (sbp.cursor >= sbp.limit)
						{
							return;
						}

						sbp.cursor++;
						break;
				}
			}
		}

		private bool r_RV()
		{
			return I_pV <= sbp.cursor;
		}

		private bool r_R1()
		{
			return I_p1 <= sbp.cursor;
		}

		private bool r_R2()
		{
			return I_p2 <= sbp.cursor;
		}

		private void r_attached_pronoun()
		{
			sbp.ket = sbp.cursor;
			if (sbp.FindAmongBackwards(a_2, 37) != 0) /* !! */
			{
				sbp.bra = sbp.cursor;
				var among_var = sbp.FindAmongBackwards(a_3, 5);
				if (among_var != 0 /* !! */ && r_RV())
				{
					switch (among_var)
					{
						case 1:
							sbp.SliceDelete();
							break;
						case 2:
							sbp.SliceFrom("e");
							break;
					}
				}
			}
		}

		private bool r_standard_suffix()
		{
			sbp.ket = sbp.cursor;
			var among_var = sbp.FindAmongBackwards(a_6, 51);
			if (among_var == 0) /* !! */
			{
				return false;
			}

			sbp.bra = sbp.cursor;
			switch (among_var)
			{
				case 1:
					if (!r_R2())
					{
						return false;
					}

					sbp.SliceDelete();
					break;
				case 2:
					if (!r_R2())
					{
						return false;
					}

					sbp.SliceDelete();
					sbp.ket = sbp.cursor;
					if (sbp.EqualsSegmentBackwards(2, "ic"))
					{
						sbp.bra = sbp.cursor;
						if (r_R2())
						{
							sbp.SliceDelete();
						}
					}

					break;
				case 3:
					if (!r_R2())
					{
						return false;
					}

					sbp.SliceFrom("log");
					break;
				case 4:
					if (!r_R2())
					{
						return false;
					}

					sbp.SliceFrom("u");
					break;
				case 5:
					if (!r_R2())
					{
						return false;
					}

					sbp.SliceFrom("ente");
					break;
				case 6:
					if (!r_RV())
					{
						return false;
					}

					sbp.SliceDelete();
					break;
				case 7:
					if (!r_R1())
					{
						return false;
					}

					sbp.SliceDelete();
					sbp.ket = sbp.cursor;
					among_var = sbp.FindAmongBackwards(a_4, 4);
					if (among_var != 0) /* !! */
					{
						sbp.bra = sbp.cursor;
						if (r_R2())
						{
							sbp.SliceDelete();
							if (among_var == 1)
							{
								sbp.ket = sbp.cursor;
								if (sbp.EqualsSegmentBackwards(2, "at"))
								{
									sbp.bra = sbp.cursor;
									if (r_R2())
									{
										sbp.SliceDelete();
									}
								}
							}
						}
					}

					break;
				case 8:
					if (!r_R2())
					{
						return false;
					}

					sbp.SliceDelete();
					sbp.ket = sbp.cursor;
					among_var = sbp.FindAmongBackwards(a_5, 3);
					if (among_var != 0) /* !! */
					{
						sbp.bra = sbp.cursor;
						if (among_var == 1)
						{
							if (r_R2())
							{
								sbp.SliceDelete();
							}
						}
					}

					break;
				case 9:
					if (!r_R2())
					{
						return false;
					}

					sbp.SliceDelete();
					sbp.ket = sbp.cursor;
					if (sbp.EqualsSegmentBackwards(2, "at"))
					{
						sbp.bra = sbp.cursor;
						if (r_R2())
						{
							sbp.SliceDelete();
							sbp.ket = sbp.cursor;
							if (sbp.EqualsSegmentBackwards(2, "ic"))
							{
								sbp.bra = sbp.cursor;
								if (r_R2())
								{
									sbp.SliceDelete();
								}
							}
						}
					}

					break;
			}

			return true;
		}

		private void r_verb_suffix()
		{
			if (sbp.cursor >= I_pV)
			{
				var v_1 = sbp.limit_backward;
				sbp.limit_backward = I_pV;
				sbp.ket = sbp.cursor;
				var among_var = sbp.FindAmongBackwards(a_7, 87);
				if (among_var != 0) /* !! */
				{
					sbp.bra = sbp.cursor;
					if (among_var == 1)
					{
						sbp.SliceDelete();
					}
				}

				sbp.limit_backward = v_1;
			}
		}

		private void habr6()
		{
			var v_1 = sbp.limit - sbp.cursor;
			sbp.ket = sbp.cursor;
			if (sbp.InGroupingBackwards(g_AEIO, 97, 242))
			{
				sbp.bra = sbp.cursor;
				if (r_RV())
				{
					sbp.SliceDelete();
					sbp.ket = sbp.cursor;
					if (sbp.EqualsSegmentBackwards(1, "i"))
					{
						sbp.bra = sbp.cursor;
						if (r_RV())
						{
							sbp.SliceDelete();
							return;
						}
					}
				}
			}

			sbp.cursor = sbp.limit - v_1;
		}

		private void r_vowel_suffix()
		{
			habr6();
			sbp.ket = sbp.cursor;
			if (sbp.EqualsSegmentBackwards(1, "h"))
			{
				sbp.bra = sbp.cursor;
				if (sbp.InGroupingBackwards(g_CG, 99, 103))
				{
					if (r_RV())
					{
						sbp.SliceDelete();
					}
				}
			}
		}

		private void StemImpl()
		{
			var v_1 = sbp.cursor;
			r_prelude();
			sbp.cursor = v_1;
			r_mark_regions();
			sbp.limit_backward = v_1;
			sbp.cursor = sbp.limit;
			r_attached_pronoun();
			sbp.cursor = sbp.limit;
			if (!r_standard_suffix())
			{
				sbp.cursor = sbp.limit;
				r_verb_suffix();
			}

			sbp.cursor = sbp.limit;
			r_vowel_suffix();
			sbp.cursor = sbp.limit_backward;
			r_postlude();
		}

		#region Data

		private static readonly Among[] a_0 = 
		{
			new Among("", -1, 7),
			new Among("qu", 0, 6),
			new Among(@"á", 0, 1),
			new Among(@"é", 0, 2),
			new Among(@"í", 0, 3),
			new Among(@"ó", 0, 4),
			new Among(@"ú", 0, 5)
		};

		private static readonly Among[] a_1 = 
		{
			new Among("", -1, 3),
			new Among("I", 0, 1),
			new Among("U", 0, 2)
		};

		private static readonly Among[] a_2 = 
		{
			new Among("la", -1, -1),
			new Among("cela", 0, -1),
			new Among("gliela", 0, -1),
			new Among("mela", 0, -1),
			new Among("tela", 0, -1),
			new Among("vela", 0, -1),
			new Among("le", -1, -1),
			new Among("cele", 6, -1),
			new Among("gliele", 6, -1),
			new Among("mele", 6, -1),
			new Among("tele", 6, -1),
			new Among("vele", 6, -1),
			new Among("ne", -1, -1),
			new Among("cene", 12, -1),
			new Among("gliene", 12, -1),
			new Among("mene", 12, -1),
			new Among("sene", 12, -1),
			new Among("tene", 12, -1),
			new Among("vene", 12, -1),
			new Among("ci", -1, -1),
			new Among("li", -1, -1),
			new Among("celi", 20, -1),
			new Among("glieli", 20, -1),
			new Among("meli", 20, -1),
			new Among("teli", 20, -1),
			new Among("veli", 20, -1),
			new Among("gli", 20, -1),
			new Among("mi", -1, -1),
			new Among("si", -1, -1),
			new Among("ti", -1, -1),
			new Among("vi", -1, -1),
			new Among("lo", -1, -1),
			new Among("celo", 31, -1),
			new Among("glielo", 31, -1),
			new Among("melo", 31, -1),
			new Among("telo", 31, -1),
			new Among("velo", 31, -1)
		};

		private static readonly Among[] a_3 =
		{
			new Among("ando", -1, 1),
			new Among("endo", -1, 1),
			new Among("ar", -1, 2),
			new Among("er", -1, 2),
			new Among("ir", -1, 2)
		};

		private static readonly Among[] a_4 = 
		{
			new Among("ic", -1, -1),
			new Among("abil", -1, -1),
			new Among("os", -1, -1),
			new Among("iv", -1, 1)
		};

		private static readonly Among[] a_5 = 
		{
			new Among("ic", -1, 1),
			new Among("abil", -1, 1),
			new Among("iv", -1, 1)
		};

		private static readonly Among[] a_6 = 
		{
			new Among("ica", -1, 1),
			new Among("logia", -1, 3),
			new Among("osa", -1, 1),
			new Among("ista", -1, 1),
			new Among("iva", -1, 9),
			new Among("anza", -1, 1),
			new Among("enza", -1, 5),
			new Among("ice", -1, 1),
			new Among("atrice", 7, 1),
			new Among("iche", -1, 1),
			new Among("logie", -1, 3),
			new Among("abile", -1, 1),
			new Among("ibile", -1, 1),
			new Among("usione", -1, 4),
			new Among("azione", -1, 2),
			new Among("uzione", -1, 4),
			new Among("atore", -1, 2),
			new Among("ose", -1, 1),
			new Among("ante", -1, 1),
			new Among("mente", -1, 1),
			new Among("amente", 19, 7),
			new Among("iste", -1, 1),
			new Among("ive", -1, 9),
			new Among("anze", -1, 1),
			new Among("enze", -1, 5),
			new Among("ici", -1, 1),
			new Among("atrici", 25, 1),
			new Among("ichi", -1, 1),
			new Among("abili", -1, 1),
			new Among("ibili", -1, 1),
			new Among("ismi", -1, 1),
			new Among("usioni", -1, 4),
			new Among("azioni", -1, 2),
			new Among("uzioni", -1, 4),
			new Among("atori", -1, 2),
			new Among("osi", -1, 1),
			new Among("anti", -1, 1),
			new Among("amenti", -1, 6),
			new Among("imenti", -1, 6),
			new Among("isti", -1, 1),
			new Among("ivi", -1, 9),
			new Among("ico", -1, 1),
			new Among("ismo", -1, 1),
			new Among("oso", -1, 1),
			new Among("amento", -1, 6),
			new Among("imento", -1, 6),
			new Among("ivo", -1, 9),
			new Among(@"ità", -1, 8),
			new Among(@"istà", -1, 1),
			new Among(@"istè", -1, 1),
			new Among(@"istì", -1, 1)
		};

		private static readonly Among[] a_7 =
		{
			new Among("isca", -1, 1),
			new Among("enda", -1, 1),
			new Among("ata", -1, 1),
			new Among("ita", -1, 1),
			new Among("uta", -1, 1),
			new Among("ava", -1, 1),
			new Among("eva", -1, 1),
			new Among("iva", -1, 1),
			new Among("erebbe", -1, 1),
			new Among("irebbe", -1, 1),
			new Among("isce", -1, 1),
			new Among("ende", -1, 1),
			new Among("are", -1, 1),
			new Among("ere", -1, 1),
			new Among("ire", -1, 1),
			new Among("asse", -1, 1),
			new Among("ate", -1, 1),
			new Among("avate", 16, 1),
			new Among("evate", 16, 1),
			new Among("ivate", 16, 1),
			new Among("ete", -1, 1),
			new Among("erete", 20, 1),
			new Among("irete", 20, 1),
			new Among("ite", -1, 1),
			new Among("ereste", -1, 1),
			new Among("ireste", -1, 1),
			new Among("ute", -1, 1),
			new Among("erai", -1, 1),
			new Among("irai", -1, 1),
			new Among("isci", -1, 1),
			new Among("endi", -1, 1),
			new Among("erei", -1, 1),
			new Among("irei", -1, 1),
			new Among("assi", -1, 1),
			new Among("ati", -1, 1),
			new Among("iti", -1, 1),
			new Among("eresti", -1, 1),
			new Among("iresti", -1, 1),
			new Among("uti", -1, 1),
			new Among("avi", -1, 1),
			new Among("evi", -1, 1),
			new Among("ivi", -1, 1),
			new Among("isco", -1, 1),
			new Among("ando", -1, 1),
			new Among("endo", -1, 1),
			new Among("Yamo", -1, 1),
			new Among("iamo", -1, 1),
			new Among("avamo", -1, 1),
			new Among("evamo", -1, 1),
			new Among("ivamo", -1, 1),
			new Among("eremo", -1, 1),
			new Among("iremo", -1, 1),
			new Among("assimo", -1, 1),
			new Among("ammo", -1, 1),
			new Among("emmo", -1, 1),
			new Among("eremmo", 54, 1),
			new Among("iremmo", 54, 1),
			new Among("immo", -1, 1),
			new Among("ano", -1, 1),
			new Among("iscano", 58, 1),
			new Among("avano", 58, 1),
			new Among("evano", 58, 1),
			new Among("ivano", 58, 1),
			new Among("eranno", -1, 1),
			new Among("iranno", -1, 1),
			new Among("ono", -1, 1),
			new Among("iscono", 65, 1),
			new Among("arono", 65, 1),
			new Among("erono", 65, 1),
			new Among("irono", 65, 1),
			new Among("erebbero", -1, 1),
			new Among("irebbero", -1, 1),
			new Among("assero", -1, 1),
			new Among("essero", -1, 1),
			new Among("issero", -1, 1),
			new Among("ato", -1, 1),
			new Among("ito", -1, 1),
			new Among("uto", -1, 1),
			new Among("avo", -1, 1),
			new Among("evo", -1, 1),
			new Among("ivo", -1, 1),
			new Among("ar", -1, 1),
			new Among("ir", -1, 1),
			new Among(@"erà", -1, 1),
			new Among(@"irà", -1, 1),
			new Among(@"erò", -1, 1),
			new Among(@"irò", -1, 1)
		};

		private static readonly int[] g_v = {17, 65, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 128, 8, 2, 1};
		private static readonly int[] g_AEIO = {17, 65, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 128, 8, 2};
		private static readonly int[] g_CG = {17};

		#endregion
	}
}