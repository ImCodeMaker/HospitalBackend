using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Application.Common;

/// <summary>
/// Issues DGII NCF (Número de Comprobante Fiscal) sequence numbers.
/// </summary>
public interface INcfService
{
    /// <summary>
    /// Atomically reserves and returns the next NCF for the given type.
    /// Returns null when the authorized range is exhausted or expired.
    /// </summary>
    Task<string?> ReserveNextAsync(NcfTypeEnum type, CancellationToken ct = default);
}
