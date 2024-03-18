using System;
using System.Printing;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nthn.Agda;

namespace nthn.Agda.Tests;

[TestClass]
[TestSubject(typeof(AgdaLookup))]
public class AgdaLookupTest
{

    [TestMethod]
    public void TextExact()
    {
        var lookup = new AgdaLookup();
        string[] result;
        
        Console.WriteLine("Testing single matches");
        Console.WriteLine("Testing simple case ('_2' to '₂')");
        
        result = lookup.ExactMatches("_2");
        Assert.AreEqual(result.Length, 1);
        Assert.AreEqual(result[0], "₂");

        Console.WriteLine("Testing long case ('^\\turned r with long leg and retroflex hook' to '𐞧')");
        
        result = lookup.ExactMatches("^\\turned r with long leg and retroflex hook");
        Assert.AreEqual(result.Length, 1);
        Assert.AreEqual(result[0], "𐞧");
        
        Console.WriteLine("Testing multiple output (':')");

        result = lookup.ExactMatches(":");
        // "∶ ⦂ ː ꞉ ˸ ፥ ፦ ： ﹕ ︓  "
        Assert.AreEqual(result.Length, 11);
        Assert.AreEqual(result[0], "\u2236");
        Assert.AreEqual(result[3], "\ua789");

        Console.WriteLine("Testing invalid case");

        result = lookup.ExactMatches("thisisnotavalidkey");
        Assert.AreEqual(result.Length, 0);
    }

    [TestMethod]
    public void TestNumber()
    {
        var lookup = new AgdaLookup();
        string result;

        Console.WriteLine("Testing on ':'");
        // "∶ ⦂ ː ꞉ ˸ ፥ ፦ ： ﹕ ︓  "
        result = lookup.NumberMatch(":4");
        Assert.AreEqual(result, "\ua789");

        Console.WriteLine("Testing out-of-bounds array indexing");
        
        result = lookup.NumberMatch(":0");
        Assert.AreEqual(result, null);
        result = lookup.NumberMatch(":100");
        Assert.AreEqual(result, null);
        
        Console.WriteLine("Testing invalid case");
        result = lookup.NumberMatch("_2"); // there is not a '_' options list
        Assert.AreEqual(result, null);
    }
}