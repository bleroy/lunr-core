using System.Threading;

namespace Lunr
{
    public class TokenSetIdProvider
    {
        private int _counter = 0;

        public static readonly TokenSetIdProvider Instance = new TokenSetIdProvider();

        public TokenSetIdProvider() { }

        public int Next()
        {
            Interlocked.Increment(ref _counter);
            return _counter;
        }
    }
}
