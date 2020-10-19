using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;

using LazyStackAuth;
using LazyStackDevUtil;

namespace LazyStackAuthTest
{
    [TestClass]
    public class CallApiTests
    {
        public CallApiTests()
        {
            Setup.SignUpTestUser1<CallApiTests>(); // Make sure TestUser1 exists in UserPool

            var awsSettings = new AwsSettings("LazyStackAuthTest", "us-east-1");
            var json = awsSettings.BuildJson();
            appConfig = new ConfigurationBuilder()
                .AddUserSecrets<CallApiTests>()
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
                .Build();

            // SignIn
            var userLogin = "TestUser1"; // Cognito Login
            var password = "TestUser1!"; // Cognito Password
            authProvider = new AuthProviderCognito(appConfig);
            var authProcess = new AuthProcess(authProvider);
            var result = authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();
            Assert.IsTrue(result == AuthModuleEvent.Authorized);

            // Get Stack configuration
            awsStack = new AuthStack(appConfig);
        }

        IConfiguration appConfig;
        AuthStack awsStack;
        IAuthProvider authProvider;

        [TestMethod]
        public void CallHttpApiUnsecure()
        {
            var api = awsStack.AwsRestApiGateways["HttpApiUnsecure"];
            Assert.IsTrue(api.Name.Equals("HttpApiUnsecure"));
            Assert.IsTrue(api.Type.Equals("HttpApi"));
            Assert.IsTrue(api.IsSecure == false);

            var client = new AuthRESTClient(api,authProvider);
            var httpResponse = client.ExecAsync(HttpMethod.Get, "test").GetAwaiter().GetResult();
            Assert.IsTrue(httpResponse.ToString().Equals("OK"), "incorrect status");
        }

        [TestMethod]
        public void CallHttpApiSecure()
        {
            var api = awsStack.AwsRestApiGateways["HttpApiSecure"];
            Assert.IsTrue(api.Name.Equals("HttpApiSecure"));
            Assert.IsTrue(api.Type.Equals("HttpApi"));
            Assert.IsTrue(api.IsSecure == true);

            var client = new AuthRESTClient(api, authProvider);
            var httpResponse = client.ExecAsync(HttpMethod.Get, "test").GetAwaiter().GetResult();
            Assert.IsTrue(httpResponse.ToString().Equals("OK"), "incorrect status");
        }

        [TestMethod]
        public void CallApiUnsecure()
        {
            var api = awsStack.AwsRestApiGateways["ApiUnsecure"];
            Assert.IsTrue(api.Name.Equals("ApiUnsecure"));
            Assert.IsTrue(api.Type.Equals("Api"));
            Assert.IsTrue(api.IsSecure == false);

            var client = new AuthRESTClient(api, authProvider);
            var httpResponse = client.ExecAsync(HttpMethod.Get, "test").GetAwaiter().GetResult();
            Assert.IsTrue(httpResponse.ToString().Equals("OK"), "incorrect status");
        }

        [TestMethod]
        public void CallApiSecure()
        { 
            var api = awsStack.AwsRestApiGateways["ApiSecure"];
            Assert.IsTrue(api.Name.Equals("ApiSecure"));
            Assert.IsTrue(api.Type.Equals("Api"));
            Assert.IsTrue(api.IsSecure == true);

            var client = new AuthRESTClient(api, authProvider);
            var httpResponse = client.ExecAsync(HttpMethod.Get, "test").GetAwaiter().GetResult();
            Assert.IsTrue(httpResponse.ToString().Equals("OK"), "incorrect status");
        }
    }
}
