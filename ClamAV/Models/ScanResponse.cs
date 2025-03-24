namespace ClamAV.Models;

public class ScanResponse
{
    public string? RequestId { get; set; }
    public int InfectedFiles { get; set; }
    public int ScannedFiles { get; set; }

    public IEnumerable<FileMetadata> FileMetadatas { get; set; }
    public string? Description { get; set; }
    public bool IsClean { get; set; }
    public DateTime ScanDate { get; set; }
}
