using Moq;
using PuppeteerSharp;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Application.Services.Tests;
public class CatolicaServiceTests
{
    private readonly Mock<IPage> _mockPage;
    private readonly Mock<IBrowser> _mockBrowser;
    private readonly StreamWriter _logWriter;
    private readonly MemoryStream _logStream;
    private readonly string _usuarioCatolica;
    private readonly string _senhaCatolica;
    private readonly string _diretorioTemp;

    public CatolicaServiceTests()
    {
        _mockPage = new Mock<IPage>();
        _mockBrowser = new Mock<IBrowser>();
        _logStream = new MemoryStream();
        _logWriter = new StreamWriter(_logStream);
        _usuarioCatolica = "teste";
        _senhaCatolica = "teste";
        _diretorioTemp = $@"C:\temp\PdfCatolicaTeste\{Guid.NewGuid()}";
    }

    [Fact]
    public async Task SiteCatolica_ShouldThrowException_WhenUserIsEmpty()
    {
        var service = new CatolicaService(_logWriter, string.Empty, _senhaCatolica, _mockBrowser.Object, _diretorioTemp);

        var exception = await Assert.ThrowsAsync<Exception>(() => service.SiteCatolica(_mockPage.Object));
        Assert.Equal("O parâmetro 'usuarioCatolica' está vazio", exception.Message);
    }

    [Fact]
    public async Task SiteCatolica_ShouldThrowException_WhenPasswordIsEmpty()
    {
        var service = new CatolicaService(_logWriter, _usuarioCatolica, string.Empty, _mockBrowser.Object, _diretorioTemp);

        var exception = await Assert.ThrowsAsync<Exception>(() => service.SiteCatolica(_mockPage.Object));
        Assert.Equal("O parâmetro 'senhaCatolica' está vazio", exception.Message);
    }

    [Fact]
    public async Task SiteCatolica_ShouldOpenPageSuccessfully_WhenCredentialsAreValid()
    {
        var mockResponse = new Mock<IResponse>();

        _mockPage.Setup(page => page.GoToAsync(It.IsAny<string>(), null, new WaitUntilNavigation[0]))
            .ReturnsAsync(mockResponse.Object);

        _mockPage.Setup(page => page.TypeAsync(It.IsAny<string>(), It.IsAny<string>(), null))
            .Returns(Task.CompletedTask);

        _mockPage.Setup(page => page.ClickAsync(It.IsAny<string>(), null))
            .Returns(Task.CompletedTask);

        var service = new CatolicaService(_logWriter, _usuarioCatolica, _senhaCatolica, _mockBrowser.Object, _diretorioTemp);

        await service.SiteCatolica(_mockPage.Object);

        _mockPage.Verify(page => page.GoToAsync(It.IsAny<string>(), null, new WaitUntilNavigation[0]), Times.Once);
        _mockPage.Verify(page => page.TypeAsync("#User", _usuarioCatolica, null), Times.Once);
        _mockPage.Verify(page => page.TypeAsync("#Pass", _senhaCatolica, null), Times.Once);
        _mockPage.Verify(page => page.ClickAsync(It.IsAny<string>(), null));
    }
}
