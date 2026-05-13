using System.Globalization;
using HotChocolate.Language;
using HotChocolate.Types;

namespace HospitalApp.WebAPI.GraphQL.Types;

/// <summary>
/// DateTime scalar that tolerates non-UTC values by treating Unspecified Kind as UTC
/// and converting Local to UTC during serialization. Prevents HotChocolate from
/// throwing when entities return DateTimes with mixed Kind values.
/// </summary>
public class FlexibleDateTimeType : ScalarType<DateTime, StringValueNode>
{
    private const string Format = "yyyy-MM-ddTHH:mm:ss.fffZ";

    public FlexibleDateTimeType() : base("DateTime")
    {
        Description = "ISO-8601 timestamp. Tolerates Unspecified/Local DateTime values.";
    }

    protected override DateTime ParseLiteral(StringValueNode valueSyntax)
    {
        if (DateTime.TryParse(valueSyntax.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var d))
            return d;
        throw new SerializationException("Could not parse DateTime literal.", this);
    }

    protected override StringValueNode ParseValue(DateTime runtimeValue)
        => new(Normalize(runtimeValue).ToString(Format, CultureInfo.InvariantCulture));

    public override IValueNode ParseResult(object? resultValue) => resultValue switch
    {
        null => NullValueNode.Default,
        DateTime d => new StringValueNode(Normalize(d).ToString(Format, CultureInfo.InvariantCulture)),
        string s => new StringValueNode(s),
        _ => throw new SerializationException("Could not serialize DateTime.", this),
    };

    public override bool TrySerialize(object? runtimeValue, out object? resultValue)
    {
        if (runtimeValue is null) { resultValue = null; return true; }
        if (runtimeValue is DateTime d)
        {
            resultValue = Normalize(d).ToString(Format, CultureInfo.InvariantCulture);
            return true;
        }
        resultValue = null;
        return false;
    }

    public override bool TryDeserialize(object? resultValue, out object? runtimeValue)
    {
        if (resultValue is null) { runtimeValue = null; return true; }
        if (resultValue is DateTime d) { runtimeValue = Normalize(d); return true; }
        if (resultValue is string s && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed))
        {
            runtimeValue = parsed;
            return true;
        }
        runtimeValue = null;
        return false;
    }

    private static DateTime Normalize(DateTime d) => d.Kind switch
    {
        DateTimeKind.Utc => d,
        DateTimeKind.Local => d.ToUniversalTime(),
        _ => DateTime.SpecifyKind(d, DateTimeKind.Utc),
    };
}
