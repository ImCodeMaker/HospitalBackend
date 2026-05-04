using Asp.Versioning;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Pacs.Commands.DeleteDicomStudy;
using HospitalApp.Core.Application.Features.Pacs.Commands.UploadDicomStudy;
using HospitalApp.Core.Application.Features.Pacs.Queries.GetDicomStudiesByConsult;
using HospitalApp.Core.Application.Features.Pacs.Queries.GetDicomStudyById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IoPath = System.IO.Path;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "ClinicalStaff")]
public class DicomController(IMediator mediator, IFileStorageService fileStorage) : BaseController
{
    private static readonly long MaxDicomFileSizeBytes = 200 * 1024 * 1024; // 200 MB

    /// <summary>List DICOM studies for a consult.</summary>
    [HttpGet("consult/{consultId:guid}")]
    public async Task<IActionResult> GetByConsult(Guid consultId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDicomStudiesByConsultQuery(consultId), ct);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Upload a DICOM file (.dcm) to a consult.</summary>
    [HttpPost("consult/{consultId:guid}")]
    [RequestSizeLimit(209_715_200L)]
    [Authorize(Policy = "DoctorOrAdmin")]
    public async Task<IActionResult> Upload(
        Guid consultId,
        IFormFile file,
        [FromForm] string? modality,
        [FromForm] string? studyInstanceUid,
        [FromForm] string? accessionNumber,
        [FromForm] DateTime? studyDate,
        [FromForm] string? description,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest(new { error = "File is empty." });

        if (file.Length > MaxDicomFileSizeBytes)
            return BadRequest(new { error = "File exceeds 200 MB limit." });

        var ext = IoPath.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".dcm" && ext != ".dicom")
            return BadRequest(new { error = "Only .dcm / .dicom files are accepted." });

        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(new UploadDicomStudyCommand(
            consultId,
            GetCurrentUserId(),
            stream,
            file.FileName,
            file.Length,
            modality,
            studyInstanceUid,
            accessionNumber,
            studyDate,
            description
        ), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetByConsult), new { consultId }, new { id = result.Data })
            : StatusCode(result.StatusCode, new { error = result.Error });
    }

    /// <summary>Get a single DICOM study metadata and serving URL.</summary>
    [HttpGet("{studyId:guid}")]
    public async Task<IActionResult> GetById(Guid studyId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetDicomStudyByIdQuery(studyId), ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        var url = fileStorage.GetUrl(result.Data!.OriginalFileName);
        return Ok(new { study = result.Data, url });
    }

    /// <summary>Delete a DICOM study (Admin only).</summary>
    [HttpDelete("{studyId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid studyId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteDicomStudyCommand(studyId), ct);
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, new { error = result.Error });
    }
}
