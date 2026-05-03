using HospitalApp.Core.Domain.Entities;

namespace HospitalApp.Core.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Patient> Patients { get; }
    IGenericRepository<Consult> Consults { get; }
    IGenericRepository<MedicalPrescription> Prescriptions { get; }
    IGenericRepository<LabOrder> LabOrders { get; }
    IGenericRepository<LabResult> LabResults { get; }
    IGenericRepository<Appointment> Appointments { get; }
    IGenericRepository<Medication> Medications { get; }
    IGenericRepository<StockTransaction> StockTransactions { get; }
    IGenericRepository<Invoice> Invoices { get; }
    IGenericRepository<InvoiceLineItem> InvoiceLineItems { get; }
    IGenericRepository<Payment> Payments { get; }
    IGenericRepository<Specialty> Specialties { get; }
    IGenericRepository<InsuranceCompany> InsuranceCompanies { get; }
    IGenericRepository<ConsultImage> ConsultImages { get; }
    IGenericRepository<Employee> Employees { get; }
    IGenericRepository<PayrollRecord> PayrollRecords { get; }
    IGenericRepository<RecruitmentApplication> RecruitmentApplications { get; }
    IGenericRepository<CajaShift> CajaShifts { get; }
    IGenericRepository<CashTransaction> CashTransactions { get; }
    IGenericRepository<ConsultFieldTemplate> ConsultFieldTemplates { get; }
    IGenericRepository<ClinicSettings> ClinicSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
