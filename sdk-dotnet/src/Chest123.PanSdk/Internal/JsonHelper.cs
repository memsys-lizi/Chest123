using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chest123.PanSdk.Internal;

internal static class JsonHelper
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    internal static string ToQueryString(object? query)
    {
        if (query is null) return string.Empty;
        var values = new List<string>();
        if (query is IEnumerable<KeyValuePair<string, object?>> pairs)
        {
            foreach (var pair in pairs) Add(values, pair.Key, pair.Value);
        }
        else
        {
            foreach (var property in query.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var name = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
                Add(values, name, property.GetValue(query));
            }
        }

        return values.Count == 0 ? string.Empty : "?" + string.Join("&", values);
    }

    private static void Add(List<string> values, string key, object? value)
    {
        if (value is null) return;
        var text = value switch
        {
            bool b => b ? "true" : "false",
            DateTimeOffset dto => dto.ToString("O"),
            DateTime dt => dt.ToString("O"),
            _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)
        };
        if (string.IsNullOrEmpty(text)) return;
        values.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(text)}");
    }
}
