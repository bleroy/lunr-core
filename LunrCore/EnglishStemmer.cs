using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Lunr
{
    public sealed class EnglishStemmer : StemmerBase
    {
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en");

        private static readonly Dictionary<string, string> step2list = new Dictionary<string, string>
        {
            { "ational", "ate" },
            { "tional", "tion" },
            { "enci", "ence" },
            { "anci", "ance" },
            { "izer", "ize" },
            { "bli", "ble" },
            { "alli", "al" },
            { "entli", "ent" },
            { "eli", "e" },
            { "ousli", "ous" },
            { "ization", "ize" },
            { "ation", "ate" },
            { "ator", "ate" },
            { "alism", "al" },
            { "iveness", "ive" },
            { "fulness", "ful" },
            { "ousness", "ous" },
            { "aliti", "al" },
            { "iviti", "ive" },
            { "biliti", "ble" },
            { "logi", "log" }
        };

        private static readonly Dictionary<string, string> step3list = new Dictionary<string, string>
        {
            { "icate", "ic" },
            { "ative", "" },
            { "alize", "al" },
            { "iciti", "ic" },
            { "ical", "ic" },
            { "ful", "" },
            { "ness", "" }
        };

        private const string c = "[^aeiou]"; // consonant
        private const string v = "[aeiouy]"; // vowel
        private const string C = c + "[^aeiouy]*"; // consonant sequence
        private const string V = v + "[aeiou]*"; // vowel sequence
        private const string mgr0 = "^(" + C + ")?" + V + C; // [C]VC... is m>0
        private const string meq1 = "^(" + C + ")?" + V + C + "(" + V + ")?$"; // [C]VC[V] is m=1
        private const string mgr1 = "^(" + C + ")?" + V + C + V + C; // [C]VCVC... is m>1
        private const string s_v = "^(" + C + ")?" + v; // vowel in stem

        private static readonly Regex re_mgr0 = new Regex(mgr0);
        private static readonly Regex re_mgr1 = new Regex(mgr1);
        private static readonly Regex re_meq1 = new Regex(meq1);
        private static readonly Regex re_s_v = new Regex(s_v);

        private static readonly Regex re_1a = new Regex("^(.+?)(ss|i)es$");
        private static readonly Regex re2_1a = new Regex("^(.+?)([^s])s$");
        private static readonly Regex re_1b = new Regex("^(.+?)eed$");
        private static readonly Regex re2_1b = new Regex("^(.+?)(ed|ing)$");
        private static readonly Regex re_1b_2 = new Regex(".$");
        private static readonly Regex re2_1b_2 = new Regex("(at|bl|iz)$");
        private static readonly Regex re3_1b_2 = new Regex("([^aeiouylsz])\\1$");
        private static readonly Regex re4_1b_2 = new Regex("^" + C + v + "[^aeiouwxy]$");

        private static readonly Regex re_1c = new Regex("^(.+?[^aeiou])y$");
        private static readonly Regex re_2 = new Regex("^(.+?)(ational|tional|enci|anci|izer|bli|alli|entli|eli|ousli|ization|ation|ator|alism|iveness|fulness|ousness|aliti|iviti|biliti|logi)$");

        private static readonly Regex re_3 = new Regex("^(.+?)(icate|ative|alize|iciti|ical|ful|ness)$");

        private static readonly Regex re_4 = new Regex("^(.+?)(al|ance|ence|er|ic|able|ible|ant|ement|ment|ent|ou|ism|ate|iti|ous|ive|ize)$");
        private static readonly Regex re2_4 = new Regex("^(.+?)(s|t)(ion)$");

        private static readonly Regex re_5 = new Regex("^(.+?)e$");
        private static readonly Regex re_5_1 = new Regex("ll$");
        private static readonly Regex re3_5 = new Regex("^" + C + v + "[^aeiouwxy]$");

        public override string Stem(string w)
        {
            if (w.Length < 3) return w;

            char firstch = w[0];
            if (firstch == 'y')
            {
                w = char.ToUpper(firstch, culture) + w.Substring(1);
            }

            // Step 1a
            Regex re = re_1a;
            Regex re2 = re2_1a;

            if (re.IsMatch(w)) { w = re.Replace(w, "$1$2"); }
            else if (re2.IsMatch(w)) { w = re2.Replace(w, "$1$2"); }

            // Step 1b
            re = re_1b;
            re2 = re2_1b;
            if (re.IsMatch(w))
            {
                GroupCollection fp = re.Match(w).Groups;
                re = re_mgr0;
                if (re.IsMatch(fp[1].Value))
                {
                    re = re_1b_2;
                    w = re.Replace(w, "");
                }
            }
            else if (re2.IsMatch(w))
            {
                GroupCollection fp = re2.Match(w).Groups;
                string stem = fp[1].Value;
                re2 = re_s_v;
                if (re2.IsMatch(stem))
                {
                    w = stem;
                    re2 = re2_1b_2;
                    Regex re3 = re3_1b_2;
                    Regex re4 = re4_1b_2;
                    if (re2.IsMatch(w)) { w += "e"; }
                    else if (re3.IsMatch(w)) { re = re_1b_2; w = re.Replace(w, ""); }
                    else if (re4.IsMatch(w)) { w += "e"; }
                }
            }

            // Step 1c - replace suffix y or Y by i if preceded by a non-vowel which is not the first letter of the word (so cry -> cri, by -> by, say -> say)
            re = re_1c;
            if (re.IsMatch(w))
            {
                GroupCollection fp = re.Match(w).Groups;
                string stem = fp[1].Value;
                w = stem + "i";
            }

            // Step 2
            re = re_2;
            if (re.IsMatch(w))
            {
                GroupCollection fp = re.Match(w).Groups;
                string stem = fp[1].Value;
                string suffix = fp[2].Value;
                re = re_mgr0;
                if (re.IsMatch(stem))
                {
                    w = stem + step2list[suffix];
                }
            }

            // Step 3
            re = re_3;
            if (re.IsMatch(w))
            {
                GroupCollection fp = re.Match(w).Groups;
                string stem = fp[1].Value;
                string suffix = fp[2].Value;
                re = re_mgr0;
                if (re.IsMatch(stem))
                {
                    w = stem + step3list[suffix];
                }
            }

            // Step 4
            re = re_4;
            re2 = re2_4;
            if (re.IsMatch(w))
            {
                GroupCollection fp = re.Match(w).Groups;
                string stem = fp[1].Value;
                re = re_mgr1;
                if (re.IsMatch(stem))
                {
                    w = stem;
                }
            }
            else if (re2.IsMatch(w))
            {
                GroupCollection fp = re2.Match(w).Groups;
                string stem = fp[1].Value + fp[2].Value;
                re2 = re_mgr1;
                if (re2.IsMatch(stem))
                {
                    w = stem;
                }
            }

            // Step 5
            re = re_5;
            if (re.IsMatch(w))
            {
                GroupCollection fp = re.Match(w).Groups;
                string stem = fp[1].Value;
                re = re_mgr1;
                re2 = re_meq1;
                Regex re3 = re3_5;
                if (re.IsMatch(stem) || (re2.IsMatch(stem) && !(re3.IsMatch(stem))))
                {
                    w = stem;
                }
            }

            re = re_5_1;
            re2 = re_mgr1;
            if (re.IsMatch(w) && re2.IsMatch(w))
            {
                re = re_1b_2;
                w = re.Replace(w, "");
            }

            // and turn initial Y back to y
            if (firstch == 'y')
            {
                w = char.ToLower(firstch, culture) + w.Substring(1);
            }

            return w;
        }
    }
}
