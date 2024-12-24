using System.Security.Cryptography;
using System.Text;
using Ashy.Wpa2Decoder.Library.Models;

namespace Ashy.Wpa2Decoder.Library;

public class Wpa2Crypto
{
    // Generate Pairwise Master Key (PMK) from passphrase and SSID using PBKDF2
    public static byte[] GeneratePmk(string passphrase, string ssid)
    {
        // Convert the passphrase and SSID to byte arrays
        byte[] passphraseBytes = Encoding.UTF8.GetBytes(passphrase);
        byte[] ssidBytes = Encoding.UTF8.GetBytes(ssid);

        // Use PBKDF2 to generate the PMK
        using (var pbkdf2 =
               new Rfc2898DeriveBytes(passphraseBytes, ssidBytes, 4096, HashAlgorithmName.SHA1)) // 4096 iterations as per WPA2 spec
        {
            // PBKDF2 will return the derived key. The length of the PMK is 256 bits (32 bytes)
            return pbkdf2.GetBytes(32);
        }
    }

    internal static void Hash_SHA1(byte[] key, byte[] data, byte[] output, int outputOffset)
    {
        using (var hmac = new HMACSHA1(key))
        {
            hmac.Initialize();
            hmac.TransformBlock(data, 0, data.Length, data, 0);
            hmac.TransformFinalBlock([], 0, 0);
            Array.Copy(hmac.Hash!, 0, output, outputOffset, hmac.Hash!.Length);
        }
    }

    // private static void MAC_HMAC_MD5(int keySize, byte[] key, int dataSize, byte[] data, byte[] output)
    // {
    //     using (var hmac = new HMACMD5(key))
    //     {
    //         hmac.TransformBlock(data, 0, dataSize, data, 0);
    //         Array.Copy(hmac.Hash!, 0, output, 0, keySize);
    //     }
    // }
    
    public static byte[] GeneratePtk(byte[] stmac, byte[] bssid, byte[] anonce, byte[] snonce, byte[] pmk)
    {
        // Create a Span<byte> buffer for the pke (total 100 bytes)
        Span<byte> pke = new byte[100];

        // Get the byte array for the string "Pairwise key expansion\0" (23 bytes)
        byte[] header = Encoding.ASCII.GetBytes("Pairwise key expansion\0");

        // Copy the header into the pke buffer (first 23 bytes)
        header.CopyTo(pke);

        // Efficiently copy MAC addresses and nonces based on comparison
        if (CompareBytes(stmac, bssid) < 0)
        {
            stmac.CopyTo(pke.Slice(23, 6));   // Copy stmac to pke[23..29]
            bssid.CopyTo(pke.Slice(29, 6));   // Copy bssid to pke[29..35]
        }
        else
        {
            bssid.CopyTo(pke.Slice(23, 6));  // Copy bssid to pke[23..29]
            stmac.CopyTo(pke.Slice(29, 6));  // Copy stmac to pke[29..35]
        }

        if (CompareBytes(snonce, anonce) < 0)
        {
            snonce.CopyTo(pke.Slice(35, 32)); // Copy snonce to pke[35..67]
            anonce.CopyTo(pke.Slice(67, 32)); // Copy anonce to pke[67..99]
        }
        else
        {
            anonce.CopyTo(pke.Slice(35, 32)); // Copy anonce to pke[35..67]
            snonce.CopyTo(pke.Slice(67, 32)); // Copy snonce to pke[67..99]
        }

        const int digestSha1MacLen = 20;
        byte[] ptk = new byte[80];

        // Four iterations to generate PTK
        for (int i = 0; i < 4; i++)
        {
            pke[99] = (byte)i; // Set iteration value
            Hash_SHA1(pmk, pke.ToArray(), ptk, i * digestSha1MacLen);
        }

        return ptk;
    }

    // Helper method to compare byte arrays (returns negative, zero, or positive value)
    public static int CompareBytes(byte[] array1, byte[] array2)
    {
        int minLength = Math.Min(array1.Length, array2.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (array1[i] < array2[i]) return -1;
            if (array1[i] > array2[i]) return 1;
        }

        return 0; // Arrays are equal
    }
    
    public static bool Test(string password, PcapSummary.KeyParameters keyParameters)
    {
        // Generate PMK directly using the password and SSID
        var pmk = GeneratePmk(password, keyParameters.Ssid);

        // Generate PTK using spans (avoid allocations for Mac/BSSID)
        var clientMac = Bytes.From(keyParameters.ClientMac).Array;
        var bssid = Bytes.From(keyParameters.Bssid).Array;
        var anonce = keyParameters.ANonce.Array;
        var snonce = keyParameters.SNonce.Array;
        var ptk = GeneratePtk(clientMac, bssid, anonce, snonce, pmk);

        // Create a Span to directly access the required part of M2Data without allocating extra arrays
        var eapol = keyParameters.M2Data.Array.AsSpan(34, keyParameters.M2Data.Array.Length - 38);  // Skip first 34 bytes and last 4 bytes

        // Generate MIC for M2 directly
        var micM2 = GenerateMicForMessage(ptk, eapol.ToArray());

        // Compare MIC directly with M2Mic
        return Bytes.CompareBytes(keyParameters.M2Mic.Array, micM2) == 0;
    }

    public static byte[] GenerateMicForMessage(byte[] ptk, byte[] eapol)
    {
        // Ensure we don't create unnecessary byte arrays
        var kck = ptk.AsSpan(0, 16);  // KCK is the first 16 bytes of PTK
    
        // EAPOL message processing
        var eapolSpan = eapol.AsSpan();
        var bytesBeforeMic = eapolSpan[..81];  // First 81 bytes
        var bytesAfterMic = eapolSpan[97..];   // After the 97th byte (81 + 16)

        // Clear MIC (replace MIC portion with zeros)
        Span<byte> clearEapol = new byte[bytesBeforeMic.Length + 16 + bytesAfterMic.Length];
        bytesBeforeMic.CopyTo(clearEapol);
        new Span<byte>(clearEapol.Slice(bytesBeforeMic.Length, 16).ToArray(), 0, 16).Clear();  // MIC part cleared with zeros
        bytesAfterMic.CopyTo(clearEapol.Slice(bytesBeforeMic.Length + 16));

        // Hash the KCK with the cleared EAPOL message
        var mic = new byte[20];
        Hash_SHA1(kck.ToArray(), clearEapol.ToArray(), mic, 0);
    
        // Return only the first 16 bytes of the MIC
        return mic.Take(16).ToArray();
    }
}