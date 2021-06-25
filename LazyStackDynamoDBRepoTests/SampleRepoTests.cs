using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
//using LazyStackAuth;





namespace LazyStackDynamoDBRepoTests
{
    //Envelope Lib > Envelope derived class
    //-Acts as wapper for the object payload passed
    //-Includes some metadata
    //-Envalope type and payload type defined via generics in classes using Envalopes

    //Repo Lib > Repo derived class
    //-Uses methods in parent to perform calls to DynamoDB
    //-Class instance controlled by dependency injection, here registered as singleton

    //DynamoDB 
    //instance must be deployed prior to run, can use serverless.template in this folder
    //deploys from/to aws acc specified in AWS extension dialogue, likely .aws default prof



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
        public async Task Create()
        {
            var sampleRepo = serviceProvider.GetService<ISampleRepo>();
            var Cow = new Sample()
            {
                Id = 1,
                Category = "Bovine",
                Name = "Cow"
            };

            var response = await sampleRepo.SeedSampleAsync(Cow);
            var value = (response.Result as ObjectResult)?.Value as Sample;
            Console.WriteLine($"{value.ToString()}");

        }
        [TestMethod]
        public async Task Read()
        {
            var sampleRepo = serviceProvider.GetService<ISampleRepo>();
            long Id = 1;
            var response = await sampleRepo.GetSampleByIdAsync(Id);
            var value = (response.Result as ObjectResult)?.Value as Sample;
            Console.WriteLine($"{value.ToString()}");
        }
        [TestMethod]
        public void Update()
        {
            var sampleRepo = serviceProvider.GetService<ISampleRepo>();
        }
        [TestMethod]
        public void Delete()
        {
            var sampleRepo = serviceProvider.GetService<ISampleRepo>();
        }
        [TestMethod]
        public void List()
        {
            var sampleRepo = serviceProvider.GetService<ISampleRepo>();
        }
    }
}
