namespace HospitalApp.Core.Application.Common;

/// <summary>
/// Validates and normalizes identity documents (national IDs + passports)
/// for a given country.
/// </summary>
public interface IDocumentValidationService
{
    /// <summary>Returns the catalog of supported countries (ISO 3166-1).</summary>
    IReadOnlyList<CountryInfo> GetCountries();

    /// <summary>Validates a document. Always returns a result — never throws.</summary>
    DocumentValidationResult Validate(DocumentValidationRequest request);
}

public sealed record CountryInfo(
    string Code,           // ISO 3166-1 alpha-2
    string NameEs,
    string NameEn,
    string NationalityEs,
    bool HasNationalIdRule,
    bool HasPassportRule,
    string? NationalIdExample,
    string? PassportExample);

public sealed record DocumentValidationRequest(
    string DocumentType,   // "NationalId" | "Passport" | "Other"
    string Value,
    string CountryCode);   // ISO 3166-1 alpha-2

public sealed record DocumentValidationResult(
    bool Ok,
    string? Normalized,
    string? Error,
    string? Algorithm);    // human-readable algorithm name (e.g. "DGII Luhn", "Verhoeff")
