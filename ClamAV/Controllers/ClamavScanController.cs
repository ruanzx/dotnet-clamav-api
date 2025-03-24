using ClamAV.Models;
using ClamAV.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClamAV.Controllers;

[ApiController]
[Route("[controller]")]
public class ClamavScanController : ControllerBase
{
    private readonly IClamavScanService _clamavScanService;
    private readonly ILogger<ClamavScanController> _logger;

    public ClamavScanController(
        IClamavScanService clamavScanService,
        ILogger<ClamavScanController> logger)
    {
        _clamavScanService = clamavScanService;
        _logger = logger;
    }

    /// <summary>
    /// Scans a file from a multipart form with additional metadata
    /// </summary>
    /// <returns>Scan result with metadata</returns>
    [HttpPost("scan-files")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100MB limit
    public async Task<IActionResult> ScanFilesAsync([FromForm] ScanRequest request)
    {
        if (request.Files == null || request.Files.Count == 0)
        {
            return BadRequest("No files uploaded");
        }

        _logger.LogInformation("Scanning {Count} files", request.Files.Count);

        var uniqueDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(uniqueDir);
        _logger.LogInformation($"Created folder {uniqueDir}");

        try
        {
            foreach (var file in request.Files)
            {
                // save file to disk with unique folder name
                var filePath = Path.Combine(uniqueDir, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            var result = await _clamavScanService.ClamdscanAsync(uniqueDir, new string[] { });

            var response = new ScanResponse
            {
                RequestId = request.RequestId,
                InfectedFiles = result.InfectedFiles,
                ScannedFiles = result.ScannedFiles,
                FileMetadatas = request.Files.Select(x => new FileMetadata
                {
                    FileName = x.FileName,
                    FileSize = x.Length
                }),
                Description = request.Description,
                IsClean = result.InfectedFiles == 0,
                ScanDate = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning files");
            return StatusCode(500, "Error processing file scan");
        }
        finally
        {
            // cleanup
            Directory.Delete(uniqueDir, true);
            _logger.LogInformation($"Deleted folder {uniqueDir}");
        }
    }
}
