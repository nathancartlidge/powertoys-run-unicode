using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace nthn.Agda
{
    public class AgdaLookup
    {
        public Dictionary<string, string> KeyValuePairs = new Dictionary<string, string>()
        {
        };
        
        public string[] ExactMatches(string key)
        {
            if (keyValuePairs.ContainsKey(key))
            {
                string value = KeyValuePairs.GetValueOrDefault(key, "");
                if (value != "")
                {
                    string[] candidates = value.Split(" ");
                    return candidates;
                }
            }

            return [];
        }

        private Regex _numberMatcher = new Regex(@"^(.*?)(\d+)$");
        
        public string NumberMatch(string keyAndIndex)
        {
            Match match = _numberMatcher.Match(keyAndIndex);
            if (match.Success)
            {
                string key = match.Groups[1].Value;
                int index = int.Parse(match.Groups[2].Value) - 1;
                string[] matches = ExactMatches(key);
                if (0 <= index && index < matches.Length)
                {
                    return matches[index];
                }
            }

            return null;
        }
    }
}