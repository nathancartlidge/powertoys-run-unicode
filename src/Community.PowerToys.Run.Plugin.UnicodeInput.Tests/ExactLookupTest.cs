using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(AgdaLookup))]
public class ExactLookupTest
{
    [TestMethod]
    public void TestSimpleMatch()
    {
        Console.WriteLine("Testing single matches");
        Console.WriteLine("Testing simple case ('_2' to '₂')");

        var result = AgdaLookup.ExactMatches("_2");
        Assert.AreEqual(result.Count, 1);
        Assert.AreEqual(result[0], "₂");
    }

    [TestMethod]
    public void TestLongMatch()
    {
        Console.WriteLine("Testing long case ('^\\turned r with long leg and retroflex hook' to '𐞧')");

        var result = AgdaLookup.ExactMatches("^\\turned r with long leg and retroflex hook");
        Assert.AreEqual(result.Count, 1);
        Assert.AreEqual(result[0], "𐞧");
    }

    [TestMethod]
    public void TestOptionsMatch()
    {
        Console.WriteLine("Testing multiple output (':')");

        var result = AgdaLookup.ExactMatches(":");
        // "∶ ⦂ ː ꞉ ˸ ፥ ፦ ： ﹕ ︓  "
        Assert.AreEqual(result.Count, 11);
        Assert.AreEqual(result[0], "\u2236");
        Assert.AreEqual(result[3], "\ua789");
    }

    [TestMethod]
    public void TestInvalidMatch()
    {
        Console.WriteLine("Testing invalid case");

        var result = AgdaLookup.ExactMatches("thisisnotavalidkey");
        Assert.AreEqual(result.Count, 0);
    }
}