namespace HospitalApp.WebAPI.Extensions;

public static class CsvHelper
{
    public static byte[] ToCsv<T>(IEnumerable<T> items)
    {
        var props = typeof(T).GetProperties();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(string.Join(",", props.Select(p => p.Name)));
        foreach (var item in items)
            sb.AppendLine(string.Join(",", props.Select(p => EscapeCsv(p.GetValue(item)?.ToString()))));
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string? value)
    {
        if (value is null) return string.Empty;
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
