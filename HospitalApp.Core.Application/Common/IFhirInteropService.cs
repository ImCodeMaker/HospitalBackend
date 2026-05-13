namespace HospitalApp.Core.Application.Common;

/// <summary>
/// HL7 FHIR R4 interoperability layer.
/// Maps internal entities → FHIR resources for SISALRIL / SRS national network exchange.
/// Stub today — pin real FHIR endpoint + OAuth client credentials later.
/// </summary>
public interface IFhirInteropService
{
    Task<string> ExportPatientAsync(Guid patientId, CancellationToken ct = default);
    Task<string> ExportConsultAsync(Guid consultId, CancellationToken ct = default);
    Task<bool> PushToRegistryAsync(string fhirBundleJson, CancellationToken ct = default);
}
