using ManagedCommon;
using System.Text;
using Wox.Plugin;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Wox.Infrastructure;
using Wox.Plugin.Common;
using Microsoft.PowerToys.Settings.UI.Library;

namespace Community.PowerToys.Run.Plugin.UnicodeInput;

public partial class Main : IPlugin, IContextMenu, ISettingProvider
{
    private string IconPath { get; set; }

    private readonly AgdaLookup _agdaLookup = new();
    private readonly HtmlLookup _htmlLookup = new();
    private readonly Typer _typer = new();
    
    private PluginInitContext Context { get; set; }
    public string Name => "Unicode Input";

    public string Description => "Agda-style Unicode Input";
    private static int MaxResults => 8;
    
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedMember.Global
    public static string PluginID => "778f24fc48714097b30303f83d5bed6a";

    // --- CUSTOM SETTINGS SUPPORT -------------------------------------------------------------------------------------
    // user-configurable variables
    private bool _doTyping;
    private int _typeDelay;
    
    // ReSharper disable once UnusedMember.Global
    public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>()
    {
        new()
        {
            Key = "DoTyping",
            DisplayLabel = "Use Typing",
            DisplayDescription = "If checked, unicode symbols will be typed instead of copied by default.",
            Value = true,
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
        },
        new()
        {
            Key = "BeginTypeDelay",
            DisplayLabel = "Type Delay (ms)",
            DisplayDescription = "How long (in milliseconds) to wait before typing begins.",
            NumberValue = 100,
            PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Numberbox,
        },
    };
    
    public System.Windows.Controls.Control CreateSettingPanel()
    {
        // we do not need to implement this method
        throw new NotImplementedException();
    }

    public void UpdateSettings(PowerLauncherPluginSettings settings)
    {
        if (settings?.AdditionalOptions is null)
        {
            return;
        }

        var doTyping = settings.AdditionalOptions.FirstOrDefault(x => x.Key == "DoTyping");
        var typeDelay = settings.AdditionalOptions.FirstOrDefault(x => x.Key == "BeginTypeDelay");

        _doTyping = doTyping?.Value ?? true;
        _typeDelay = (int)(typeDelay?.NumberValue ?? 200);
    }
    
    // -----------------------------------------------------------------------------------------------------------------
    
    private Result MakeResult(string prefix, string suffix, IReadOnlyList<string> choices,
                              IReadOnlyList<char> nextChar, int score, bool isHtml = false)
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
        
        // the default action changes depending upon `_doTyping`, so should the prompt
        // todo: -> "these symbols" when appropriate
        subtitleStringBuilder.Append(_doTyping ? " Input this symbol" : " Copy this symbol to the clipboard");
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
                if (_doTyping)
                {
                    Task.Run(() => _typer.Type(choices[0], _typeDelay));
                }
                else
                {
                    Clipboard.SetText(choices[0]);
                }
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

        ContextMenuResult remainingOption;
        if (_doTyping)
        {
          	remainingOption = new ContextMenuResult
            {
                PluginName = Name,
                Title = $"Copy symbol {symbol} to clipboard (Ctrl+C)",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Glyph = "\ue8c8", // Copy
                AcceleratorKey = Key.C,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ =>
                {
                    Clipboard.SetText(symbol);
                    return true;
                }
            };
        }
        else
        {
			remainingOption = new ContextMenuResult
            {
                PluginName = Name,
                Title = $"Input symbol {symbol} (Ctrl+I)",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Glyph = "\ue765", // Keyboard
                AcceleratorKey = Key.I,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ =>
                {
                    Task.Run(() => _typer.Type(symbol, _typeDelay));
                    return true;
                }
            };
        }
        
        return
        [
            remainingOption,
            new ContextMenuResult
            {
                PluginName = Name,
                Title = $"Copy codepoint \\u{choiceChar:X4} to clipboard (Ctrl+Shift+C)",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Glyph = "\ue8c1", // Characters
                AcceleratorKey = Key.C,
                AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                Action = _ =>
                {
                    Clipboard.SetText($"\\u{choiceChar:X4}");
                    return true;
                }
            },
            new ContextMenuResult
            {
                PluginName = Name,
                Title = $"U+{choiceChar:X4} - Character Information (Ctrl+O)",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Glyph = "\ue721", // Magnifying glass
                AcceleratorKey = Key.O,
                AcceleratorModifiers = ModifierKeys.Control,
                Action = _ =>
                {
                    var url = $"https://unicodeplus.com/U+{choiceChar:X4}";
                    if (Helper.OpenCommandInShell(DefaultBrowserInfo.Path, DefaultBrowserInfo.ArgumentsPattern, url))
                    {
                        return true;
                    }
                    
                    Context?.API.ShowMsg($"Plugin: {Name}", "Open default browser failed.");
                    return false;
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
        var cleanedQuery = string.IsNullOrEmpty(query.ActionKeyword)
            ? query.RawQuery.Trim() // no keyword - just trim
            : query.RawQuery[query.ActionKeyword.Length..].Trim();

        return cleanedQuery.All(c => c > 127) ?
            // exclusively non-ascii characters in the query - do reverse matching
            GetAsciiPrompt(cleanedQuery) :
            // some ascii characters - do forwards matching
            GetUnicodeSymbol(cleanedQuery);
    }

    private List<Result> GetAsciiPrompt(string query)
    {
        // Exact matching - agda has a key, we provide that key
        var agdaMatches = _agdaLookup.ReverseMatch(query);
        var htmlMatches = _htmlLookup.ReverseMatch(query);
        var matches = agdaMatches.Union(htmlMatches)
            .OrderBy(key => key.Any(char.IsDigit))
            .ThenBy(key => key.Length)
            .Take(MaxResults)
            .ToList();

        if (matches.Count == 0)
        {
            return [];
        }

        return matches
            .Select(
                match => MakeResult(
                    prefix: match,
                    suffix: "",
                    choices: [query],
                    nextChar: [],
                    score: 1
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
        var partialResult = "";
        var partialResultPrefix = "";
        List<Result> results = [];
        
        // for numeric matching
        var numberKey = "";
        var numberIndex = -1;
        var numberMatches = new List<string>();

        // cleanup the little numbers where appropriate? (replace them with their big equivalents)
        query = string.Join(null, query.Select(c => (char) (c is >= '₀' and <= '₉' ? c - 8272 : c)));
        
        // Exact matching - agda has a key, we provide that key
        var exactAgdaMatches = _agdaLookup.ExactMatches(query);
        var exactHtmlMatches = _htmlLookup.ExactMatches(query);
        var exactMatches = exactAgdaMatches.Union(exactHtmlMatches).ToList();
        
        // multiple-lookup implementation (\lambda\_2 → λ₂ or \lambda\alpha → λα)
        // if there are no exact matches AND there is a backslash within the string
        // todo: can we fetch a user-defined trigger shortcut?
        while (exactMatches.Count == 0 && (query.Contains('\\') || query.Contains(' ') || query.Contains('_') || query.Contains('^')))
        {
            // 1. find the longest substring that is a word
            var longestAgdaPartialMatch = _agdaLookup.LongestPartialMatch(query);
            var longestHtmlPartialMatch = _htmlLookup.LongestPartialMatch(query);
            var (longestPartialMatch, matchedCharacter) = longestAgdaPartialMatch.Length > longestHtmlPartialMatch.Length
                ? (longestAgdaPartialMatch, _agdaLookup.Get(longestAgdaPartialMatch))
                : (longestHtmlPartialMatch, _htmlLookup.Get(longestHtmlPartialMatch));

            // 2a. if there is not a match, break out of the loop
            if (longestPartialMatch == "" || matchedCharacter == "" || longestPartialMatch.Length >= query.Length)
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
                    numberIndex = int.Parse(numberIndexString) - 1;

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
            query = query.StartsWith("\\") ? query[1..] : query;
            
            // 4. loop
            // Exact matching - agda has a key, we provide that key
            exactAgdaMatches = _agdaLookup.ExactMatches(query);
            exactHtmlMatches = _htmlLookup.ExactMatches(query);
            exactMatches = exactAgdaMatches.Union(exactHtmlMatches).ToList();
        }
        
        // partial matching
        var (validAgdaChars, partialAgdaMatches) = _agdaLookup.PartialMatches(query);
        var (validHtmlChars, partialHtmlMatches) = _htmlLookup.PartialMatches(query);
        
        var validChars = validAgdaChars.Union(validHtmlChars).Order().ToList();
        var partialMatches = partialAgdaMatches.Union(partialHtmlMatches)
            .OrderBy(key => !key.StartsWith(query)) // prioritise terms that start with our query
            .ThenBy(key => key.Length) // then order by length - Hanlon's razor, we probably want the short option
            .ToList();

        // In the case where we have nothing useful to add (e == 0 and p == 0), we should avoid polluting the list
        //  of results (e == 0 and p == 0)
        // In the case where there is only one match, there is no point attempting to show the 'No match found yet!'
        //  line - we know what the match is going to be! This is only the case when there are no exact matches and
        //  exactly one partial match, so we skip this step if both those conditions are met (e == 0 and p == 1)
        // These two conditions combine to give e == 0 and p <= 1. By inverting them, we get e != 0 || p > 1
        if (exactMatches.Count != 0 || partialMatches.Count > 1)
            results.Add(
                item: MakeResult(
                    prefix:   partialResultPrefix + query,
                    suffix:   "",
                    choices:  AddPrefix(exactMatches, partialResult),
                    nextChar: validChars,
                    score:    10
                )
            );   

        // HTML / Unicode Numerics
        if (query.StartsWith("&#") || query.StartsWith('#') || query.StartsWith('u') || query.StartsWith("U+"))
        {
            var htmlMatch = HtmlLookup.NumericMatch(query.Replace("U+", "#x").Replace("u", "#x"));
            if (htmlMatch != null)
                results.Add(
                    item: MakeResult(
                        prefix:   partialResultPrefix + query,
                        suffix:   "",
                        choices:  [partialResult + htmlMatch],
                        nextChar: [],
                        score:    1,
                        isHtml:   true
                    )
                );
        }

        // Number-indexed matching support
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
                        prefix:   partialResultPrefix + numberKey,
                        suffix:   _subscriptNumber(numberIndex + 1),
                        choices:  [partialResult + numberMatches[numberIndex]],
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
                .Take(remainingSlots)
                .Select(s =>
                    MakeResult(
                        prefix:   partialResultPrefix + s,
                        suffix:   "",
                        choices:  AddPrefix(
                            _agdaLookup.ExactMatches(s).Union(_htmlLookup.ExactMatches(s)).ToList(),
                            partialResult
                        ),
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
                    prefix:   partialResultPrefix + searchKey,
                    suffix:   _subscriptNumber(j + jStart + 1),
                    choices:  [partialResult + options[j]],
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