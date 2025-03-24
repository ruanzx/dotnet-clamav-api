using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ClamAV.Controllers;
using ClamAV.Models;
using ClamAV.Services;

namespace ClamAV.UnitTests.Controllers;

public class ClamavScanControllerTests
{
    // Sample positive virus: https://gist.github.com/mikecastrodemaria/0843f8828fef7c60558a58248fcb724c

    private readonly Mock<IClamavScanService> _clamavScanServiceMock;
    private readonly Mock<ILogger<ClamavScanController>> _loggerMock;
    private readonly ClamavScanController _controller;

    public ClamavScanControllerTests()
    {
        _clamavScanServiceMock = new Mock<IClamavScanService>();
        _loggerMock = new Mock<ILogger<ClamavScanController>>();
        _controller = new ClamavScanController(_clamavScanServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ScanFilesAsync_NoFilesUploaded_ReturnsBadRequest()
    {
        // Arrange
        var request = new ScanRequest
        {
            Files = new FormFileCollection()
        };

        // Act
        var result = await _controller.ScanFilesAsync(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No files uploaded", badRequestResult.Value);
    }

    [Fact]
    public async Task ScanFilesAsync_FilesUploaded_ReturnsOk()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var content = "Hello World from a Fake File";
        var fileName = "test.txt";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
        fileMock.Setup(_ => _.FileName).Returns(fileName);
        fileMock.Setup(_ => _.Length).Returns(ms.Length);

        var files = new FormFileCollection { fileMock.Object };
        var request = new ScanRequest
        {
            Files = files,
            RequestId = "123",
            Description = "Test file"
        };

        var scanResult = new ClamScanResult
        {
            InfectedFiles = 0,
            ScannedFiles = 1
        };

        _clamavScanServiceMock
            .Setup(x => x.ClamdscanAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync(scanResult);

        // Act
        var result = await _controller.ScanFilesAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ScanResponse>(okResult.Value);
        Assert.Equal(request.RequestId, response.RequestId);
        Assert.Equal(request.Description, response.Description);
        Assert.Equal(1, response.ScannedFiles);
        Assert.Equal(0, response.InfectedFiles);
        Assert.True(response.IsClean);
    }

    [Fact]
    public async Task ScanFilesAsync_ErrorDuringScan_ReturnsInternalServerError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        var content = "Hello World from a Fake File";
        var fileName = "test.txt";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
        fileMock.Setup(_ => _.FileName).Returns(fileName);
        fileMock.Setup(_ => _.Length).Returns(ms.Length);

        var files = new FormFileCollection { fileMock.Object };
        var request = new ScanRequest
        {
            Files = files,
            RequestId = "123",
            Description = "Test file"
        };

        _clamavScanServiceMock
            .Setup(x => x.ClamdscanAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ThrowsAsync(new Exception("Scan error"));

        // Act
        var result = await _controller.ScanFilesAsync(request);

        // Assert
        var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, internalServerErrorResult.StatusCode);
        Assert.Equal("Error processing file scan", internalServerErrorResult.Value);
    }
}