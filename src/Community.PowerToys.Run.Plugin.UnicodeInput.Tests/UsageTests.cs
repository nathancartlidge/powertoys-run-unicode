using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(Main))]
public class UsageTests
{
    [TestMethod]
    public void TestQuery()
    {
        Main main = new Main();
        
        List<Result> results = main.GetUnicodeSymbol("emptyset");
        Assert.AreEqual(results.Count, 1);
        Assert.AreEqual(results[0].Title, "emptyset \u2192 \u2205");
        
        results = main.GetUnicodeSymbol("0");
        Assert.IsTrue(results.Count > 1);
        Assert.AreEqual(results[0].Title, "0 \u2192 \u2205");
        
        results = main.GetUnicodeSymbol("uml");
        Assert.AreNotEqual(results.Count, 0);
        Assert.AreEqual(results[0].Title, "uml \u2192 \u00a8");

        results = main.GetUnicodeSymbol("λ");
        Assert.AreNotEqual(results.Count, 0);

        // regression test for issue with unicode parsing in b1cbcdd1 
        results = main.GetUnicodeSymbol("u-");
        Assert.AreNotEqual(results.Count, 0);
    }
}