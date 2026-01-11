using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin;

using UnicodeInput.Core;

namespace Community.PowerToys.Run.Plugin.UnicodeInput.Tests;

[TestClass]
[TestSubject(typeof(FileLoader))]
public class FileLoaderTests
{
    private const string Directory = ".";

    [TestMethod]
    public void TestBasicQueries()
    {
        var fl = new FileLoader(configPath: Directory);
        
        Assert.IsTrue(fl.Files.Count > 0);
        Assert.IsTrue(fl.Mappings[0].Count > 0);
    }
    
    [TestMethod]
    public void TestLoad()
    {
        var fl = new FileLoader(configPath: Directory);
        Assert.IsTrue(fl.Files.Count > 0);

        var main = new Main(Directory);
        
        var results = main.Query(new Query("emptyset"));
        Assert.IsTrue(results.Count > 0);
    }
}