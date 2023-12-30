using Microsoft.AspNetCore.Mvc;
using Spire.Doc;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;

namespace FileWebApi.Controllers;

[Route("api/word")]
[ApiController]
public class WordController : ControllerBase
{
    [HttpPut("replacement")]
    public async Task<IActionResult> Replace(
        [Required] string matchString,
        [Required] string newString,
        [Required, FromForm] IEnumerable<IFormFile> formFiles
    )
    {
        MemoryStream outZip = new();
        using (ZipArchive zip = new(outZip, ZipArchiveMode.Create, true))
        {
            foreach (IFormFile formFile in formFiles)
            {
                using Stream stream = formFile.OpenReadStream();
                Document document = new();
                document.LoadFromStream(stream, FileFormat.Docx);
                document.Replace(matchString, newString, false, true);
                using MemoryStream docxStream = new();
                document.SaveToStream(docxStream, FileFormat.Docx);
                docxStream.Position = 0;
                ZipArchiveEntry fileInZip = zip.CreateEntry(formFile.FileName, CompressionLevel.Optimal);
                using Stream streamInZip = fileInZip.Open();
                await docxStream.CopyToAsync(streamInZip);
            }
        }
        outZip.Position = 0;
        return File(outZip, "application/zip", $"wordFiles-{Guid.NewGuid()}.zip");
    }
}
