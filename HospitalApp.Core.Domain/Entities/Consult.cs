using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class Consult : SharedEntity
{
    public required Guid PatientId { get; set; }
    public required Guid SpecialtyId { get; set; }
    public required Guid DoctorId { get; set; }
    public ConsultStatusEnum Status { get; set; } = ConsultStatusEnum.Open;

    // Common vitals
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? Bmi { get; set; }
    public int? BpSystolic { get; set; }
    public int? BpDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public decimal? O2Saturation { get; set; }
    public int? RespiratoryRate { get; set; }

    // Clinical documentation
    public string? ChiefComplaint { get; set; }
    public string? ClinicalObservations { get; set; }
    public string? DiagnosisCodes { get; set; } // comma-separated ICD-10 codes
    public string? DiagnosisDescription { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? ReferralNotes { get; set; }

    // Dynamic specialty-specific fields stored as JSONB
    public string? SpecialtyData { get; set; }

    /// <summary>Dental chart state (FDI 32-tooth notation) as JSON, keyed by tooth number.</summary>
    public string? DentalChart { get; set; }

    // Timestamps
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    // Navigation
    public Patient? Patient { get; set; }
    public Specialty? Specialty { get; set; }
    public ICollection<MedicalPrescription> Prescriptions { get; set; } = [];
    public ICollection<LabOrder> LabOrders { get; set; } = [];
    public ICollection<ConsultImage> Images { get; set; } = [];
    public ICollection<Appointment> FollowUpAppointments { get; set; } = [];
    public Invoice? Invoice { get; set; }
}
