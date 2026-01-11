using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnicodeInput.Core;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(BaseLookup))]
public class ExactLookupTest
{
    private readonly BaseLookup _lookup = new(new Dictionary<string, string>
    {
        { "_2", "₂" },
        { "^\\turned r with long leg and retroflex hook", "𐞧" },
        { ":", "\u2236 \u2982 ː \ua789 \u02f8 ፥ ፦ ： ﹕ ︓ \u2005"}
    });
    
    [TestMethod]
    public void TestSimpleMatch()
    {
        Console.WriteLine("Testing single matches");
        Console.WriteLine("Testing simple case ('_2' to '₂')");

        var result = _lookup.ExactMatches("_2");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("₂", result[0]);
    }

    [TestMethod]
    public void TestLongMatch()
    {
        Console.WriteLine("Testing long case ('^\\turned r with long leg and retroflex hook' to '𐞧')");

        var result = _lookup.ExactMatches("^\\turned r with long leg and retroflex hook");
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("𐞧", result[0]);
    }

    [TestMethod]
    public void TestOptionsMatch()
    {
        Console.WriteLine("Testing multiple output (':')");

        var result = _lookup.ExactMatches(":");
        // "∶ ⦂ ː ꞉ ˸ ፥ ፦ ： ﹕ ︓  "
        Assert.AreEqual(11, result.Count);
        Assert.AreEqual("\u2236", result[0]);
        Assert.AreEqual("\ua789", result[3]);
    }

    [TestMethod]
    public void TestInvalidMatch()
    {
        Console.WriteLine("Testing invalid case");

        var result = _lookup.ExactMatches("thisisnotavalidkey");
        Assert.AreEqual(0, result.Count);
    }
}