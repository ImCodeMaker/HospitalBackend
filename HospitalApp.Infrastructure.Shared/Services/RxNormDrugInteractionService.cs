using System.Text.Json;
using HospitalApp.Core.Application.Common;
using Microsoft.Extensions.Logging;

namespace HospitalApp.Infrastructure.Shared.Services;

public class RxNormDrugInteractionService(
    IHttpClientFactory httpClientFactory,
    ILogger<RxNormDrugInteractionService> logger) : IDrugInteractionService
{
    private const string BaseUrl = "https://rxnav.nlm.nih.gov/REST/interaction/list.json";
    private const string RxcuiByNameUrl = "https://rxnav.nlm.nih.gov/REST/rxcui.json";

    public async Task<string?> ResolveRxCuiAsync(string drugName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(drugName)) return null;
        try
        {
            var client = httpClientFactory.CreateClient("RxNorm");
            var url = $"{RxcuiByNameUrl}?name={Uri.EscapeDataString(drugName)}&search=2";
            var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("idGroup", out var grp)
                && grp.TryGetProperty("rxnormId", out var ids))
            {
                var first = ids.EnumerateArray().FirstOrDefault();
                if (first.ValueKind == JsonValueKind.String)
                    return first.GetString();
            }
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "RxCUI lookup failed for {Drug}", drugName);
            return null;
        }
    }

    public async Task<List<DrugInteractionAlert>> CheckInteractionsAsync(
        IEnumerable<string> rxcuiList,
        CancellationToken ct = default)
    {
        var rxcuis = rxcuiList.ToList();
        if (rxcuis.Count < 2)
            return [];

        var url = $"{BaseUrl}?rxcuis={string.Join("+", rxcuis)}";

        try
        {
            var client = httpClientFactory.CreateClient("RxNorm");
            var response = await client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("RxNorm API returned {StatusCode} for URL {Url}", response.StatusCode, url);
                return [];
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var alerts = new List<DrugInteractionAlert>();

            if (!doc.RootElement.TryGetProperty("fullInteractionTypeGroup", out var groups))
                return alerts;

            foreach (var group in groups.EnumerateArray())
            {
                if (!group.TryGetProperty("fullInteractionType", out var types))
                    continue;

                foreach (var type in types.EnumerateArray())
                {
                    // Extract drug names from minConcept array
                    string drug1 = string.Empty;
                    string drug2 = string.Empty;

                    if (type.TryGetProperty("minConcept", out var concepts))
                    {
                        var conceptList = concepts.EnumerateArray().ToList();
                        if (conceptList.Count >= 1)
                            drug1 = conceptList[0].TryGetProperty("name", out var n1) ? n1.GetString() ?? string.Empty : string.Empty;
                        if (conceptList.Count >= 2)
                            drug2 = conceptList[1].TryGetProperty("name", out var n2) ? n2.GetString() ?? string.Empty : string.Empty;
                    }

                    if (!type.TryGetProperty("interactionPair", out var pairs))
                        continue;

                    foreach (var pair in pairs.EnumerateArray())
                    {
                        var description = pair.TryGetProperty("description", out var desc)
                            ? desc.GetString() ?? string.Empty
                            : string.Empty;

                        var severity = pair.TryGetProperty("severity", out var sev)
                            ? sev.GetString() ?? string.Empty
                            : string.Empty;

                        // Try to get drug names from pair's interactionConcept if not already set
                        var pairDrug1 = drug1;
                        var pairDrug2 = drug2;

                        if (pair.TryGetProperty("interactionConcept", out var interConcepts))
                        {
                            var icList = interConcepts.EnumerateArray().ToList();
                            if (icList.Count >= 1 && icList[0].TryGetProperty("minConceptItem", out var ic1))
                                pairDrug1 = ic1.TryGetProperty("name", out var icn1) ? icn1.GetString() ?? drug1 : drug1;
                            if (icList.Count >= 2 && icList[1].TryGetProperty("minConceptItem", out var ic2))
                                pairDrug2 = ic2.TryGetProperty("name", out var icn2) ? icn2.GetString() ?? drug2 : drug2;
                        }

                        alerts.Add(new DrugInteractionAlert(pairDrug1, pairDrug2, severity, description));
                    }
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to check drug interactions via RxNorm API for RxCUIs: {RxCuis}",
                string.Join(", ", rxcuis));
            return [];
        }
    }
}
