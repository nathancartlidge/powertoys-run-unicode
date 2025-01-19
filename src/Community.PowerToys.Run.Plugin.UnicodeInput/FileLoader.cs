using System.IO;
using System.Text.Json;

namespace Community.PowerToys.Run.Plugin.UnicodeInput;

public class FileLoader
{
    private readonly string _configPath;
    public List<string> Files { get; }
    public List<Dictionary<string, string>> Mappings { get; }

    public FileLoader(string configPath)
    {
        _configPath = configPath;
        Files = FindFiles();
        Mappings = Files.Select(f => LoadFile(f)).ToList();
    }
    
    private List<string> FindFiles()
    {
        return Directory.GetFiles(_configPath)
            .ToList()
            .FindAll(filename => filename.EndsWith(".mapping.json"));
    }
    
    private Dictionary<string, string> LoadFile(string filename)
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
