using MsgPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Tarantool.Client.Models;
using Tarantool.Client.Models.ClientMessages;
using Tarantool.Client.Serialization;

namespace Tarantool.Client
{
    /// <summary>The Tarantool space indexes accessor class.</summary>
    /// <typeparam name="T">The class for object mapping.</typeparam>
    /// <typeparam name="TKey">The <see cref="IndexKey" /> type.</typeparam>
    public class TarantoolIndex<T, TKey> : ITarantoolIndex<T, TKey>
        where TKey : IndexKey
    {
        private readonly string _indexName;

        /// <summary>Initializes a new instance of the <see cref="TarantoolIndex{T, TKey}" /> class by index id.</summary>
        /// <param name="tarantoolClient">The tarantool client.</param>
        /// <param name="space">The space.</param>
        /// <param name="indexId">The index id.</param>
        public TarantoolIndex(ITarantoolClient tarantoolClient, ITarantoolSpace<T> space, uint indexId)
        {
            TarantoolClient = tarantoolClient;
            Space = space;
            IndexId = indexId;
        }

        /// <summary>Initializes a new instance of the <see cref="TarantoolIndex{T, TKey}" /> class by index name.</summary>
        /// <param name="tarantoolClient">The tarantool client.</param>
        /// <param name="space">The space.</param>
        /// <param name="indexName">The index name.</param>
        public TarantoolIndex(ITarantoolClient tarantoolClient, ITarantoolSpace<T> space, string indexName)
        {
            TarantoolClient = tarantoolClient;
            Space = space;
            _indexName = indexName;
        }

        /// <summary>Gets the index id. Return null if id not have yet (see <see cref="EnsureHaveIndexIdAsync" />).</summary>
        public uint? IndexId { get; private set; }

        public string IndexName => _indexName;
        public string BoxPath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IndexName))
                {
                    return this.Space.BoxPath + $".index.{IndexName}";
                }
                else if (IndexId != null && IndexId != 0)
                {
                    return this.Space.BoxPath + $".index[{IndexId}]";
                }
                else
                {
                    throw new InvalidOperationException("Either name or id of this index must be present to get BoxPath");
                }
            }
        }


        private ITarantoolSpace<T> Space { get; }

        private ITarantoolClient TarantoolClient { get; }

        /// <summary>Delete from space by key.</summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Task" /> with list of deleted rows.</returns>
        public async Task<IList<T>> DeleteAsync(TKey key, CancellationToken cancellationToken)
        {
            await EnsureHaveIndexIdAsync(cancellationToken).ConfigureAwait(false);
            Debug.Assert(IndexId != null, "IndexId != null");
            var result = await TarantoolClient.DeleteAsync<T>(
                                 new DeleteRequest { SpaceId = Space.SpaceId, IndexId = IndexId.Value, Key = key.Key },
                                 cancellationToken)
                             .ConfigureAwait(false);
            return result;
        }

        /// <summary>Delete entities by keys.</summary>
        /// <param name="keys">The keys list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task DeleteMultipleAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default(CancellationToken), bool inTransaction = true)
        {
            if (keys == null || !keys.Any()) return;

            await this.Space.EnsureHaveSpaceIdAsync(cancellationToken).ConfigureAwait(false);
            await EnsureHaveIndexIdAsync(cancellationToken).ConfigureAwait(false);

            string expression = $@"
local keys=...
for i=1,{keys.Count()} do
    {this.BoxPath}:delete(keys[i])
end
";
            if (inTransaction)
            {
                expression = $@"
box.begin()
{expression}
box.commit()
";
            }

            await TarantoolClient.EvalAsync(new EvalRequest { Expression = expression, Args = new[] { keys.Select(k => k.Key) } },
                                 cancellationToken)
                             .ConfigureAwait(false);
        }

        /// <summary>Delete entities by keys.</summary>
        /// <param name="keys">The keys list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Task" /> with list of deleted rows (except for vinyl engine).</returns>
        public async Task<List<T>> DeleteMultipleAndReturnAsync(IEnumerable<TKey> keys, CancellationToken cancellationToken = default(CancellationToken), bool inTransaction = true)
        {
            if (keys == null || !keys.Any()) return new List<T>();

            await this.Space.EnsureHaveSpaceIdAsync(cancellationToken).ConfigureAwait(false);
            await EnsureHaveIndexIdAsync(cancellationToken).ConfigureAwait(false);

            string expression = $@"
local deleteds={{}}
for i=1,{keys.Count()} do
    local t = {this.BoxPath}:delete(keys[i])
    if(t~=nil)then
        table.insert(deleteds, t)
    end
end
";
            if (inTransaction)
            {
                expression = $@"
local delete = function(keys)
box.begin()
{expression}
box.commit()
return deleteds
end
local keys=...
return delete(keys)
";
            }
            else
            {
                expression = $@"
local keys=...
{expression}
return deleteds
";
            }

            var deleteds = await TarantoolClient.EvalAsync(new EvalRequest { Expression = expression, Args = new[] { keys.Select(k => k.Key) } },
                                 cancellationToken)
                             .ConfigureAwait(false);

            return deleteds.AsList().Select(i => MessagePackObjectMapper.Map<List<T>>(i)).FirstOrDefault();
        }

//        private async Task<MessagePackObject> deleteMultipleAsync_inner(IEnumerable<TKey> keys, CancellationToken cancellationToken, bool inTransaction, bool withReturn)
//        {
//            await this.Space.EnsureHaveSpaceIdAsync(cancellationToken).ConfigureAwait(false);
//            await EnsureHaveIndexIdAsync(cancellationToken).ConfigureAwait(false);

//            string expression = $@"
//local deleteds={{}}
//for i=1,{keys.Count()} do
//    local t = {this.BoxPath}:delete(keys[i])
//    if(t~=nil)then
//        table.insert(deleteds, t)
//    end
//end
//";
//            if (inTransaction)
//            {
//                expression = $@"
//local delete = function(keys)
//box.begin()
//{expression}
//box.commit()
//return deleteds
//end
//return delete({{...}})
//";
//            }
//            else
//            {
//                expression = $@"
//local keys={{...}}
//{expression}
//return deleteds
//";
//            }

//            return await TarantoolClient.EvalAsync(new EvalRequest { Expression = expression, Args = keys.Select(k => k.Key) },
//                                 cancellationToken)
//                             .ConfigureAwait(false);
//        }

        /// <summary>Ensures have index id. If not then retrieves it by name. </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Task" />.</returns>
        public async Task EnsureHaveIndexIdAsync(CancellationToken cancellationToken)
        {
            await Space.EnsureHaveSpaceIdAsync(cancellationToken).ConfigureAwait(false);
            if (IndexId != null) return;
            IndexId = (await TarantoolClient.FindIndexByNameAsync(Space.SpaceId, _indexName, cancellationToken)
                           .ConfigureAwait(false)).IndexId;
        }

        /// <summary>Select all records from space</summary>
        /// <param name="iterator">The iterator.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="cancellationToken">The cancellation Token.</param>
        /// <returns>The <see cref="Task" />.</returns>
        public async Task<IList<T>> SelectAsync(
            Iterator iterator,
            uint offset,
            uint limit,
            CancellationToken cancellationToken)
        {
            await EnsureHaveIndexIdAsync(cancellationToken).ConfigureAwait(false);
            Debug.Assert(IndexId != null, "IndexId != null");
            var result = await TarantoolClient.SelectAsync<T>(
                                 new SelectRequest
                                 {
                                     SpaceId = Space.SpaceId,
                                     IndexId = IndexId.Value,
                                     Iterator = iterator,
                                     Offset = offset,
                                     Limit = limit
                                 },
                                 cancellationToken)
                             .ConfigureAwait(false);
            return result;
        }

        /// <summary>Select from space by key</summary>
        /// <param name="key">The key filed (array or list).</param>
        /// <param name="iterator">The iterator.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="cancellationToken">The cancellation Token.</param>
        /// <returns>The <see cref="Task" />.</returns>
        public async Task<IList<T>> SelectAsync(
            TKey key,
            Iterator iterator,
            uint offset,
            uint limit,
            CancellationToken cancellationToken)
        {
            await EnsureHaveIndexIdAsync(cancellationToken).ConfigureAwait(false);
            Debug.Assert(IndexId != null, "IndexId != null");
            var result = await TarantoolClient.SelectAsync<T>(
                                 new SelectRequest
                                 {
                                     SpaceId = Space.SpaceId,
                                     IndexId = IndexId.Value,
                                     Key = key.Key,
                                     Iterator = iterator,
                                     Offset = offset,
                                     Limit = limit
                                 },
                                 cancellationToken)
                             .ConfigureAwait(false);
            return result;
        }


        /// <summary>Select from space by sub part of key</summary>
        /// <param name="partKey">The key filed (array or list).</param>
        /// <param name="iterator">The iterator.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="cancellationToken">The cancellation Token.</param>
        /// <returns>The <see cref="Task" />.</returns>
        public async Task<IList<T>> SelectBypartialKeyAsync<TPartKey>(
            TPartKey partKey,
            Iterator iterator,
            uint offset,
            uint limit,
            CancellationToken cancellationToken) where TPartKey : IndexKey
        {
            #region validation partial key type
            var partKeyTypes = typeof(TPartKey).GenericTypeArguments;
            var keyTypes = typeof(TKey).GenericTypeArguments;
            if (partKeyTypes.Length >= keyTypes.Length
                || Enumerable.Range(0, partKeyTypes.Length - 1).Any(i => partKeyTypes[i] != keyTypes[i]))
            {
                throw new TarantoolException($"Type of {nameof(partKey)} {typeof(TPartKey)} must be a subkey of {typeof(TKey)}.");
            }
            #endregion

            await EnsureHaveIndexIdAsync(cancellationToken).ConfigureAwait(false);
            Debug.Assert(IndexId != null, "IndexId != null");
            var result = await TarantoolClient.SelectAsync<T>(
                                 new SelectRequest
                                 {
                                     SpaceId = Space.SpaceId,
                                     IndexId = IndexId.Value,
                                     Key = partKey.Key,
                                     Iterator = iterator,
                                     Offset = offset,
                                     Limit = limit
                                 },
                                 cancellationToken)
                             .ConfigureAwait(false);
            return result;
        }

        /// <summary>Performs an updates in space.</summary>
        /// <param name="key">The key.</param>
        /// <param name="updateDefinition">The update operations list.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The <see cref="Task" /> with replaced data as result.</returns>
        public async Task<IList<T>> UpdateAsync(
            TKey key,
            UpdateDefinition<T> updateDefinition,
            CancellationToken cancellationToken)
        {
            await EnsureHaveIndexIdAsync(cancellationToken).ConfigureAwait(false);
            Debug.Assert(IndexId != null, "IndexId != null");
            var result = await TarantoolClient.UpdateAsync<T>(
                                 new UpdateRequest
                                 {
                                     SpaceId = Space.SpaceId,
                                     IndexId = IndexId.Value,
                                     Key = key.Key,
                                     UpdateOperations = updateDefinition.UpdateOperations
                                 },
                                 cancellationToken)
                             .ConfigureAwait(false);
            return result;
        }
    }
}