﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Tarantool.Client.Models;

namespace Tarantool.Client
{
    /// <summary>The interface to Tarantool space indexes.</summary>
    /// <typeparam name="T">The class for object mapping.</typeparam>
    /// <typeparam name="TKey">The <see cref="IndexKey"/> type.</typeparam>
    public interface ITarantoolIndex<T, in TKey> where TKey : IndexKey
    {
        /// <summary>Gets the index id. Return null if id not have yet (see <see cref="EnsureHaveIndexIdAsync" />).</summary>
        uint? IndexId { get; }

        /// <summary>Ensures have index id. If not then retrieves it by name. </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Task" />.</returns>
        Task EnsureHaveIndexIdAsync(CancellationToken cancellationToken);

        /// <summary>Select all records from space</summary>
        /// <param name="iterator">The iterator.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="cancellationToken">The cancellation Token.</param>
        /// <returns>The <see cref="Task" />.</returns>
        Task<IList<T>> SelectAsync(
            Iterator iterator = Iterator.All,
            uint offset = 0,
            uint limit = int.MaxValue,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Select from space by key</summary>
        /// <param name="key">The key value.</param>
        /// <param name="iterator">The iterator.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="cancellationToken">The cancellation Token.</param>
        /// <returns>The <see cref="Task" />.</returns>
        Task<IList<T>> SelectAsync(
            TKey key,
            Iterator iterator = Iterator.Eq,
            uint offset = 0,
            uint limit = int.MaxValue,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}