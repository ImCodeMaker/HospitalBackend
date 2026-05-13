namespace HospitalApp.Core.Application.Common;

/// <summary>
/// DGII e-CF (Comprobante Fiscal Electrónico) integration.
/// Builds the e-CF XML envelope, signs with the company digital certificate,
/// pushes to DGII's SOAP web service, and persists the tracking code (TrackId).
/// Stub today — operates only as a local sequence (see <see cref="INcfService"/>).
/// </summary>
public interface IDgiiEcfService
{
    Task<EcfSubmissionResult> SubmitAsync(EcfSubmissionRequest req, CancellationToken ct = default);
    Task<EcfStatus> CheckStatusAsync(string trackId, CancellationToken ct = default);
}

public record EcfSubmissionRequest(
    Guid InvoiceId,
    string Ncf,
    string SignedXml);

public record EcfSubmissionResult(
    bool Accepted,
    string? TrackId,
    string? RejectionReason);

public record EcfStatus(
    string Status, // Accepted / Rejected / InProcess
    string? Reason,
    DateTime CheckedAt);
