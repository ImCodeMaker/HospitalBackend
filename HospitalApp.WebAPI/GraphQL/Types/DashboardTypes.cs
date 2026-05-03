#pragma warning disable CS1591

namespace HospitalApp.WebAPI.GraphQL.Types;

public record AppointmentSummary(
    Guid Id,
    string PatientName,
    DateTime ScheduledDate,
    int DurationMinutes,
    string Type,
    string Status,
    string? Reason
);

public record LabOrderSummary(
    Guid Id,
    string TestName,
    string Priority,
    string PatientName,
    DateTime CreatedAt,
    bool IsExternal
);

public record LowStockAlert(
    Guid Id,
    string Name,
    string GenericName,
    int CurrentStock,
    int MinimumStockThreshold,
    bool IsOutOfStock
);

public record DoctorDashboard(
    int TodayAppointmentCount,
    int PendingConsultCount,
    int UnreviewedLabResultsCount,
    IReadOnlyList<AppointmentSummary> TodayAppointments
);

public record AdminDashboard(
    int TotalActivePatients,
    int TodayConsultCount,
    int MonthConsultCount,
    decimal TodayRevenue,
    decimal MonthRevenue,
    int PendingInvoicesCount,
    int LowStockCount,
    IReadOnlyList<LowStockAlert> LowStockAlerts
);

public record ReceptionistDashboard(
    int TodayAppointmentCount,
    int ConfirmedCount,
    int AttendedCount,
    int PendingBillingCount,
    IReadOnlyList<AppointmentSummary> TodayAppointments
);

public record LabTechDashboard(
    int PendingOrdersCount,
    int UrgentOrdersCount,
    int InProgressCount,
    IReadOnlyList<LabOrderSummary> PendingOrders
);
