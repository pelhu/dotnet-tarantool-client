using System.Collections.Generic;

namespace Tarantool.Client.Models
{
    /// <summary>The index key.</summary>
    public abstract class IndexKey
    {
        /// <summary>Gets or sets the key as IEnumerable.</summary>
        public IEnumerable<object> Key { get; protected set; }

        public static IndexKey<T> Create<T>(T keyValue)
        {
            return new IndexKey<T>(keyValue);
        }

        public static IndexKey<T1, T2> Create<T1, T2>(T1 keyValue1, T2 keyValue2)
        {
            return new IndexKey<T1, T2>(keyValue1, keyValue2);
        }

        public static IndexKey<T1, T2, T3> Create<T1, T2, T3>(T1 keyValue1, T2 keyValue2, T3 keyValue3)
        {
            return new IndexKey<T1, T2, T3>(keyValue1, keyValue2, keyValue3);
        }

        public static IndexKey<T1, T2, T3, T4> Create<T1, T2, T3, T4>(T1 keyValue1, T2 keyValue2, T3 keyValue3, T4 keyValue4)
        {
            return new IndexKey<T1, T2, T3, T4>(keyValue1, keyValue2, keyValue3, keyValue4);
        }
    }
}