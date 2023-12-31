namespace FileWebApi.Models;

/// <summary>
/// Request model for /word/replacement
/// </summary>
public class WordReplacementRequest
{
    /// <summary>
    /// Word files (.docx)
    /// </summary>
    public required IEnumerable<IFormFile> FormFiles { get; set; }
}
