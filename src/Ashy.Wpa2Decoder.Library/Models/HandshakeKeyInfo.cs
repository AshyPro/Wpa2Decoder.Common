using System.Text.Json.Serialization;

namespace Ashy.Wpa2Decoder.Library.Models;

public record HandshakeKeyInfo(ushort KeyFlags)
{
    public HandshakeMessageNo? DetectMessage()
    {
        return (IsBit(0x0080, KeyFlags), //ASK
                IsBit(0x0100, KeyFlags), //MIC
                IsBit(0x1000, KeyFlags), //Encrypt
                IsBit(0x0300, KeyFlags)) switch
            {
                (true, false, false, false) => HandshakeMessageNo.M1,
                (false, true, false, false) => HandshakeMessageNo.M2,
                (true, true, true, true) => HandshakeMessageNo.M3,
                (false, true, false, true) => HandshakeMessageNo.M4,
                _ => null
            };
    }

    public string CipherDescription => (KeyFlags & 0x7) switch
    {
        1 => "1:WEP (Wired Equivalent Privacy) with CRC32 Checksum (Deprecated)",
        2 => "2:AES (CCMP) with HMAC-SHA1 MIC (WPA2)",
        3 => "3:AES (GCM) for Encryption and Integrity (WPA3)",
        4 => "4:TKIP (Temporal Key Integrity Protocol) with MIC (WPA)",
        _ => "Invalid Key Descriptor Version"
    };

    // ReSharper disable once RedundantExplicitPositionalPropertyDeclaration
    [JsonConverter(typeof(HexadecimalConverter))]
    public ushort KeyFlags { get; init; } = KeyFlags;

    private static bool IsBit(ushort mask, ushort data) => (mask & data) == mask;
}