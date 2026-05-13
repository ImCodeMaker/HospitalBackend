using HospitalApp.Core.Application.Common;
using Microsoft.Extensions.Logging;

namespace HospitalApp.Infrastructure.Shared.Services;

/// <summary>
/// Stub WhatsApp service. Logs only. Replace with TwilioWhatsAppService once a WA-approved sender + templates are provisioned.
/// </summary>
public class StubWhatsAppService(ILogger<StubWhatsAppService> logger) : IWhatsAppService
{
    public Task<bool> SendAsync(string toPhoneE164, string templateName, IDictionary<string, string> parameters, CancellationToken ct = default)
    {
        logger.LogInformation("[WhatsApp STUB] template={Template} to={To} params={Params}",
            templateName, toPhoneE164, string.Join(",", parameters.Select(p => $"{p.Key}={p.Value}")));
        return Task.FromResult(true);
    }

    public Task<bool> SendFreeformAsync(string toPhoneE164, string body, CancellationToken ct = default)
    {
        logger.LogInformation("[WhatsApp STUB] freeform to={To} body={Body}", toPhoneE164, body);
        return Task.FromResult(true);
    }
}

/// <summary>Stub e-prescription signer. Returns the payload unchanged with a marker signature.</summary>
public class StubElectronicPrescriptionSigner(ILogger<StubElectronicPrescriptionSigner> logger) : IElectronicPrescriptionSigner
{
    public Task<SignedPrescription> SignAsync(SignPrescriptionRequest req, CancellationToken ct = default)
    {
        logger.LogInformation("[ePrescription STUB] sign prescription={Id}", req.PrescriptionId);
        return Task.FromResult(new SignedPrescription(
            req.PrescriptionId,
            req.PrescriptionXml + "<!-- STUB SIGNATURE -->",
            "STUB-" + Guid.NewGuid().ToString("N"),
            DateTime.UtcNow));
    }

    public Task<string?> SubmitAsync(SignedPrescription signed, CancellationToken ct = default)
    {
        logger.LogInformation("[ePrescription STUB] submit signed prescription {Id}", signed.PrescriptionId);
        return Task.FromResult<string?>("EHEALTH-STUB-" + Guid.NewGuid().ToString("N")[..12]);
    }
}

/// <summary>Stub FHIR interop. Returns minimal Patient/Encounter JSON shapes.</summary>
public class StubFhirInteropService(ILogger<StubFhirInteropService> logger) : IFhirInteropService
{
    public Task<string> ExportPatientAsync(Guid patientId, CancellationToken ct = default)
    {
        logger.LogInformation("[FHIR STUB] export patient {Id}", patientId);
        return Task.FromResult($$"""{"resourceType":"Patient","id":"{{patientId}}"}""");
    }

    public Task<string> ExportConsultAsync(Guid consultId, CancellationToken ct = default)
    {
        logger.LogInformation("[FHIR STUB] export consult {Id}", consultId);
        return Task.FromResult($$"""{"resourceType":"Encounter","id":"{{consultId}}"}""");
    }

    public Task<bool> PushToRegistryAsync(string fhirBundleJson, CancellationToken ct = default)
    {
        logger.LogInformation("[FHIR STUB] push bundle {Size} bytes", fhirBundleJson.Length);
        return Task.FromResult(true);
    }
}

/// <summary>Stub DGII e-CF submission service. Marks every invoice as Accepted locally.</summary>
public class StubDgiiEcfService(ILogger<StubDgiiEcfService> logger) : IDgiiEcfService
{
    public Task<EcfSubmissionResult> SubmitAsync(EcfSubmissionRequest req, CancellationToken ct = default)
    {
        logger.LogInformation("[DGII e-CF STUB] submit NCF={Ncf} invoice={Id}", req.Ncf, req.InvoiceId);
        return Task.FromResult(new EcfSubmissionResult(true, "STUB-" + Guid.NewGuid().ToString("N")[..16], null));
    }

    public Task<EcfStatus> CheckStatusAsync(string trackId, CancellationToken ct = default)
    {
        logger.LogInformation("[DGII e-CF STUB] check status {TrackId}", trackId);
        return Task.FromResult(new EcfStatus("Accepted", null, DateTime.UtcNow));
    }
}
