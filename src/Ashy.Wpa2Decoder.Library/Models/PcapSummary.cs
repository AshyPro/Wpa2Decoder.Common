

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global as setters required for deserialization

namespace Ashy.Wpa2Decoder.Library.Models;

public record PcapSummary
{
    public record HandshakeInfo(string Bssid) {
        
        public record Packet(HandshakeMessageNo Message, int PacketNumber, DateTime TimeVal, string ClientMac, HandshakeKeyInfo HandshakeKeyInfo, Bytes Nonce, Bytes Mic, Bytes WpaKeyData, Bytes AllPacketBytes);

        public List<Packet> Packets { get; init; } = [];
        public string Ssid { get; set; } = string.Empty;
        public string CapturedMessages=>string.Join(",", Packets.Select(p => p.Message.ToString()));
    }

    public record KeyParameters(
        string Ssid,
        string Bssid,
        string ClientMac,
        Bytes ANonce,
        Bytes SNonce,
        string Cipher,
        Bytes M2Data,
        Bytes M2Mic,
        Bytes? M3Mic,
        Bytes? M4Mic,
        Bytes? M2WpaKeyData,
        Bytes? M3WpaKeyData);

    public Dictionary<string, HandshakeInfo> Handshakes { get; init; } = [];
    public List<WifiNetwork> WifiNetworks { get; init; } = [];
    public List<KeyParameters> KeyParametersList { get; set; } = [];
}