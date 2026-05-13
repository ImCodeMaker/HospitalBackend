namespace HospitalApp.Core.Domain.Enums;

/// <summary>
/// NCF (Número de Comprobante Fiscal) types per DGII Dominican Republic.
/// </summary>
public enum NcfTypeEnum
{
    /// <summary>B01 — Crédito Fiscal (for ITBIS-registered businesses).</summary>
    CreditoFiscal = 1,
    /// <summary>B02 — Consumo (default for general public).</summary>
    Consumo = 2,
    /// <summary>B14 — Régimen Especial.</summary>
    RegimenEspecial = 14,
    /// <summary>B15 — Gubernamental.</summary>
    Gubernamental = 15,
    /// <summary>B16 — Exportaciones.</summary>
    Exportaciones = 16,
}

public static class NcfTypeExtensions
{
    public static string GetPrefix(this NcfTypeEnum type) => type switch
    {
        NcfTypeEnum.CreditoFiscal => "B01",
        NcfTypeEnum.Consumo => "B02",
        NcfTypeEnum.RegimenEspecial => "B14",
        NcfTypeEnum.Gubernamental => "B15",
        NcfTypeEnum.Exportaciones => "B16",
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };
}
