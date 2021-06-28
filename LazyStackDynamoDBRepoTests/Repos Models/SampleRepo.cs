using System.Collections.Generic;
using LazyStackDynamoDBRepo;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace LazyStackDynamoDBRepoTests
{
    public class SampleEnvelope : DataEnvelope<Sample>
    {
        /// Called in EntityInstance set Set
        protected override void SetDbRecordFromEnvelopeInstance()
        {
            // Set the Envelope Key fields from the EntityInstance data
            TypeName = "Sample.v1.0.0";
            // Primary Key is PartitionKey + SortKey 
            PK = "Samples:"; // Partition key
            SK = $"Sample:{EntityInstance.Id}"; // sort/range key

            // The base method copies information from the envelope keys into the dbRecord
            base.SetDbRecordFromEnvelopeInstance();
        }
    }

    public interface ISampleRepo : IDYDBRepository<SampleEnvelope, Sample>
    {
        Task<StatusCodeResult> ClearSamplesAsync(); // Clear table
        Task<StatusCodeResult> SeedSampleAsync(); //Create sample records
        Task<ActionResult<Sample>> GetSampleByIdAsync(long sampleId); //Read
        Task<ActionResult<Sample>> PutSampleByIdAsync(Sample sample); //Update
        Task<StatusCodeResult> DeleteSampleByIdAsync(long sampleId); //Delete
        Task<ActionResult<List<Sample>>> ListSamplesAsync(); //List
        Task<ActionResult<List<SampleEnvelope>>> ListSampleEnvelopesAsync(); 
    }

    public class SampleRepo : DYDBRepository<SampleEnvelope, Sample>, ISampleRepo
    {
        public SampleRepo(
            IAmazonDynamoDB client
            ) : base(client, envVarTableName: "TABLE_NAME") {}

        // This dictionary allows us to use class attribute names that happen to also be
        // DynamoDB reserved words in our Qury and ProjectionExpressions
        Dictionary<string, string> _ExpressionAttributeNames = new Dictionary<string, string>()
        {
            {"#Data", "Data" },
            {"#Status", "Status" },
            {"#General", "General" }
        };

        public async Task<StatusCodeResult> ClearSamplesAsync()
        {
            var response = await ListSampleEnvelopesAsync();

            if (response.Value == null) // Something went wrong! Return error 
                return response.Result as StatusCodeResult;
            
            // Delete each existing record
            foreach (var r in response.Value)
            {
                var delResponse = await DeleteAsync(r.PK, r.SK);
                if (delResponse.StatusCode != 200)
                    return delResponse; // Error encountered during delete
            }
            return new OkResult();
        }


        public async Task<StatusCodeResult> SeedSampleAsync()
        {
            // Create a few records in database 
            var response = await CreateAsync(new Sample() { Id = 1, Category = "bovine", Name = "Bonnie" });
            if (response.Value == null) return response.Result as StatusCodeResult;

            response = await CreateAsync(new Sample() { Id = 2, Category = "bovine", Name = "Ralph" });
            if (response.Value == null) return response.Result as StatusCodeResult;

            response = await CreateAsync(new Sample() { Id = 3, Category = "bovine", Name = "Fred" });
            if (response.Value == null) return response.Result as StatusCodeResult;

            response = await CreateAsync(new Sample() { Id = 4, Category = "bovine", Name = "Freckles" });
            if (response.Value == null) return response.Result as StatusCodeResult;

            response = await CreateAsync(new Sample() { Id = 5, Category = "sheep", Name = "Sandy" });
            if (response.Value == null) return response.Result as StatusCodeResult;

            response = await CreateAsync(new Sample() { Id = 6, Category = "sheep", Name = "Bonnie" });
            if (response.Value == null) return response.Result as StatusCodeResult;

            response = await CreateAsync(new Sample() { Id = 7, Category = "sheep", Name = "Crazy" });
            if (response.Value == null) return response.Result as StatusCodeResult;

            return new OkResult();
        }

        public async Task<ActionResult<Sample>> GetSampleByIdAsync(long orderId)
        {
            return await ReadAsync("Samples:", "Sample:" + orderId.ToString());
        }

        public async Task<ActionResult<Sample>> PutSampleByIdAsync(Sample sample)
        {
            return await UpdateAsync(sample);
        }

        public async Task<StatusCodeResult> DeleteSampleByIdAsync(long orderId)
        {
            return await DeleteAsync("Samples:", "Sample:" + orderId.ToString());
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
                ProjectionExpression = "PK, SK, #Data, TypeName, #Status, UpdateUtcTick, CreateUtcTick, #General"
            };
            return await ListAsync(queryRequest);
        }

        public async Task<ActionResult<List<SampleEnvelope>>> ListSampleEnvelopesAsync()
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
                ProjectionExpression = "PK, SK, #Data, TypeName, #Status, UpdateUtcTick, CreateUtcTick, #General"
            };
            return await ListEAsync(queryRequest);
        }

    }
}