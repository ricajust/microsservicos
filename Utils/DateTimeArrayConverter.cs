using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DateTimeArrayConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            if (reader.Read() && reader.TokenType == JsonTokenType.Number)
            {
                int year = reader.GetInt32();
                if (reader.Read() && reader.TokenType == JsonTokenType.Number)
                {
                    int month = reader.GetInt32();
                    if (reader.Read() && reader.TokenType == JsonTokenType.Number)
                    {
                        int day = reader.GetInt32();
                        if (reader.Read() && reader.TokenType == JsonTokenType.EndArray) // Adicionado esta verificação
                        {
                            return new DateTime(year, month, day);
                        }
                    }
                }
            }
            throw new JsonException("Formato de data inválido (esperado array de 3 números: [ano, mês, dia]).");
        }

        // Se não for um array, tenta desserializar da forma padrão (pode ser null ou em outro formato)
        return JsonSerializer.Deserialize<DateTime?>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Value.Year);
            writer.WriteNumberValue(value.Value.Month);
            writer.WriteNumberValue(value.Value.Day);
            writer.WriteEndArray();
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}