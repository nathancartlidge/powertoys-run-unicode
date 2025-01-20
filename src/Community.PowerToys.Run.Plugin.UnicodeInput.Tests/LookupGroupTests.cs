using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(LookupGroup))]
public class LookupGroupTests
{
    private const string Directory = @"..\..\..\..\Community.PowerToys.Run.Plugin.UnicodeInput";
    private readonly FileLoader _loader = new(Directory);
    private readonly LookupGroup _group;
    
    public LookupGroupTests()
    {
        _group = new LookupGroup(_loader.Mappings, _loader.AgdaMapping, _loader.HtmlMapping);
    }
    
    [TestMethod]
    public void TestSymbolFromNumber()
    {
        Assert.AreEqual('①', LookupGroup.SymbolFromNumber(1));
        Assert.AreEqual('②', LookupGroup.SymbolFromNumber(2));
        Assert.AreEqual('③', LookupGroup.SymbolFromNumber(3));

        Assert.AreEqual('\u2469', LookupGroup.SymbolFromNumber(10));
        Assert.AreEqual('\u2473', LookupGroup.SymbolFromNumber(20));
        Assert.AreEqual('\u325a', LookupGroup.SymbolFromNumber(30));
        Assert.AreEqual('\u32b5', LookupGroup.SymbolFromNumber(40));
        Assert.AreEqual('\u32bf', LookupGroup.SymbolFromNumber(50));
    }

    [TestMethod]
    public void TestGetLookupSources()
    {
        Assert.AreEqual("\u25e2\u26ca", _group.GetLookupSources("alpha", "α"));
        Assert.AreEqual("\u25e2", _group.GetLookupSources("---", "─"));
        Assert.AreEqual("\u25e2", _group.GetLookupSources("---", "│"));
    }
}