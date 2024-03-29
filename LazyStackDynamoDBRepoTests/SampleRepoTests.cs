using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace LazyStackDynamoDBRepoTests
{
     [TestClass]
    public class SampleRepoTests
    {
        IServiceCollection services = new ServiceCollection();
        IServiceProvider serviceProvider;

        public SampleRepoTests()
        {
            IConfiguration appConfig = new ConfigurationBuilder().Build();
            services.AddDefaultAWSOptions(appConfig.GetAWSOptions());
            services.AddAWSService<Amazon.DynamoDBv2.IAmazonDynamoDB>();
            services.AddSingleton<ISampleRepo, SampleRepo>();
            serviceProvider = services.BuildServiceProvider();
            Environment.SetEnvironmentVariable("TABLE_NAME", "TestRepo-DB");
        }

        [TestMethod]
        public async Task TestSample()
        {
            // SampleRepo is a simple table
            // PK = "Samples:" // Partition Key
            // SK = "Sample:" + Id.ToString(); // Sort Key
            // Primary key = PK + SK
            // Properties:
            //  int Id 
            //  string Category
            //  string Name
            //  long CreateUtcTick
            //  long UpdateUtcTick

            var sampleRepo = serviceProvider.GetRequiredService<ISampleRepo>();

            // Clear existing records
            var response = await sampleRepo.ClearSamplesAsync();
            Assert.IsTrue(response is OkResult, response.ToString());

            // Add some records
            response = await sampleRepo.SeedSampleAsync();
            Assert.IsTrue(response is OkResult, response.ToString());

            // Try to create an existing record - should fail with ConflictResult
            var createResponse = await sampleRepo.CreateAsync(new Sample() { Id = 1, Category = "bovine", Name = "Bonnie" });
            Assert.IsTrue(createResponse.Result is ConflictResult, "Opps, allowed create of existing item");

            // Update a record 
            var sampleResponse = await sampleRepo.ReadAsync("Sample:", "1:");
            Assert.IsNotNull(sampleResponse.Value, sampleResponse.ToString());
            var sample = sampleResponse.Value;
            sample.Name = "Randy";
            var updateUtcTick = sample.UpdateUtcTick;
            sample = (await sampleRepo.UpdateAsync(sample)).Value;
            Assert.IsTrue(sample != null); 
            Assert.IsTrue(updateUtcTick < sample.UpdateUtcTick, "UpdateUtcTick violation");
            Assert.IsTrue(sample.Name.Equals("Randy"), "Update failed. Name does not match.");

            // Test Optimistic Lock 
            // Read the record twice
            var sample1 = (await sampleRepo.ReadAsync("Sample:", "1:")).Value;
            Debug.WriteLine($"Sample1.UpdateUtcTick={sample1.UpdateUtcTick}");

            var sample2 = (await sampleRepo.ReadAsync("Sample:", "1:")).Value;
            Debug.WriteLine($"Sample2.UpdateUtcTick={sample2.UpdateUtcTick}");

            // Update first record instance
            var sample3 = (await sampleRepo.UpdateAsync(sample1)).Value;
            Debug.WriteLine($"Sample3.UpdateUtcTick={sample3.UpdateUtcTick}");

            // Try and update second record - should fail with ConflictResult
            var sampleResponse4 = await sampleRepo.UpdateAsync(sample2);
            Assert.IsTrue(sampleResponse4.Result is ConflictResult, "Didn't get ConflictResult");

            // Test Delete
            var deleteReponse = await sampleRepo.DeleteSampleByIdAsync(2);
            Assert.IsTrue(deleteReponse is OkResult, "Delete failed");
        }
        [TestMethod]
        public async Task TestSample2()
        {
            // SampleRepo is a simple table
            // PK = "Samples:" // Partition Key
            // SK = "Sample:" + Id.ToString(); // Sort Key
            // Primary key = PK + SK
            // Properties:
            //  int Id 
            //  string Category
            //  string Name
            //  long CreateUtcTick
            //  long UpdateUtcTick

            var sampleRepo = serviceProvider.GetRequiredService<ISampleRepo>();
            sampleRepo.UpdateReturnsOkResult = false; // just return value

            // Clear existing records
            var response = await sampleRepo.ClearSamplesAsync();
            Assert.IsTrue(response is OkResult, response.ToString());

            // Add some records
            response = await sampleRepo.SeedSampleAsync();
            Assert.IsTrue(response is OkResult, response.ToString());

            // Try to create an existing record - should fail with ConflictResult
            var createResponse = await sampleRepo.CreateAsync(new Sample() { Id = 1, Category = "bovine", Name = "Bonnie" });
            Assert.IsTrue(createResponse.Result is ConflictResult, "Opps, allowed create of existing item");

            // Update a record 
            var sample = (await sampleRepo.ReadAsync("Sample:", "1:")).Value;
            sample.Name = "Randy";
            var updateUtcTick = sample.UpdateUtcTick;
            sample = (await sampleRepo.UpdateAsync(sample)).Value;
            Assert.IsTrue(sample != null);
            Assert.IsTrue(updateUtcTick < sample.UpdateUtcTick, "UpdateUtcTick violation");
            Assert.IsTrue(sample.Name.Equals("Randy"), "Update failed. Name does not match.");

            // Test Optimistic Lock 
            // Read the record twice
            var sample1 = (await sampleRepo.ReadAsync("Sample:", "1:")).Value;
            Debug.WriteLine($"Sample1.UpdateUtcTick={sample1.UpdateUtcTick}");

            var sample2 = (await sampleRepo.ReadAsync("Sample:", "1:")).Value;
            Debug.WriteLine($"Sample2.UpdateUtcTick={sample2.UpdateUtcTick}");

            // Update first record instance
            var sample3 = (await sampleRepo.UpdateAsync(sample1)).Value;
            Assert.IsTrue(sample3 != null);
            Debug.WriteLine($"Sample3.UpdateUtcTick={sample3.UpdateUtcTick}");

            // Try and update second record - should fail with ConflictResult
            var sampleResponse4 = await sampleRepo.UpdateAsync(sample2);
            Assert.IsTrue(sampleResponse4.Result is ConflictResult, "Didn't get ConflictResult");

            // Test Delete
            var deleteReponse = await sampleRepo.DeleteSampleByIdAsync(2);
            Assert.IsTrue(deleteReponse is OkResult, "Delete failed");
        }
    }
}
