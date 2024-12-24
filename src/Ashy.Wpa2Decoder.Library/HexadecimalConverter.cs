using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ashy.Wpa2Decoder.Library;

public class HexadecimalConverter : JsonConverter<ushort>
{
    public override ushort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the value as a string, then parse it as a hexadecimal ushort
        string hexValue = reader.GetString() ?? string.Empty;
        return Convert.ToUInt16(hexValue, 16); // Convert from hex string to ushort
    }

    public override void Write(Utf8JsonWriter writer, ushort value, JsonSerializerOptions options)
    {
        // Write the value as a hex string
        writer.WriteStringValue(value.ToString("X4"));  // "X4" formats as hex with 4 digits
    }
}