using ManagedCommon;
using System.Text;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.UnicodeInput;

public class Main : IPlugin
{
    private string IconPath { get; set; }

    private readonly AgdaLookup _agdaLookup = new();
    private readonly HtmlLookup _htmlLookup = new();

    private PluginInitContext Context { get; set; }
    public string Name => "Unicode Input";

    public string Description => "Agda-style Unicode Input";
    private static int MaxResults => 8;

    // ReSharper disable once InconsistentNaming
    public static string PluginID => "778f24fc48714097b30303f83d5bed6a";

    private Result MakeResult(string prefix, IReadOnlyList<string> choices, IReadOnlyList<char> nextChar, int score)
    {
        var titleStringBuilder = new StringBuilder();
        titleStringBuilder.Append(prefix);
        if (choices.Count == 0)
        {
            // no exact match, but there are options if you keep typing - return a hint
            return new Result
            {
                Title = prefix,
                SubTitle = "No match found yet - keep typing! " + _arrayToString(nextChar),
                IcoPath = IconPath,
                // if there is only one possible letter to be typed, this could easily get in the way
                Score = nextChar.Count <= 1 ? score - 2 : score - 1,
                Action = _ => false
            };
        }

        titleStringBuilder.Append(" \u2192 ");
        titleStringBuilder.Append(choices[0]);

        var subtitleStringBuilder = new StringBuilder();
        subtitleStringBuilder.Append("Copy this symbol to the clipboard");
        if (choices.Count > 1)
        {
            subtitleStringBuilder.Append(" -- ");
            subtitleStringBuilder.Append('[');
            subtitleStringBuilder.Append(choices.Count);
            subtitleStringBuilder.Append(" variations available!]");
        }

        if (nextChar.Count != 0)
        {
            subtitleStringBuilder.Append(" -- ");
            subtitleStringBuilder.Append(_arrayToString(nextChar));
        }
            
        return new Result
        {
            Title = titleStringBuilder.ToString(),
            SubTitle = subtitleStringBuilder.ToString(),
            IcoPath = IconPath,
            Score = score,
            Action = _ =>
            {
                Clipboard.SetText(choices[0]);
                return true;
            }
        };
    }

    private static string _arrayToString(IReadOnlyList<char> l, string separator = "")
    {
        var sb = new StringBuilder();
        sb.Append("[ ");
            
        for (var i = 0; i < l.Count; i++)
        {
            sb.Append(l[i]);
            if (i != l.Count - 1)
            {
                sb.Append(separator);
            }
        }

        sb.Append(" ]");
        return sb.ToString();
    }

    private static string _subscriptNumber(int i)
    {
        var output = new StringBuilder();
        foreach (var c in i.ToString())
        {
            output.Append((char) (c + 8272));
        }
        return output.ToString();
    }

    public List<Result> Query(Query query)
    {
        // Clean up the raw query by discarding the keyword and trimming
        return Query(string.IsNullOrEmpty(query.ActionKeyword)
            ? query.RawQuery.Trim() // no keyword - just trim
            : query.RawQuery[query.ActionKeyword.Length..].Trim()); // keyword - remove it, then trim
    }
        
    private List<Result> Query(string query)
    {
        List<Result> results = [];
        
        // Exact matching - agda has a key, we provide that key
        var exactAgdaMatches = _agdaLookup.ExactMatches(query);
        var exactHtmlMatches = _htmlLookup.ExactMatches(query);
        var exactMatches = exactAgdaMatches.Union(exactHtmlMatches).ToList();
        
        var (validAgdaChars, partialAgdaMatches) = _agdaLookup.PartialMatch(query);
        var (validHtmlChars, partialHtmlMatches) = _htmlLookup.PartialMatch(query);

        var validChars = validAgdaChars.Union(validHtmlChars).ToList();
        var partialMatches = partialAgdaMatches.Union(partialHtmlMatches).ToList();
        
        // In the case where we have nothing useful to add (e == 0 and p == 0), we should avoid polluting the list
        //  of results (e == 0 and p == 0)
        // In the case where there is only one match, there is no point attempting to show the 'No match found yet!'
        //  line - we know what the match is going to be! This is only the case when there are no exact matches and
        //  exactly one partial match, so we skip this step if both those conditions are met (e == 0 and p == 1)
        // These two conditions combine to give e == 0 and p <= 1. By inverting them, we get e != 0 || p > 1
        if (exactMatches.Count != 0 || partialMatches.Count > 1)
        {
            results.Add(
                item: MakeResult(
                    prefix:   query,
                    choices:  exactMatches,
                    nextChar: validChars,
                    score:    0
                )
            );   
        }

        // Number-indexed matching support - agda-only
        var (k, i, numberMatch) = _agdaLookup.NumberMatch(query);
        if (numberMatch != null)
        {
            results.Add(
                item: MakeResult(
                    prefix:   k + _subscriptNumber(i + 1),
                    choices:  [numberMatch],
                    nextChar: [],
                    score:    0
                )
            );
        }

        // HTML Numerics 
        if (query.Contains('#'))
        {
            var htmlMatch = HtmlLookup.NumericMatch(query);
            if (htmlMatch != null)
            {
                results.Add(
                    item: MakeResult(
                        prefix:   query,
                        choices:  [htmlMatch],
                        nextChar: [],
                        score:    0
                    )
                );
            }
        }
        
        // Partial Match candidates (to fill remaining slots)
        var remainingSlots = int.Max(0, MaxResults - results.Count);
        if (remainingSlots <= 0) return results; // early stopping

        results.AddRange(
            collection: partialMatches
                .OrderBy(it => it.Length)
                .Take(remainingSlots)
                .Select(s =>
                    MakeResult(
                        prefix: s,
                        choices: _agdaLookup.ExactMatches(s).Union(_htmlLookup.ExactMatches(s)).ToList(),
                        nextChar: [],
                        score: partialMatches.Count == 1 ? 0 : -1
                    )
                )
        );

        // Number-indexed alternatives (to fill remaining slots
        remainingSlots = int.Max(0, MaxResults - results.Count);
        if (remainingSlots <= 0) return results; // early stopping
            
        int jStart;
        string searchKey;
        List<string> options;

        // which number should we start from?
        // - if our search was for a particular number, show subsequent options
        // - otherwise, start from 1
        // - if neither of these conditions apply, just return
        if (numberMatch != null)
        {
            options = _agdaLookup.ExactMatches(k)[(i + 1)..];
            searchKey = k;
            jStart = i + 1;
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
                item: MakeResult(
                    prefix:   searchKey + _subscriptNumber(j + jStart + 1),
                    choices:  [options[j]],
                    nextChar: [],
                    score:    -1
                )
            );
        }

        return results;
    }

    public void Init(PluginInitContext context)
    {
        Context = context;
        Context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(Context.API.GetCurrentTheme());
    }

    private void UpdateIconPath(Theme theme)
    {
        IconPath = theme is Theme.Light or Theme.HighContrastWhite ? "images/agda.light.png" : "images/agda.dark.png";
    }

    private void OnThemeChanged(Theme currentTheme, Theme newTheme)
    {
        UpdateIconPath(newTheme);
    }
}