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
    /// <typeparam name="THelper"></typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract
        class DYDBRepository<TEnv, THelper, T> : IDYDBRepository<TEnv, THelper, T>
              where TEnv : class, IDYDBEnvelope, new()
              where THelper : IEntityHelper<T, TEnv>, new()
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

        public Dictionary<string, AttributeValue> ToDocument(TEnv item)
        {
            var result = new Dictionary<string, AttributeValue>
            {
                { "Data", new AttributeValue() { S = item.Data } },
                { "TypeName", new AttributeValue() { S = item.TypeName } },
                { "PK", new AttributeValue() { S = item.PK } },
                { "SK", new AttributeValue() { S = item.SK } },
                { "CreateUtcTick", new AttributeValue { N = item.CreateUtcTick.ToString() } },
                { "UpdateUtcTick", new AttributeValue { N = item.UpdateUtcTick.ToString() } }
            };


            if (!string.IsNullOrEmpty(item.SK1))
                result.Add("SK1", new AttributeValue() { S = item.SK1 });

            if (!string.IsNullOrEmpty(item.SK2))
                result.Add("SK2", new AttributeValue() { S = item.SK2 });

            if (!string.IsNullOrEmpty(item.SK3))
                result.Add("SK3", new AttributeValue() { S = item.SK3 });

            if (!string.IsNullOrEmpty(item.SK4))
                result.Add("SK4", new AttributeValue() { S = item.SK4 });

            if (!string.IsNullOrEmpty(item.SK5))
                result.Add("SK5", new AttributeValue() { S = item.SK5 });

            if (!string.IsNullOrEmpty(item.GSI1PK))
                result.Add("GSI1PK", new AttributeValue() { S = item.GSI1PK });

            if (!string.IsNullOrEmpty(item.GSI1SK))
                result.Add("GSI1SK", new AttributeValue() { S = item.GSI1SK });

            if (!string.IsNullOrEmpty(item.Status))
                result.Add("Status", new AttributeValue() { S = item.Status });

            if (!string.IsNullOrEmpty(item.General))
                result.Add("General", new AttributeValue() { S = item.General });

            return result;
        }

        public TEnv ToEnv(Dictionary<string,AttributeValue> item)
        {
            var env = new TEnv();
            if(item.TryGetValue("PK", out AttributeValue pk))
                env.PK = pk.S;

            if(item.TryGetValue("Data", out AttributeValue data))
                env.Data = data.S;

            if(item.TryGetValue("TypeName", out AttributeValue typeName))
                env.TypeName = typeName.S;

            if(item.TryGetValue("SK", out AttributeValue sk))
                env.SK = sk.S;

            if(item.TryGetValue("CreateUtcTick", out AttributeValue createUtcTick))
                env.CreateUtcTick = long.Parse(createUtcTick.N);

            if (item.TryGetValue("UpdateUtcTick", out AttributeValue updateUtcTick))
                env.UpdateUtcTick = long.Parse(updateUtcTick.N);

            if (item.TryGetValue("SK1", out AttributeValue sk1))
                env.SK1 = sk1.S;

            if (item.TryGetValue("SK2", out AttributeValue sk2))
                env.SK2 = sk2.S;

            if (item.TryGetValue("SK3", out AttributeValue sk3))
                env.SK3 = sk3.S;

            if (item.TryGetValue("SK4", out AttributeValue sk4))
                env.SK4 = sk4.S;

            if (item.TryGetValue("SK5", out AttributeValue sk5))
                env.SK5 = sk5.S;

            if (item.TryGetValue("GSI1PK", out AttributeValue gsi1pk))
                env.GSI1PK = gsi1pk.S;

            if (item.TryGetValue("GSI1SK", out AttributeValue gsi1sk))
                env.GSI1SK = gsi1sk.S;

            if (item.TryGetValue("Status", out AttributeValue status))
                env.Status = status.S;

            if (item.TryGetValue("General", out AttributeValue general))
                env.General = general.S;

            return env;
        }

         
        public async Task<IActionResult> CreateAsync(T data)
        {
            var helper = new THelper() { Instance = data };
            var now = DateTime.UtcNow;
            helper.SetCreateUtcTick(now.Ticks);
            helper.SetUpdateUtcTick(now.Ticks);
            TEnv envelope = new TEnv();
            helper.UpdateEnvelope(envelope, null, serialize: true); // Update evenlope attributes from entity data

            try
            {
                var item = ToDocument(envelope);

                var request = new PutItemRequest()
                {
                    TableName = tablename,
                    Item = item
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
                TEnv envelope = (sK == null)
                    ? envelope = await ReadEAsync(pK)
                    : await ReadEAsync(pK, sK);

                if (envelope == null)
                    return new StatusCodeResult((int)DBTransError.KeyNotFound);

                var Data = new T();
                var helper = new THelper() { Instance = Data };
                Data = helper.Deserialize(envelope);
                return new ObjectResult(Data);

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
                return ToEnv(response.Item);
            }
            catch
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
                    { {"PK", new AttributeValue {S = pK}}
                    }
                };
                var response = await client.GetItemAsync(request);
                var envelope = ToEnv(response.Item);
                return envelope;
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

            var helper = new THelper() { Instance = data };
            var newEnvelope = new TEnv();
            helper.UpdateEnvelope(newEnvelope); // Move data into newEnvelope from Instance

            try
            {
                var dbEnvelope = default(TEnv); // this will hold existing disk verion of the item
                // If the entity has an internal UpdateTick then the envelope will have a non-zero
                // UpdateTick value
                if (newEnvelope.UpdateUtcTick != 0) // Perform optimistic lock processing
                {
                    // Read existing item from disk
                    dbEnvelope = await ReadEAsync(newEnvelope.PK, newEnvelope.SK);

                    if (dbEnvelope == null) // Darn, could not find the record!
                        return new StatusCodeResult((int)DBTransError.KeyNotFound);

                    // Compare UpdateTicks and return ENTITY DATA LOADED FROM DB if they don't match
                    // This allows the client to compare new and old and take remedial action
                    if (dbEnvelope.UpdateUtcTick != newEnvelope.UpdateUtcTick) // Darn, they don't match!
                    {
                        helper.Instance = new T();
                        var dbData = helper.Deserialize(dbEnvelope);
                        return new StatusCodeResult((int)DBTransError.NewerLastUpdateFound);
                    }
                }

                // Ok to update envelope and item if we got this far
                var now = DateTime.UtcNow;
                helper.SetUpdateUtcTick(now.Ticks);
                helper.UpdateEnvelope(newEnvelope, dbEnvelope, true);
                // Write data to database

                var request = new PutItemRequest()
                {
                    TableName = tablename,
                    Item = ToDocument(newEnvelope)
                };

                await client.PutItemAsync(request);

                return new OkObjectResult(helper.Instance);
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
                var helper = new THelper();
                var list = new List<TEnv>();
                foreach(Dictionary<string, AttributeValue> item in response.Items)
                    list.Add(ToEnv(item));
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
                var helper = new THelper();
                var response = await client.QueryAsync(queryRequest);
                var list = new List<T>();
                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    var envelope = ToEnv(item);
                    T instance = helper.Deserialize(envelope);
                    list.Add(instance);
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
