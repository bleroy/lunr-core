#if NETSTANDARD2_0
using System;
using System.Text;

namespace Lunr
{
    internal static class StringBuilderExtensions
    {
        public unsafe static void Append(this StringBuilder sb, ReadOnlySpan<char> value)
        {
            fixed (char* ptr = value)
            {
                sb.Append(ptr, value.Length);
            }
        }
    }
}
#endif
