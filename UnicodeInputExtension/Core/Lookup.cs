using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UnicodeInputExtension.Core;

public partial class Lookup
{
    private readonly LookupGroup? _lookups;
    private readonly int _maxResults;
    
    [GeneratedRegex(@"^(.*?)(\d+)$")]
    private static partial Regex NumberMatcherRegex();

    private readonly Regex _numberMatcher = NumberMatcherRegex();

    
    public Lookup()
    {
        _lookups = null;
        _maxResults = 5;
    }
    
    public Lookup(string directory, int maxResults)
    {
        var loader = new FileLoader(directory);
        _lookups = new LookupGroup(loader.Mappings, loader.AgdaMapping, loader.HtmlMapping);
        _maxResults = maxResults;
    }
    
    private static string _subscriptNumber(int i)
    {
        var output = new StringBuilder();
        foreach (var c in i.ToString(CultureInfo.InvariantCulture))
        {
            output.Append((char) (c + 8272));
        }
        return output.ToString();
    }
    
    public List<Result> Query(string query, string actionKeyword = "")
    {
        if (_lookups == null)
        {
            // we cannot function if lookups failed
            // todo: just in case, maybe make this an explicit error message?
            return [];
        }
        
        // Clean up the raw query by discarding the keyword and trimming
        var cleanedQuery = string.IsNullOrEmpty(actionKeyword)
            ? query.Trim() // no keyword - just trim
            : query[actionKeyword.Length..].Trim();

        return cleanedQuery.All(c => c > 127) ?
            // exclusively non-ascii characters in the query - do reverse matching
            GetAsciiPrompt(cleanedQuery) :
            // some ascii characters - do forwards matching
            GetUnicodeSymbol(cleanedQuery);
    }

    public string GetLookupSources(string exactQuery, string result)
    {
        return _lookups?.GetLookupSources(exactQuery, result) ?? "";
    } 
    
    private List<Result> GetAsciiPrompt(string query)
    {
        if (_lookups == null)
        {
            // we cannot function if lookups failed
            // todo: just in case, maybe make this an explicit error message?
            return [];
        }
        
        // Exact matching - agda has a key, we provide that key
        var matches =  _lookups.ReverseMatch(query)
            .Take(_maxResults)
            .ToList();

        if (matches.Count == 0)
        {
            return [];
        }

        return matches
            .Select(
                match => new Result(
                    UserInput: match,
                    ResultIndex: null,
                    Choices: [query],
                    ValidNextChars: [],
                    Score: 1
                )
            )
            .ToList();
    }

    private static List<string> AddPrefix(List<string> results, string prefix)
    {
        return results.Select(result => prefix + result).ToList();
    }
    
    private List<Result> GetUnicodeSymbol(string query)
    {
        if (_lookups == null)
        {
            // we cannot function if lookups failed
            // todo: just in case, maybe make this an explicit error message?
            return [];
        }
        
        var partialResult = "";
        var partialResultPrefix = "";
        List<Result> results = [];
        
        // for numeric matching
        var numberKey = "";
        var numberIndex = -1;
        var numberMatches = new List<string>();

        // cleanup the little numbers where appropriate? (replace them with their big equivalents)
        query = string.Join(null, query.Select(c => (char) (c is >= '₀' and <= '₉' ? c - 8272 : c)));
        
        // clean up the little arrow, if present
        var firstSegment = query.Split('\u2192').First().Trim();
        if (firstSegment.Length > 0)
        {
            query = firstSegment;
        }
        
        // Exact matching - agda has a key, we provide that key
        var exactMatches = _lookups.ExactMatches(query);
        
        // multiple-lookup implementation (\lambda\_2 → λ₂ or \lambda\alpha → λα)
        // if there are no exact matches AND there is a backslash within the string
        // todo: can we fetch a user-defined trigger shortcut?
        while (exactMatches.Count == 0 && (query.Contains('\\') || query.Contains(' ') || query.Contains('_') || query.Contains('^')))
        {
            // 1. find the longest substring that is a word
            var longestPartialMatch = _lookups.LongestPartialMatch(query);
            var matchedCharacter = _lookups.Get(longestPartialMatch);

            // 2a. if there is not a match, break out of the loop
            if (longestPartialMatch == "" || matchedCharacter == null || longestPartialMatch.Length >= query.Length)
                break;
            
            // 2b. we want to restrict the possible values for the next character to our approved set
            //     (' ', '\', '_', '^') or a number
            var nextCharacter = query[longestPartialMatch.Length];
            if (nextCharacter is not (' ' or '\\' or '_' or '^'))
            {
                // support numeric inputs here
                if (matchedCharacter.Contains(' ') && nextCharacter is >= '0' and <= '9')
                {
                    // note that this is slightly different behaviour to the other implementation later down!
                    // todo: review both
                    var numberIndexString = string.Join(null,
                        // get the longest possible consecutive string of digits from the string
                        query[longestPartialMatch.Length..]
                            .TakeWhile(c => c is >= '0' and <= '9')
                    );
                    numberIndex = int.Parse(numberIndexString, CultureInfo.InvariantCulture) - 1;

                    var matchedCharacterSplit = matchedCharacter.Split(' ');
                    if (numberIndex >= 0 && numberIndex < matchedCharacterSplit.Length)
                    {
                        matchedCharacter = matchedCharacterSplit[numberIndex];
                        longestPartialMatch += _subscriptNumber(numberIndex + 1);
                    }
                    else break;
                }
                else break;
            } else if (matchedCharacter.Contains(' '))
                // handle multiple character matches (eg \l), even when no index provided (take the first one)
                matchedCharacter = matchedCharacter.Split(' ').First();
            
            // todo: handle unicode numerics (eg '\u03B1')
            
            // we only reach this point if we have a match
            // 3a. prepend this matched character to all responses
            partialResult += matchedCharacter;
            partialResultPrefix += string.Concat(longestPartialMatch, ' ');
            if (partialResultPrefix.Length > 12)
                partialResultPrefix = string.Concat(
                    "⋯",
                    partialResultPrefix.AsSpan(
                        partialResultPrefix.Length - 10,
                        10
                    )
                );
            
            // 3b. remove that part of the word from the query, so it doesn't interfere with other 
            query = query[longestPartialMatch.Length..].Trim();
            query = query.StartsWith('\\') ? query[1..] : query;
            
            // 4. loop
            // Exact matching - agda has a key, we provide that key
            exactMatches = _lookups.ExactMatches(query);
        }
        
        // partial matching
        var (validChars, partialMatches) = _lookups.PartialMatches(query);

        // In the case where we have nothing useful to add (e == 0 and p == 0), we should avoid polluting the list
        //  of results (e == 0 and p == 0)
        // In the case where there is only one match, there is no point attempting to show the 'No match found yet!'
        //  line - we know what the match is going to be! This is only the case when there are no exact matches and
        //  exactly one partial match, so we skip this step if both those conditions are met (e == 0 and p == 1)
        // These two conditions combine to give e == 0 and p <= 1. By inverting them, we get e != 0 || p > 1
        if (exactMatches.Count != 0 || partialMatches.Count > 1)
            results.Add(
                item: new Result(
                    UserInput:   partialResultPrefix + query,
                    ResultIndex: null,
                    Choices:  AddPrefix(exactMatches, partialResult),
                    ValidNextChars: validChars,
                    Score:    10
                )
            );   

        // HTML / Unicode Numerics
        if (query.StartsWith("&#", StringComparison.InvariantCulture) || query.StartsWith('#') || 
            query.StartsWith('u') || query.StartsWith("U+", StringComparison.InvariantCulture))
        {
            var htmlMatch = HtmlLookup.NumericMatch(query.Replace("U+", "#x").Replace("u", "#x"));
            if (htmlMatch != null)
                results.Add(
                    item: new Result(
                        UserInput:   partialResultPrefix + query,
                        ResultIndex: null,
                        Choices:  [partialResult + htmlMatch],
                        ValidNextChars: [],
                        Score:    1,
                        IsHtml:   true
                    )
                );
        }

        // Number-indexed matching support
        var match = _numberMatcher.Match(query);
        if (match.Success)
        {
            numberKey = match.Groups[1].Value;
            numberIndex = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) - 1;
            numberMatches = _lookups.ExactMatches(numberKey);
            if (0 <= numberIndex && numberIndex < numberMatches.Count)
            {
                results.Add(
                    item: new Result(
                        UserInput:      partialResultPrefix + numberKey,
                        ResultIndex:    numberIndex,
                        Choices:        [partialResult + numberMatches[numberIndex]],
                        ValidNextChars: [],
                        Score:    1
                    )
                );
            }
        }
        
        // Partial Match candidates (to fill remaining slots)
        var remainingSlots = int.Max(0, _maxResults - results.Count);
        if (remainingSlots <= 0) return results; // early stopping

        results.AddRange(
            collection: partialMatches
                .Take(remainingSlots)
                .Select(s =>
                    new Result(
                        UserInput:   partialResultPrefix + s,
                        ResultIndex: null,
                        Choices:  AddPrefix(
                            _lookups.ExactMatches(s),
                            partialResult
                        ),
                        ValidNextChars: [],
                        Score:    partialMatches.Count == 1 ? 0 : -1
                    )
                )
        );

        // Number-indexed alternatives (to fill remaining slots)
        remainingSlots = int.Max(0, _maxResults - results.Count);
        if (remainingSlots <= 0) return results; // early stopping
            
        int jStart;
        string searchKey;
        List<string> options;

        // which number should we start from?
        // - if our search was for a particular number, show subsequent options
        // - otherwise, start from 1
        // - if neither of these conditions apply, just return
        if (match.Success && numberIndex != 0 && numberIndex < numberMatches.Count)
        {
            options = numberMatches[(numberIndex + 1)..];
            searchKey = numberKey;
            jStart = numberIndex + 1;
        }
        else if (exactMatches.Count > 1)
        {
            options = exactMatches[1..];
            searchKey = query;
            jStart = 1;
        }
        else return results;
            
        for (var j = 0; j < int.Min(remainingSlots, options.Count); j++)
        {
            results.Add(
                item: new Result(
                    UserInput:      partialResultPrefix + searchKey,
                    ResultIndex:    j + jStart,
                    Choices:        [partialResult + options[j]],
                    ValidNextChars: [],
                    Score:    -1
                )
            );
        }

        return results;
    }
}
