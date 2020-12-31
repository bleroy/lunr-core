using System;

namespace Lunr.Multi
{
    public sealed class FrenchStemmer : StemmerBase
    {
        private SnowballProgram sbp;
        private int I_p2;
        private int I_p1;
        private int I_pV;

        public FrenchStemmer()
        {
            sbp = new SnowballProgram();
            I_p2 = 0;
            I_p1 = 0;
            I_pV = 0;
        }

        public override string Stem(string w)
        {
            sbp.SetCurrent(w);
            Stem();
            return sbp.GetCurrent();
        }

        #region Data

        private static readonly Among[] a_0;
        private static readonly Among[] a_1;
        private static readonly Among[] a_2;
        private static readonly Among[] a_3;
        private static readonly Among[] a_4;
        private static readonly Among[] a_5;
        private static readonly Among[] a_6;
        private static readonly Among[] a_7;
        private static readonly Among[] a_8;

        private static readonly int[] g_v;
        private static readonly int[] g_keep_with_s;
        
        static FrenchStemmer()
        {
            a_0 = new[]
            {
                new Among("col", -1, -1), 
                new Among("par", -1, -1), 
                new Among("tap", -1, -1)
            };
            a_1 = new[]
            {
                new Among("", -1, 4), 
                new Among("I", 0, 1), 
                new Among("U", 0, 2), 
                new Among("Y", 0, 3)
            };
            a_2 = new[]
            {
                new Among("iqU", -1, 3), 
                new Among("abl", -1, 3), 
                new Among("I\u00E8r", -1, 4),
                new Among("i\u00E8r", -1, 4), 
                new Among("eus", -1, 2), 
                new Among("iv", -1, 1)
            };
            a_3 = new[]
            {
                new Among("ic", -1, 2), 
                new Among("abil", -1, 1), 
                new Among("iv", -1, 3)
            };
            a_4 = new[]
            {
                new Among("iqUe", -1, 1), 
                new Among("atrice", -1, 2), 
                new Among("ance", -1, 1),
                new Among("ence", -1, 5), 
                new Among("logie", -1, 3),
                new Among("able", -1, 1),
                new Among("isme", -1, 1), 
                new Among("euse", -1, 11), 
                new Among("iste", -1, 1),
                new Among("ive", -1, 8),
                new Among("if", -1, 8), 
                new Among("usion", -1, 4), 
                new Among("ation", -1, 2), 
                new Among("ution", -1, 4),
                new Among("ateur", -1, 2),
                new Among("iqUes", -1, 1), 
                new Among("atrices", -1, 2), 
                new Among("ances", -1, 1),
                new Among("ences", -1, 5), 
                new Among("logies", -1, 3),
                new Among("ables", -1, 1), 
                new Among("ismes", -1, 1), 
                new Among("euses", -1, 11),
                new Among("istes", -1, 1), 
                new Among("ives", -1, 8),
                new Among("ifs", -1, 8), 
                new Among("usions", -1, 4), 
                new Among("ations", -1, 2),
                new Among("utions", -1, 4), 
                new Among("ateurs", -1, 2),
                new Among("ments", -1, 15), 
                new Among("ements", 30, 6), 
                new Among("issements", 31, 12),
                new Among("it\u00E9s", -1, 7), 
                new Among("ment", -1, 15),
                new Among("ement", 34, 6), 
                new Among("issement", 35, 12), 
                new Among("amment", 34, 13),
                new Among("emment", 34, 14), 
                new Among("aux", -1, 10),
                new Among("eaux", 39, 9), 
                new Among("eux", -1, 1), 
                new Among("it\u00E9", -1, 7)
            };
            a_5 = new[]
            {
                new Among("ira", -1, 1), 
                new Among("ie", -1, 1),
                new Among("isse", -1, 1), 
                new Among("issante", -1, 1),
                new Among("i", -1, 1), 
                new Among("irai", 4, 1), 
                new Among("ir", -1, 1), 
                new Among("iras", -1, 1),
                new Among("ies", -1, 1),
                new Among("\u00EEmes", -1, 1), 
                new Among("isses", -1, 1), 
                new Among("issantes", -1, 1),
                new Among("\u00EEtes", -1, 1), 
                new Among("is", -1, 1),
                new Among("irais", 13, 1), 
                new Among("issais", 13, 1), 
                new Among("irions", -1, 1),
                new Among("issions", -1, 1), 
                new Among("irons", -1, 1),
                new Among("issons", -1, 1), 
                new Among("issants", -1, 1), 
                new Among("it", -1, 1),
                new Among("irait", 21, 1), 
                new Among("issait", 21, 1),
                new Among("issant", -1, 1), 
                new Among("iraIent", -1, 1), 
                new Among("issaIent", -1, 1),
                new Among("irent", -1, 1), 
                new Among("issent", -1, 1),
                new Among("iront", -1, 1), 
                new Among("\u00EEt", -1, 1), 
                new Among("iriez", -1, 1),
                new Among("issiez", -1, 1), 
                new Among("irez", -1, 1), 
                new Among("issez", -1, 1)
            };
            a_6 = new[]
            {
                new Among("a", -1, 3),
                new Among("era", 0, 2),
                new Among("asse", -1, 3), 
                new Among("ante", -1, 3),
                new Among("\u00E9e", -1, 2),
                new Among("ai", -1, 3), 
                new Among("erai", 5, 2), 
                new Among("er", -1, 2), 
                new Among("as", -1, 3),
                new Among("eras", 8, 2), 
                new Among("\u00E2mes", -1, 3),
                new Among("asses", -1, 3), 
                new Among("antes", -1, 3), 
                new Among("\u00E2tes", -1, 3),
                new Among("\u00E9es", -1, 2), 
                new Among("ais", -1, 3),
                new Among("erais", 15, 2), 
                new Among("ions", -1, 1), 
                new Among("erions", 17, 2),
                new Among("assions", 17, 3), 
                new Among("erons", -1, 2),
                new Among("ants", -1, 3), 
                new Among("\u00E9s", -1, 2), 
                new Among("ait", -1, 3),
                new Among("erait", 23, 2), 
                new Among("ant", -1, 3),
                new Among("aIent", -1, 3), 
                new Among("eraIent", 26, 2), 
                new Among("\u00E8rent", -1, 2),
                new Among("assent", -1, 3), 
                new Among("eront", -1, 2),
                new Among("\u00E2t", -1, 3), 
                new Among("ez", -1, 2), 
                new Among("iez", 32, 2), 
                new Among("eriez", 33, 2),
                new Among("assiez", 33, 3),
                new Among("erez", 32, 2), 
                new Among("\u00E9", -1, 2)
            };
            a_7 = new[]
            {
                new Among("e", -1, 3), 
                new Among("I\u00E8re", 0, 2), 
                new Among("i\u00E8re", 0, 2),
                new Among("ion", -1, 1), 
                new Among("Ier", -1, 2), 
                new Among("ier", -1, 2), 
                new Among("\u00EB", -1, 4)
            };
            a_8 = new[]
            {
                new Among("ell", -1, -1), 
                new Among("eill", -1, -1),
                new Among("enn", -1, -1), 
                new Among("onn", -1, -1),
                new Among("ett", -1, -1)
            };
            g_v = new[] {17, 65, 16, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128, 130, 103, 8, 5};
            g_keep_with_s = new[] {1, 65, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 128};
        }

        #endregion
        
        private bool habr1(string c1, string c2, int v_1)
        {
            if (sbp.eq_s(1, c1))
            {
                sbp.ket = sbp.cursor;
                if (sbp.in_grouping(g_v, 97, 251))
                {
                    sbp.slice_from(c2);
                    sbp.cursor = v_1;
                    return true;
                }
            }
            return false;
        }

        private bool habr2(string c1, string c2, int v_1)
        {
            if (sbp.eq_s(1, c1))
            {
                sbp.ket = sbp.cursor;
                sbp.slice_from(c2);
                sbp.cursor = v_1;
                return true;
            }
            return false;
        }

        private void r_prelude()
        {
            while (true)
            {
                var v_1 = sbp.cursor;
                if (sbp.in_grouping(g_v, 97, 251))
                {
                    sbp.bra = sbp.cursor;
                    var v_2 = sbp.cursor;
                    if (habr1("u", "U", v_1))
                        continue;
                    sbp.cursor = v_2;
                    if (habr1("i", "I", v_1))
                        continue;
                    sbp.cursor = v_2;
                    if (habr2("y", "Y", v_1))
                        continue;
                }
                sbp.cursor = v_1;
                sbp.bra = v_1;

                if (!habr1("y", "Y", v_1))
                {
                    sbp.cursor = v_1;
                    if (sbp.eq_s(1, "q"))
                    {
                        sbp.bra = sbp.cursor;
                        if (habr2("u", "U", v_1))
                            continue;
                    }
                    sbp.cursor = v_1;
                    if (v_1 >= sbp.limit)
                        return;
                    sbp.cursor++;
                }
            }
        }

        private bool habr3()
        {
            while (!sbp.in_grouping(g_v, 97, 251))
            {
                if (sbp.cursor >= sbp.limit)
                    return true;
                sbp.cursor++;
            }
            while (!sbp.out_grouping(g_v, 97, 251))
            {
                if (sbp.cursor >= sbp.limit)
                    return true;
                sbp.cursor++;
            }
            return false;
        }

        private void r_mark_regions()
        {
            var v_1 = sbp.cursor;
            var I_pV = sbp.limit;
            var I_p1 = I_pV;
            var I_p2 = I_pV;
            if (sbp.in_grouping(g_v, 97, 251) && sbp.in_grouping(g_v, 97, 251) &&
              sbp.cursor < sbp.limit)
                sbp.cursor++;
            else
            {
                sbp.cursor = v_1;
                if (sbp.find_among(a_0, 3) == 0) /* !! */
                {
                    sbp.cursor = v_1;
                    do
                    {
                        if (sbp.cursor >= sbp.limit)
                        {
                            sbp.cursor = I_pV;
                            break;
                        }
                        sbp.cursor++;
                    } while (!sbp.in_grouping(g_v, 97, 251));
                }
            }
            I_pV = sbp.cursor;
            sbp.cursor = v_1;
            if (!habr3())
            {
                I_p1 = sbp.cursor;
                if (!habr3())
                    I_p2 = sbp.cursor;
            }
        }

        private void r_postlude()
        {
            while (true)
            {
                var v_1 = sbp.cursor;
                sbp.bra = v_1;
                var among_var = sbp.find_among(a_1, 4);
                if (among_var == 0) /* !! */
                    break;
                sbp.ket = sbp.cursor;
                switch (among_var)
                {
                    case 1:
                        sbp.slice_from("i");
                        break;
                    case 2:
                        sbp.slice_from("u");
                        break;
                    case 3:
                        sbp.slice_from("y");
                        break;
                    case 4:
                        if (sbp.cursor >= sbp.limit)
                            return;
                        sbp.cursor++;
                        break;
                }
            }
        }

        private bool r_RV() => I_pV <= sbp.cursor;
        private bool r_R1() => I_p1 <= sbp.cursor;
        private bool r_R2() => I_p2 <= sbp.cursor;

        private bool r_standard_suffix()
        {
            sbp.ket = sbp.cursor;
            var among_var = sbp.find_among_b(a_4, 43);
            if (among_var != 0) /* !! */
            {
                sbp.bra = sbp.cursor;
                switch (among_var)
                {
                    case 1:
                        if (!r_R2())
                            return false;
                        sbp.slice_del();
                        break;
                    case 2:
                        if (!r_R2())
                            return false;
                        sbp.slice_del();
                        sbp.ket = sbp.cursor;
                        if (sbp.eq_s_b(2, "ic"))
                        {
                            sbp.bra = sbp.cursor;
                            if (!r_R2())
                                sbp.slice_from("iqU");
                            else
                                sbp.slice_del();
                        }
                        break;
                    case 3:
                        if (!r_R2())
                            return false;
                        sbp.slice_from("log");
                        break;
                    case 4:
                        if (!r_R2())
                            return false;
                        sbp.slice_from("u");
                        break;
                    case 5:
                        if (!r_R2())
                            return false;
                        sbp.slice_from("ent");
                        break;
                    case 6:
                        if (!r_RV())
                            return false;
                        sbp.slice_del();
                        sbp.ket = sbp.cursor;
                        among_var = sbp.find_among_b(a_2, 6);
                        if (among_var != 0) /* !! */
                        {
                            sbp.bra = sbp.cursor;
                            switch (among_var)
                            {
                                case 1:
                                    if (r_R2())
                                    {
                                        sbp.slice_del();
                                        sbp.ket = sbp.cursor;
                                        if (sbp.eq_s_b(2, "at"))
                                        {
                                            sbp.bra = sbp.cursor;
                                            if (r_R2())
                                                sbp.slice_del();
                                        }
                                    }
                                    break;
                                case 2:
                                    if (r_R2())
                                        sbp.slice_del();
                                    else if (r_R1())
                                        sbp.slice_from("eux");
                                    break;
                                case 3:
                                    if (r_R2())
                                        sbp.slice_del();
                                    break;
                                case 4:
                                    if (r_RV())
                                        sbp.slice_from("i");
                                    break;
                            }
                        }
                        break;
                    case 7:
                        if (!r_R2())
                            return false;
                        sbp.slice_del();
                        sbp.ket = sbp.cursor;
                        among_var = sbp.find_among_b(a_3, 3);
                        if (among_var != 0) /* !! */
                        {
                            sbp.bra = sbp.cursor;
                            switch (among_var)
                            {
                                case 1:
                                    if (r_R2())
                                        sbp.slice_del();
                                    else
                                        sbp.slice_from("abl");
                                    break;
                                case 2:
                                    if (r_R2())
                                        sbp.slice_del();
                                    else
                                        sbp.slice_from("iqU");
                                    break;
                                case 3:
                                    if (r_R2())
                                        sbp.slice_del();
                                    break;
                            }
                        }
                        break;
                    case 8:
                        if (!r_R2())
                            return false;
                        sbp.slice_del();
                        sbp.ket = sbp.cursor;
                        if (sbp.eq_s_b(2, "at"))
                        {
                            sbp.bra = sbp.cursor;
                            if (r_R2())
                            {
                                sbp.slice_del();
                                sbp.ket = sbp.cursor;
                                if (sbp.eq_s_b(2, "ic"))
                                {
                                    sbp.bra = sbp.cursor;
                                    if (r_R2())
                                        sbp.slice_del();
                                    else
                                        sbp.slice_from("iqU");
                                    break;
                                }
                            }
                        }
                        break;
                    case 9:
                        sbp.slice_from("eau");
                        break;
                    case 10:
                        if (!r_R1())
                            return false;
                        sbp.slice_from("al");
                        break;
                    case 11:
                        if (r_R2())
                            sbp.slice_del();
                        else if (!r_R1())
                            return false;
                        else
                            sbp.slice_from("eux");
                        break;
                    case 12:
                        if (!r_R1() || !sbp.out_grouping_b(g_v, 97, 251))
                            return false;
                        sbp.slice_del();
                        break;
                    case 13:
                        if (r_RV())
                            sbp.slice_from("ant");
                        return false;
                    case 14:
                        if (r_RV())
                            sbp.slice_from("ent");
                        return false;
                    case 15:
                        var v_1 = sbp.limit - sbp.cursor;
                        if (sbp.in_grouping_b(g_v, 97, 251) && r_RV())
                        {
                            sbp.cursor = sbp.limit - v_1;
                            sbp.slice_del();
                        }
                        return false;
                }
                return true;
            }
            return false;
        }

        private bool r_i_verb_suffix()
        {
            if (sbp.cursor < I_pV)
                return false;
            var v_1 = sbp.limit_backward;
            sbp.limit_backward = I_pV;
            sbp.ket = sbp.cursor;
            var among_var = sbp.find_among_b(a_5, 35);
            if (among_var == 0) /* !! */
            {
                sbp.limit_backward = v_1;
                return false;
            }
            sbp.bra = sbp.cursor;
            if (among_var == 1)
            {
                if (!sbp.out_grouping_b(g_v, 97, 251))
                {
                    sbp.limit_backward = v_1;
                    return false;
                }
                sbp.slice_del();
            }
            sbp.limit_backward = v_1;
            return true;
        }

        private bool r_verb_suffix()
        {
            if (sbp.cursor < I_pV)
                return false;
            var v_2 = sbp.limit_backward;
            sbp.limit_backward = I_pV;
            sbp.ket = sbp.cursor;
            var among_var = sbp.find_among_b(a_6, 38);
            if (among_var == 0) /* !! */
            {
                sbp.limit_backward = v_2;
                return false;
            }
            sbp.bra = sbp.cursor;
            switch (among_var)
            {
                case 1:
                    if (!r_R2())
                    {
                        sbp.limit_backward = v_2;
                        return false;
                    }
                    sbp.slice_del();
                    break;
                case 2:
                    sbp.slice_del();
                    break;
                case 3:
                    sbp.slice_del();
                    var v_3 = sbp.limit - sbp.cursor;
                    sbp.ket = sbp.cursor;
                    if (sbp.eq_s_b(1, "e"))
                    {
                        sbp.bra = sbp.cursor;
                        sbp.slice_del();
                    }
                    else
                        sbp.cursor = sbp.limit - v_3;
                    break;
            }
            sbp.limit_backward = v_2;
            return true;
        }

        private void r_residual_suffix()
        {
            var v_1 = sbp.limit - sbp.cursor;
            sbp.ket = sbp.cursor;
            if (sbp.eq_s_b(1, "s"))
            {
                sbp.bra = sbp.cursor;
                var v_2 = sbp.limit - sbp.cursor;
                if (sbp.out_grouping_b(g_keep_with_s, 97, 232))
                {
                    sbp.cursor = sbp.limit - v_2;
                    sbp.slice_del();
                }
                else
                    sbp.cursor = sbp.limit - v_1;
            }
            else
                sbp.cursor = sbp.limit - v_1;
            if (sbp.cursor >= I_pV)
            {
                var v_4 = sbp.limit_backward;
                sbp.limit_backward = I_pV;
                sbp.ket = sbp.cursor;
                var among_var = sbp.find_among_b(a_7, 7);
                if (among_var != 0) /* !! */
                {
                    sbp.bra = sbp.cursor;
                    switch (among_var)
                    {
                        case 1:
                            if (r_R2())
                            {
                                var v_5 = sbp.limit - sbp.cursor;
                                if (!sbp.eq_s_b(1, "s"))
                                {
                                    sbp.cursor = sbp.limit - v_5;
                                    if (!sbp.eq_s_b(1, "t"))
                                        break;
                                }
                                sbp.slice_del();
                            }
                            break;
                        case 2:
                            sbp.slice_from("i");
                            break;
                        case 3:
                            sbp.slice_del();
                            break;
                        case 4:
                            if (sbp.eq_s_b(2, "gu"))
                                sbp.slice_del();
                            break;
                    }
                }
                sbp.limit_backward = v_4;
            }
        }

        private void r_un_double()
        {
            var v_1 = sbp.limit - sbp.cursor;
            if (sbp.find_among_b(a_8, 5) != 0) /* !! */
            {
                sbp.cursor = sbp.limit - v_1;
                sbp.ket = sbp.cursor;
                if (sbp.cursor > sbp.limit_backward)
                {
                    sbp.cursor--;
                    sbp.bra = sbp.cursor;
                    sbp.slice_del();
                }
            }
        }

        private void r_un_accent()
        {
            var v_2 = 1;
            while (sbp.out_grouping_b(g_v, 97, 251))
                v_2--;
            if (v_2 <= 0)
            {
                sbp.ket = sbp.cursor;
                var v_1 = sbp.limit - sbp.cursor;
                if (!sbp.eq_s_b(1, "\u00E9"))
                {
                    sbp.cursor = sbp.limit - v_1;
                    if (!sbp.eq_s_b(1, "\u00E8"))
                        return;
                }
                sbp.bra = sbp.cursor;
                sbp.slice_from("e");
            }
        }

        private void habr5()
        {
            if (!r_standard_suffix())
            {
                sbp.cursor = sbp.limit;
                if (!r_i_verb_suffix())
                {
                    sbp.cursor = sbp.limit;
                    if (!r_verb_suffix())
                    {
                        sbp.cursor = sbp.limit;
                        r_residual_suffix();
                        return;
                    }
                }
            }
            sbp.cursor = sbp.limit;
            sbp.ket = sbp.cursor;
            if (sbp.eq_s_b(1, "Y"))
            {
                sbp.bra = sbp.cursor;
                sbp.slice_from("i");
            }
            else
            {
                sbp.cursor = sbp.limit;
                if (sbp.eq_s_b(1, "\u00E7"))
                {
                    sbp.bra = sbp.cursor;
                    sbp.slice_from("c");
                }
            }
        }

        private bool Stem()
        {
            var v_1 = sbp.cursor;
            r_prelude();
            sbp.cursor = v_1;
            r_mark_regions();
            sbp.limit_backward = v_1;
            sbp.cursor = sbp.limit;
            habr5();
            sbp.cursor = sbp.limit;
            r_un_double();
            sbp.cursor = sbp.limit;
            r_un_accent();
            sbp.cursor = sbp.limit_backward;
            r_postlude();
            return true;
        }
    }
}
