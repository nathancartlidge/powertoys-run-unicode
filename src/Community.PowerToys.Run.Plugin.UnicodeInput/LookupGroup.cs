using System.Text;

namespace Community.PowerToys.Run.Plugin.UnicodeInput;

public class LookupGroup
{
    private readonly Dictionary<char, BaseLookup> _mappings;

    private static char _symbolFromNumber(int input)
    {
        // see https://unicodeplus.com/decomposition/Circle
        return input switch
        {
            <= 20 => (char)(input + 0x245F),
            <= 35 => (char)(input + 0x323C),
            <= 50 => (char)(input + 0x328D),
            _ => (char)(input + 0x35E) // hopefully this will never happen, as it may lead to issues
        };
    }
    
    public LookupGroup(List<Dictionary<string, string>> mappings, Dictionary<string, string> agdaLookup,
        Dictionary<string, string> htmlLookup)
    {
        var userMappings = mappings
            .Select((mapping, index) => (_symbolFromNumber(index), new BaseLookup(mapping)))
            .ToDictionary(v => v.Item1, v => v.Item2);
        
        // add in the default mapping sets with their specialised symbols:
        userMappings['\u25e2'] = new BaseLookup(agdaLookup);
        userMappings['\u26ca'] = new HtmlLookup(htmlLookup);

        _mappings = userMappings;
    }
    
    public string GetLookupSources(string exactQuery, string result)
    {
        // returns a list of all symbols that have a matching lookup
        // note that we use contains here to avoid dealing with numeric indices
        var sources = _mappings
            .Where(i => (i.Value.Get(exactQuery) ?? "").Contains(result))
            .Select(i => i.Key)
            .ToList();
        return string.Concat(sources);
    }

    public List<string> ExactMatches(string input)
    {
        return _mappings
            .SelectMany(i => i.Value.ExactMatches(input))
            .Distinct()
            .ToList();
    }

    public List<string> ReverseMatch(string input)
    {
        return _mappings
            .SelectMany(i => i.Value.ReverseMatch(input))
            .Distinct()
            .OrderBy(key => key.Any(char.IsDigit))
            .ThenBy(key => key.Length)
            .ToList();
    }

    public (List<char>, List<string>) PartialMatches(string input)
    {
        var partialMatches = _mappings
            .Select(i => i.Value.PartialMatches(input))
            .ToList();
        var chars = partialMatches
            .SelectMany(i => i.Item1)
            .Distinct()
            .ToList();
        var matches = partialMatches
            .SelectMany(i => i.Item2)
            .Distinct()
            .OrderBy(key => !key.StartsWith(input)) // prioritise terms that start with our query
            .ThenBy(key => key.Length) // then order by length - Hanlon's razor, we probably want the short option
            .ToList();
        return (chars, matches);
    }
    
    public string LongestPartialMatch(string input)
    {
        return _mappings
            .Select(i => i.Value.LongestPartialMatch(input))
            .Distinct()
            .OrderBy(key => -key.Length)
            .FirstOrDefault("");
    }
    
    public string? Get(string key)
    {
        if (key == "")
        {
            return null;
        }
        var match = _mappings
            .Where(i => i.Value.Get(key) != null)
            .Select(i => i.Value.Get(key))
            .FirstOrDefault("");
        return match == "" ? null : match;
    }
}