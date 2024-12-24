using System.Text;
using System.Text.Json.Serialization;

namespace Ashy.Wpa2Decoder.Library.Models;

public record Bytes(byte[] Array)
{
    public static Bytes From(byte[] bytes) => new(bytes);
    public static Bytes From(IEnumerable<byte> bytes) => new(bytes.ToArray());
    
    /// Any hex string format is accepted
    public static Bytes From(string hexString) => new(HexStringToByteArray(hexString));
    
    public static Bytes Empty => From([]);
    
    [JsonIgnore]
    public bool IsEmpty => Array.Length == 0 || Array.All(b => b == 0);
    
    [JsonIgnore]
    public string UnicodeString => Encoding.UTF8.GetString(Array);

    /// String format XX:XX:XX:XX:XX:XX
    [JsonIgnore]
    public string MacString => string.Join(":", Array.Select(b => b.ToString("X2")));
    
    /// String format XXXXXXXXXXXX
    [JsonIgnore]
    public string HexString => Array.Select(b => b.ToString("X2")).Aggregate((a, b) => a + b);
   
    /// String format XX XX XX XX XX XX
    [JsonIgnore]
    public string HexSpaceString => string.Join(" ", Array.Select(b => b.ToString("X2")));

    public override string ToString() => HexSpaceString;

    public virtual bool Equals(Bytes? other)
    {
        if (other is null) return false;
        return CompareBytes(Array, other.Array) == 0;
    }
    
    public virtual bool Equals(byte[]? other)
    {
        if (other is null) return false;
        return CompareBytes(Array, other) == 0;
    }
    //
    // public override bool Equals(object? other)
    // {
    //     if (other is null) return false;
    //     if (other is byte[] bytesArray) return Equals(bytesArray);
    //     if (other is Bytes bytes) return Equals(bytes);
    //     return Equals(Array, other);
    // }
    
    // Helper method to compare byte arrays (returns negative, zero, or positive value)
    public static int CompareBytes(byte[] array1, byte[] array2)
    {
        // First, compare based on length
        if (array1.Length < array2.Length) return -1;
        if (array1.Length > array2.Length) return 1;
        
        int minLength = Math.Min(array1.Length, array2.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (array1[i] < array2[i]) return -1;
            if (array1[i] > array2[i]) return 1;
        }

        return 0; // Arrays are equal
    }

    // Override GetHashCode for consistency with Equals and == operator
    public override int GetHashCode()
    {
        return UnicodeString?.GetHashCode() ?? 0;
    }
    
    // Method to convert a hex string to a byte array
    public static byte[] HexStringToByteArray(string hexString)
    {
        hexString = hexString.Replace(" ", "").Replace(":","");
        // Ensure the hex string length is even
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException("Hex string must have an even length.");
        }

        byte[] byteArray = new byte[hexString.Length / 2];
        
        for (int i = 0; i < hexString.Length; i += 2)
        {
            byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
        }

        return byteArray;
    }
}