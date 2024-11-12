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
        Assert.AreEqual(exactMatches.Count, 1);
        Assert.AreEqual(nextChars.Count, 0);
        Assert.AreNotEqual(partialMatches.Count, 0);
        
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