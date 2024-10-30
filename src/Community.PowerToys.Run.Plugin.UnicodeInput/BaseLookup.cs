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
        var match = _keyValuePairs.GetValueOrDefault(key, "");
        return match == "" ? null : match;
    }
}