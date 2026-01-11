using System.IO;
using System.Text.Json;

namespace Community.PowerToys.Run.Plugin.UnicodeInput;

public class FileLoader
{
    private readonly string _configPath;
    public List<string> Files { get; }
    public List<Dictionary<string, string>> Mappings { get; }
    public Dictionary<string, string> HtmlMapping { get; }
    public Dictionary<string, string> AgdaMapping { get; }

    public FileLoader(string configPath)
    {
        _configPath = configPath;
        Files = FindFiles().Select(Path.GetFileName).ToList();
        Mappings = FindFiles().Select(LoadFile).ToList();
        HtmlMapping = LoadFile(Path.Join(_configPath, "html.mapping.json"));
        AgdaMapping = LoadFile(Path.Join(_configPath, "agda.mapping.json"));
    }
    
    private List<string> FindFiles()
    {
        return Directory.GetFiles(_configPath)
            .ToList()
            .FindAll(filename => Path.GetFileName(filename).EndsWith(".mapping.json"))
            .Where(filename => Path.GetFileName(filename) != "agda.mapping.json" &&
                               Path.GetFileName(filename) != "html.mapping.json")
            .OrderBy(Path.GetFileName)
            .ToList();
    }
    
    private static Dictionary<string, string> LoadFile(string filename)
    {
        if (!File.Exists(filename) || !filename.EndsWith(".mapping.json"))
        {
            // invalid file
            return new Dictionary<string, string>();
        }

        var jsonContent = File.ReadAllText(filename);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) 
               ?? new Dictionary<string, string>();
    }
}
