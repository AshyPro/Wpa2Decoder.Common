using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Xunit;

namespace Ashy.Wpa2Decoder.Library.Tests;

[TestSubject(typeof(PasswordDictionaryGenerator))]
public class PasswordDictionaryGeneratorTest
{

    [Fact]
    public void GetBasicWordModifications_BasicModifications()
    {
        var actual = PasswordDictionaryGenerator.GetBasicWordModifications("heLLo");
        var expected = new List<string> { "heLLo", "HELLO", "hello", "Hello", "HeLLo" };
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void GetAllWordModifications_Transformations()
    {
        var actual = PasswordDictionaryGenerator.GetAllWordModifications("aaBc", new Dictionary<string, string>{{"aa", "2a"}});
        var expected = new List<string> { "aaBc", "AABC", "aabc", "Aabc", "AaBc", "2aBc", "2ABC", "2abc", "2abc", "2aBc" }.Distinct();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void GetAllWordTransformations_AllCapitalized()
    {
        var expected = new List<string> { "bar", "baR", "b@r", "b@R", "b4r", "b4R", "bAr", "bAR", "Bar", "BaR", "B@r", "B@R", "B4r", "B4R", "BAr", "BAR" };
        
        var actual = PasswordDictionaryGenerator.GetAllWordTransformations("bar", new Dictionary<char, string[]>{{'a', new[]{"@", "4"}}}, false);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void GetAllWordTransformations_FirstCapitalized()
    {
        var expected = new List<string> { "bar", "b@r", "b4r", "Bar", "B@r", "B4r" };
        
        var actual = PasswordDictionaryGenerator.GetAllWordTransformations("bar", new Dictionary<char, string[]>{{'a', new[]{"@", "4"}}}, true);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void GetAllWordTransformations_Empty_AllCapitalized()
    {
        var expected = new List<string> { "bar", "baR", "bAr", "bAR", "Bar", "BaR", "BAr", "BAR" };
        
        var actual = PasswordDictionaryGenerator.GetAllWordTransformations("bar", new Dictionary<char, string[]>(), false);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void GetAllWordTransformations_Empty_FirstCapitalized()
    {
        var expected = new List<string> { "bar", "Bar" };
        
        var actual = PasswordDictionaryGenerator.GetAllWordTransformations("bar", new Dictionary<char, string[]>(), true);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ModificationsAndTransformations_FirstCapitalized()
    {
        var expected = new List<string> { "aBc", "ABc", "ABC", "abc", "Abc" };
        
        var mods = PasswordDictionaryGenerator.GetAllWordModifications("aBc", new Dictionary<string, string>());
        var actual = mods.SelectMany(x => PasswordDictionaryGenerator.GetAllWordTransformations(x, new Dictionary<char, string[]>(), true)).Distinct();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ModificationsAndTransformations_FirstCapitalized_v2()
    {
        var expected = new List<string> { "hello", "Hello", "HELLO"};
        
        var mods = PasswordDictionaryGenerator.GetAllWordModifications("hello", new Dictionary<string, string>());
        var actual = mods.SelectMany(x => PasswordDictionaryGenerator.GetAllWordTransformations(x, new Dictionary<char, string[]>(), true)).Distinct();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ModificationsAndTransformations_FirstCapitalized_v3()
    {
        var expected = new List<string> { "Hello", "HELLO", "hello"};
        
        var mods = PasswordDictionaryGenerator.GetAllWordModifications("Hello", new Dictionary<string, string>());
        var actual = mods.SelectMany(x => PasswordDictionaryGenerator.GetAllWordTransformations(x, new Dictionary<char, string[]>(), true)).Distinct();
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ModificationsAndTransformations_AllCapitalized()
    {
        var expected = new List<string> { "aBc", "aBC", "ABc", "ABC", "abc", "abC", "Abc", "AbC" };
        
        var mods = PasswordDictionaryGenerator.GetAllWordModifications("aBc", new Dictionary<string, string>());
        var actual = mods.SelectMany(x => PasswordDictionaryGenerator.GetAllWordTransformations(x, new Dictionary<char, string[]>(), false)).Distinct();
        Assert.Equal(expected, actual);
    }
}