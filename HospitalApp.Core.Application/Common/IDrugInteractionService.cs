namespace HospitalApp.Core.Application.Common;

public interface IDrugInteractionService
{
    Task<List<DrugInteractionAlert>> CheckInteractionsAsync(IEnumerable<string> rxcuiList, CancellationToken ct = default);

    /// <summary>Resolve a drug name to its primary RxCUI via RxNorm. Returns null if no match.</summary>
    Task<string?> ResolveRxCuiAsync(string drugName, CancellationToken ct = default);
}

public record DrugInteractionAlert(string Drug1, string Drug2, string Severity, string Description);
