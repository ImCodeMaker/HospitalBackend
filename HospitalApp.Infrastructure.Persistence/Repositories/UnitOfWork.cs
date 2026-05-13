using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using HospitalApp.Infrastructure.Persistence.Context;

namespace HospitalApp.Infrastructure.Persistence.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IGenericRepository<Patient>? _patients;
    private IGenericRepository<Consult>? _consults;
    private IGenericRepository<MedicalPrescription>? _prescriptions;
    private IGenericRepository<LabOrder>? _labOrders;
    private IGenericRepository<LabResult>? _labResults;
    private IGenericRepository<Appointment>? _appointments;
    private IGenericRepository<Medication>? _medications;
    private IGenericRepository<StockTransaction>? _stockTransactions;
    private IGenericRepository<Invoice>? _invoices;
    private IGenericRepository<InvoiceLineItem>? _invoiceLineItems;
    private IGenericRepository<Payment>? _payments;
    private IGenericRepository<Specialty>? _specialties;
    private IGenericRepository<InsuranceCompany>? _insuranceCompanies;
    private IGenericRepository<ConsultImage>? _consultImages;
    private IGenericRepository<Employee>? _employees;
    private IGenericRepository<PayrollRecord>? _payrollRecords;
    private IGenericRepository<RecruitmentApplication>? _recruitmentApplications;
    private IGenericRepository<CajaShift>? _cajaShifts;
    private IGenericRepository<CashTransaction>? _cashTransactions;
    private IGenericRepository<ConsultFieldTemplate>? _consultFieldTemplates;
    private IGenericRepository<ClinicSettings>? _clinicSettings;
    private IGenericRepository<DicomStudy>? _dicomStudies;
    private IGenericRepository<NcfSequence>? _ncfSequences;
    private IGenericRepository<NoShowOutreachLog>? _noShowLogs;
    private IGenericRepository<SatisfactionSurvey>? _satisfactionSurveys;
    private IGenericRepository<ControlledSubstanceLog>? _controlledLogs;
    private IGenericRepository<Vendor>? _vendors;
    private IGenericRepository<PurchaseOrder>? _purchaseOrders;
    private IGenericRepository<SupplierPayment>? _supplierPayments;

    public IGenericRepository<Patient> Patients => _patients ??= new GenericRepository<Patient>(context);
    public IGenericRepository<Consult> Consults => _consults ??= new GenericRepository<Consult>(context);
    public IGenericRepository<MedicalPrescription> Prescriptions => _prescriptions ??= new GenericRepository<MedicalPrescription>(context);
    public IGenericRepository<LabOrder> LabOrders => _labOrders ??= new GenericRepository<LabOrder>(context);
    public IGenericRepository<LabResult> LabResults => _labResults ??= new GenericRepository<LabResult>(context);
    public IGenericRepository<Appointment> Appointments => _appointments ??= new GenericRepository<Appointment>(context);
    public IGenericRepository<Medication> Medications => _medications ??= new GenericRepository<Medication>(context);
    public IGenericRepository<StockTransaction> StockTransactions => _stockTransactions ??= new GenericRepository<StockTransaction>(context);
    public IGenericRepository<Invoice> Invoices => _invoices ??= new GenericRepository<Invoice>(context);
    public IGenericRepository<InvoiceLineItem> InvoiceLineItems => _invoiceLineItems ??= new GenericRepository<InvoiceLineItem>(context);
    public IGenericRepository<Payment> Payments => _payments ??= new GenericRepository<Payment>(context);
    public IGenericRepository<Specialty> Specialties => _specialties ??= new GenericRepository<Specialty>(context);
    public IGenericRepository<InsuranceCompany> InsuranceCompanies => _insuranceCompanies ??= new GenericRepository<InsuranceCompany>(context);
    public IGenericRepository<ConsultImage> ConsultImages => _consultImages ??= new GenericRepository<ConsultImage>(context);
    public IGenericRepository<Employee> Employees => _employees ??= new GenericRepository<Employee>(context);
    public IGenericRepository<PayrollRecord> PayrollRecords => _payrollRecords ??= new GenericRepository<PayrollRecord>(context);
    public IGenericRepository<RecruitmentApplication> RecruitmentApplications => _recruitmentApplications ??= new GenericRepository<RecruitmentApplication>(context);
    public IGenericRepository<CajaShift> CajaShifts => _cajaShifts ??= new GenericRepository<CajaShift>(context);
    public IGenericRepository<CashTransaction> CashTransactions => _cashTransactions ??= new GenericRepository<CashTransaction>(context);
    public IGenericRepository<ConsultFieldTemplate> ConsultFieldTemplates => _consultFieldTemplates ??= new GenericRepository<ConsultFieldTemplate>(context);
    public IGenericRepository<ClinicSettings> ClinicSettings => _clinicSettings ??= new GenericRepository<ClinicSettings>(context);
    public IGenericRepository<DicomStudy> DicomStudies => _dicomStudies ??= new GenericRepository<DicomStudy>(context);
    public IGenericRepository<NcfSequence> NcfSequences => _ncfSequences ??= new GenericRepository<NcfSequence>(context);
    public IGenericRepository<NoShowOutreachLog> NoShowOutreachLogs => _noShowLogs ??= new GenericRepository<NoShowOutreachLog>(context);
    public IGenericRepository<SatisfactionSurvey> SatisfactionSurveys => _satisfactionSurveys ??= new GenericRepository<SatisfactionSurvey>(context);
    public IGenericRepository<ControlledSubstanceLog> ControlledSubstanceLogs => _controlledLogs ??= new GenericRepository<ControlledSubstanceLog>(context);
    public IGenericRepository<Vendor> Vendors => _vendors ??= new GenericRepository<Vendor>(context);
    public IGenericRepository<PurchaseOrder> PurchaseOrders => _purchaseOrders ??= new GenericRepository<PurchaseOrder>(context);
    public IGenericRepository<SupplierPayment> SupplierPayments => _supplierPayments ??= new GenericRepository<SupplierPayment>(context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}
