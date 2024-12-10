using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(Main))]
public class UsageTests
{
    private static List<Tuple<string, string>> GetTitles(List<Result> results)
    {
        return results
            .Select(result => result.Title) // get the title from each result
            .Select(title => title.Split(" \u2192 ")) // split across the arrow character. because we require the spaces, this hopefully won't break
            .Select(parts => new Tuple<string, string>(parts[0], parts[1])) // make the tuple
            .ToList(); // make it into a list
    }
    
    [TestMethod]
    public void TestBasicQueries()
    {
        var main = new Main();

        var results = main.Query(new Query("emptyset"));
        var titles = GetTitles(results);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("emptyset", titles[0].Item1);
        Assert.AreEqual("∅", titles[0].Item2);
        
        results = main.Query(new Query("0"));
        titles = GetTitles(results);
        Assert.IsTrue(results.Count > 1);
        Assert.AreEqual("0", titles[0].Item1);
        Assert.AreEqual("∅", titles[0].Item2);
        
        results = main.Query(new Query("uml"));
        titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.AreEqual("uml", titles[0].Item1);
        Assert.AreEqual("\u00a8", titles[0].Item2);
        
        // regression test for issue with Unicode parsing in b1cbcdd1 
        results = main.Query(new Query("u-"));
        Assert.AreNotEqual(results.Count, 0);
        
        // support for little numbers (for copy-pasting suggested queries)
        results = main.Query(new Query("l₂"));
        titles = GetTitles(results);
        Assert.AreNotEqual(results.Count, 0);
        Assert.AreEqual("⇐", titles[0].Item2);
    }

    [TestMethod]
    public void TestReverseQueries()
    {
        var main = new Main();
        
        var results = main.Query(new Query("λ"));
        var titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.IsTrue(titles.Contains(new Tuple<string, string>("lambda", "λ")));
        Assert.IsTrue(titles.Contains(new Tuple<string, string>("Gl", "λ")));
    }

    [TestMethod]
    public void TestNumber()
    {
        var main = new Main();

        // "∶ ⦂ ː ꞉ ˸ ፥ ፦ ： ﹕ ︓  "
        var results = main.Query(new Query(":4"));
        var titles = GetTitles(results);

        Assert.AreEqual(8, results.Count);
        Assert.AreEqual(":₄", titles[0].Item1); // we should rewrite the first part as subscript numbers
        Assert.AreEqual("\ua789", titles[0].Item2); // we should return the correct item from the list
    }

    [TestMethod]
    public void TestArrow()
    {
        var main = new Main();
        var results = main.Query(new Query("alpha \u2192 α"));
        var titles = GetTitles(results);

        Assert.AreEqual("alpha", titles[0].Item1); // we should ignore the part after the arrow
        Assert.AreEqual("α", titles[0].Item2); // we should return the correct item from the list
    }
    
    [TestMethod]
    public void TestInvalidNumbers()
    {
        // out-of-bounds indexing
        var main = new Main();
        
        // too small - we should skip to the next valid item (1)
        var results = main.Query(new Query(":0"));
        var titles = GetTitles(results);
        Assert.AreEqual(":₁", titles[0].Item1);
        Assert.AreEqual("∶", titles[0].Item2);
        
        // too large - show nothing
        results = main.Query(new Query(":20"));
        Assert.AreEqual(0, results.Count);

        // weird (negative) - show nothing
        results = main.Query(new Query(":-1"));
        Assert.AreEqual(0, results.Count);

        // no numeric options available - show nothing
        results = main.Query(new Query("alpha2"));
        Assert.AreEqual(0, results.Count);
    }
    
    [TestMethod]
    public void TestMultiple()
    {
        var main = new Main();
        
        var results = main.Query(new Query("alpha\\beta\\gamma"));
        var titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.AreEqual("αβγ", titles[0].Item2);
        
        results = main.Query(new Query("gamma\\^\\gamma\\gamma"));
        titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.AreEqual("γˠγ", titles[0].Item2);
        
        // test with space and backslash between components
        results = main.Query(new Query("alpha \\beta"));
        titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.AreEqual("αβ", titles[0].Item2);
        
        // special cases for completion
        // test with just a space between components
        results = main.Query(new Query("alpha beta"));
        titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.AreEqual("αβ", titles[0].Item2);
        
        // test with just an underscore
        results = main.Query(new Query("alpha_2"));
        titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.AreEqual("α₂", titles[0].Item2);

        // we shouldn't allow the multi-input to trigger unintentionally - there should be a break character
        results = main.Query(new Query("alphabeta gamma"));
        Assert.AreEqual(0, results.Count);
        
        // regression test: we shouldn't return multiple options in this group
        results = main.Query(new Query("l alpha"));
        titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.IsFalse(titles[0].Item2.Contains(" "));
        
        // we should handle numeric matches somehow
        results = main.Query(new Query("l2 alpha"));
        titles = GetTitles(results);
        Assert.AreNotEqual(0, results.Count);
        Assert.AreEqual("⇐α", titles[0].Item2);
        
        // test with an invalid first part
        results = main.Query(new Query("labmda beta"));
        Assert.AreEqual(0, results.Count);
        
        // regression test: `\^\j with crossed-tai` predicts weirdly
        // results = main.Query(new Query("^\\j with crossed-tai"));
        // titles = GetTitles(results);
        // Assert.AreNotEqual(0, results.Count);
        // Assert.AreEqual("\\^\\j with crossed-tail", titles[0].Item1);
    }
}