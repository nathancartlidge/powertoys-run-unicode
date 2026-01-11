using ManagedCommon;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Wox.Plugin;
using System.Windows.Input;
using Wox.Infrastructure;
using Wox.Plugin.Common;
using Microsoft.PowerToys.Settings.UI.Library;
using UnicodeInput.Core;
using Result = Wox.Plugin.Result;

namespace Community.PowerToys.Run.Plugin.UnicodeInput;

public partial class Main : IPlugin, IContextMenu, ISettingProvider
{
    private string IconPath { get; set; }

    private Lookup _lookup;
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

    static Main()
    {
        AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!.Resolving += (context, assemblyName) =>
        {
            if (assemblyName.Name != "UnicodeInput.Core") return null;

            var assemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, $"{assemblyName.Name}.dll");
            if (File.Exists(assemblyPath))
            {
                return context.LoadFromAssemblyPath(assemblyPath);
            }
            return null;
        };
    }

    public Main()
    {
        _lookup = new Lookup();
    }
    
    public Main(string directory)
    {
        _lookup = new Lookup(directory, MaxResults);
    }
    
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
    
    private Result MakeResult(string userInput, int? resultIndex, IReadOnlyList<string> choices,
                              IReadOnlyList<char> validNextChars, int score, string sources)
    {
        var titleStringBuilder = new StringBuilder();
        var subtitleStringBuilder = new StringBuilder();

        titleStringBuilder.Append(userInput);
        if (resultIndex is not null)
        {
            // to make the type checker happy
            var value = resultIndex + 1 ?? 0;
            titleStringBuilder.Append(_subscriptNumber(value));
        }
        if (choices.Count == 0)
        {
            // no exact match, but there are options if you keep typing - return a hint
            return new Result
            {
                Title = userInput + resultIndex,
                SubTitle = "No match found yet - keep typing! " + _arrayToString(validNextChars),
                IcoPath = IconPath,
                // if there is only one possible letter to be typed, this could easily get in the way
                Score = validNextChars.Count <= 1 ? score - 2 : score - 1,
                Action = _ => false
            };
        }

        // we have at least one choice
        titleStringBuilder.Append(" \u2192 ");
        titleStringBuilder.Append(choices[0]);
        
        if (sources.Length != 0)
        {
            subtitleStringBuilder.Append(sources + ' ');
        }
        
        // the default action changes depending upon `_doTyping`, so should the prompt
        // todo: -> "these symbols" when appropriate
        subtitleStringBuilder.Append(_doTyping ? "Input this symbol" : "Copy this symbol to the clipboard");
        if (choices.Count > 1)
        {
            subtitleStringBuilder.Append(" -- ");
            subtitleStringBuilder.Append('[');
            subtitleStringBuilder.Append(choices.Count);
            subtitleStringBuilder.Append(" variations available!]");
        }

        if (validNextChars.Count != 0)
        {
            subtitleStringBuilder.Append(" -- ");
            subtitleStringBuilder.Append(_arrayToString(validNextChars));
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
        if (string.IsNullOrEmpty(symbol)) return [];
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
        var results = _lookup.Query(query.RawQuery, query.ActionKeyword);
        
        return results.Select(r => 
           MakeResult(
               r.UserInput, 
               r.ResultIndex, 
               r.Choices, 
               r.ValidNextChars, 
               r.Score, 
               r.Sources
           )
        ).ToList();
    }

    public void Init(PluginInitContext context)
    {
        Context = context;
        Context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(Context.API.GetCurrentTheme());

        _lookup = new Lookup(Context.CurrentPluginMetadata.PluginDirectory, MaxResults);
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