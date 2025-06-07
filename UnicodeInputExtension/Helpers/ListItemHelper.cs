using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Windows.System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using UnicodeInputExtension.Core;
using UnicodeInputExtension.Pages;

namespace UnicodeInputExtension.Helpers;

public class ListItemHelper
{
    private static string _arrayToString(List<char> characters, string separator = "")
    {
        var sb = new StringBuilder();
        sb.Append("[ ");
            
        for (var i = 0; i < characters.Count; i++)
        {
            sb.Append(characters[i]);
            if (i != characters.Count - 1)
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
        foreach (var c in i.ToString(CultureInfo.InvariantCulture))
        {
            output.Append((char) (c + 8272));
        }
        return output.ToString();
    }
    
    public static ListItem MakeItem(Result result)
    {
        var title = new StringBuilder();
        var subtitle = new StringBuilder();
        
        title.Append(result.UserInput);
        if (result.ResultIndex is not null)
        {
            var value = result.ResultIndex + 1 ?? 0;
            title.Append(_subscriptNumber(value));
        }

        if (result.Choices.Count == 0)
        {
            // no exact matches available, but if you keep typing there are possible matches
            return new ListItem(new NoOpCommand())
            {
                Title = title.ToString(),
                Subtitle = "No match found yet; keep typing! " + _arrayToString(result.ValidNextChars),
                Icon = IconHelpers.FromRelativePath("Assets\\logo.png"),
                
            };
        }
        
        // if we have got to this point, we must have at least one choice - show it!
        title.Append(" \u2192 ");
        title.Append(result.Choices[0]);

        var icon = IconHelpers.FromRelativePath("Assets\\logo.png");
        if (!result.Sources.Contains(LookupGroup.AgdaSource))
        {
            icon = IconHelpers.FromRelativePath(result.Sources.Length > 0 && result.Sources.Contains(LookupGroup.HtmlSource) 
                ? "Assets\\logo-html.png" 
                : "Assets\\logo-custom.png");
        }
        
        subtitle.Append(result.Sources);

        List<IContextItem> moreCommands = [];
        if (result.Choices[0].Length == 1)
        {
            var resultCharacter = char.ConvertToUtf32(result.Choices[0], 0);
            var resultCodepoint = $"\\u{resultCharacter:X4}";

            moreCommands = [
                new CommandContextItem(new ClipboardCommand(resultCodepoint, "Copy Codepoint"))
                {
                    Title = "Copy Codepoint",
                    Icon = new IconInfo("\uE8C8"),
                    RequestedShortcut = KeyChordHelpers.FromModifiers(ctrl: true, shift: true, vkey: VirtualKey.C)
                },
                new CommandContextItem(new OpenUrlCommand($"https://https://unicodeplus.com/U+{resultCharacter:X4}"))
                {
                    Title = "Character Information",
                    Icon = new IconInfo("\uE721"),
                    RequestedShortcut = KeyChordHelpers.FromModifiers(ctrl: true, vkey: VirtualKey.O)
                }
            ];
        }
        
        if (result.Choices.Count > 1)
        {
            if (subtitle.Length != 0) { subtitle.Append(" — "); }
            subtitle.Append('[');
            subtitle.Append(result.Choices.Count);
            subtitle.Append(" variants available!]");
            moreCommands.Insert(0,
                new CommandContextItem(new UnicodeInputDetailsExtensionPage(result.Choices)) 
                    { Title = $"View {result.Choices.Count} Variants" }
            );
        }

        if (result.ValidNextChars.Count != 0)
        {
            if (subtitle.Length != 0) { subtitle.Append(" — "); }
            subtitle.Append(_arrayToString(result.ValidNextChars));
        }
        
        // todo: no typing support as of yet; see https://github.com/microsoft/PowerToys/issues/39667
        return new ListItem(new ClipboardCommand(result.Choices[0]))
        // return new ListItem(new TyperCommand(result.Choices[0]))
        {
            Title = title.ToString(),
            Subtitle = subtitle.ToString(),
            Icon = icon,
            TextToSuggest = result.UserInput,
            MoreCommands = moreCommands.ToArray(),
        };
    }
}