#pragma warning disable CS1591

namespace HospitalApp.WebAPI.GraphQL.Types;

public record RevenueTrendPoint(DateTime Date, decimal Total);

public record SpecialtyVolumeSlice(Guid SpecialtyId, string Specialty, int Count);

public record DiagnosisFrequency(string DiagnosisCode, string Description, int Count);

public record HourlyVolumeCell(int DayOfWeek, int Hour, int Count);

public record DoctorRevenueSlice(Guid DoctorId, string DoctorName, decimal TotalRevenue, int ConsultCount);

public record PaymentMethodSlice(string Method, int Count, decimal Total);

public record StaffPerformanceRow(
    Guid UserId,
    string FullName,
    string Role,
    int PatientsAttended,
    decimal RevenueGenerated,
    int CompletedTasks);

public record AgeBucket(string Range, int Count);

public record GenderSlice(string Gender, int Count);

public record DemographicsReport(
    int TotalActive,
    IReadOnlyList<AgeBucket> AgeBuckets,
    IReadOnlyList<GenderSlice> GenderBreakdown);

public record PrescriptionFrequency(string DrugName, int Count, int UniquePatients);

public record ConsultVolumeByDoctorRow(Guid DoctorId, string DoctorName, int ConsultCount, double AverageMinutes);

public record ControlledSubstanceEntry(
    Guid MedicationId,
    string MedicationName,
    string? ClassCode,
    int CurrentStock,
    int Dispensed30d,
    DateTime? LastDispensedAt);
