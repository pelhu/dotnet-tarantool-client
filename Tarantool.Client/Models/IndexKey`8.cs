﻿namespace Tarantool.Client.Models
{
    /// <summary>The index key of four fileds.</summary>
    /// <typeparam name="TK1">The type for key's part 1 mapping</typeparam>
    /// <typeparam name="TK2">The type for key's part 2 mapping</typeparam>
    /// <typeparam name="TK3">The type for key's part 3 mapping</typeparam>
    /// <typeparam name="TK4">The type for key's part 4 mapping</typeparam>
    /// <typeparam name="TK5">The type for key's part 5 mapping</typeparam>
    /// <typeparam name="TK6">The type for key's part 6 mapping</typeparam>
    /// <typeparam name="TK7">The type for key's part 7 mapping</typeparam>
    /// <typeparam name="TK8">The type for key's part 8 mapping</typeparam>
    public class IndexKey<TK1, TK2, TK3, TK4, TK5, TK6, TK7, TK8> : IndexKey
    {
        /// <summary>Initializes a new instance of the <see cref="IndexKey{TK1,TK2,TK3,TK4,TK5,TK6,TK7,TK8}" /> class.</summary>
        /// <param name="keyPart1Value">The key's part 1 value.</param>
        /// <param name="keyPart2Value">The key's part 2 value.</param>
        /// <param name="keyPart3Value">The key's part 3 value.</param>
        /// <param name="keyPart4Value">The key's part 4 value.</param>
        /// <param name="keyPart5Value">The key's part 5 value.</param>
        /// <param name="keyPart6Value">The key's part 6 value.</param>
        /// <param name="keyPart7Value">The key's part 7 value.</param>
        /// <param name="keyPart8Value">The key's part 8 value.</param>
        public IndexKey(TK1 keyPart1Value, TK2 keyPart2Value, TK3 keyPart3Value, TK4 keyPart4Value, TK5 keyPart5Value, TK6 keyPart6Value, TK7 keyPart7Value, TK8 keyPart8Value)
        {
            Key = new object[] { keyPart1Value, keyPart2Value, keyPart3Value, keyPart4Value, keyPart5Value, keyPart6Value, keyPart7Value, keyPart8Value };
        }
    }
}