using ClamAV.Models;

namespace ClamAV.Services;

public interface IClamavScanService
{
    Task<ClamScanResult> ClamavscanAsync(string workingDirPath, string[] userParams);
    Task<ClamScanResult> ClamdscanAsync(string workingDirPath, string[] userParams);
}
