using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LazyStackDynamoDBRepoTests
{
    [TestClass]
    public class RepoTests
    {

        public RepoTests()
        {
            // Use AWS CloudFormation to get configuration of the LazyStackRepoTest stack.
            // If you don't have this stack published - publish the serverless.template included in this project
            // The stackname used here is LzRepoTestStack. It contains only a Type: AWS::DynamoDB::Table resource.
        }

        [TestMethod]
        public void Test()
        {



        }
    }
}
