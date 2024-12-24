using Ashy.Wpa2Decoder.Library.Models;
using JetBrains.Annotations;
using Xunit;

namespace Ashy.Wpa2Decoder.Library.Tests.Models;

[TestSubject(typeof(Bytes))]
public class BytesTest
{
    [Fact]
    public void BytesCompare_Shorter()
    {   
        var bytes1 = Bytes.From([0x01, 0x02, 0x03]);
        var bytes2 = Bytes.From([0x01, 0x02, 0x03, 0x04]);
        
        Assert.True(Bytes.CompareBytes(bytes1.Array, bytes2.Array) < 0);
    }
    
    [Fact]
    public void BytesCompare_Longer()
    {   
        var bytes1 = Bytes.From([0x01, 0x02, 0x03, 0x04]);
        var bytes2 = Bytes.From([0x01, 0x02, 0x03]);
        
        Assert.True(Bytes.CompareBytes(bytes1.Array, bytes2.Array) > 0);
    }

    [Fact]
    public void BytesCompare_Equal()
    {   
        var bytes1 = Bytes.From([0x01, 0x02, 0x03, 0x04]);
        var bytes2 = Bytes.From([0x01, 0x02, 0x03, 0x04]);
        
        Assert.Equal(0, Bytes.CompareBytes(bytes1.Array, bytes2.Array));
    }
    
    [Fact]
    public void BytesCompare_Greater()
    {   
        var bytes1 = Bytes.From([0x01, 0x02, 0x03, 0x04]);
        var bytes2 = Bytes.From([0x01, 0x02, 0x03, 0x00]);
        
        Assert.True(Bytes.CompareBytes(bytes1.Array, bytes2.Array) > 0);
    }
    
    [Fact]
    public void BytesCompare_Smaller()
    {   
        var bytes1 = Bytes.From([0x01, 0x02, 0x00, 0x04]);
        var bytes2 = Bytes.From([0x01, 0x02, 0x03, 0x04]);
        
        Assert.True(Bytes.CompareBytes(bytes1.Array, bytes2.Array) < 0);
    }

    [Fact]
    public void BytesEqual_Equal()
    {
        var bytes1 = Bytes.From([0x01, 0x02, 0x03, 0x04]);
        var bytes2 = Bytes.From([0x01, 0x02, 0x03, 0x04]);
        
        Assert.Equal(bytes1, bytes2);
    }
}