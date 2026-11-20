using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Healthcare;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] Formats =
    {
        "hh:mm tt",   // 01:00 AM
        "h:mm tt",    // 1:00 AM
        "HH:mm",      // 13:00
        "H:mm"        // 9:00
    };

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString()!;
        if (TimeOnly.TryParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        throw new JsonException($"Cannot convert '{value}' to TimeOnly. Use formats like '01:00 AM' or '13:00'");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("hh:mm tt"));
    }
}