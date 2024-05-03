using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(AgdaLookup))]
public class PartialLookupTest
{
    private readonly AgdaLookup _lookup = new();

    [TestMethod]
    public void TestPartialSingleMatch()
    {
        // qe[d]
        var exactMatches = AgdaLookup.ExactMatches("qe");
        var (nextChars, partialMatches) = _lookup.PartialMatch("qe");
        Assert.AreEqual(exactMatches.Count, 0);
        Assert.AreEqual(nextChars.Count, 1);
        Assert.AreEqual(nextChars[0], 'd');
        Assert.AreEqual(partialMatches.Count, 1);
        Assert.AreEqual(partialMatches[0], "qed");
    }

    [TestMethod]
    public void TestPartialMultipleMatch()
    {
        // G[ABCDEFGHIKLMNOPRSTUXZabcdefghiklmnoprstuxz]
        var (nextChars, partialMatches) = _lookup.PartialMatch("G");
        var expected = "ABCDEFGHIKLMNOPRSTUXZabcdefghiklmnoprstuxz";
        foreach (var c in expected)
        {
            Assert.IsTrue(nextChars.Contains(c));
        }

        Assert.IsTrue(partialMatches.Contains("Gamma"));
    }
    
    [TestMethod]
    public void TestPartialNoMatch()
    {
        // invalid
        var (nextChars, partialMatches) = _lookup.PartialMatch("thisisnotavalidkey");
        Assert.IsTrue(nextChars.Count == 0);
        Assert.IsTrue(partialMatches.Count == 0);
    }
}