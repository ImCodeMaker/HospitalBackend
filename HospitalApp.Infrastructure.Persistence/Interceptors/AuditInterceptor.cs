using System.Text.Json;
using HospitalApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HospitalApp.Infrastructure.Persistence.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> _auditedTables =
    [
        nameof(Patient), nameof(Consult), nameof(Invoice), nameof(Payment),
        nameof(MedicalPrescription), nameof(LabOrder), nameof(LabResult),
        nameof(Medication), nameof(Employee), nameof(PayrollRecord)
    ];

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, ct);
        AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private static void AddAuditEntries(DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                        && _auditedTables.Contains(e.Entity.GetType().Name))
            .ToList();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "INSERT",
                EntityState.Modified => "UPDATE",
                EntityState.Deleted => "DELETE",
                _ => "UNKNOWN"
            };

            var recordId = entry.Properties
                .FirstOrDefault(p => p.Metadata.Name == "Id")?.CurrentValue;

            if (recordId is not Guid id) continue;

            string? oldValues = null;
            string? newValues = null;

            if (entry.State == EntityState.Modified)
            {
                var old = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                var current = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                oldValues = JsonSerializer.Serialize(old, _jsonOptions);
                newValues = JsonSerializer.Serialize(current, _jsonOptions);
            }
            else if (entry.State == EntityState.Added)
            {
                var current = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newValues = JsonSerializer.Serialize(current, _jsonOptions);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var current = entry.Properties
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                oldValues = JsonSerializer.Serialize(current, _jsonOptions);
            }

            context.Set<AuditLog>().Add(new AuditLog
            {
                TableName = entry.Entity.GetType().Name,
                RecordId = id,
                Action = action,
                ChangedAt = DateTime.UtcNow,
                OldValues = oldValues,
                NewValues = newValues,
            });
        }
    }
}
