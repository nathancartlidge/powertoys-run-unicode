using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(Main))]
public class FileLoaderTests
{
    [TestMethod]
    public void TestBasicQueries()
    {
        var fl = new FileLoader(configPath: "..\\..\\..");
        
        Assert.IsTrue(fl.Files.Count > 0);
        Assert.IsTrue(fl.Mappings[0].Count > 0);
    }
}