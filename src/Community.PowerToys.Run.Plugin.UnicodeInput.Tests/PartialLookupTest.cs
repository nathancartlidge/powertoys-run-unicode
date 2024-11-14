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
        var exactMatches = _lookup.ExactMatches("qe");
        var (nextChars, partialMatches) = _lookup.PartialMatches("qe");
        Assert.AreEqual(0, exactMatches.Count);
        Assert.AreEqual(1, nextChars.Count);
        Assert.AreEqual('d', nextChars[0]);
        Assert.AreEqual(1, partialMatches.Count);
        Assert.AreEqual("qed", partialMatches[0]);
    }

    [TestMethod]
    public void TestPartialMultipleMatch()
    {
        // G[ABCDEFGHIKLMNOPRSTUXZabcdefghiklmnoprstuxz]
        var (nextChars, partialMatches) = _lookup.PartialMatches("G");
        const string expected = "ABCDEFGHIKLMNOPRSTUXZabcdefghiklmnoprstuxz";
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
        var (nextChars, partialMatches) = _lookup.PartialMatches("thisisnotavalidkey");
        Assert.IsTrue(nextChars.Count == 0);
        Assert.IsTrue(partialMatches.Count == 0);
    }
    
    [TestMethod]
    public void TestEndMatch()
    {
        // 0
        var exactMatches = _lookup.ExactMatches("0");
        var (nextChars, partialMatches) = _lookup.PartialMatches("0");
        Assert.AreEqual(1, exactMatches.Count);
        Assert.AreEqual(0, nextChars.Count);
        Assert.AreNotEqual(0, partialMatches.Count);
        
        // [b]ot / ot[imes] 
        (_, partialMatches) = _lookup.PartialMatches("ot");
        Assert.IsTrue(partialMatches.Contains("otimes"));
        Assert.IsTrue(partialMatches.Contains("bot"));
    }

    [TestMethod]
    public void TestReverseLookup()
    {
        // up arrow
        var reverseMatches = _lookup.ReverseMatch("↑");
        Assert.IsTrue(reverseMatches.Contains("u"));
        Assert.IsTrue(reverseMatches.Contains("u-"));
        Assert.IsTrue(reverseMatches.Contains("uparrow"));
        
        // lambda
        reverseMatches = _lookup.ReverseMatch("λ");
        Assert.IsTrue(reverseMatches.Contains("Gl"));
        Assert.IsTrue(reverseMatches.Contains("lambda"));
    }
}