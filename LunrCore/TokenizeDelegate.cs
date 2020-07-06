using System.Collections.Generic;
using System.Globalization;

namespace Lunr
{
    public delegate IEnumerable<Token> TokenizeDelegate(
        object obj,
        IDictionary<string, object> metadata,
        CultureInfo culture);
}
