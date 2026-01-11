using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnicodeInput.Core;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(HtmlLookup))]
public class HtmlLookupTest
{
    private readonly HtmlLookup _lookup = new(new Dictionary<string, string>());

    [TestMethod]
    public void TestNumber()
    {
        Console.WriteLine("Testing on '!'");
        var result = HtmlLookup.NumericMatch("#33;");
        Assert.AreEqual("!", result);
        
        Console.WriteLine("Testing on '!' without semicolon");
        result = HtmlLookup.NumericMatch("#33");
        Assert.AreEqual("!", result);
        
        Console.WriteLine("Testing on a more complex symbol");
        result = HtmlLookup.NumericMatch("#x2460;");
        Assert.AreEqual("\u2460", result);
    }
}