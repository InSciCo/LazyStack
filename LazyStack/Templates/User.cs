using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Amazon;
using Amazon.Runtime;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentityProvider.Model;

namespace __AppName__ClientSDK.Client
{
    public class User
    {
        public User(
            string regionEndpointStr,
            string userPoolId,
            string identityPoolId,
            string clientId,
            string userName,
            string password,
            string tempPassword
            )
        {
            RegionEndpointStr = regionEndpointStr;
            RegionEndpoint = RegionEndpoint.GetBySystemName(regionEndpointStr);

            //////////////////////////////////////
            // Authenticate user in a User Pool //
            //////////////////////////////////////
            System.Console.WriteLine("Authenticate user in a User Pool");
            AmazonCognitoIdentityProviderClient providerClient =
                new AmazonCognitoIdentityProviderClient(
                    new AnonymousAWSCredentials(),
                    RegionEndpoint);
            CognitoUserPool userPool = new CognitoUserPool(userPoolId, clientId, providerClient);
            CognitoUser = new CognitoUser(userName, clientId, userPool, providerClient);

            System.Console.WriteLine("Calling UserPool secured API Gateway");

            // First try with permanent password - this will fail if user was created by admin with temporary password'
            // if login attempt fails then we respond to the NEW_PASSWORD_REQUIRED challenge and log in with the
            // permantent password.
            string sessionID = string.Empty;
            try
            {
                AuthFlowResponse context = CognitoUser.StartWithSrpAuthAsync(
                    new InitiateSrpAuthRequest()
                    {
                        Password = password
                    }
                    ).GetAwaiter().GetResult();
                sessionID = context.SessionID;
            }
            catch (NotAuthorizedException)
            {
                try
                {
                    AuthFlowResponse context = CognitoUser.StartWithSrpAuthAsync(
                        new InitiateSrpAuthRequest()
                        {
                            Password = tempPassword
                        }
                        ).GetAwaiter().GetResult();

                    providerClient.RespondToAuthChallengeAsync(
                        new RespondToAuthChallengeRequest()
                        {
                            ClientId = clientId,
                            ChallengeName = ChallengeNameType.NEW_PASSWORD_REQUIRED,
                            ChallengeResponses = new Dictionary<string, string>()
                            {
                                { "USERNAME", CognitoUser.Username },
                                { "NEW_PASSWORD", password }
                            },
                            Session = context.SessionID
                        }).GetAwaiter().GetResult();

                    context = CognitoUser.StartWithSrpAuthAsync(
                        new InitiateSrpAuthRequest()
                        {
                            Password = password
                        }
                        ).GetAwaiter().GetResult();
                    sessionID = context.SessionID;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine($"Login Error: {e.Message}");
                }
            }

            IdToken = CognitoUser.SessionTokens.IdToken;
            JwtToken = new JwtSecurityToken(jwtEncodedString: IdToken);

            System.Console.WriteLine($"Call Cognito to get temporary credentitals for user in Identity Pool");
            // Note: creates Identity Pool identity if it doesn't exist
            AWSCredentials = CognitoUser.GetCognitoAWSCredentials(identityPoolId, RegionEndpoint);

            Identity = AWSCredentials.GetIdentityIdAsync().GetAwaiter().GetResult(); // Identity Pool Identity
            System.Console.WriteLine($"IdentityPoolIdentity {Identity}");
        }

        public CognitoAWSCredentials AWSCredentials;
        public RegionEndpoint RegionEndpoint { get; }
        public string RegionEndpointStr { get; }
        public CognitoUser CognitoUser { get; }
        public string Identity { get; }
        public JwtSecurityToken JwtToken { get; }
        public string IdToken { get; }
    }
}
