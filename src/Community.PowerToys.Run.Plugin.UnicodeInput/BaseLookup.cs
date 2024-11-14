#nullable enable
namespace Community.PowerToys.Run.Plugin.UnicodeInput;

public class BaseLookup
{
    private readonly Dictionary<string, string> _keyValuePairs;
    private readonly List<string> _sortedKeys;

    protected BaseLookup(Dictionary<string, string> keyValuePairs)
    {
        _keyValuePairs = keyValuePairs;
        _sortedKeys = _keyValuePairs.Keys.ToList().Order(StringComparer.Ordinal).ToList();
    }
    
    public List<string> ExactMatches(string input)
    {
        if (!_keyValuePairs.ContainsKey(input)) return [];
            
        var value = _keyValuePairs.GetValueOrDefault(input, "");
        if (value == "") return [];
            
        var candidates = value.Split(" ");
        return candidates.ToList();

    }

    public List<string> ReverseMatch(string input)
    {
        var matches = _keyValuePairs
            .Where(kv => kv.Value.Contains(input))
            .Select(
                kv => kv.Value.Contains(' ')
                    ? kv.Key + GetIndex(kv.Value) // multi-select optionn
                    : kv.Key // single option
            );
        return matches.ToList();

        string GetIndex(string value)
        {
            var candidates = value.Split(" ").ToList();
            var index = candidates.IndexOf(input);
            // if index is zero, then it is the default option - no name needed!
            return index > 0 ? (1 + index).ToString() : "";
        }
    }
    
    public (List<char>, List<string>) PartialMatches(string input)
    {
        // todo: case sensitivity?
        if (string.IsNullOrEmpty(input))
        {
            return ([], []);
        }
        
        // find matches that contain the specified string: eg [vd]ash, lam[bda], [text]musical[alnote]
        var containsMatches = ContainsMatches(input);
        
        var validNextCharacters = containsMatches
            .FindAll(key => key.StartsWith(input) && key.Length > input.Length)
            .Select(key => key[input.Length..][0])
            .Distinct()
            .ToList();

        return (
            validNextCharacters,
            containsMatches
        );
    }
    
    private List<string> ContainsMatches(string input)
    {
        return _sortedKeys.FindAll(key =>
            key.Contains(input, StringComparison.Ordinal)
            && key != input
        );
    }

    public string LongestPartialMatch(string input)
    {
        return _sortedKeys
                // find all keys that match the start of the input
                .FindAll(key => input.StartsWith(key, StringComparison.Ordinal))
                // find the longest (sort in reverse length order)
                .OrderBy(key => -key.Length)
                // take the first of these values
                .FirstOrDefault("");
    }
    
    // private List<string> CaseMatches(string input)
    // {
    //     if (input == input.ToLower())
    //     {
    //         // we only want to perform case matching if the input is already lowercase - otherwise, the user has
    //         //  probably already decided they want a particular case and are con
    //     }
    //     else
    //     {
    //         return [];
    //     }
    // }
    
    public string? Get(string key)
    {
        if (key == "")
        {
            return null;
        }
        var match = _keyValuePairs.GetValueOrDefault(key, "");
        return match == "" ? null : match;
    }
}