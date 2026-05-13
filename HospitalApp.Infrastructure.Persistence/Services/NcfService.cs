using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalApp.Infrastructure.Persistence.Services;

/// <summary>
/// Issues NCF sequence numbers using row-level locking inside a transaction
/// so two concurrent invoice creations cannot reserve the same number.
/// </summary>
public class NcfService(ApplicationDbContext db, ILogger<NcfService> logger) : INcfService
{
    public async Task<string?> ReserveNextAsync(NcfTypeEnum type, CancellationToken ct = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var typeId = (int)type;
        var seq = await db.NcfSequences
            .FromSqlInterpolated($"""
                SELECT * FROM "NcfSequences"
                WHERE "Type" = {typeId} AND "IsActive" = TRUE
                FOR UPDATE
            """)
            .FirstOrDefaultAsync(ct);

        if (seq is null)
        {
            logger.LogWarning("No active NCF range configured for type {Type}", type);
            return null;
        }

        if (DateTime.UtcNow > seq.ExpirationDate)
        {
            logger.LogWarning("NCF range for {Type} expired on {Date}", type, seq.ExpirationDate);
            return null;
        }

        if (seq.CurrentSequence > seq.MaxSequence)
        {
            logger.LogWarning("NCF range for {Type} exhausted at {Max}", type, seq.MaxSequence);
            return null;
        }

        var assigned = seq.CurrentSequence;
        seq.CurrentSequence++;
        seq.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var ncf = $"{type.GetPrefix()}{assigned:D8}";
        logger.LogInformation("Reserved NCF {Ncf} for type {Type}", ncf, type);
        return ncf;
    }
}
