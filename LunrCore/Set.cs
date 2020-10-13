using System.Collections.Generic;

namespace Lunr
{
    public interface ISet<T>
    {
        bool Contains(T item);

        ISet<T> Intersect(ISet<T> other);

        ISet<T> Union(ISet<T> other);
    }

    public sealed class Set<T> : ISet<T>
    {
        public static ISet<T> Empty = new EmptySety<T>();
        public static ISet<T> Complete = new CompleteSet<T>();

        private readonly HashSet<T> _innerSet;

        public Set(IEnumerable<T> elements)
            => _innerSet = new HashSet<T>(elements);

        public Set(params T[] elements) : this((IEnumerable<T>)elements) { }

        public bool Contains(T item) => _innerSet.Contains(item);

        public ISet<T> Intersect(ISet<T> other)
        {
            if (other is EmptySety<T>) return Empty;
            if (other is CompleteSet<T>) return this;
            if (other is Set<T> otherSet)
            {
                var result = new HashSet<T>(_innerSet);
                result.IntersectWith(otherSet._innerSet);
                return new Set<T>(result);
            }
            return other.Intersect(this);
        }

        public ISet<T> Union(ISet<T> other)
        {
            if (other is EmptySety<T>) return this;
            if (other is CompleteSet<T>) return Complete;
            if (other is Set<T> otherSet)
            {
                var result = new HashSet<T>(_innerSet);
                result.UnionWith(otherSet._innerSet);
                return new Set<T>(result);
            }
            return other.Union(this);
        }

        private sealed class EmptySety<TEmpty> : ISet<TEmpty>
        {
            public bool Contains(TEmpty item) => false;

            public ISet<TEmpty> Intersect(ISet<TEmpty> other) => Set<TEmpty>.Empty;

            public ISet<TEmpty> Union(ISet<TEmpty> other) => other;
        }

        private sealed class CompleteSet<TComplete> : ISet<TComplete>
        {
            public bool Contains(TComplete item) => true;

            public ISet<TComplete> Intersect(ISet<TComplete> other) => other;

            public ISet<TComplete> Union(ISet<TComplete> other) => Set<TComplete>.Complete;
        }
    }
}
