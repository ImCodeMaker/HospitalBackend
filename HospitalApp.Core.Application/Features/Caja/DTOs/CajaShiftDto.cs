namespace HospitalApp.Core.Application.Features.Caja.DTOs;

public class CajaShiftDto
{
    public Guid Id { get; init; }
    public Guid OpenedByUserId { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal? ClosingBalance { get; init; }
    public decimal? ExpectedBalance { get; init; }
    public decimal? Discrepancy { get; init; }
    public bool IsOpen { get; init; }
    public DateTime? ClosedAt { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<CashTransactionDto> Transactions { get; init; } = [];
}

public class CashTransactionDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Description { get; init; }
    public bool IsApproved { get; init; }
    public DateTime CreatedAt { get; init; }
}
