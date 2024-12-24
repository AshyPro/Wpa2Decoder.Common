using System.Linq;
using Ashy.Wpa2Decoder.Library.Models;
using JetBrains.Annotations;
using Xunit;

namespace Ashy.Wpa2Decoder.Library.Tests;

[TestSubject(typeof(Wpa2Crypto))]
public class Wpa2CryptoTest
{

     [Fact]
    public void GeneratePmk()
    {
         string passPhrase = "10zZz10ZZzZ";
         string ssid = "Netgear 2/158";
        
         string expectedPmk = "01b809f9ab2fb5dc47984f52fb2d112e13d84ccb6b86d4a7193ec5299f851c48";
         
         var actualPmk = Wpa2Crypto.GeneratePmk(passPhrase, ssid);
         
         Assert.Equal(Bytes.From(expectedPmk), Bytes.From(actualPmk));
    }

    [Fact]
    public void GenerateInvalidPtk()
    {
        var clientMac = Bytes.From("001346FE320C");
        var bssid = Bytes.From("00146C7E4080");
        //var snonce = Bytes.From("59168BC3A5DF18D71EFB6423F340088DAB9E1BA2BBC58659E07B3764B0DE8570");
        var zeroSnonce = Bytes.From("0000000000000000000000000000000000000000000000000000000000000000");
        var anonce = Bytes.From("225854B0444DE3AF06D1492B852984F04CF6274C0E3218B8681756864DB7A055");
        var pmk = Bytes.From("EE51883793A6F68E9615FE73C80A3AA6F2DD0EA537BCE627B929183CC6E57925");

        var oPtk = Bytes.From(
            "0DDEAE8083F92CA9AFDB250DDEE5251BC0EEB47EF22AF79E25346E8B73E2CA7D94B0605F2EED66D86076B338A665FEE39FDE221EB1386B3DA7AC6ABE7EE0001FBD92ABECC8BA49F05DFF8F501EFAAACC");

        var actualPtk = Wpa2Crypto.GeneratePtk(clientMac.Array, bssid.Array, anonce.Array, zeroSnonce.Array,
            pmk.Array);
         
        Assert.Equal(oPtk, Bytes.From(actualPtk));
    }
    
    [Fact]
    public void GenerateValidPtk()
    {
        var clientMac = Bytes.From("001346FE320C");
        var bssid = Bytes.From("00146C7E4080");
        var snonce = Bytes.From("59168BC3A5DF18D71EFB6423F340088DAB9E1BA2BBC58659E07B3764B0DE8570");
        var anonce = Bytes.From("225854B0444DE3AF06D1492B852984F04CF6274C0E3218B8681756864DB7A055");
        var pmk = Bytes.From("EE51883793A6F68E9615FE73C80A3AA6F2DD0EA537BCE627B929183CC6E57925");
        var ePtk =
            Bytes.From(
                "EA0E404633C802450302868CCAA749DE5CBA5ABCB267E2DE1D5E21E57ACCD5079B31E9FF220E132AE4F6ED9EF1ACC88545825FC32EE55961395AE43734D6C10798EF5AFE42C07426471868A577D4D17E");

        var actualPtk = Wpa2Crypto.GeneratePtk(clientMac.Array, bssid.Array, anonce.Array, snonce.Array,
            pmk.Array);
         
        Assert.Equal(ePtk, Bytes.From(actualPtk));
    }
    
    // string pmkHex = "C0BC1A9478FE305EF4EFDAF40D7BB7CFB17E9609A5E29B08861A90492C502EC6";
    // //string bssid = "Praneeth";
    // string bssidHex = "60E327F814A0";
    // string clientMacHex = "C0F4E64B6ACF";
    // string anonceHex = "ac9871c9ca129468708ca0d554e22f4f8b6eaa6dbaa121d2233bf33cbc29d346";
    // string snonceHex = "5214c4dbe4a567e78b8f30b2b016a2d90ea50c27d408614c1fc0a0934a889ada";
    // string expectedPtk =
    //     "fb18560e63909f84f31d39da03a5d82fdc78c3b56f1870544308b84dee2144b87615729c4884a545d392c20b3f697025633245fc5a0fa15efeb0c82501f3a7b4";
    
    
        
    // string passPhrase = "10zZz10ZZzZ";
    // string ssid = "Netgear 2/158";
    // var APmac = Bytes.From("001e2ae0bdd0");
    // var Clientmac = Bytes.From("cc08e0620bc8");
    // var ANonce = Bytes.From("61c9a3f5cdcdf5fae5fd760836b8008c863aa2317022c7a202434554fb38452b");
    // var SNonce = Bytes.From("60eff10088077f8b03a0e2fc2fc37e1fe1f30f9f7cfbcfb2826f26f3379c4318");
    // var data = Bytes.From("0103005ffe01090020000000000000000100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
    // string desiredPmk = "01b809f9ab2fb5dc47984f52fb2d112e13d84ccb6b86d4a7193ec5299f851c48";
    // string desiredPtk = "bf49a95f0494f44427162f38696ef8b6";
    // string desiredMic = "45282522bc6707d6a70a0317a3ed48f0";
    //

    [Fact]
    public void GenerateMicForMessage()
    {
        var ptk = Bytes.From(
            "38B91FF2A1650899720B5B35BA3D6ED77A22A80820FE9312F70590A7F91D0FF92EFA7D8D74F3453DFC4C37FD56152FF01E089D11B9A85273BCC0515EC33A204AFAF0E56C042C28C147C4E7754F900FC5");
        var eapolFromM2 = Bytes.From(
            "0103007502010A0010000000000000000173E281069B989CCD6F4397085156222E20F1A7E166DD37830EFC6A72C9D2E82800000000000000000000000000000000000000000000000000000000000000009D70D600D1FF129DC4DD54B2CD338262001630140100000FAC040100000FAC040100000FAC020C00");

        var expectedMic = Bytes.From("9d70d600d1ff129dc4dd54b2cd338262");

        var actualMic = Wpa2Crypto.GenerateMicForMessage(ptk.Array, eapolFromM2.Array);
        
        Assert.Equal(expectedMic, Bytes.From(actualMic));
    }

    [Fact]
    public void Hash_SHA1()
    {
        var key = Bytes.From("86D373667BCF92EF2B62A82210D14639D91E2AB7D868A40C052AE9F839F2585E");
        var data = Bytes.From(
            "5061697277697365206B657920657870616E73696F6E0014EBB6FEFD3D1A33C35BDA1023A2920888EB920FA2E36758534E1AB554495379EB339E5209A5FA392926EB3F73E281069B989CCD6F4397085156222E20F1A7E166DD37830EFC6A72C9D2E82800");
        int offset = 0;

        var expectedFirst20Hash = Bytes.From("CD617D0B1898E37E97E73A697E2A32E515C47233");

        var actualHash = new byte[100];
        Wpa2Crypto.Hash_SHA1(key.Array, data.Array, actualHash, offset);

        Assert.Equal(expectedFirst20Hash, Bytes.From(actualHash.Take(20).ToArray()));
    }
}