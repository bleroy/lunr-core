#if NETSTANDARD2_0
namespace Lunr
{
    internal static class StringExtensions
    {
        public static bool Contains(this string text, char value)
        {
            return text.IndexOf(value) > -1;
        }
    }
}
#endif
