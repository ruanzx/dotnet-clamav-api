namespace ClamAV.Models;

public class ClamScanResult
{
    public int ScannedFiles { get; set; }
    public int InfectedFiles { get; set; }
    public int ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ScanFile> Files { get; set; } = new List<ScanFile>();
}
