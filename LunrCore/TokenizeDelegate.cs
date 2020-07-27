using System.Collections.Generic;
using System.Globalization;

namespace Lunr
{
    public delegate IEnumerable<Token> TokenizeDelegate(object obj, TokenMetadata metadata, CultureInfo culture);
}
