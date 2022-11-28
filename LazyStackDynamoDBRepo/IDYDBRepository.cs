using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
 
namespace LazyStackDynamoDBRepo;


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
    bool UpdateReturnsOkResult { get; set; }
    bool AlwaysCache { get; set; }
    long CacheTimeSeconds { get; set; }
    long MaxItems { get; set; }

    /// <summary>
    /// Flush the cache
    /// </summary>
    /// <returns></returns>
    Task FlushCache(string table = null);

    /// <summary>
    /// Call CreateEAsync and return T
    /// </summary>
    /// <param name="data"></param>
    /// <returns>ActionResult</returns>
    Task<ActionResult<T>> CreateAsync(T data, string table = null, bool? useCache = null);

    Task<ActionResult<T>> CreateNAsync(string topicId, string payloadParentId, string payloadId,  T data, string table = null, bool? useCache = null);

    /// <summary>
    /// Create PutItemRequest and return pair (TEnv, T)
    /// </summary>
    /// <param name="data"></param>
    /// <param name="table"></param>
    /// <param name="useCache"></param>
    /// <returns></returns>
    Task<ActionResult<TEnv>> CreateEAsync(T data, string table = null, bool? useCache = null);

    /// <summary>
    /// Read data entity (calls ReadEAsync)
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="sK"></param>
    /// <returns>ActionResult<T></returns>
    Task<ActionResult<T>> ReadAsync(string pK, string sK = null, string table = null, bool? useCache = null);


    /// <summary>
    /// Read dBrecord (envelope)
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="sK"></param>
    /// <returns></returns>
    Task<ActionResult<TEnv>> ReadEAsync(string pK, string sK = null, string table = null, bool? useCache = null);

    /// <summary>
    /// Call CreateEAsync and return T
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    Task<ActionResult<T>> UpdateAsync(T data, string table = null);

    Task<ActionResult<T>> UpdateNAsync(string topicId, string? payloadParentId, string payloadId, T data, string table = null);
    Task<ActionResult<TEnv>> UpdateEAsync(T data, string table = null);


    /// <summary>
    /// Create PutItemRequest and call PutItmeAsync. Use UpdateUtcTick to do optimistic lock. Return T
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="sK"></param>
    /// <returns></returns>
    Task<StatusCodeResult> DeleteAsync(string pK, string sK = null, string table = null);
    Task<StatusCodeResult> DeleteNAsync(string topicId, string payloadParentId, string payloadId, string pK, string sK = null, string table = null);
    Task<ActionResult<TEnv>> DeleteEAsync(string pK, string sK = null, string table = null);

    /// <summary>
    /// Call QueryAsync and return list of envelopes
    /// </summary>
    /// <param name="queryRequest"></param>
    /// <returns>List<TEnv></TEnv></returns>
    Task<ActionResult<ICollection<TEnv>>> ListEAsync(QueryRequest queryRequest, bool? useCache = null);

    /// <summary>
    /// Call QueryAsync and return list of data objects of type T
    /// </summary>
    /// <param name="queryRequest"></param>
    /// <returns>List<T></returns>
    Task<ActionResult<ICollection<T>>> ListAsync(QueryRequest queryRequest, bool? useCache = null);

    /// <summary>
    /// Return all records having the primary key value specified
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="expressionAttributeNames"></param>
    /// <param name="projectionExpression"></param>
    /// <returns></returns>
    QueryRequest QueryEquals(string pK, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null);

    /// <summary>
    /// Return a simple query request using {keyField} = SKval on index PK-{keyField}-Index
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="keyField"></param>
    /// <param name="key"></param>
    /// <param name="expressionAttributeNames"></param>
    /// <returns></returns>
    QueryRequest QueryEquals(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null);


    /// <summary>
    /// Return a simple query request using begins_with({keyField}, SKval) on index PK-{keyField}-Index
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="keyField"></param>
    /// <param name="key"></param>
    /// <param name="expressionAttributeNames"></param>
    /// <returns></returns>
    QueryRequest QueryBeginsWith(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null);

    Task<ActionResult> CreateNotification(string topicId, string? payloadParent, string payloadId, string payload, string payloadType, string payloadAction, long createdAt, string table);
}
