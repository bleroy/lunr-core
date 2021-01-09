namespace Lunr.Globalization.de
{
    public sealed class GermanStemmer : StemmerBase
    {
        private SnowballProgram sbp;
        private int I_x;
        private int I_p2;
        private int I_p1;
        
        public GermanStemmer()
        {
            sbp = new SnowballProgram();
            I_x = 0;
            I_p2 = 0;
            I_p1 = 0;
        }

        public override string Stem(string w)
        {
            sbp.SetCurrent(w);
            StemImpl();
            return sbp.GetCurrent();
        }

        #region Data

        private static readonly Among[] a_0;
        private static readonly Among[] a_1;
        private static readonly Among[] a_2;
        private static readonly Among[] a_3;
        private static readonly Among[] a_4;

        private static readonly int[] g_v;
        private static readonly int[] g_s_ending;
        private static readonly int[] g_st_ending;

        static GermanStemmer()
        {
            a_0 = new[]
            { 
                new Among("", -1, 6), 
                new Among("U", 0, 2), 
                new Among("Y", 0, 1), 
                new Among("\u00E4", 0, 3),
                new Among("\u00F6", 0, 4), 
                new Among("\u00FC", 0, 5)
            };
            a_1 = new[]
            { 
                new Among("e", -1, 2), 
                new Among("em", -1, 1),
                new Among("en", -1, 2), 
                new Among("ern", -1, 1),
                new Among("er", -1, 1), 
                new Among("s", -1, 3),
                new Among("es", 5, 2)
            };
            a_2 = new[]
            {
                new Among("en", -1, 1),
                new Among("er", -1, 1), 
                new Among("st", -1, 2),
                new Among("est", 2, 1)
            };
            a_3 = new[]
            {
                new Among("ig", -1, 1), 
                new Among("lich", -1, 1)
            };
            a_4 = new[]
            {
                new Among("end", -1, 1),
                new Among("ig", -1, 2), 
                new Among("ung", -1, 1),
                new Among("lich", -1, 3), 
                new Among("isch", -1, 2),
                new Among("ik", -1, 2), 
                new Among("heit", -1, 3),
                new Among("keit", -1, 4)
            };
            g_v = new[] { 17, 65, 16, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 0, 32, 8};
            g_s_ending = new [] { 117, 30, 5 };
            g_st_ending = new [] { 117, 30, 4 };
        }

        #endregion

        private bool habr1(string c1, string c2, int v_1)
        {
            if (sbp.EqualsSegment(1, c1)) {
                sbp.ket = sbp.cursor;
                if (sbp.InGrouping(g_v, 97, 252)) {
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
            while (true) {
                var v_2 = sbp.cursor;
                sbp.bra = v_2;
                if (sbp.EqualsSegment(1, "\u00DF")) {
                    sbp.ket = sbp.cursor;
                    sbp.SliceFrom("ss");
                } else {
                    if (v_2 >= sbp.limit)
                        break;
                    sbp.cursor = v_2 + 1;
                }
            }
            sbp.cursor = v_1;
            while (true) {
               var v_3 = sbp.cursor;
                while (true) {
                    var v_4 = sbp.cursor;
                    if (sbp.InGrouping(g_v, 97, 252)) {
                        var v_5 = sbp.cursor;
                        sbp.bra = v_5;
                        if (habr1("u", "U", v_4))
                            break;
                        sbp.cursor = v_5;
                        if (habr1("y", "Y", v_4))
                            break;
                    }
                    if (v_4 >= sbp.limit) {
                        sbp.cursor = v_3;
                        return;
                    }
                    sbp.cursor = v_4 + 1;
                }
            }
        }

        private bool habr2()
        {
            while (!sbp.InGrouping(g_v, 97, 252)) {
                if (sbp.cursor >= sbp.limit)
                    return true;
                sbp.cursor++;
            }
            while (!sbp.OutGrouping(g_v, 97, 252)) {
                if (sbp.cursor >= sbp.limit)
                    return true;
                sbp.cursor++;
            }
            return false;
        }

        private void r_mark_regions()
        {
            I_p1 = sbp.limit;
            I_p2 = I_p1;
            var c = sbp.cursor + 3;
            if (0 <= c && c <= sbp.limit) {
                I_x = c;
                if (!habr2()) {
                    I_p1 = sbp.cursor;
                    if (I_p1 < I_x)
                        I_p1 = I_x;
                    if (!habr2())
                        I_p2 = sbp.cursor;
                }
            }
        }

        private void r_postlude()
        {
            while (true) {
                var v_1 = sbp.cursor;
                sbp.bra = v_1;
                var among_var = sbp.FindAmong(a_0, 6);
                if (among_var == 0) /* !! */
                    return;
                sbp.ket = sbp.cursor;
                switch (among_var) {
                    case 1:
                        sbp.SliceFrom("y");
                        break;
                    case 2:
                    case 5:
                        sbp.SliceFrom("u");
                        break;
                    case 3:
                        sbp.SliceFrom("a");
                        break;
                    case 4:
                        sbp.SliceFrom("o");
                        break;
                    case 6:
                        if (sbp.cursor >= sbp.limit)
                            return;
                        sbp.cursor++;
                        break;
                }
            }
        }

        private bool r_R1() => I_p1 <= sbp.cursor;
        private bool r_R2() => I_p2 <= sbp.cursor;

        private void r_standard_suffix()
        {
            var v_1 = sbp.limit - sbp.cursor;
            sbp.ket = sbp.cursor;
            var among_var = sbp.FindAmongBackwards(a_1, 7);
            if (among_var != 0) /* !! */
            {
                sbp.bra = sbp.cursor;
                if (r_R1())
                {
                    switch (among_var)
                    {
                        case 1:
                            sbp.SliceDelete();
                            break;
                        case 2:
                            sbp.SliceDelete();
                            sbp.ket = sbp.cursor;
                            if (sbp.EqualsSegmentBackwards(1, "s"))
                            {
                                sbp.bra = sbp.cursor;
                                if (sbp.EqualsSegmentBackwards(3, "nis"))
                                    sbp.SliceDelete();
                            }

                            break;
                        case 3:
                            if (sbp.InGroupingBackwards(g_s_ending, 98, 116))
                                sbp.SliceDelete();
                            break;
                    }
                }
            }

            sbp.cursor = sbp.limit - v_1;
            sbp.ket = sbp.cursor;
            among_var = sbp.FindAmongBackwards(a_2, 4);
            if (among_var != 0) /* !! */
            {
                sbp.bra = sbp.cursor;
                if (r_R1())
                {
                    switch (among_var)
                    {
                        case 1:
                            sbp.SliceDelete();
                            break;
                        case 2:
                            if (sbp.InGroupingBackwards(g_st_ending, 98, 116))
                            {
                                var c = sbp.cursor - 3;
                                if (sbp.limit_backward <= c && c <= sbp.limit)
                                {
                                    sbp.cursor = c;
                                    sbp.SliceDelete();
                                }
                            }

                            break;
                    }
                }
            }

            sbp.cursor = sbp.limit - v_1;
            sbp.ket = sbp.cursor;
            among_var = sbp.FindAmongBackwards(a_4, 8);
            if (among_var != 0) /* !! */
            {
                sbp.bra = sbp.cursor;
                if (r_R2())
                {
                    switch (among_var)
                    {
                        case 1:
                            sbp.SliceDelete();
                            sbp.ket = sbp.cursor;
                            if (sbp.EqualsSegmentBackwards(2, "ig"))
                            {
                                sbp.bra = sbp.cursor;
                                var v_2 = sbp.limit - sbp.cursor;
                                if (!sbp.EqualsSegmentBackwards(1, "e"))
                                {
                                    sbp.cursor = sbp.limit - v_2;
                                    if (r_R2())
                                        sbp.SliceDelete();
                                }
                            }

                            break;
                        case 2:
                            var v_3 = sbp.limit - sbp.cursor;
                            if (!sbp.EqualsSegmentBackwards(1, "e"))
                            {
                                sbp.cursor = sbp.limit - v_3;
                                sbp.SliceDelete();
                            }

                            break;
                        case 3:
                            sbp.SliceDelete();
                            sbp.ket = sbp.cursor;
                            var v_4 = sbp.limit - sbp.cursor;
                            if (!sbp.EqualsSegmentBackwards(2, "er"))
                            {
                                sbp.cursor = sbp.limit - v_4;
                                if (!sbp.EqualsSegmentBackwards(2, "en"))
                                    break;
                            }

                            sbp.bra = sbp.cursor;
                            if (r_R1())
                                sbp.SliceDelete();
                            break;
                        case 4:
                            sbp.SliceDelete();
                            sbp.ket = sbp.cursor;
                            among_var = sbp.FindAmongBackwards(a_3, 2);
                            if (among_var != 0) /* !! */
                            {
                                sbp.bra = sbp.cursor;
                                if (r_R2() && among_var == 1)
                                    sbp.SliceDelete();
                            }

                            break;
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
            r_standard_suffix();
            sbp.cursor = sbp.limit_backward;
            r_postlude();
        }
    }
}