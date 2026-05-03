using Asp.Versioning;
using HospitalApp.Core.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IoPath = System.IO.Path;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class FilesController(IFileStorageService fileStorage) : BaseController
{
    private static readonly HashSet<string> _allowedExtensions =
        [".jpg", ".jpeg", ".png", ".pdf", ".webp", ".gif"];

    /// <summary>Upload a file to a specific folder (consult-images, lab-results, insurance-cards, documents).</summary>
    [HttpPost("{folder}")]
    [RequestSizeLimit(52_428_800L)]
    public async Task<IActionResult> Upload(string folder, IFormFile file, CancellationToken ct)
    {
        var allowedFolders = new[] { "consult-images", "lab-results", "insurance-cards", "documents", "hr-documents" };
        if (!allowedFolders.Contains(folder))
            return BadRequest(new { error = "Invalid upload folder." });

        var ext = IoPath.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(ext))
            return BadRequest(new { error = $"File type '{ext}' not allowed." });

        if (file.Length == 0)
            return BadRequest(new { error = "File is empty." });

        await using var stream = file.OpenReadStream();
        var path = await fileStorage.SaveAsync(stream, file.FileName, folder, ct);
        var url = fileStorage.GetUrl(path);

        return Ok(new { path, url });
    }

    /// <summary>Delete a file by its relative path.</summary>
    [HttpDelete]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete([FromQuery] string path, CancellationToken ct)
    {
        await fileStorage.DeleteAsync(path, ct);
        return NoContent();
    }
}
