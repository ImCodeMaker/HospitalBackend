using Asp.Versioning;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[AllowAnonymous]
public class PatientSignupController(IUnitOfWork uow) : BaseController
{
    /// <summary>Public patient self-registration. Creates a PendingVerification patient awaiting admin approval.</summary>
    [HttpPost]
    public async Task<IActionResult> Signup([FromBody] PatientSignupRequest body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.FirstName) || string.IsNullOrWhiteSpace(body.LastName))
            return BadRequest(new { error = "First name and last name are required." });
        if (string.IsNullOrWhiteSpace(body.DocumentNumber))
            return BadRequest(new { error = "Document number is required." });
        if (body.BirthDate == default)
            return BadRequest(new { error = "Birth date is required." });

        // Duplicate check
        var dup = (await uow.Patients.FindAsync(
            p => p.DocumentType == body.DocumentType && p.DocumentNumber == body.DocumentNumber, ct))
            .FirstOrDefault();
        if (dup is not null)
            return Conflict(new { error = "Ya existe un paciente con este documento. Contáctanos para activar tu cuenta." });

        var patient = new Patient
        {
            FirstName = body.FirstName,
            LastName = body.LastName,
            DocumentType = body.DocumentType,
            DocumentNumber = body.DocumentNumber,
            Nationality = body.Nationality ?? "Dominicana",
            HomeAddress = body.Address ?? "—",
            BirthDate = body.BirthDate,
            Gender = body.Gender,
            Status = PatientsStatus.PendingVerification,
            Email = body.Email,
            Phone = body.Phone,
        };

        await uow.Patients.AddAsync(patient, ct);
        await uow.SaveChangesAsync(ct);

        return Created("", new { id = patient.Id, status = patient.Status.ToString() });
    }
}

public record PatientSignupRequest(
    string FirstName,
    string LastName,
    DocumentTypeEnum DocumentType,
    string DocumentNumber,
    DateTime BirthDate,
    GendersEnum Gender,
    string? Nationality,
    string? Address,
    string? Phone,
    string? Email);
