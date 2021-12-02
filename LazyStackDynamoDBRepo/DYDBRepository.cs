using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
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
            tablename = Environment.GetEnvironmentVariable(envVarTableName);
        }

        #region Fields
        protected string tablename;
        protected IAmazonDynamoDB client;
        #endregion

        #region Properties 
        private Boolean _UpdateReturnOkResults = true;
        public Boolean UpdateReturnsOkResult { 
            get { return _UpdateReturnOkResults;  } 
            set { _UpdateReturnOkResults = value; }
        }
        #endregion
        public async Task<ActionResult<T>> CreateAsync(T data)
        {
            try
            {
                var now = DateTime.UtcNow.Ticks;
                TEnv envelope = new TEnv() 
                { 
                    EntityInstance = data,
                    CreateUtcTick = now,
                    UpdateUtcTick = now
                };

                // Wait until just before write to serialize EntityInstance (captures updates to UtcTick fields)
                envelope.DbRecord.Add("Data", new AttributeValue() { S = JsonConvert.SerializeObject(envelope.EntityInstance)});

                var request = new PutItemRequest()
                {
                    TableName = tablename,
                    Item = envelope.DbRecord,
                    ConditionExpression = "attribute_not_exists(PK)" // Technique to avoid replacing an existing record
                };

                await client.PutItemAsync(request);
                return data; 
            }
            catch (ConditionalCheckFailedException ex) { return new ConflictResult(); }
            catch (AmazonDynamoDBException ex) { return new StatusCodeResult(400); }
            catch (AmazonServiceException) { return new StatusCodeResult(500); }
            catch { return new StatusCodeResult(500); }
        }

        public async Task<ActionResult<T>> ReadAsync(string pK,  string sK = null)
        {
            try
            {
                var response = await ReadEAsync(pK, sK);
                if (response.Value == null)
                    return response.Result;
                return response.Value.EntityInstance;
            }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(406); }
        }

        public async Task<ActionResult<TEnv>> ReadEAsync(string pK, string sK = null)
        {
            try
            {
                var request = new GetItemRequest()
                {
                    TableName = tablename,
                    Key = new Dictionary<string, AttributeValue>()
                    { 
                        {"PK", new AttributeValue {S = pK}},
                        {"SK", new AttributeValue {S = sK } }
                    }
                };
                var response = await client.GetItemAsync(request);
                return new TEnv() { DbRecord = response.Item };
            }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(406); }
        }

        public async Task<ActionResult<T>> UpdateAsync(T data)
        {
            if (data.Equals(null))
                return new StatusCodeResult(400);

            TEnv envelope = new TEnv() { EntityInstance = data };

            try
            {
                var OldUpdateUtcTick = envelope.UpdateUtcTick;
                var now = DateTime.UtcNow.Ticks;
                envelope.UpdateUtcTick = now; // The UpdateUtcTick Set calls SetUpdateUtcTick where you can update your entity data record 

                // Waiting until just before write to serialize EntityInstance (captures updates to UtcTick fields)
                envelope.DbRecord.Add("Data", new AttributeValue() { S = JsonConvert.SerializeObject(envelope.EntityInstance) });

                // Write data to database - use conditional put to avoid overwriting newer data
                var request = new PutItemRequest()
                {
                    TableName = tablename,
                    Item = envelope.DbRecord,
                    ConditionExpression = "UpdateUtcTick = :OldUpdateUtcTick",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":OldUpdateUtcTick", new AttributeValue() {N = OldUpdateUtcTick.ToString()} }
                    }
                };
                await client.PutItemAsync(request);
                if (UpdateReturnsOkResult)
                    return new OkObjectResult(envelope.EntityInstance);
                else
                    return envelope.EntityInstance;
            }
            catch (ConditionalCheckFailedException) { return new ConflictResult(); }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(500); }
        }

        public async Task<StatusCodeResult> DeleteAsync(string pK, string sK = null)
        {
            try
            {
                if (string.IsNullOrEmpty(pK))
                    return new StatusCodeResult(406); // bad key

                var request = new DeleteItemRequest()
                {
                    TableName = tablename,
                    Key = new Dictionary<string, AttributeValue>()
                    {
                        {"PK", new AttributeValue {S= pK} },
                        {"SK", new AttributeValue {S = sK} }
                    }
                };
                
                await client.DeleteItemAsync(request);
                return new OkResult();
            }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(406); }
        }

        public async Task<ActionResult<ICollection<TEnv>>> ListEAsync(QueryRequest queryRequest)
        {
            try
            {
                var response = await client.QueryAsync(queryRequest);
                var list = new List<TEnv>();
                foreach(Dictionary<string, AttributeValue> item in response?.Items)
                    list.Add(new TEnv() { DbRecord = item });
                return list;
            }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(500); }
        }

        public async Task<ActionResult<ICollection<T>>> ListAsync(QueryRequest queryRequest)
        {
            try
            {
                var response = await client.QueryAsync(queryRequest);
                var list = new List<T>();
                foreach (Dictionary<string, AttributeValue> item in response?.Items)
                    list.Add(new TEnv() { DbRecord = item }.EntityInstance);
                return list;
            }
            catch (AmazonDynamoDBException) { return new StatusCodeResult(500); }
            catch (AmazonServiceException) { return new StatusCodeResult(503); }
            catch { return new StatusCodeResult(500); }
        }
    }
}
