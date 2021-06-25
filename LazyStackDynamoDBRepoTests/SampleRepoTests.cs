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

        public void PrettyPrint(Sample sample)
        {
            Console.WriteLine("\n");
            Console.WriteLine("DEBUG");
            Console.WriteLine($"Name: {sample.Name}");
            Console.WriteLine($"Id: {sample.Id}");
            Console.WriteLine($"Category: {sample.Category}");
            Console.WriteLine($"CreateUtcTick: {sample.CreateUtcTick}");
            Console.WriteLine($"UpDateUtcTick: {sample.UpdateUtcTick}");
        }

        [TestMethod]
        public void Create()
        {
            var sampleRepo = serviceProvider.GetService<ISampleRepo>();
            var Cow = new Sample()
            {
                Id = 1,
                Category = "Bovine",
                Name = "Cow"
            };
            //var wakka = (sampleRepo.SeedSampleAsync(Cow) as OkObjectResult)?.Value as Sample;
            var cursedOrb = sampleRepo.SeedSampleAsync(Cow);
            //var test = cursedOrb.Result.Value; //value is null?
            //var test = cursedOrb.Result.GetType().Name; //value is "ActionResult`1"
            //var test = cursedOrb.GetType().Name; //value is ??
            var test = cursedOrb.Result.Result;
            Console.WriteLine($"???: {test}");

            //var queryResult = await ListAsync(queryRequest);
            //var pets = (queryResult.Result as OkObjectResult)?.Value as ICollection<Pet>;


        }
        [TestMethod]
        public void Read()
        {
            var sampleRepo = serviceProvider.GetService<ISampleRepo>();
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
