using Microsoft.AspNetCore.Mvc;

namespace LazyStackDynamoDBRepo;

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
    public DYDBRepository(IAmazonDynamoDB client)
    {
        this.client = client;
    }

    #region Fields
    protected string tablename;
    protected IAmazonDynamoDB client;
    protected Dictionary<string, (TEnv envelope, long lastReadTick)> cache = new();
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
    /// <summary>
    /// Time To Live in Seconds. Set to 0 to disable. 
    /// Default is 0.
    /// Override GetTTL() for custom behavior.
    /// </summary>
    public long TTL { get; set; } = 0;
    public bool UseIsDeleted { get; set; } 
    public bool UseSoftDelete { get; set; }
    public string PK { get; set; }
    #endregion

    protected virtual long GetTTL()
    {
        if (TTL == 0)
            return 0;
        // We don't use createdAt in case we are doing time windows for testing. Instead, we always  
        // use the current time + 48 hours for TTL. 
        return (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds + TTL;
    }
    /// <summary>
    /// Topics to insert place in optional Topics attribute. 
    /// Override in derived class to suite your messaging requirements.
    /// </summary>
    /// <returns>Json String</returns>
    protected virtual string SetTopics()
    {
        return string.Empty;
    }
    /// <summary>
    /// Make sure cache has less than MaxItems 
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
            foreach (var (envelope, lastReadTick) in cacheOrderByUpdateTick)
            {
                if (i > numToFlush) return;
                cache.Remove($"{envelope.PK}{envelope.SK}");
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
    public virtual async Task<ActionResult<TEnv>> CreateEAsync(T data, string table = null, bool? useCache = null) => await CreateEAsync(data, new CallerInfo() { Table = table}, useCache);
    public virtual async Task<ActionResult<TEnv>> CreateEAsync(T data, ICallerInfo callerInfo = null,  bool? useCache = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;

        if (string.IsNullOrEmpty(table))
            table = tablename;

        bool useCache2 = (useCache != null) ? (bool)useCache : AlwaysCache;
        try
        {
            var now = DateTime.UtcNow.Ticks;
            TEnv envelope = new()
            {
                EntityInstance = data,
                CreateUtcTick = now,
                UpdateUtcTick = now
            };

            envelope.SealEnvelope();

            // Wait until just before write to serialize EntityInstance (captures updates to UtcTick fields)
            envelope.DbRecord.Add("Data", new AttributeValue() { S = JsonConvert.SerializeObject(envelope.EntityInstance) });

            AddOptionalAttributes(envelope, callerInfo); // Adds TTL, Topics when specified

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

            return envelope;
        }
        catch (ConditionalCheckFailedException) { return new ConflictResult(); }
        catch (AmazonDynamoDBException) { return new StatusCodeResult(400); }
        catch (AmazonServiceException) { return new StatusCodeResult(500); }
        catch { return new StatusCodeResult(500); }
    }
    public virtual async Task<ActionResult<T>> CreateAsync(T data, string table = null, bool? useCache = null) => await CreateAsync(data, new CallerInfo() { Table = table }, useCache);    
    public virtual async Task<ActionResult<T>> CreateAsync(T data, ICallerInfo callerInfo = null, bool? useCache = null)
    {
        callerInfo ??= new CallerInfo();
        var result = await CreateEAsync(data, callerInfo, useCache);
        if (result.Result is not null)
            return result.Result;
        return result.Value.EntityInstance;
    }

    public virtual async Task<ActionResult<T>> ReadAsync(string id, ICallerInfo callerInfo, bool? useCache = null) => await ReadAsync(this.PK, id, callerInfo, useCache);
    public virtual async Task<ActionResult<T>> ReadAsync(string pK, string sK = null, string table = null, bool? useCache = null) => await ReadAsync(pK, sK, new CallerInfo() { Table = table }, useCache); 
    public virtual async Task<ActionResult<T>> ReadAsync(string pK, string sK = null, ICallerInfo callerInfo = null, bool? useCache = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;
        if (string.IsNullOrEmpty(table))
            table = tablename;

        try
        {
            var response = await ReadEAsync(pK, sK, callerInfo, useCache: useCache);
            if (response.Value == null)
                return response.Result;

            return response.Value.EntityInstance;
        }

        catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
        catch (AmazonServiceException) { return new StatusCodeResult(503); }
        catch { return new StatusCodeResult(406); }
    }
    public virtual async Task<ActionResult<TEnv>> ReadEAsync(string id, ICallerInfo callerInfo = null, bool? useCache = null) => await ReadEAsync(this.PK, id, callerInfo, useCache);
    public virtual async Task<ActionResult<TEnv>> ReadEAsync(string pK, string sK = null, string table = null, bool? useCache = null) => await ReadEAsync(pK, sK, new CallerInfo() { Table = table }, useCache);
    public virtual async Task<ActionResult<TEnv>> ReadEAsync(string pK, string sK = null, ICallerInfo callerInfo = null, bool? useCache = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;
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
    /// <summary>
    /// Add optional attributes to envelope prior to create or update. 
    /// This routine currently handels the optional attributes TTL and Topics.
    /// </summary>
    /// <param name="envelope"></param>
    public virtual void AddOptionalAttributes(TEnv envelope, ICallerInfo callerInfo, bool isDeleted = false)
    {
        // Add TTL attribute when GetTTL() is not 0
        var ttl = GetTTL();
        if (ttl != 0)
            envelope.DbRecord.Add("TTL", new AttributeValue() { N = ttl.ToString() });

        // Add Topics attribute when GetTopics() is not empty 
        var topics = SetTopics();
        if (!string.IsNullOrEmpty(topics))
            envelope.DbRecord.Add("Topics", new AttributeValue() { S = topics });
    }
    public virtual async Task<ActionResult<TEnv>> UpdateEAsync(T data, string table = null) => await UpdateEAsync(data, new CallerInfo() { Table = table });
    public virtual async Task<ActionResult<TEnv>> UpdateEAsync(T data, ICallerInfo callerInfo = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;

        if (string.IsNullOrEmpty(table))
            table = tablename;

        if (data.Equals(null))
            return new StatusCodeResult(400);

        TEnv envelope = new() { EntityInstance = data };

        try
        {
            var OldUpdateUtcTick = envelope.UpdateUtcTick;
            var now = DateTime.UtcNow.Ticks;
            envelope.UpdateUtcTick = now; // The UpdateUtcTick Set calls SetUpdateUtcTick where you can update your entity data record 

            envelope.SealEnvelope();

            // Waiting until just before write to serialize EntityInstance (captures updates to UtcTick fields)
            envelope.DbRecord.Add("Data", new AttributeValue() { S = JsonConvert.SerializeObject(envelope.EntityInstance) });

            AddOptionalAttributes(envelope, callerInfo); // Adds TTL, Topics when specified

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
                return envelope;
        }
        catch (ConditionalCheckFailedException) { return new ConflictResult(); } // STatusCode 409
        catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
        catch (AmazonServiceException) { return new StatusCodeResult(503); }
        catch { return new StatusCodeResult(500); }
    }

    public virtual async Task<ActionResult<T>> UpdateAsync(T data, string table = null) => await UpdateAsync(data, new CallerInfo() { Table = table });
    public virtual async Task<ActionResult<T>> UpdateAsync(T data, ICallerInfo callerInfo = null)
    {
        callerInfo ??= new CallerInfo();    
        var result = await UpdateEAsync(data, callerInfo);
        if (result.Result is not null)
            return result.Result;
        return result.Value.EntityInstance;
    }
    public virtual async Task<StatusCodeResult> DeleteAsync(string id, ICallerInfo callerInfo = null) => await DeleteAsync(this.PK, id, callerInfo);
    public virtual async Task<StatusCodeResult> DeleteAsync(string pK, string sK = null, string table = null) => await DeleteAsync(pK, sK, new CallerInfo() { Table = table});
    public virtual async Task<StatusCodeResult> DeleteAsync(string pK, string sK = null, ICallerInfo callerInfo = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;
        if (string.IsNullOrEmpty(table))
            table = tablename;
        try
        {
            if (string.IsNullOrEmpty(pK))
                return new StatusCodeResult(406); // bad key

            if(UseIsDeleted)
            {
                // UseIsDeleted allows our Notifications process will receive
                // the SessionId of the client that deleted the record. This 
                // in turn allows us to avoid sending that notification back
                // to the originating client. 
                var readResult = await ReadEAsync(pK, sK, callerInfo);
                var envelope = readResult.Value;
                if (envelope is null)
                    return new StatusCodeResult(200);
                envelope.IsDeleted = true;
                envelope.UseTTL = UseSoftDelete; // DynamoDB will delete records after TTL reached
                var updateResult = await UpdateEAsync(envelope.EntityInstance, callerInfo);
                if (updateResult.Result is not null)
                    return (StatusCodeResult)updateResult.Result;
            }

            if(!UseSoftDelete)
            {            
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
            }

            var key = $"{table}:{pK}{sK}";
            if (cache.ContainsKey(key)) cache.Remove(key);
            PruneCache();

            return new StatusCodeResult(200);
        }
        catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
        catch (AmazonServiceException) { return new StatusCodeResult(503); }
        catch { return new StatusCodeResult(406); }
    }
    public virtual async Task<(ActionResult<ICollection<TEnv>> actionResult, long responseSize)> ListEAndSizeAsync(QueryRequest queryRequest, bool? useCache = null, int limit = 0)
    {
        var table = queryRequest.TableName;
        bool useCache2 = (useCache != null) ? (bool)useCache : AlwaysCache;
        Dictionary<string, AttributeValue> lastEvaluatedKey = null;
        try
        {
            var list = new List<TEnv>();
            var responseSize = 0; 
            do
            {
                if (lastEvaluatedKey is not null)
                    queryRequest.ExclusiveStartKey = lastEvaluatedKey;
                if (limit != 0)
                    queryRequest.Limit = limit;

                var response = await client.QueryAsync(queryRequest);
                foreach (Dictionary<string, AttributeValue> item in response?.Items)
                {
                    var envelope = new TEnv() { DbRecord = item };
                    responseSize += envelope.JsonSize;
                    if (responseSize > 5120)
                        break;

                    list.Add(envelope);
                    var key = $"{table}:{envelope.PK}{envelope.SK}";
                    if (useCache2 || cache.ContainsKey(key))
                        cache[key] = (envelope, DateTime.UtcNow.Ticks);
                }
            } while (responseSize <= 5120 && lastEvaluatedKey != null && list.Count < limit);
            var statusCode = lastEvaluatedKey == null ? 200 : 206;
            PruneCache();
            return (new ObjectResult(list) { StatusCode = statusCode }, responseSize);
        }
        catch (AmazonDynamoDBException) { return (new StatusCodeResult(500), 0); } 
        catch (AmazonServiceException) { return (new StatusCodeResult(503), 0); }
        catch { return (new StatusCodeResult(500), 0); }
    }
    public virtual async Task<ActionResult<ICollection<TEnv>>> ListEAsync(QueryRequest queryRequest, bool? useCache = null, int limit = 0)
    {
        var (actionResult, _) = await ListEAndSizeAsync(queryRequest, useCache, limit);
        return actionResult;
    }
    /// <summary>
    /// ListAndSizeAsync returns up to "roughly" 5MB of data to stay under the 
    /// 6Mb limit imposed on API Gateway Response bodies.
    /// 
    /// Since DynamoDB queries are limited to 1MB of data, we use pagination to do 
    /// multiple reads as necessary up to approximately 5MB of data.
    /// 
    /// If the query exceeds the 5MB data limit, we return only that
    /// data and a StatusCode 206 (partial result).
    /// 
    /// If you want more pagination control, use the limit argument to control 
    /// how many records are returned in the query. When more records than the 
    /// limit are available, a Status 206 will be returned. The other size limits 
    /// still apply so you might get back fewer records than the limit specified 
    /// even when you set a limit. For instance, if you specify a limit of 20
    /// and each record is 500k in size, then only the first 10 records would be 
    /// returned and the status code would be 206.
    /// 
    /// On the client side, use the status code 200, not the number of records
    /// returned, to recognize end of list.
    /// 
    /// </summary>
    /// <param name="queryRequest"></param>
    /// <param name="useCache"></param>
    /// <returns>Task&lt;(ActionResult&lt;ICollection<T>> actionResult,long responseSize)&gt;</returns>
    public virtual async Task<(ActionResult<ICollection<T>> actionResult,long responseSize)> ListAndSizeAsync(QueryRequest queryRequest, bool? useCache = null, int limit = 0)
    {
        try
        {
            var list = new List<T>();
            var envelopesResult = await ListEAndSizeAsync(queryRequest, useCache, limit);
            IActionResult queryResult = envelopesResult.actionResult.Result as StatusCodeResult;

            if(!IsResultOk(queryResult))
                return (new ObjectResult(list) { StatusCode = GetStatusCode(queryResult) }, envelopesResult.responseSize);    
             
            foreach (var envelope in envelopesResult.actionResult.Value)
                list.Add(envelope.EntityInstance);

            return (new ObjectResult(list) { StatusCode = GetStatusCode(queryResult) }, envelopesResult.responseSize);
        }
        catch (AmazonDynamoDBException) { return (new StatusCodeResult(500), 0); }
        catch (AmazonServiceException) { return (new StatusCodeResult(503), 0); }
        catch { return (new StatusCodeResult(500), 0); }
    }
    public virtual async Task<ActionResult<ICollection<T>>> ListAsync(QueryRequest queryRequest, bool? useCache = null, int limit = 0)
    {
        var (actionResult, _) = await ListAndSizeAsync(queryRequest, useCache, limit);
        return actionResult;
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
        value ??= "#Data, TypeName, #Status, UpdateUtcTick, CreateUtcTick, #General";
        return value;
    }

    public virtual QueryRequest QueryEquals(string pK, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null) 
        => QueryEquals(pK, expressionAttributeNames, projectionExpression, new CallerInfo() { Table = table});
    public virtual QueryRequest QueryEquals(string pK, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, ICallerInfo callerInfo = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;
        if (string.IsNullOrEmpty(table))
            table = tablename;

        expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
        projectionExpression = GetProjectionExpression(projectionExpression);

        var query = new QueryRequest()
        {
            TableName = table,
            KeyConditionExpression = $"PK = :PKval",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":PKval", new AttributeValue() {S = pK} },
                {":IsDeleted", new AttributeValue() {BOOL = false } } 
            },
            ExpressionAttributeNames = expressionAttributeNames,
            ProjectionExpression = projectionExpression
        };
        if(UseIsDeleted)
            query.FilterExpression = "IsDeleted = :IsDeleted";

        return query;

    }
    public virtual QueryRequest QueryEquals(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null)
        => QueryEquals(pK, keyField, key, expressionAttributeNames, projectionExpression, new CallerInfo() { Table = table });
    public virtual QueryRequest QueryEquals(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, ICallerInfo callerInfo = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;
        if (string.IsNullOrEmpty(table))
            table = tablename;

        expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
        projectionExpression = GetProjectionExpression(projectionExpression);

        var indexName = (string.IsNullOrEmpty(keyField) || keyField.Equals("SK")) ? null : $"PK-{keyField}-Index";

        var query =  new QueryRequest()
        {
            TableName = table,
            KeyConditionExpression = $"PK = :PKval and {keyField} = :SKval",
            IndexName = indexName,
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":PKval", new AttributeValue() {S = pK} },
                {":SKval", new AttributeValue() {S =  key } },
                {":IsDeleted", new AttributeValue() {BOOL = false } }
            },
            ExpressionAttributeNames = expressionAttributeNames,
            ProjectionExpression = projectionExpression
        };
        if (UseIsDeleted)
            query.FilterExpression = "IsDeleted = :IsDeleted";
        return query;
    }

    public virtual QueryRequest QueryBeginsWith(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null)
        => QueryBeginsWith(pK, keyField, key, expressionAttributeNames, projectionExpression, new CallerInfo() { Table = table });
    public virtual QueryRequest QueryBeginsWith(string pK, string keyField, string key, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, ICallerInfo callerInfo = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;
        if (string.IsNullOrEmpty(table))
            table = tablename;

        expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
        projectionExpression = GetProjectionExpression(projectionExpression);

        var indexName = (string.IsNullOrEmpty(keyField) || keyField.Equals("SK")) ? null : $"PK-{keyField}-Index";

        var query = new QueryRequest()
        {
            TableName = table,
            KeyConditionExpression = $"PK = :PKval and begins_with({keyField}, :SKval)",
            IndexName = indexName,
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":PKval", new AttributeValue() {S = pK} },
                {":SKval", new AttributeValue() {S =  key } },
                {":IsDeleted", new AttributeValue() {BOOL = false } }
            },
            ExpressionAttributeNames = expressionAttributeNames,
            ProjectionExpression = projectionExpression
        };
        if (UseIsDeleted)
            query.FilterExpression = "IsDeleted = :IsDeleted";
        return query;
    }

    public virtual QueryRequest QueryBeginsWith(string pK, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, string table = null)
        => QueryBeginsWith(pK, expressionAttributeNames, projectionExpression, new CallerInfo() { Table = table });
    public virtual QueryRequest QueryBeginsWith(string pK, Dictionary<string, string> expressionAttributeNames = null, string projectionExpression = null, ICallerInfo callerInfo = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;
        if (string.IsNullOrEmpty(table))
            table = tablename;

        expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
        projectionExpression = GetProjectionExpression(projectionExpression);

        var query = new QueryRequest()
        {
            TableName = table,
            KeyConditionExpression = $"PK = :PKval",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":PKval", new AttributeValue() {S = pK} },
                {":IsDeleted", new AttributeValue() {BOOL = false } }
            },
            ExpressionAttributeNames = expressionAttributeNames,
            ProjectionExpression = projectionExpression
        };
        if (UseIsDeleted)
            query.FilterExpression = "IsDeleted = :IsDeleted";
        return query;
    }

    public virtual QueryRequest QueryRange(
        string pK,
        string keyField,
        string keyStart,
        string keyEnd,
        Dictionary<string, string> expressionAttributeNames = null,
        string projectionExpression = null,
        string table = null)
        => QueryRange(pK, keyField, keyStart, keyEnd, expressionAttributeNames, projectionExpression, new CallerInfo() { Table = table });

    public virtual QueryRequest QueryRange(
        string pK,
        string keyField,
        string keyStart,
        string keyEnd,
        Dictionary<string, string> expressionAttributeNames = null,
        string projectionExpression = null,
        ICallerInfo callerInfo = null)
    {
        callerInfo ??= new CallerInfo();
        var table = callerInfo.Table;
        if (string.IsNullOrEmpty(table))
            table = tablename;

        expressionAttributeNames = GetExpressionAttributeNames(expressionAttributeNames);
        projectionExpression = GetProjectionExpression(projectionExpression);

        var indexName = (string.IsNullOrEmpty(keyField) || keyField.Equals("SK")) ? null : $"PK-{keyField}-Index";

        var query = new QueryRequest()
        {
            TableName = table,
            KeyConditionExpression = $"PK = :PKval and {keyField} between :SKStart and :SKEnd",
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
        return query;
    }
    protected bool IsResultOk(IActionResult actionResult)
    {
        if(actionResult is StatusCodeResult statusCodeResult)
        {
            int statusCode = statusCodeResult.StatusCode;
            if (statusCode >= 200 && statusCode < 300)
                return true;
        }
        return false;
    }
    protected int GetStatusCode(IActionResult actionResult)
    {
        if (actionResult is StatusCodeResult statusCodeResult)
            return statusCodeResult.StatusCode;
        return 500;
    }
}
