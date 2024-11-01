using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace AICBank.Core.Util;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected a string to parse DateTime.");
        }

        var dateString = reader.GetString();

        // Tenta converter com o formato especificado
        if (DateTime.TryParseExact(dateString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return date;
        }

        throw new JsonException($"Unable to parse '{dateString}' as a DateTime with the format '{DateFormat}'.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat, CultureInfo.InvariantCulture));
    }
}
