#if NETSTANDARD2_0

using System;
using System.Text;

namespace Lunr;

internal static class StringBuilderExtensions
{
    public static void Append(this StringBuilder sb, ReadOnlySpan<char> text)
    {
        sb.Append(text.ToString());
    }
}

#endif