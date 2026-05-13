namespace HospitalApp.Core.Domain.Entities;

/// <summary>
/// Post-visit patient satisfaction survey response.
/// One per consult.
/// </summary>
public class SatisfactionSurvey : SharedEntity
{
    public required Guid ConsultId { get; set; }
    public required Guid PatientId { get; set; }
    public required Guid DoctorId { get; set; }

    /// <summary>Net Promoter Score 0-10.</summary>
    public int? Nps { get; set; }
    /// <summary>Overall satisfaction rating 1-5 stars.</summary>
    public int? Rating { get; set; }
    public string? Comment { get; set; }

    public DateTime InvitationSentAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public string? Token { get; set; } // one-time link token

    public Consult? Consult { get; set; }
    public Patient? Patient { get; set; }
}
