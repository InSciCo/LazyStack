using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

// Notes: todo - review during implementaiton
// For the moment we are not writing connection records as
// we may not need them. The current intent is to return 
// the connectionId to the client and allow the client to 
// pass in the connectionId as part of any subscription they
// may make. Since we use the subscription records to send
// out messages, there is no reason to have a separate 
// connection record. 

// Possible issues:
// What happens if some other authenticated client subscribes 
// using the connectionId? The connected client would receive 
// unexpected notifications. Not sure this is a big deal but 
// worth thinking about from a security perspective. 
// Subscription requests arrive on the REST API and carry 
// the LzUser. We could ensure that only only one LzUser 
// is assocated with any one connectionId. Of course, a 
// LzUser may have multiple clients and therefore multiple 
// current connectionIds. If any of this becomes an issue 
// we can write an initial subscription record in this 
// routine with just connectionId and LzUserId (no topics).

namespace LazyStackWSLambda;
public class Function
{
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    private static readonly string TABLE_NAME = System.Environment.GetEnvironmentVariable("TABLE_NAME")!;
    private static readonly string COGNITO_POOL_ID = Environment.GetEnvironmentVariable("COGNITO_POOL_ID")!;
    private static readonly string COGNITO_REGION = Environment.GetEnvironmentVariable("COGNITO_REGION")!;

    private static readonly HttpClient httpClient = new HttpClient();

    public Function()
    {
    }

    [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
    public async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        logger.Info("Received a WebSocket event.");

        switch (request.RequestContext.RouteKey)
        {
            case "$connect":
                return await OnConnect(request);
            case "$disconnect":
                await OnDisconnect(request);
                break;
            default:
                await OnMessage(request);
                break;
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = "Request processed successfully."
        };
    }

    private async Task<APIGatewayProxyResponse> OnConnect(APIGatewayProxyRequest request)
    {
        await Task.Delay(0);
        string connectionId = request.RequestContext.ConnectionId;

        var token = request.Headers["Authorization"];
        if (!await ValidateTokenAsync(token))
        {
            logger.Warn($"Invalid token for connection: {connectionId}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 401, // Unauthorized
                Body = "Invalid token."
            };
        }
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonConvert.SerializeObject(new { ConnectionId = connectionId })
        };
    }

    private async Task OnDisconnect(APIGatewayProxyRequest request)
    {
        await Task.Delay(0);
    }

    private async Task OnMessage(APIGatewayProxyRequest request)
    {
        await Task.Delay(0);
    }

    private async Task<bool> ValidateTokenAsync(string token)
    {
        //IdentityModelEventSource.ShowPII = true; // Show more detailed exceptions (optional)

        var cognitoIssuer = $"https://cognito-idp.{COGNITO_REGION}.amazonaws.com/{COGNITO_POOL_ID}";
        var jwksUrl = $"{cognitoIssuer}/.well-known/jwks.json";

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


}
