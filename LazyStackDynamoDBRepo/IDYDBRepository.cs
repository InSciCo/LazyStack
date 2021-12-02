using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;

namespace LazyStackDynamoDBRepo
{

    /// <summary>
    /// Use DynamoDB via the DynamoDBv2.Model namespace (low level inteface)
    /// https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/DynamoDBv2/NDynamoDBv2Model.html
    /// CRUDL operations map onto the low level access operations available in the namespace.
    /// </summary>
    /// <typeparam name="TEnv"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface IDYDBRepository<TEnv,T>
        where TEnv : class, IDataEnvelope<T>, new()
        where T : class, new()
    {
        Boolean UpdateReturnsOkResult { get; set; }

        /// <summary>
        /// Create PutItemRequest and call PutItemAsync
        /// </summary>
        /// <param name="data"></param>
        /// <returns>ActionResult</returns>
        Task<ActionResult<T>> CreateAsync(T data);

        /// <summary>
        /// Read data entity (calls ReadEAsync)
        /// </summary>
        /// <param name="pK"></param>
        /// <param name="sK"></param>
        /// <returns>ActionResult<T></returns>
        Task<ActionResult<T>> ReadAsync(string pK, string sK = null);


        /// <summary>
        /// Read dBrecord (envelope)
        /// </summary>
        /// <param name="pK"></param>
        /// <param name="sK"></param>
        /// <returns></returns>
        Task<ActionResult<TEnv>> ReadEAsync(string pK, string sK = null);

        /// <summary>
        /// Ccreate PutItemRequest and call PutItmeAsync. Use UpdateUtcTick to do optimistic lock.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ActionResult<T>> UpdateAsync(T data);

        /// <summary>
        /// Call DeleteItemAsyunc
        /// </summary>
        /// <param name="pK"></param>
        /// <param name="sK"></param>
        /// <returns></returns>
        Task<StatusCodeResult> DeleteAsync(string pK, string sK = null);


        /// <summary>
        /// Call QueryAsync and return list of envelopes
        /// </summary>
        /// <param name="queryRequest"></param>
        /// <returns>List<TEnv></TEnv></returns>
        Task<ActionResult<ICollection<TEnv>>> ListEAsync(QueryRequest queryRequest);

        /// <summary>
        /// Call QueryAsync and return list of data objects of type T
        /// </summary>
        /// <param name="queryRequest"></param>
        /// <returns>List<T></returns>
        Task<ActionResult<ICollection<T>>> ListAsync(QueryRequest queryRequest);
    }
}
