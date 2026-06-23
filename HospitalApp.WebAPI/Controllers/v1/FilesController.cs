using Asp.Versioning;
using HospitalApp.Core.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IoPath = System.IO.Path;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class FilesController(IFileStorageService fileStorage, IMalwareScanner malwareScanner) : BaseController
{
    private static readonly Dictionary<string, string[]> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"],
        [".png"] = ["image/png"],
        [".pdf"] = ["application/pdf"],
        [".webp"] = ["image/webp"],
        [".gif"] = ["image/gif"],
    };

    private static readonly HashSet<string> AllowedFolders =
        ["consult-images", "lab-results", "insurance-cards", "documents", "hr-documents"];

    /// <summary>Upload a file to a specific folder (consult-images, lab-results, insurance-cards, documents).</summary>
    [HttpPost("{folder}")]
    [RequestSizeLimit(52_428_800L)]
    public async Task<IActionResult> Upload(string folder, IFormFile file, CancellationToken ct)
    {
        if (!AllowedFolders.Contains(folder) || folder.Contains("..") || folder.Contains('/') || folder.Contains('\\'))
            return BadRequest(new { error = "Invalid upload folder." });

        var ext = IoPath.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedContentTypes.TryGetValue(ext, out var allowedContentTypes))
            return BadRequest(new { error = $"File type '{ext}' not allowed." });

        if (file.Length == 0)
            return BadRequest(new { error = "File is empty." });

        if (!allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { error = "File content type does not match the allowed type for this extension." });

        await using var stream = file.OpenReadStream();
        var scan = await malwareScanner.ScanAsync(stream, file.FileName, ct);
        if (!scan.IsClean)
            return BadRequest(new { error = "File failed malware scan.", scan.Signature, scan.Error });

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
