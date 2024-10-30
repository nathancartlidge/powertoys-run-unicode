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
        
    public (List<char>, List<string>) PartialMatch(string input)
    {
        if (string.IsNullOrEmpty(key))
        if (string.IsNullOrEmpty(input))
        {
            return ([], []);
        }
        // goal: we want all elements of _sortedKeys that have `input` as a prefix.
        //       theoretically we can just do this with a filter, but I want to be fancy about it
        //       so, instead I intend to do this with a binary search:
        //        - look for the location of input
        //        - look for the location of input+1
        //       these will be my elements
            
        // get the starting point
        var firstIndex = _sortedKeys.BinarySearch(input, StringComparer.Ordinal);
        if (firstIndex < 0)
        {
            firstIndex = ~ firstIndex;
        }
        else
        {
            firstIndex += 1;
        }
            
        // get the next element - because the list is sorted, this can just be done by incrementing the last char
        var allButLast = input[..^1];
        var lastChar = input[^1];
        var incrementedCode = lastChar + 1;
        var incrementedChar = (char) incrementedCode;
        var subsequentKey = allButLast + incrementedChar;

        var lastIndex = _sortedKeys.BinarySearch(subsequentKey, StringComparer.Ordinal);
        if (lastIndex < 0)
        {
            lastIndex = ~ lastIndex;
        }

        var validNextChars = new List<char>();
        var validPartialMatches = new List<string>();

        for (var i = firstIndex; i < lastIndex; i++)
        {
            var value = _sortedKeys[i];
            validPartialMatches.Add(value);
            if (validNextChars.Count == 0 || value[input.Length] != validNextChars.Last())
            {
                validNextChars.Add(value[input.Length]);
            }
        }
            
        return (validNextChars, validPartialMatches);
    }
    
    public List<string> PartialEndMatch(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }
        
        // goal: we want all elements of _sortedKeys that have `input` as a suffix.
        //       sadly I don't think there is an easy way to test this as far as I know, so it's an O(N)
        var validPartialMatches = new List<String>();
        return _sortedKeys
            .FindAll(key => key.EndsWith(input, StringComparison.Ordinal) && key != input)
            // note: we remove exact matches above
            .OrderBy(key => key.Length)
            .ToList();
    }

    public string? Get(string key)
    {
        var match = _keyValuePairs.GetValueOrDefault(key, "");
        return match == "" ? null : match;
    }
}