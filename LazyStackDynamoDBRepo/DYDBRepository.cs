using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace LazyStackDynamoDBRepo
{
    /// <summary>
    /// Map CRUDL operations onto DynamoDBv2.Model namespace operations (low level access)
    /// DynamoDB offers a variety of access libraries. 
    /// This class uses the "Low Level" interfaces available in the DynamoDBv2.Model namespace.
    /// https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/DynamoDBv2/NDynamoDBv2Model.html
    /// </summary>
    /// <typeparam name="TEnv"></typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract
        class DYDBRepository<TEnv, T> : IDYDBRepository<TEnv, T>
              where TEnv : class, IDataEnvelope<T>, new()
              where T : class, new()
    {

        public DYDBRepository(IAmazonDynamoDB client, string envVarTableName)
        {
            this.client = client;
            // Get table name from specified Environmental Variable
            if (!string.IsNullOrEmpty(envVarTableName))
                tablename = Environment.GetEnvironmentVariable(envVarTableName);
            else
                tablename = "";
        }

        #region Fields
        protected string tablename;
        protected IAmazonDynamoDB client;
        protected Dictionary<string, (TEnv envelope, long lastReadTick)> cache = new Dictionary<string, (TEnv, long)>();
        #endregion

        #region Properties 
        private bool _UpdateReturnOkResults = true;
        public bool UpdateReturnsOkResult
        {
            get { return _UpdateReturnOkResults; }
            set { _UpdateReturnOkResults = value; }
        }
        public bool AlwaysCache { get; set; } = false;
        private long cacheTime = 0;
        public long CacheTimeSeconds
        {
            get { return cacheTime / 10000000; } // 10 million ticks in a second, 600 million ticks in a minute
            set { cacheTime = value * 10000000; }
        }

        public long MaxItems { get; set; }


        #endregion

        /// <summary>
        /// Make sure cach has less than MaxItems 
        /// MaxItems == 0 means infinite cache
        /// </summary>
        /// <returns></returns>
        protected void PruneCache(string table = null)
        {
            if (string.IsNullOrEmpty(table))
                table = tablename;

            if (MaxItems == 0) return;
            if (cache.Count > MaxItems)
            {
                var numToFlush = cache.Count - MaxItems;
                // Simple flush the oldest strategy
                var cacheOrderByUpdateTick = cache.Values.OrderBy(item => item.lastReadTick);
                int i = 0;
                foreach (var item in cacheOrderByUpdateTick)
                {
                    if (i > numToFlush) return;
                    cache.Remove($"{item.envelope.PK}{item.envelope.SK}");
                }
            }
        }

        public async Task FlushCache(string table = null)
        {
            if (string.IsNullOrEmpty(table))
                table = tablename;

            await Task.Delay(0);
            cache = new Dictionary<string, (TEnv, long)>();
        }

        public async Task<ActionResult<T>> CreateAsync(T data, string table = null, bool? useCache = null)
        {
            if (string.IsNullOrEmpty(table))
                table = tablename;

            bool useCache2 = (useCache != null) ? (bool)useCache : AlwaysCache;
            try
            {
                var now = DateTime.UtcNow.Ticks;
                TEnv envelope = new TEnv()
                {
                    EntityInstance = data,
                    CreateUtcTick = now,
                    UpdateUtcTick = now
                };

                envelope.SealEnvelope();

                // Wait until just before write to serialize EntityInstance (captures updates to UtcTick fields)
                envelope.DbRecord.Add("Data", new AttributeValue() { S = JsonConvert.SerializeObject(envelope.EntityInstance) });

                var request = new PutItemRequest()
                {
                    TableName = table,
                    Item = envelope.DbRecord,
                    ConditionExpression = "attribute_not_exists(PK)" // Technique to avoid replacing an existing record
                };



                await client.PutItemAsync(request);

                if (useCache2)
                {
                    cache[$"{table}:{envelope.PK}{envelope.SK}"] = (envelope, DateTime.UtcNow.Ticks);
                    PruneCache();
                }

                return data;
            }
            catch (ConditionalCheckFailedException ex) { return new ConflictResult(); }
            catch (AmazonDynamoDBException ex) { return new StatusCodeResult(400); }
            catch (AmazonServiceException) { return new StatusCodeResult(500); }
            catch { return new StatusCodeResult(500); }
        }


        public async Task<ActionResult<T>> ReadAsync(string pK, string sK = null, string table = null, bool? useCache = null)
        {
            if (string.IsNullOrEmpty(table))
                table = tablename;

            try
            {
                var response = await ReadEAsync(pK, sK, table, useCache: useCache);
                if (response.Value == null)
                    return response.Result;

                return response.Value.EntityInstance;
            }

            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(406); }
        }



        public async Task<ActionResult<TEnv>> ReadEAsync(string pK, string sK = null, string table = null, bool? useCache = null)
        {
            if (string.IsNullOrEmpty(table))
                table = tablename;

            bool useCache2 = (useCache != null) ? (bool)useCache : AlwaysCache;
            try
            {
                var key = $"{table}:{pK}{sK}";
                if ((useCache2) && cache.ContainsKey(key))
                {
                    TEnv cachedEnvelope;
                    long lastReadTicks;
                    (cachedEnvelope, lastReadTicks) = cache[key];
                    PruneCache(table);
                    if (DateTime.UtcNow.Ticks - lastReadTicks < cacheTime)
                        return cachedEnvelope;
                }

                var request = new GetItemRequest()
                {
                    TableName = table,
                    Key = new Dictionary<string, AttributeValue>()
                    {
                        {"PK", new AttributeValue {S = pK}},
                        {"SK", new AttributeValue {S = sK } }
                    }
                };
                var response = await client.GetItemAsync(request);

                var item = new TEnv() { DbRecord = response.Item };
                if (useCache2)
                {
                    cache[key] = (item, DateTime.UtcNow.Ticks);
                    PruneCache();
                }

                return item;
            }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(406); }
        }

        public async Task<ActionResult<T>> UpdateAsync(T data, string table = null)
        {

            if (string.IsNullOrEmpty(table))
                table = tablename;

            if (data.Equals(null))
                return new StatusCodeResult(400);

            TEnv envelope = new TEnv() { EntityInstance = data };

            try
            {
                var OldUpdateUtcTick = envelope.UpdateUtcTick;
                var now = DateTime.UtcNow.Ticks;
                envelope.UpdateUtcTick = now; // The UpdateUtcTick Set calls SetUpdateUtcTick where you can update your entity data record 

                envelope.SealEnvelope();

                // Waiting until just before write to serialize EntityInstance (captures updates to UtcTick fields)
                envelope.DbRecord.Add("Data", new AttributeValue() { S = JsonConvert.SerializeObject(envelope.EntityInstance) });

                // Write data to database - use conditional put to avoid overwriting newer data
                var request = new PutItemRequest()
                {
                    TableName = table,
                    Item = envelope.DbRecord,
                    ConditionExpression = "UpdateUtcTick = :OldUpdateUtcTick",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":OldUpdateUtcTick", new AttributeValue() {N = OldUpdateUtcTick.ToString()} }
                    }
                };



                await client.PutItemAsync(request);

                var key = $"{table}:{envelope.PK}{envelope.SK}";
                if (cache.ContainsKey(key)) cache[key] = (envelope, DateTime.UtcNow.Ticks);
                PruneCache();

                if (UpdateReturnsOkResult)
                {
                    return new OkObjectResult(envelope.EntityInstance);
                }
                else
                    return envelope.EntityInstance;
            }
            catch (ConditionalCheckFailedException) { return new ConflictResult(); } // STatusCode 409
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(500); }
        }

        public async Task<StatusCodeResult> DeleteAsync(string pK, string sK = null, string table = null)
        {
            if (string.IsNullOrEmpty(table))
                table = tablename;

            try
            {
                if (string.IsNullOrEmpty(pK))
                    return new StatusCodeResult(406); // bad key

                var request = new DeleteItemRequest()
                {
                    TableName = table,
                    Key = new Dictionary<string, AttributeValue>()
                    {
                        {"PK", new AttributeValue {S= pK} },
                        {"SK", new AttributeValue {S = sK} }
                    }
                };

                await client.DeleteItemAsync(request);

                var key = $"{table}:{pK}{sK}";
                if (cache.ContainsKey(key)) cache.Remove(key);
                PruneCache();

                return new OkResult();
            }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(406); }
        }

        public async Task<ActionResult<ICollection<TEnv>>> ListEAsync(QueryRequest queryRequest, bool? useCache = null)
        {
            var table = queryRequest.TableName;
            bool useCache2 = (useCache != null) ? (bool)useCache : AlwaysCache;
            try
            {
                var response = await client.QueryAsync(queryRequest);
                var list = new List<TEnv>();
                foreach (Dictionary<string, AttributeValue> item in response?.Items)
                {
                    var envelope = new TEnv() { DbRecord = item };
                    list.Add(envelope);
                    var key = $"{table}:{envelope.PK}{envelope.SK}";
                    if (useCache2 || cache.ContainsKey(key))
                        cache[key] = (envelope, DateTime.UtcNow.Ticks);
                }
                PruneCache();
                return list;
            }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(500); }
        }

        public async Task<ActionResult<ICollection<T>>> ListAsync(QueryRequest queryRequest, bool? useCache = null)
        {
            var table = queryRequest.TableName;
            bool useCache2 = (useCache != null) ? (bool)useCache : AlwaysCache;
            try
            {
                var response = await client.QueryAsync(queryRequest);
                var list = new List<T>();
                foreach (Dictionary<string, AttributeValue> item in response?.Items)
                {
                    var envelope = new TEnv() { DbRecord = item };
                    list.Add(envelope.EntityInstance);
                    var key = $"{table}:{envelope.PK}{envelope.SK}";
                    if (useCache2 || cache.ContainsKey(key))
                        cache[key] = (envelope, DateTime.UtcNow.Ticks);
                }
                PruneCache();
                return list;
            }
            catch (AmazonDynamoDBException e)
            {
                ;
                return new StatusCodeResult(500);
            }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(500); }
        }


        protected Dictionary<string, string> GetExpressionAttributeNames(Dictionary<string, string> value)
        {
            if (value != null)
                return value;

            return new Dictionary<string, string>()
            {
                {"#Data", "Data" },
                {"#Status", "Status" },
                {"#General", "General" }
            };
        }

        protected string GetProjectionExpression(string value)
        {
            if (value == null)
                value = "#Data, TypeName, #Status, UpdateUtcTick, CreateUtcTick, #General";
            return value;
        }

        public QueryRequest QueryEquals(string pK, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null)
        {
            expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
            projectionExpression = GetProjectionExpression(projectionExpression);
            if (string.IsNullOrEmpty(table))
                table = tablename;

            return new QueryRequest()
            {
                TableName = table,
                KeyConditionExpression = $"PK = :PKval",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":PKval", new AttributeValue() {S = pK} }
                },
                ExpressionAttributeNames = expressionAttributeNames,
                ProjectionExpression = projectionExpression
            };

        }

        public QueryRequest QueryEquals(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null)
        {
            expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
            projectionExpression = GetProjectionExpression(projectionExpression);
            if (string.IsNullOrEmpty(table))
                table = tablename;

            var indexName = (string.IsNullOrEmpty(keyField) || keyField.Equals("SK")) ? null : $"PK-{keyField}-Index";

            return new QueryRequest()
            {
                TableName = table,
                KeyConditionExpression = $"PK = :PKval and {keyField} = :SKval",
                IndexName = indexName,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":PKval", new AttributeValue() {S = pK} },
                    {":SKval", new AttributeValue() {S =  key } }
                },
                ExpressionAttributeNames = expressionAttributeNames,
                ProjectionExpression = projectionExpression
            };
        }

        public QueryRequest QueryBeginsWith(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null)
        {
            expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
            projectionExpression = GetProjectionExpression(projectionExpression);
            if (string.IsNullOrEmpty(table))
                table = tablename;

            var indexName = (string.IsNullOrEmpty(keyField) || keyField.Equals("SK")) ? null : $"PK-{keyField}-Index";

            return new QueryRequest()
            {
                TableName = table,
                KeyConditionExpression = $"PK = :PKval and begins_with({keyField}, :SKval)",
                IndexName = indexName,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":PKval", new AttributeValue() {S = pK} },
                    {":SKval", new AttributeValue() {S =  key } }
                },
                ExpressionAttributeNames = expressionAttributeNames,
                ProjectionExpression = projectionExpression
            };

        }

        public QueryRequest QueryBeginsWith(string pK, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null)
        {
            expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
            projectionExpression = GetProjectionExpression(projectionExpression);
            if (string.IsNullOrEmpty(table))
                table = tablename;

            return new QueryRequest()
            {
                TableName = table,
                KeyConditionExpression = $"PK = :PKval",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":PKval", new AttributeValue() {S = pK} }
                },
                ExpressionAttributeNames = expressionAttributeNames,
                ProjectionExpression = projectionExpression
            };
        }

        public QueryRequest QueryRange(
            string pK,
            string keyField,
            string keyStart,
            string keyEnd,
            Dictionary<string, string> expressionAttributeNames = null,
            string projectionExpression = null,
            string table = null)
        {
            expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
            projectionExpression = GetProjectionExpression(projectionExpression);
            if (string.IsNullOrEmpty(table))
                table = tablename;

            var indexName = (string.IsNullOrEmpty(keyField) || keyField.Equals("SK")) ? null : $"PK-{keyField}-Index";

            return new QueryRequest()
            {
                TableName = table,
                KeyConditionExpression = $"PK = :PKval and {keyField} between :SKStart and :SKEnd ",
                IndexName = indexName,
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":PKval", new AttributeValue() {S = pK} },
                    {":SKStart", new AttributeValue() {S =  keyStart }},
                    {":SKEnd", new AttributeValue() {S = keyEnd} }
                },
                ExpressionAttributeNames = expressionAttributeNames,
                ProjectionExpression = projectionExpression
            };

        }


    }
}
