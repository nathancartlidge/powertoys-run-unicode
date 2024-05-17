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
        
    public List<string> ExactMatches(string key)
    {
        if (!_keyValuePairs.ContainsKey(key)) return [];
            
        var value = _keyValuePairs.GetValueOrDefault(key, "");
        if (value == "") return [];
            
        var candidates = value.Split(" ");
        return candidates.ToList();

    }
        
    public (List<char>, List<string>) PartialMatch(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return ([], []);
        }
        // goal: we want all elements of _sortedKeys that have `key` as a suffix.
        //       theoretically we can just do this with a filter, but I want to be fancy about it
        //       so, instead I intend to do this with a binary search:
        //        - look for the location of key
        //        - look for the location of key+1
        //       these will be my elements
            
        // get the starting point
        var firstIndex = _sortedKeys.BinarySearch(key, StringComparer.Ordinal);
        if (firstIndex < 0)
        {
            firstIndex = ~ firstIndex;
        }
        else
        {
            firstIndex += 1;
        }
            
        // get the next element - because the list is sorted, this can just be done by incrementing the last char
        var allButLast = key[..^1];
        var lastChar = key[^1];
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
            if (validNextChars.Count == 0 || value[key.Length] != validNextChars.Last())
            {
                validNextChars.Add(value[key.Length]);
            }
        } 
            
        return (validNextChars, validPartialMatches);
    }
}