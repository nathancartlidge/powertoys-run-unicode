using ManagedCommon;
using System.Text;
using Wox.Plugin;
using System.Text.RegularExpressions;
using Wox.Infrastructure;
using Wox.Plugin.Common;

namespace Community.PowerToys.Run.Plugin.UnicodeInput;

public partial class Main : IPlugin, IContextMenu
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

    private Result MakeResult(string prefix, string suffix, IReadOnlyList<string> choices, IReadOnlyList<char> nextChar,
                              int score, bool isHtml = false)
    {
        var titleStringBuilder = new StringBuilder();
        titleStringBuilder.Append(prefix);
        titleStringBuilder.Append(suffix);
        if (choices.Count == 0)
        {
            // no exact match, but there are options if you keep typing - return a hint
            return new Result
            {
                Title = prefix + suffix,
                SubTitle = "No match found yet - keep typing! " + _arrayToString(nextChar),
                IcoPath = IconPath,
                // if there is only one possible letter to be typed, this could easily get in the way
                Score = nextChar.Count <= 1 ? score - 2 : score - 1,
                Action = _ => false
            };
        }

        // we have at least one choice
        titleStringBuilder.Append(" \u2192 ");
        titleStringBuilder.Append(choices[0]);

        var subtitleStringBuilder = new StringBuilder();
        var agdaGet = _agdaLookup.Get(prefix);
        var htmlGet = _htmlLookup.Get(prefix);
        if (agdaGet != null && agdaGet.Contains(choices[0]))
        {
            subtitleStringBuilder.Append('\u25e2');
        }
        if (isHtml || (htmlGet != null && htmlGet.Contains(choices[0])))
        {
            subtitleStringBuilder.Append('\u26ca');
        }
        
        subtitleStringBuilder.Append(" Copy this symbol to the clipboard");
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
            },
            ContextData = choices[0],
        };
    }
    
    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        if (selectedResult?.ContextData is null) return [];
        var symbol = selectedResult.ContextData.ToString()!;
        var choiceChar = char.ConvertToUtf32(symbol, 0);

        return
        [
            new ContextMenuResult
            {
                PluginName = Name,
                Title = $"Copy symbol ({symbol}) to clipboard",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Glyph = "\ue8c8", // Copy
                Action = _ =>
                {
                    Clipboard.SetText(symbol);
                    return true;
                }
            },
            new ContextMenuResult
            {
                PluginName = Name,
                Title = $"Copy codepoint (\\u{choiceChar:X4}) to clipboard",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Glyph = "\ue8c1", // Characters
                Action = _ =>
                {
                    Clipboard.SetText($"\\u{choiceChar:X4}");
                    return true;
                }
            },
            new ContextMenuResult
            {
                PluginName = Name,
                Title = $"U+{choiceChar:X4} - Character Information",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Glyph = "\ue721", // Magnifying glass
                Action = _ =>
                {
                    var url = $"https://unicodeplus.com/U+{choiceChar:X4}";
                    // var url = "https://www.google.com/";
                    
                    if (!Helper.OpenCommandInShell(DefaultBrowserInfo.Path, DefaultBrowserInfo.ArgumentsPattern, url))
                    {
                        Context?.API.ShowMsg($"Plugin: {Name}", "Open default browser failed.");
                        return false;
                    }

                    return true;
                }
            }
        ];
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
    
    [GeneratedRegex(@"^(.*?)(\d+)$")]
    private static partial Regex NumberMatcherRegex();

    private readonly Regex _numberMatcher = NumberMatcherRegex();
    
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
        return QueryString(string.IsNullOrEmpty(query.ActionKeyword)
            ? query.RawQuery.Trim() // no keyword - just trim
            : query.RawQuery[query.ActionKeyword.Length..].Trim()); // keyword - remove it, then trim
    }
        
    public List<Result> QueryString(string query)
    {
        List<Result> results = [];
        
        // Exact matching - agda has a key, we provide that key
        var exactAgdaMatches = _agdaLookup.ExactMatches(query);
        var exactHtmlMatches = _htmlLookup.ExactMatches(query);
        var exactMatches = exactAgdaMatches.Union(exactHtmlMatches).ToList();
        
        var (validAgdaChars, partialAgdaMatches) = _agdaLookup.PartialMatch(query);
        var partialAgdaEndMatches = _agdaLookup.PartialEndMatch(query);
        var (validHtmlChars, partialHtmlMatches) = _htmlLookup.PartialMatch(query);
        var partialHtmlEndMatches = _htmlLookup.PartialEndMatch(query);

        var validChars = validAgdaChars.Union(validHtmlChars).ToList();
        var partialMatches = partialAgdaMatches
            .Union(partialAgdaEndMatches)
            .Union(partialHtmlMatches)
            .Union(partialHtmlEndMatches)
            .ToList();

        validChars.Sort();
        partialMatches.Sort();

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
                    suffix:   "",
                    choices:  exactMatches,
                    nextChar: validChars,
                    score:    10
                )
            );   
        }

        // HTML Numerics 
        if (query.StartsWith("&#") || query.StartsWith('#') || query.StartsWith('u') || query.StartsWith("U+"))
        {
            var htmlMatch = HtmlLookup.NumericMatch(query.Replace("U+", "#x").Replace("u", "#x"));
            if (htmlMatch != null)
            {
                results.Add(
                    item: MakeResult(
                        prefix:   query,
                        suffix:   "",
                        choices:  [htmlMatch],
                        nextChar: [],
                        score:    1,
                        isHtml:   true
                    )
                );
            }
        }
        
        // Number-indexed matching support
        var numberKey = "";
        var numberIndex = -1;
        var numberMatches = new List<string>();
        
        var match = _numberMatcher.Match(query);
        if (match.Success)
        {
            numberKey = match.Groups[1].Value;
            numberIndex = int.Parse(match.Groups[2].Value) - 1;

            numberMatches = _agdaLookup.ExactMatches(numberKey).Union(_htmlLookup.ExactMatches(numberKey)).ToList();
            if (0 <= numberIndex && numberIndex < numberMatches.Count)
            {
                results.Add(
                    item: MakeResult(
                        prefix:   numberKey,
                        suffix:   _subscriptNumber(numberIndex + 1),
                        choices:  [numberMatches[numberIndex]],
                        nextChar: [],
                        score:    1
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
                        prefix:   s,
                        suffix:   "",
                        choices:  _agdaLookup.ExactMatches(s).Union(_htmlLookup.ExactMatches(s)).ToList(),
                        nextChar: [],
                        score:    partialMatches.Count == 1 ? 0 : -1
                    )
                )
        );

        // Number-indexed alternatives (to fill remaining slots)
        remainingSlots = int.Max(0, MaxResults - results.Count);
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
                item: MakeResult(
                    prefix:   searchKey,
                    suffix:   _subscriptNumber(j + jStart + 1),
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