using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


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
         
        public async Task<IActionResult> CreateAsync(T data)
        {
            try
            {
                TEnv envelope = new TEnv() { EntityInstance = data };
                var request = new PutItemRequest()
                {
                    TableName = tablename,
                    Item = envelope.DbRecord
                };
                await client.PutItemAsync(request);
                return new ObjectResult(data) { DeclaredType = typeof(T) };
            }
            catch (AmazonDynamoDBException)
            {
                return new StatusCodeResult(400);
            }
            catch (AmazonServiceException)
            {
                return new StatusCodeResult(500);
            }
            catch
            {
                return new StatusCodeResult(500);
            }
        }

        // PKval does not include the PKPrefix
        public async Task<IActionResult> ReadAsync(string pKPrefix, string pKval)
        {
            return await ReadAsync(pKPrefix, pKval, sKPrefix:null, sKval: null);
        }

        // PKval does not include the PKPrefix
        // SKval does not include the SKPrefix
        public async Task<IActionResult> ReadAsync(string pKPrefix, string pKval, string sKPrefix, string sKval)
        {
            string pK = pKPrefix + pKval;
            string sK = (sKPrefix != null) ? sKPrefix + sKval : null;

            try
            {
                TEnv envelope = new TEnv();
                envelope = (sK == null)
                    ? envelope = await ReadEAsync(pK)
                    : await ReadEAsync(pK, sK);

                if (envelope == null)
                    return new StatusCodeResult((int)DBTransError.KeyNotFound);

                return new ObjectResult(envelope.EntityInstance);

            }
            catch (AmazonDynamoDBException)
            {
                return new StatusCodeResult((int)DBTransError.DBError);
            }
            catch (AmazonServiceException)
            {
                return new StatusCodeResult((int)DBTransError.RemoteServiceDown);
            }
            catch
            {
                return new StatusCodeResult(500);
            }

        }

        protected async Task<TEnv> ReadEAsync(string pK, string sK)
        {
            try
            {
                var request = new GetItemRequest()
                {
                    TableName = tablename,
                    Key = new Dictionary<string, AttributeValue>()
                    { {"PK", new AttributeValue {S = pK}},
                      {"SK", new AttributeValue {S = sK } }
                    }
                };
                var response = await client.GetItemAsync(request);
                return new TEnv() { DbRecord = response.Item };
            }
            catch (Exception e)
            {
                return default;
            }
        }

        protected async Task<TEnv> ReadEAsync(string pK)
        {
            try
            {
                var request = new GetItemRequest()
                {
                    TableName = tablename,
                    Key = new Dictionary<string, AttributeValue>()
                    { 
                        {"PK", new AttributeValue {S = pK}}
                    }
                };
                var response = await client.GetItemAsync(request);
                return new TEnv() { DbRecord = response.Item };
            }
            catch
            {
                return default;
            }
        }


        public async Task<IActionResult> UpdateAsync(T data)
        {
            if (data.Equals(null))
                return new StatusCodeResult(400);

            TEnv envelope = new TEnv() { EntityInstance = data };

            try
            {
                var dbEnvelope = new TEnv(); // this will hold existing disk verion of the item
                // If the entity has an internal UpdateTick then the envelope will have a non-zero
                // UpdateTick value

                if (envelope.UpdateUtcTick != 0) // Perform optimistic lock processing
                {
                    // Read existing item from disk
                    dbEnvelope = await ReadEAsync(envelope.PK, envelope.SK);

                    if (dbEnvelope == null) // Darn, could not find the record!
                        return new StatusCodeResult((int)DBTransError.KeyNotFound);

                    if (dbEnvelope.UpdateUtcTick != envelope.UpdateUtcTick) // Darn, they don't match!
                    {
                        return new StatusCodeResult((int)DBTransError.NewerLastUpdateFound);
                    }
                }

                // Ok to update envelope and item if we got this far
                var now = DateTime.UtcNow;
                envelope.UpdateUtcTick = now.Ticks;

                // Write data to database
                var request = new PutItemRequest()
                {
                    TableName = tablename,
                    Item = envelope.DbRecord
                };

                await client.PutItemAsync(request);

                return new OkObjectResult(envelope.DbRecord);
            }
            catch (AmazonDynamoDBException)
            {
                return new StatusCodeResult((int)DBTransError.DBError);
            }
            catch (AmazonServiceException)
            {
                return new StatusCodeResult((int)DBTransError.RemoteServiceDown);
            }
            catch
            {
                return new StatusCodeResult(500);
            }
        }


        // PKval does not include PKPrefix
        public async Task<IActionResult> DeleteAsync(string pKPrefix, string pKval)
        {
            string PK = pKPrefix + pKval;

            try
            {
                if (string.IsNullOrEmpty(pKval))
                    return new StatusCodeResult((int)DBTransError.BadKey);

                var request = new DeleteItemRequest()
                {
                    TableName = tablename,
                    Key = new Dictionary<string, AttributeValue>()
                    {
                        {"PK", new AttributeValue {S= PK} }
                    }
                };
                var response = await client.DeleteItemAsync(request);

                return new OkResult();

            }
            catch (AmazonDynamoDBException)
            {
                return new StatusCodeResult((int)DBTransError.DBError);
            }
            catch (AmazonServiceException)
            {
                return new StatusCodeResult((int)DBTransError.RemoteServiceDown);
            }
            catch
            {
                return new StatusCodeResult(500);
            }

        }

        // PKval does not include PKPrefix
        // SKval does not icnlude SKPrefix
        // todo - can we recognize and handle a Key Missing condition?
        // todo - should we check if the record being deleted has the same lastupdatetick?
        // todo - this would imply we need to pass in the entity! Too much overhead?
        public async Task<IActionResult> DeleteAsync(string pKPrefix, string pKval, string sKPrefix, string sKval)
        {
            string PK = pKPrefix + pKval;
            string SK = (sKPrefix == null) ? null : sKPrefix + sKval;
            try
            {
                if (string.IsNullOrEmpty(PK) || string.IsNullOrEmpty(sKval))
                    return new StatusCodeResult((int) DBTransError.BadKey);

                var request = new DeleteItemRequest()
                {
                    TableName = tablename,
                    Key = new Dictionary<string, AttributeValue>()
                    {
                        {"PK", new AttributeValue {S= PK} },
                        {"SK", new AttributeValue {S = SK} }
                    }
                };
                var response = await client.DeleteItemAsync(request);

                return new OkResult();

            }
            catch (AmazonDynamoDBException)
            {
                return new StatusCodeResult((int)DBTransError.DBError);
            }
            catch (AmazonServiceException)
            {
                return new StatusCodeResult((int)DBTransError.RemoteServiceDown);
            }
            catch
            {
                return new StatusCodeResult(500);
            }
        }

        public async Task<ICollection<TEnv>> ListEAsync(QueryRequest queryRequest)
        {
            try
            {
                var response = await client.QueryAsync(queryRequest);
                var list = new List<TEnv>();
                foreach(Dictionary<string, AttributeValue> item in response.Items)
                    list.Add(new TEnv() { DbRecord = item });
                return list;
            }
            catch
            {
                return default(List<TEnv>);
            }
        }

        public async Task<IActionResult> ListAsync(QueryRequest queryRequest)
        {
            try
            {
                var response = await client.QueryAsync(queryRequest);
                var list = new List<T>();
                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    list.Add(new TEnv() { DbRecord = item }.EntityInstance);
                }
                return new OkObjectResult(list);
            }
            catch (AmazonDynamoDBException e)
            {
                Debug.WriteLine($"QueryAsync Error: {e.Message}");
                return new StatusCodeResult((int)DBTransError.DBError);
            }
            catch (AmazonServiceException)
            {
                return new StatusCodeResult((int)DBTransError.RemoteServiceDown);
            }
            catch
            {
                return new StatusCodeResult(500);
            }
        }

    }
}
