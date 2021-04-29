using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.PolygonModule.Models
{
    public sealed class NullLookup<TKey, TElement> : ILookup<TKey, TElement>, IEnumerator<IGrouping<TKey, TElement>>
    {
        public IEnumerable<TElement> this[TKey key] => Enumerable.Empty<TElement>();

        public int Count => 0;

        IGrouping<TKey, TElement> IEnumerator<IGrouping<TKey, TElement>>.Current => null;

        object IEnumerator.Current => throw new NotImplementedException();

        public bool Contains(TKey key) => false;

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() => this;

        public void Dispose() { }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool IEnumerator.MoveNext() => false;

        void IEnumerator.Reset() { }
    }
}
