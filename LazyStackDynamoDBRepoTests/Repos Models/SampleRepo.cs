using System;
using System.Collections.Generic;
using System.Text;
using LazyStackDynamoDBRepo;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace LazyStackDynamoDBRepoTests
{
    public class SampleEnvelope : DataEnvelope<Sample>
    {
        protected override void SetDbRecordFromEntityInstance()
        {
            // Set the Envelope Key fields from the EntityInstance data
            TypeName = "Sample.v1.0.0";
            CreateUtcTick = EntityInstance.CreateUtcTick;
            UpdateUtcTick = EntityInstance.UpdateUtcTick;
            // Primary Key is PartitionKey + SortKey 
            PK = "Samples:"; // Partition key
            SK = $"Sample:{EntityInstance.Id}"; // sort/range key

            // The base method copies information from the envelope keys into the dbRecord
            base.SetDbRecordFromEntityInstance();
        }

        protected override void SetEntityInstanceFromDbRecord()
        {
            base.SetEntityInstanceFromDbRecord();
            EntityInstance.CreateUtcTick = CreateUtcTick;
            EntityInstance.UpdateUtcTick = UpdateUtcTick;
        }
    }

    public interface ISampleRepo : IDYDBRepository<SampleEnvelope, Sample>
    {
        Task<ActionResult<Sample>> SeedSampleAsync(Sample sample); //Create
        Task<ActionResult<Sample>> GetSampleByIdAsync(long sampleId); //Read
        Task<ActionResult<Sample>> PutSampleByIdAsync(Sample sample); //Update
        Task<IActionResult> DeleteSampleByIdAsync(long sampleId); //Delete
        Task<ActionResult<List<Sample>>> ListSamplesAsync(); //List
    }
    public class SampleRepo : DYDBRepository<SampleEnvelope, Sample>, ISampleRepo
    {
        public SampleRepo(
            IAmazonDynamoDB client
            ) : base(client, envVarTableName: "TABLE_NAME")
        {
        }
        
 

        // This dictionary allows us to use class attribute names that happen to also be
        // DynamoDB reserved words in our ProjectionExpressions
        Dictionary<string, string> _ExpressionAttributeNames = new Dictionary<string, string>()
        {
            {"#Data", "Data" },
            {"#Status", "Status" },
            {"#General", "General" }
        };

        public async Task<ActionResult<Sample>> SeedSampleAsync(Sample sample)
        {
            return await CreateAsync(sample);
        }

        public async Task<ActionResult<Sample>> GetSampleByIdAsync(long orderId)
        {
            return await ReadAsync("Sample:", orderId.ToString());
        }

        public async Task<ActionResult<Sample>> PutSampleByIdAsync(Sample sample)
        {
            return await UpdateAsync(sample);
        }

        public async Task<IActionResult> DeleteSampleByIdAsync(long orderId)
        {
            return await DeleteAsync("Sample:", orderId.ToString());
        }

        public async Task<ActionResult<List<Sample>>> ListSamplesAsync()
        {
            var queryRequest = new QueryRequest()
            {
                TableName = tablename,
                KeyConditionExpression = "PK = :PKval AND begins_with(SK, :SKval)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            {":PKval", new AttributeValue() {S = "Samples:"} },
                            {":SKval", new AttributeValue() {S = "Sample:"} }
                        },
                ExpressionAttributeNames = _ExpressionAttributeNames,
                ProjectionExpression = "#Data, TypeName, #Status, UpdateUtcTick, CreateUtcTick, #General"
            };
            //return await ListAsync(queryRequest);
            return new NoContentResult();
        }
    }
    
}