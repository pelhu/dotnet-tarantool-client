using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Tarantool.Client.Models;

namespace Tarantool.Client
{
    /// <summary>The interface to Tarantool space indexes.</summary>
    /// <typeparam name="T">The class for object mapping.</typeparam>
    /// <typeparam name="TKey">The <see cref="IndexKey" /> type.</typeparam>
    public interface ITarantoolIndex<T, TKey>
        where TKey : IndexKey
    {
        /// <summary>Gets the index id. Returns null if id not have yet (see <see cref="EnsureHaveIndexIdAsync" />).</summary>
        uint? IndexId { get; }
        string IndexName { get; }
        string BoxPath { get; }

        /// <summary>Delete from space by key.</summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Task" /> with list of deleted rows.</returns>
        Task<IList<T>> DeleteAsync(TKey key, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>Delete entities by keys.</summary>
        /// <param name="keys">The keys list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Task" /> with list of deleted rows.</returns>
        Task<List<T>> DeleteMultipleAsync(List<TKey> keys, CancellationToken cancellationToken, bool inTransaction = true);


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
            uint limit = uint.MaxValue,
            CancellationToken cancellationToken = default(CancellationToken));


        /// <summary>Select from space by sub part of key</summary>
        /// <param name="key">The key value.</param>
        /// <param name="iterator">The iterator.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="cancellationToken">The cancellation Token.</param>
        /// <returns>The <see cref="Task" />.</returns>
        Task<IList<T>> SelectBypartialKeyAsync<TPartKey>(
            TPartKey partKey,
            Iterator iterator = Iterator.Eq,
            uint offset=0,
            uint limit = uint.MaxValue,
            CancellationToken cancellationToken = default(CancellationToken)) where TPartKey : IndexKey;

        /// <summary>Performs an updates in space.</summary>
        /// <param name="key">The key.</param>
        /// <param name="updateDefinition">The update operations list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Task" /> with replaced data as result.</returns>
        Task<IList<T>> UpdateAsync(
            TKey key,
            UpdateDefinition<T> updateDefinition,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}