using Xunit;
using TesteLambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;

namespace TesteLambda.Tests
{
    public class LambdaHandlerTests
    {
        [Fact]
        public void TestHandleRequest()
        {
            // Arrange
            var handler = new LambdaHandler();

            // Act
            var response = handler.HandleRequest();

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Teste deu certo 3!", response.Body);
        }
    }
}
