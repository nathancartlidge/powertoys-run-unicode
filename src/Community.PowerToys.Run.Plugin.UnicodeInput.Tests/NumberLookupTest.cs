using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(AgdaLookup))]
public class NumberLookupTest
{
    private readonly AgdaLookup _lookup = new();

    [TestMethod]
    public void TestNumber()
    {
        Console.WriteLine("Testing on ':'");
        // "∶ ⦂ ː ꞉ ˸ ፥ ፦ ： ﹕ ︓  "
        var (_, _, result) = _lookup.NumberMatch(":4");
        Assert.AreEqual(result, "\ua789");
    }
    
    [TestMethod]
    public void TestInvalidNumbers()
    {
        Console.WriteLine("Testing out-of-bounds array indexing");
        
        var (_, _, result) = _lookup.NumberMatch(":0");
        Assert.AreEqual(result, null);
        (_, _, result) = _lookup.NumberMatch(":100");
        Assert.AreEqual(result, null);
        
        Console.WriteLine("Testing invalid case");
        (_, _, result) = _lookup.NumberMatch("_2"); // there is not a '_' options list
        Assert.AreEqual(result, null);
    }
}