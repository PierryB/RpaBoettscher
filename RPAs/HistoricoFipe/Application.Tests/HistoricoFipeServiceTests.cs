using Moq;
using PuppeteerSharp;
using Application.Services;

namespace Application.Tests;

public class HistoricoFipeServiceTests
{
    private readonly StreamWriter _logFile;
    private readonly StreamWriter _csvFile;
    private readonly Mock<IPage> _mockPage;
    private readonly HistoricoFipeService _service;
    private static readonly string[] value = ["Marca1", "Marca2"];

    public HistoricoFipeServiceTests()
    {
        var memoryStreamLog = new MemoryStream();
        var memoryStreamCsv = new MemoryStream();

        _logFile = new StreamWriter(memoryStreamLog);
        _csvFile = new StreamWriter(memoryStreamCsv);
        _mockPage = new Mock<IPage>(MockBehavior.Strict);
        _service = new HistoricoFipeService(_logFile, _csvFile, "11/2024", "https://veiculos.fipe.org.br/");
    }

    [Fact]
    public async Task SiteFipe_ShouldThrowException_WhenMesConferenciaIsInvalid()
    {
        var service = new HistoricoFipeService(_logFile, _csvFile, "invalidDate", "https://veiculos.fipe.org.br/");

        await Assert.ThrowsAsync<SiteNavigationException>(() => service.SiteFipe(_mockPage.Object));
    }

    [Fact]
    public async Task SiteFipe_ShouldCallOpenFipeSite_WhenValidParams()
    {
        var mockResponse = new Mock<IResponse>();
        var mockElement = new Mock<IElementHandle>();

        _mockPage.Setup(p => p.GoToAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<WaitUntilNavigation[]>()))
                 .ReturnsAsync(mockResponse.Object);

        _mockPage.SetupSequence(p => p.XPathAsync(It.IsAny<string>()))
                 .ReturnsAsync([])
                 .ReturnsAsync([mockElement.Object]);
    }

    [Fact]
    public async Task FetchOptions_ShouldReturnValidOptions()
    {
        _mockPage.Setup(p => p.EvaluateFunctionAsync<string[]>(It.IsAny<string>())).ReturnsAsync(value);

        var options = await _service.FetchOptions(_mockPage.Object, "#selectMarcacarro");

        Assert.Contains("Marca1", options);
        Assert.Contains("Marca2", options);
    }
}
