using System;
using System.IO;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;

using LazyStackAuth;
using LazyStackDevUtil;

namespace LazyStackAuthTest
{
    [TestClass]
    public class SignUpTests
    {
        public SignUpTests()
        {
            var awsSettings = new AwsSettings("LazyStackAuthTest", "us-east-1");
            var json = awsSettings.BuildJson();
            appConfig = new ConfigurationBuilder()
                .AddUserSecrets<SignUpTests>() 
                .AddJsonStream(new MemoryStream(Encoding.ASCII.GetBytes(json)))
                .Build();
        }

        IConfiguration appConfig;
        string userLogin;
        string password;

        [TestMethod]
        public void TestMethod1()
        {

            //var authProvider = new AuthProviderCognito(appConfig);
            //var authProcess = new AuthProcess(authProvider);
            //// Try SignIn with TestUser1
            //authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();

            //if (!authProcess.IsAuthorized)
            //{   // SignUp

            //    var email = appConfig["Gmail:Email"];

            //    var verificationCodeSendTime = DateTime.UtcNow.AddSeconds(-3); // verficationCode sent after this time
            //    var authResult = authProcess.StartSignUpAsync(userLogin, password, email).GetAwaiter().GetResult();

            //    if (authResult == AuthModuleEvent.UserLoginAlreadyUsed)
            //    {
            //        authResult = authProcess.ResendVerifySignupCodeAsync(userLogin).GetAwaiter().GetResult();
            //    }
            //    else if (authProcess.CurrentChallenge != AuthChallenges.SignUp)
            //        throw new Exception($"Unexpected problem in user signup. Auth Error {authResult}");

            //    var verificationCode = AuthEmail.GetAuthCode(verificationCodeSendTime);

            //    // Verify SignUp
            //    authResult = authProcess.VerifySignUpAsync(verificationCode).GetAwaiter().GetResult();
            //    if (authResult != AuthModuleEvent.SignedUp)
            //        throw new Exception($"Auth Error {authResult}");

            //    // SignIn to new account
            //    authResult = authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();
            //    if (!authProcess.IsAuthorized)
            //        throw new Exception($"Auth Error {authResult}");
            //}

        }
    }
}
