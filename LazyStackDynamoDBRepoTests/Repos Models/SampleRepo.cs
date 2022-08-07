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
        public override void SealEnvelope()
        {
            // Set the Envelope Key fields from the EntityInstance data
            TypeName = "Sample.v1.0.0";
            // Primary Key is PartitionKey + SortKey 
            PK = "Sample:"; // Partition key
            SK = $"{EntityInstance.Id}:"; // sort/range key

            // The base method copies information from the envelope keys into the dbRecord
            base.SealEnvelope();
        }
    }

    public interface ISampleRepo : IDYDBRepository<SampleEnvelope, Sample>
    {
        Task<StatusCodeResult> ClearSamplesAsync(); // Clear table
        Task<StatusCodeResult> SeedSampleAsync(); //Create sample records
        Task<ActionResult<Sample>> GetSampleByIdAsync(long sampleId); //Read
        Task<ActionResult<Sample>> PutSampleByIdAsync(Sample sample); //Update
        Task<StatusCodeResult> DeleteSampleByIdAsync(long sampleId); //Delete
        Task<ActionResult<ICollection<Sample>>> ListSamplesAsync(); //List
        Task<ActionResult<ICollection<SampleEnvelope>>> ListSampleEnvelopesAsync();
    }

    public class SampleRepo : DYDBRepository<SampleEnvelope, Sample>, ISampleRepo
    {
        public SampleRepo(
            IAmazonDynamoDB client
            ) : base(client, envVarTableName: "TABLE_NAME")
        {
            UpdateReturnsOkResult = false; // just return value
        }

        const string PK = "Sample:";

        public async Task<StatusCodeResult> ClearSamplesAsync()
        {
            // This example shows how to retrieve only the fields you really need/want from the table record. In this case, all we need 
            // is the PK and SK fields. When you call ListEAsync you need to be very specific about what you want. You need to be doing something 
            // special, and low level, to really need to use ListEAsync. You could just as easily have used ListAsync and extracted the SK 
            // value from the corresponding entity.Id field. This use case, of deleting a bunch of records, is a good use case to consider. For instance,
            // what if each of these records stored a large entity? Then using this approach would dramatically reduce the amount of data returned for
            // processing.
            var response = await ListEAsync(QueryBeginsWith(PK, expressionAttributeNames: new Dictionary<string, string>(), projectionExpression: "PK, SK"));

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
            => await ReadAsync("Sample:", orderId.ToString());

        public async Task<ActionResult<Sample>> PutSampleByIdAsync(Sample sample)
            => await UpdateAsync(sample);

        public async Task<StatusCodeResult> DeleteSampleByIdAsync(long orderId)
            => await DeleteAsync("Sample:", orderId.ToString());

        public async Task<ActionResult<ICollection<Sample>>> ListSamplesAsync()
            => await ListAsync(QueryBeginsWith(PK));

        public async Task<ActionResult<ICollection<SampleEnvelope>>> ListSampleEnvelopesAsync()
            => await ListEAsync(QueryBeginsWith(PK));

    }
}