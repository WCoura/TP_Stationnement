using System.Text.Json;
using System.Text.Json.Serialization;

namespace TP_Stationnement.Model;

// À l'écran, on veut l'heure de Montréal. Le JSON part donc avec le "Z"
// de l'UTC, et le navigateur fait la conversion.
public class ConvertisseurDateUtc : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Horloge.CommeUtc(reader.GetDateTime());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Horloge.CommeUtc(value));
    }
}
