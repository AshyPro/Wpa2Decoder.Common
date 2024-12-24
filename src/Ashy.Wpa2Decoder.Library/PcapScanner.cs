using System.Drawing;
using Ashy.Wpa2Decoder.Library.Models;
using OutbreakLabs.LibPacketGremlin.Extensions;
using OutbreakLabs.LibPacketGremlin.PacketFactories;
using OutbreakLabs.LibPacketGremlin.Packets;
using OutbreakLabs.LibPacketGremlin.Packets.Beacon802_11Support;
using SharpPcap;
using SharpPcap.LibPcap;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Ashy.Wpa2Decoder.Library;

public static class PcapScanner
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Parameters
    {
        public required long TotalBytes { get; init; }
        public required IProgressBar Progress { get; init; }
        public required string PcapFile { get; init; }
    }

    public static PcapSummary ScanFile(Parameters parameters)
    {
        int currentBytes = 0;
        int packetCount = 1;
        var pcapSummary = new PcapSummary();
        
        using var device = new CaptureFileReaderDevice(parameters.PcapFile);
        device.Open(new DeviceConfiguration());
        device.OnPacketArrival += DeviceOnPacketArrival;
        device.Capture();

        void DeviceOnPacketArrival(object s, PacketCapture e)
        {
            bool ValidateTags(int tagsCount, byte[] tagBytes)
            {
                int index = 0;
                for (var i = 0; i < tagsCount; i++)
                {
                    index += 1;//skip tagType byte
                    if (index > tagBytes.Length)
                    {
                        return false;
                    }
                    int length = tagBytes[index];
                    index += length + 1;
                }
                return index == tagBytes.Length;
            } 
            try
            {
                var wifi = IEEE802_11Factory.Instance.ParseAs(e.Data.ToArray());
                if (wifi is { SubType: 8 })
                {
                    if (wifi.Payload is Beacon802_11 beacon)
                    {
                        bool validTagLength = ValidateTags(beacon.Tags.Count, beacon.ToArray().Skip(12).ToArray());
                        if (validTagLength && beacon.Tags.Count > 0 && beacon.Tags[0] is SSIDTag { TagLength: > 0 and <= 32 } tag)
                        {
                            var ssid = Bytes.From(tag.SSIDBytes).UnicodeString;
                            var bssid = Bytes.From(wifi.BSSID).MacString;
                            if (!string.IsNullOrEmpty(ssid) && !ssid.Contains("\\"))
                            {
                                pcapSummary.WifiNetworks.Add(new WifiNetwork(ssid, bssid));
                            }
                        }
                    }

                    if (wifi.Payload is LLC<SNAP> snap)
                    {
                        if (snap.Payload is SNAP<IEEE802_1x> auth)
                        {
                            if (auth.Payload is IEEE802_1x<EapolKey> eapol)
                            {
                                if (eapol.Payload is EapolKey eapolKey)
                                {
                                    var nonce = Bytes.From(eapolKey.Nonce);
                                    var bssid = Bytes.From(wifi.BSSID).MacString;
                                    var destinationMac = Bytes.From(wifi.Destination).MacString;
                                    var sourceMac = Bytes.From(wifi.Source).MacString;
                                    var mic = Bytes.From(eapolKey.MIC);
                                    var keyFlags = eapolKey.KeyInformation;
                                    var wpaKeyData = Bytes.From(eapolKey.Data);
                                    AddHandshakePacket(pcapSummary, packetCount, e.Header.Timeval.Date, Bytes.From(e.Data.ToArray()), bssid,
                                        sourceMac, destinationMac, nonce, mic, wpaKeyData, keyFlags);
                                }
                            }
                        }

                    }
                }

                packetCount++;
                currentBytes += e.Data.Length + 16;
                parameters.Progress.Report(currentBytes, $"{currentBytes / 1000.0:F2}Kb in {packetCount} packets");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), Color.Red);
            }
        }

        parameters.Progress.Report(parameters.TotalBytes, $"{parameters.TotalBytes/1000.0:F2}Kb in {packetCount} packets");
        return pcapSummary;
    }
    
    private static void AddHandshakePacket(PcapSummary pcapSummary, int packetNumber, DateTime timeVal, Bytes packetData, string bssid, string sourceMac, string destinationMac, Bytes nonce, Bytes mic, Bytes wpaKeyData, ushort keyFlags)
    {
        var updatedHandshake = pcapSummary.Handshakes.TryGetValue(bssid, out var existingHandshake) 
            ? existingHandshake 
            : new PcapSummary.HandshakeInfo(bssid);
        var message = new HandshakeKeyInfo(keyFlags).DetectMessage();
        if (message is null)throw new Exception("Invalid handshake message");
        var clientMac = message is HandshakeMessageNo.M1 or HandshakeMessageNo.M3
            ? destinationMac
            : sourceMac;
        if (updatedHandshake.Ssid == string.Empty)
        {
            updatedHandshake.Ssid = pcapSummary.WifiNetworks.FirstOrDefault(x => x.Bssid == updatedHandshake.Bssid)?.Ssid ?? string.Empty;
        }
        updatedHandshake.Packets.Add(new PcapSummary.HandshakeInfo.Packet(message.Value, packetNumber, timeVal, clientMac, 
            new HandshakeKeyInfo(keyFlags), nonce, mic, wpaKeyData, packetData));
        pcapSummary.Handshakes[bssid] = updatedHandshake;
    }


    public static List<PcapSummary.KeyParameters> AnalyzeWhatCanBeCracked(PcapSummary pcapSummary)
    {
        var result = new List<PcapSummary.KeyParameters>();
        foreach (var handshakeDictionary in pcapSummary.Handshakes)
        {
            var handshake = handshakeDictionary.Value;
            foreach (var m2 in handshake.Packets.Where(p => p.Message == HandshakeMessageNo.M2))
            {
                var followingM3Packets = handshake.Packets.Where(p => p.Message == HandshakeMessageNo.M3 && p.PacketNumber > m2.PacketNumber);
                foreach (var m3 in followingM3Packets)
                {
                    var m1 = handshake.Packets.Where(p => p.Message == HandshakeMessageNo.M1
                                                          && m2 != null
                                                          && p.PacketNumber < m2.PacketNumber
                                                          && p.Nonce == m3.Nonce)
                        .OrderBy(p=>p.PacketNumber)
                        .LastOrDefault();
                    if(m1 is null)continue;
                    var m4 = handshake.Packets.Where(p => p.Message == HandshakeMessageNo.M4
                                                          && p.PacketNumber > m3.PacketNumber)
                        .OrderBy(p => p.PacketNumber)
                        .FirstOrDefault();
                    result.Add(new PcapSummary.KeyParameters(handshake.Ssid, handshake.Bssid,  m2.ClientMac, m1.Nonce, m2.Nonce,
                        m2.HandshakeKeyInfo.CipherDescription, m2!.AllPacketBytes, m2.Mic, m3.Mic, m4?.Mic, m2.WpaKeyData, m3.WpaKeyData));
                }
            }
        }
        return result;
    }
}