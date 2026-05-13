namespace HospitalApp.Core.Application.Common;

/// <summary>
/// Digital-signature service for e-prescriptions per DR eHealth (SISALRIL) standard.
/// Doctor's certificate (.p12) signs an XML payload describing the prescription.
/// Stub today — wire actual DR eHealth gateway + certificate loading later.
/// </summary>
public interface IElectronicPrescriptionSigner
{
    /// <summary>Sign a prescription XML payload with the doctor's certificate. Returns signed XML.</summary>
    Task<SignedPrescription> SignAsync(SignPrescriptionRequest req, CancellationToken ct = default);

    /// <summary>Submit the signed prescription to the national eHealth registry. Returns the issued tracking number.</summary>
    Task<string?> SubmitAsync(SignedPrescription signed, CancellationToken ct = default);
}

public record SignPrescriptionRequest(
    Guid PrescriptionId,
    Guid DoctorId,
    string DoctorCertificateThumbprint,
    string PrescriptionXml);

public record SignedPrescription(
    Guid PrescriptionId,
    string SignedXml,
    string SignatureValue,
    DateTime SignedAt);
