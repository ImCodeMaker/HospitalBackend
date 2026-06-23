using HospitalApp.Core.Domain.Entities;
using HospitalApp.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Infrastructure.Persistence.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<InsuranceCompany> InsuranceCompanies => Set<InsuranceCompany>();
    public DbSet<Consult> Consults => Set<Consult>();
    public DbSet<MedicalPrescription> MedicalPrescriptions => Set<MedicalPrescription>();
    public DbSet<ConsultImage> ConsultImages => Set<ConsultImage>();
    public DbSet<LabOrder> LabOrders => Set<LabOrder>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<RecruitmentApplication> RecruitmentApplications => Set<RecruitmentApplication>();
    public DbSet<CajaShift> CajaShifts => Set<CajaShift>();
    public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();
    public DbSet<ConsultFieldTemplate> ConsultFieldTemplates => Set<ConsultFieldTemplate>();
    public DbSet<ClinicSettings> ClinicSettings => Set<ClinicSettings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<DicomStudy> DicomStudies => Set<DicomStudy>();
    public DbSet<NcfSequence> NcfSequences => Set<NcfSequence>();
    public DbSet<NoShowOutreachLog> NoShowOutreachLogs => Set<NoShowOutreachLog>();
    public DbSet<SatisfactionSurvey> SatisfactionSurveys => Set<SatisfactionSurvey>();
    public DbSet<ControlledSubstanceLog> ControlledSubstanceLogs => Set<ControlledSubstanceLog>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<SupplierPayment> SupplierPayments => Set<SupplierPayment>();
    public DbSet<IdempotencyRequest> IdempotencyRequests => Set<IdempotencyRequest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Rename Identity tables for clarity
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<ApplicationRole>().ToTable("roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("user_tokens");

        // Force every DateTime/DateTime? property to UTC on materialization.
        // Without this the GraphQL DateTime scalar throws on Unspecified Kind values.
        var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        var utcNullableConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var prop in entityType.GetProperties())
            {
                if (prop.ClrType == typeof(DateTime)) prop.SetValueConverter(utcConverter);
                else if (prop.ClrType == typeof(DateTime?)) prop.SetValueConverter(utcNullableConverter);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Core.Domain.Entities.SharedEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
