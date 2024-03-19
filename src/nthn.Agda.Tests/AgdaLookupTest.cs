using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nthn.Agda;

namespace nthn.Agda.Tests;

[TestClass]
[TestSubject(typeof(AgdaLookup))]
public class AgdaLookupTest
{
    public AgdaLookup Lookup = new AgdaLookup();

    [TestMethod]
    public void TextExact()
    {
        string[] result;
        
        Console.WriteLine("Testing single matches");
        Console.WriteLine("Testing simple case ('_2' to '₂')");
        
        result = Lookup.ExactMatches("_2");
        Assert.AreEqual(result.Length, 1);
        Assert.AreEqual(result[0], "₂");

        Console.WriteLine("Testing long case ('^\\turned r with long leg and retroflex hook' to '𐞧')");
        
        result = Lookup.ExactMatches("^\\turned r with long leg and retroflex hook");
        Assert.AreEqual(result.Length, 1);
        Assert.AreEqual(result[0], "𐞧");
        
        Console.WriteLine("Testing multiple output (':')");

        result = Lookup.ExactMatches(":");
        // "∶ ⦂ ː ꞉ ˸ ፥ ፦ ： ﹕ ︓  "
        Assert.AreEqual(result.Length, 11);
        Assert.AreEqual(result[0], "\u2236");
        Assert.AreEqual(result[3], "\ua789");

        Console.WriteLine("Testing invalid case");

        result = Lookup.ExactMatches("thisisnotavalidkey");
        Assert.AreEqual(result.Length, 0);
    }

    [TestMethod]
    public void TestNumber()
    {
        string result;

        Console.WriteLine("Testing on ':'");
        // "∶ ⦂ ː ꞉ ˸ ፥ ፦ ： ﹕ ︓  "
        result = Lookup.NumberMatch(":4");
        Assert.AreEqual(result, "\ua789");

        Console.WriteLine("Testing out-of-bounds array indexing");
        
        result = Lookup.NumberMatch(":0");
        Assert.AreEqual(result, null);
        result = Lookup.NumberMatch(":100");
        Assert.AreEqual(result, null);
        
        Console.WriteLine("Testing invalid case");
        result = Lookup.NumberMatch("_2"); // there is not a '_' options list
        Assert.AreEqual(result, null);
    }

    [TestMethod]
    public void TestPartial()
    {
        // todo: more tests are still needed to fully exclude the risk of off-by-one cases!
        List<char> nextChars;
        List<string> partialMatches;
        
        // G[ABCDEFGHIKLMNOPRSTUXZabcdefghiklmnoprstuxz]
        (nextChars, partialMatches) = Lookup.PartialMatch("G");
        var expected = "ABCDEFGHIKLMNOPRSTUXZabcdefghiklmnoprstuxz";
        foreach (var c in expected)
        {
            Assert.IsTrue(nextChars.Contains(c));
        }
        Assert.IsTrue(partialMatches.Contains("Gamma"));
        
        // invalid
        (nextChars, partialMatches) = Lookup.PartialMatch("thisisnotavalidkey");
        Assert.IsTrue(nextChars.Count == 0);
        Assert.IsTrue(partialMatches.Count == 0);
    }
}