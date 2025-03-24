using System.ComponentModel.DataAnnotations;

namespace ClamAV.Models;

/// <summary>
/// Request model for file scan with metadata
/// </summary>
public class ScanRequest
{
    /// <summary>
    /// The file to scan
    /// </summary>
    [Required]
    public IFormFileCollection Files { get; set; }

    /// <summary>
    /// Optional unique identifier for the scan request
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Optional description of the files being scanned
    /// </summary>
    public string? Description { get; set; }
}
