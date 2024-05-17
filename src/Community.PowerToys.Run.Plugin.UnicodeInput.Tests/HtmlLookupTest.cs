using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(HtmlLookup))]
public class HtmlLookupTest
{
    private readonly HtmlLookup _lookup = new();

    [TestMethod]
    public void TestNumber()
    {
        Console.WriteLine("Testing on '!'");
        var result = HtmlLookup.NumericMatch("#33;");
        Assert.AreEqual(result, "!");
    }
}