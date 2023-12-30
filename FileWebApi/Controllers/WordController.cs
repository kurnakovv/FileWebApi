using FileWebApi.Constants;
using Microsoft.AspNetCore.Cors.Infrastructure;
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
        if (!formFiles.Any())
        {
            return NotFound("Files not found.");
        }
        IEnumerable<IFormFile> invalidFileFormats = formFiles.Where(f => !f.FileName.EndsWith(".docx"));
        if (invalidFileFormats.Any())
        {
            string names = string.Join(", ", invalidFileFormats.Select(x => $"'{x.FileName}'").ToList());
            return Conflict($"Incorrect file format, can only be used Doc files (ended with \".docx\").\nIncorrect files: {names}");
        }
        IEnumerable<IFormFile> invalidFileSize = formFiles.Where(x => x.Length > WordConstants.MAX_LIMIT);
        if (invalidFileSize.Any())
        {
            string names = string.Join(", ", invalidFileSize.Select(x => $"'{x.FileName}'").ToList());
            return Conflict($"Incorrect file size (max {WordConstants.MAX_LIMIT} MB).\nIncorrect files: {names}");
        }
        IEnumerable<string> duplicateFileNames = formFiles
            .Select(x => x.FileName)
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key);
        if (duplicateFileNames.Any())
        {
            string names = string.Join(", ", duplicateFileNames.ToList());
            return Conflict($"Duplicate file names.\nIncorrect files: {names}");
        }
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
