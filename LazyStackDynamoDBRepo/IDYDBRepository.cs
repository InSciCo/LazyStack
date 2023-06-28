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
    /// Create record
    /// </summary>
    /// <param name="data"></param>
    /// <returns>ActionResult<typeparamref name="T"/></returns>
    Task<ActionResult<T>> CreateAsync(T data, string table = null, bool? useCache = null);
    /// <summary>
    ///  Create record, Produce notification
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="payloadParentId"></param>
    /// <param name="payloadId"></param>
    /// <param name="data"></param>
    /// <param name="table"></param>
    /// <param name="useCache"></param>
    /// <returns>ActionResult<typeparamref name="T"/></returns>
    Task<ActionResult<T>> CreateNAsync(string topicId, string payloadParentId, string payloadId,  T data, string table = null, bool? useCache = null);
    /// <summary>
    /// Create data Envelope
    /// </summary>
    /// <param name="data"></param>
    /// <param name="table"></param>
    /// <param name="useCache"></param>
    /// <returns>ActionResult<typeparamref name="TEnv"/></returns>
    Task<ActionResult<TEnv>> CreateEAsync(T data, string table = null, bool? useCache = null);
    /// <summary>
    /// Read record
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="sK"></param>
    /// <returns>ActionResult<typeparamref name="T"/></returns>
    Task<ActionResult<T>> ReadAsync(string pK, string sK = null, string table = null, bool? useCache = null);
    /// <summary>
    /// Read record Envelope
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="sK"></param>
    /// <returns>AcitonResult<typeparamref name="TEnv"/></returns>
    Task<ActionResult<TEnv>> ReadEAsync(string pK, string sK = null, string table = null, bool? useCache = null);
    /// <summary>
    /// Update record
    /// </summary>
    /// <param name="data"></param>
    /// <returns>ActionResult<typeparamref name="T"/></returns>
    Task<ActionResult<T>> UpdateAsync(T data, string table = null);
    /// <summary>
    /// Update record, produce Notification
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="payloadParentId"></param>
    /// <param name="payloadId"></param>
    /// <param name="data"></param>
    /// <param name="table"></param>
    /// <returns><ActionResult<typeparamref name="T"/></ActionResult></returns>
    Task<ActionResult<T>> UpdateNAsync(string topicId, string? payloadParentId, string payloadId, T data, string table = null);
    /// <summary>
    /// Update record Envelope
    /// </summary>
    /// <param name="data"></param>
    /// <param name="table"></param>
    /// <returns>ActionResult<typeparamref name="TEnv"/></returns>
    Task<ActionResult<TEnv>> UpdateEAsync(T data, string table = null);


    /// <summary>
    /// Delete record. 
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="sK"></param>
    /// <returns>StatusCodeResult</returns>
    Task<StatusCodeResult> DeleteAsync(string pK, string sK = null, string table = null);
    /// <summary>
    /// Delete record, produce notification
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="payloadParentId"></param>
    /// <param name="payloadId"></param>
    /// <param name="pK"></param>
    /// <param name="sK"></param>
    /// <param name="table"></param>
    /// <returns>StatusCodeResult</returns>
    Task<StatusCodeResult> DeleteNAsync(string topicId, string payloadParentId, string payloadId, string pK, string sK = null, string table = null);
    /// <summary>
    /// Delete record Envelope
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="sK"></param>
    /// <param name="table"></param>
    /// <returns>ActionResult<typeparamref name="TEnv"/></returns>
    Task<ActionResult<TEnv>> DeleteEAsync(string pK, string sK = null, string table = null);

    /// <summary>
    /// Call QueryAsync and return list of envelopes
    /// </summary>
    /// <param name="queryRequest"></param>
    /// <returns>List<typeparamref name="TEnv"/></returns>
    Task<ActionResult<ICollection<TEnv>>> ListEAsync(QueryRequest queryRequest, bool? useCache = null);

    /// <summary>
    /// Call QueryAsync and return list of data objects of type T
    /// </summary>
    /// <param name="queryRequest"></param>
    /// <returns>List<typeparamref name="T"/></returns>
    Task<ActionResult<ICollection<T>>> ListAsync(QueryRequest queryRequest, bool? useCache = null);

    /// <summary>
    /// Return a simple query request for records matching query arguments
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="expressionAttributeNames"></param>
    /// <param name="projectionExpression"></param>
    /// <returns>QueryRequest</returns>
    QueryRequest QueryEquals(string pK, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null);

    /// <summary>
    /// Return a simple query request using {keyField} = SKval on index PK-{keyField}-Index
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="keyField"></param>
    /// <param name="key"></param>
    /// <param name="expressionAttributeNames"></param>
    /// <returns>QueryRequest</returns>
    QueryRequest QueryEquals(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null);


    /// <summary>
    /// Return a simple query request using begins_with({keyField}, SKval) on index PK-{keyField}-Index
    /// </summary>
    /// <param name="pK"></param>
    /// <param name="keyField"></param>
    /// <param name="key"></param>
    /// <param name="expressionAttributeNames"></param>
    /// <returns>QueryRequest</returns>
    QueryRequest QueryBeginsWith(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null);
    /// <summary>
    /// Create a notification
    /// </summary>
    /// <param name="topicId"></param>
    /// <param name="payloadParent"></param>
    /// <param name="payloadId"></param>
    /// <param name="payload"></param>
    /// <param name="payloadType"></param>
    /// <param name="payloadAction"></param>
    /// <param name="createdAt"></param>
    /// <param name="table"></param>
    /// <returns>ActionResult</returns>
    Task<ActionResult> CreateNotification(string topicId, string? payloadParent, string payloadId, string payload, string payloadType, string payloadAction, long createdAt, string table);
}
