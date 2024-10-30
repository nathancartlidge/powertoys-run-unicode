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
        
        List<Result> results = main.QueryString("emptyset");
        Assert.AreEqual(results.Count, 1);
        Assert.AreEqual(results[0].Title, "emptyset \u2192 \u2205");
        
        results = main.QueryString("0");
        Assert.IsTrue(results.Count > 1);
        Assert.AreEqual(results[0].Title, "0 \u2192 \u2205");
        
        results = main.QueryString("uml");
        Assert.AreNotEqual(results.Count, 0);
        Assert.AreEqual(results[0].Title, "uml \u2192 \u00a8");
    }
}