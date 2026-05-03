namespace HospitalApp.Core.Domain.Entities;

public class CajaShift : SharedEntity
{
    public required Guid OpenedByUserId { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public required decimal OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? ExpectedBalance { get; set; }
    public decimal? Discrepancy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsOpen { get; set; } = true;
    public string? Notes { get; set; }

    public ICollection<CashTransaction> Transactions { get; set; } = [];
}
