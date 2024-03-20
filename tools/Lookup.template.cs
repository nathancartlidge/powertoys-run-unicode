using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;

namespace nthn.Agda
{
    public class AgdaLookup
    {
        public static Dictionary<string, string> KeyValuePairs = new Dictionary<string, string>()
        {
        }
        
        public static List<string> ExactMatches(string key)
        {
            if (KeyValuePairs.ContainsKey(key))
            {
                string value = KeyValuePairs.GetValueOrDefault(key, "");
                if (value != "")
                {
                    string[] candidates = value.Split(" ");
                    return candidates.ToList();
                }
            }

            return [];
        }

        private Regex _numberMatcher = new Regex(@"^(.*?)(\d+)$");
        
        public (string, int, string) NumberMatch(string keyAndIndex)
        {
            var match = _numberMatcher.Match(keyAndIndex);
            if (match.Success)
            {
                var key = match.Groups[1].Value;
                var index = int.Parse(match.Groups[2].Value) - 1;
                var matches = ExactMatches(key);
                if (0 <= index && index < matches.Count)
                {
                    return (key, index, matches[index]);
                }
            }

            return (null, -1, null);
        }

        private readonly List<string> _sortedKeys = KeyValuePairs.Keys.ToList().Order(StringComparer.Ordinal).ToList();

        public (List<char>, List<string>) PartialMatch(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return ([], []);
            }
            // goal: we want all elements of _sortedKeys that have `key` as a suffix.
            //       theoretically we can just do this with a filter but I want to be fancy about it
            //       so, instead I intend to do this with a binary search:
            //        - look for the location of key
            //        - look for the location of key+1
            //       these will be my elements
            
            // get the starting point
            int firstIndex = _sortedKeys.BinarySearch(key, StringComparer.Ordinal);
            if (firstIndex < 0)
            {
                firstIndex = ~ firstIndex;
            }
            else
            {
                firstIndex += 1;
            }
            
            // get the next element - because the list is sorted, this can just be done by incrementing the last char
            string allButLast = key.Substring(0, key.Length - 1);
            char lastChar = key[key.Length - 1];
            int incrementedCode = lastChar + 1;
            char incrementedChar = (char) incrementedCode;
            string subsequentKey = allButLast + incrementedChar;

            int lastIndex = _sortedKeys.BinarySearch(subsequentKey, StringComparer.Ordinal);
            if (lastIndex < 0)
            {
                lastIndex = ~ lastIndex;
            }

            List<char> validNextChars = new List<char>();
            List<string> validPartialMatches = new List<string>();

            for (int i = firstIndex; i < lastIndex; i++)
            {
                string value = _sortedKeys[i];
                validPartialMatches.Add(value);
                if (validNextChars.Count == 0 || value[key.Length] != validNextChars.Last())
                {
                    validNextChars.Add(value[key.Length]);
                }
            } 
            
            return (validNextChars, validPartialMatches);
        }
    }
}