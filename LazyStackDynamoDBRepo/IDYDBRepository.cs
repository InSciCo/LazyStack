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
        /// <summary>
        /// Create PutItemRequest and call PutItemAsync
        /// </summary>
        /// <param name="data"></param>
        /// <returns>ActionResult</returns>
        Task<IActionResult> CreateAsync(T data);

        /// <summary>
        /// Call ReadEAsync and extract data from the envelope
        /// </summary>
        /// <param name="pKPrefix"></param>
        /// <param name="pKval"></param>
        /// <returns>ActionResult</returns>
        Task<IActionResult> ReadAsync(string pKPrefix, string pKval);

        /// <summary>
        /// Call ReadAsync and extract data from the envelope
        /// </summary>
        /// <param name="pKPrefix"></param>
        /// <param name="pKval"></param>
        /// <param name="sKPrefix"></param>
        /// <param name="sKval"></param>
        /// <returns>ActionResult<T></returns>
        Task<IActionResult> ReadAsync(string pKPrefix, string pKval, string sKPrefix, string sKval);

        /// <summary>
        /// Ccreate PutItemRequest and call PutItmeAsync. Use UpdateUtcTick to do optimistic lock.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<IActionResult> UpdateAsync(T data);

        /// <summary>
        /// Call DeleteItemAsyunc
        /// </summary>
        /// <param name="pKPrefix"></param>
        /// <param name="pKval"></param>
        /// <returns></returns>
        Task<IActionResult> DeleteAsync(string pKPrefix, string pKval);

        /// <summary>
        /// Call DeleteItemAsuync
        /// </summary>
        /// <param name="pKPrefix"></param>
        /// <param name="pKval"></param>
        /// <param name="sKPrefix"></param>
        /// <param name="sKval"></param>
        /// <returns></returns>
        Task<IActionResult> DeleteAsync(string pKPrefix, string pKval, string sKPrefix, string sKval);

        /// <summary>
        /// Call QueryAsync and return list of envelopes
        /// </summary>
        /// <param name="queryRequest"></param>
        /// <returns>List<TEnv></TEnv></returns>
        Task<ICollection<TEnv>> ListEAsync(QueryRequest queryRequest);

        /// <summary>
        /// Call QueryAsync and return list of data objects of type T
        /// </summary>
        /// <param name="queryRequest"></param>
        /// <returns>List<T></returns>
        Task<IActionResult> ListAsync(QueryRequest queryRequest);
    }
}
