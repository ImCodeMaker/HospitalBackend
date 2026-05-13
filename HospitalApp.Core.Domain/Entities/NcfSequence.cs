using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

/// <summary>
/// Tracks the next available NCF sequence number per fiscal receipt type.
/// One row per NcfType. The DGII authorizes a range (CurrentSequence..MaxSequence)
/// that expires on ExpirationDate. When CurrentSequence > MaxSequence the range is
/// exhausted and a new authorization must be requested.
/// </summary>
public class NcfSequence : SharedEntity
{
    public required NcfTypeEnum Type { get; set; }
    public required long CurrentSequence { get; set; }
    public required long MaxSequence { get; set; }
    public required DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; } = true;
}
