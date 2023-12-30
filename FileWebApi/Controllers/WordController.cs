using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        return Ok();
    }
}
