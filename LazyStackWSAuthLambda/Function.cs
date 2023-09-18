using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace WebSocketAuthentication;

public class Function
{
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    private static readonly string COGNITO_POOL_ID = Environment.GetEnvironmentVariable("COGNITO_POOL_ID")!;
    private static readonly string COGNITO_REGION = Environment.GetEnvironmentVariable("COGNITO_REGION")!;

    public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandlerAsync(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
    {
        var token = input.Headers["Authorization"];

        if (await ValidateTokenAsync(token))
            return GenerateAllowPolicy(input.MethodArn, token);
        else
            throw new UnauthorizedAccessException("Invalid token");
    }

    private async Task<bool> ValidateTokenAsync(string token)
    {
        //IdentityModelEventSource.ShowPII = true; // Show more detailed exceptions (optional)

        var cognitoIssuer = $"https://cognito-idp.{COGNITO_REGION}.amazonaws.com/{COGNITO_POOL_ID}";
        var jwksUrl = $"{cognitoIssuer}/.well-known/jwks.json";

        var httpClient = new HttpClient();
        var jwks = await httpClient.GetStringAsync(jwksUrl);

        var tokenParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = cognitoIssuer,
            ValidateAudience = false, // we might want to validate this too. e.g. Client User Pool Id
            IssuerSigningKeyResolver = (s, securityToken, identifier, parameters) =>
            {
                var keys = new JsonWebKeySet(jwks).Keys;
                return (IEnumerable<SecurityKey>)keys;
            },
            ValidateLifetime = true
        };

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var user = handler.ValidateToken(token, tokenParams, out var validatedToken);
            return validatedToken != null;
        }
        catch
        {
            return false;
        }
    }

    private APIGatewayCustomAuthorizerResponse GenerateAllowPolicy(string methodArn, string token)
    {
        return new APIGatewayCustomAuthorizerResponse
        {
            PrincipalID = "user",
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                    {
                        new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                        {
                            Action = new HashSet<string> { "execute-api:Invoke" },
                            Effect = "Allow",
                            Resource = new HashSet<string> { methodArn }
                        }
                    }
            }
        };
    }


}