using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace TesteLambda
{
    public class LambdaHandler
    {
        public APIGatewayProxyResponse HandleRequest()
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Teste deu certo 3!"
            };
        }
    }
}
