using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Microsoft.Extensions.Configuration;

using LazyStackAuth;

namespace LazyStackDevUtil
{
    /// <summary>
    /// SignUpTestUser1() is a utility class to make sure a required
    /// user is in the stack's User Pool. It requires that you have
    /// configured a gmail account that the auth process can send
    /// a code to. Gmail:Email and Gmail:Password are used to 
    /// access that emaili account. These values are usually
    /// inserted into appConfig from Visual Studio UserSecrets.
    /// </summary>
    public static class Setup
    {
        public static void SignUpTestUser<T>(IConfiguration appConfig, string userLogin, string password)
            where T : class
        {
            var authProvider = new AuthProviderCognito(appConfig);
            var authProcess = new AuthProcess(authProvider);
            // Try SignIn with TestUser1
            authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();

            if (!authProcess.IsAuthorized)
            {   // SignUp

                var email = appConfig["Gmail:Email"];

                var verificationCodeSendTime = DateTime.UtcNow.AddSeconds(-3); // verficationCode sent after this time
                var authResult = authProcess.StartSignUpAsync(userLogin, password, email).GetAwaiter().GetResult();

                if (authResult == AuthModuleEvent.UserLoginAlreadyUsed)
                {
                    authResult = authProcess.ResendVerifySignupCodeAsync(userLogin).GetAwaiter().GetResult();
                }
                else if (authProcess.CurrentChallenge != AuthChallenges.SignUp)
                    throw new Exception($"Unexpected problem in user signup. Auth Error {authResult}");

                var verificationCode = AuthEmail.GetAuthCode(appConfig, verificationCodeSendTime);

                // Verify SignUp
                authResult = authProcess.VerifySignUpAsync(verificationCode).GetAwaiter().GetResult();
                if (authResult != AuthModuleEvent.SignedUp)
                    throw new Exception($"Auth Error {authResult}");

                // SignIn to new account
                authResult = authProcess.StartAuthAsync(userLogin, password).GetAwaiter().GetResult();
                if (!authProcess.IsAuthorized)
                    throw new Exception($"Auth Error {authResult}");
            }
            authProcess.SignOut();
        }
    }
}
