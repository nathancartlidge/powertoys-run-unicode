// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CommandPalette.Extensions.Toolkit;
using UnicodeInputExtension.Core;
using UnicodeInputExtension.Helpers;

namespace UnicodeInputExtension.Pages;

internal sealed partial class UnicodeInputExtensionPage : DynamicListPage
{
    private readonly Lock _resultsLock = new();
    private readonly Lookup _lookup;
    private readonly ListItem _emptyItem;
    private readonly List<ListItem> _items = [];
    
    public UnicodeInputExtensionPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Unicode Input";
        PlaceholderText = "Look up a symbol...";
        Name = "Open";
        _lookup = new Lookup(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mappings/"),
            10);
        
        _emptyItem = new ListItem(new NoOpCommand())
        {
            Title = "No results found.",
            // todo: custom icon
            Icon = IconHelpers.FromRelativePath("Assets\\logo.png"),
        };
    }

    public override ListItem[] GetItems() => _items.ToArray();

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
    
    private ListItem _makeItem(Result result)
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
        
        var sources = _lookup.GetLookupSources(result.UserInput, result.Choices[0]);
        subtitle.Append(sources);
        
        if (result.Choices.Count > 1)
        {
            subtitle.Append(" — [");
            subtitle.Append(result.Choices.Count);
            subtitle.Append(" variants available!]");
        }

        if (result.ValidNextChars.Count != 0)
        {
            subtitle.Append(" — ");
            subtitle.Append(_arrayToString(result.ValidNextChars));
        }
        
        return new ListItem(new ClipboardCommand(result.Choices[0]))
        {
            Title = title.ToString(),
            Subtitle = subtitle.ToString(),
            Icon = result.IsHtml ? IconHelpers.FromRelativePath("Assets\\logo-html.png") 
                : IconHelpers.FromRelativePath("Assets\\logo.png"),
            TextToSuggest = "keep typing!" // todo: what actually is this?
        };
    }
    
    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (oldSearch == newSearch) { return; }

        if (newSearch.Length == 0)
        {
            _items.Clear();
            _items.Add(_emptyItem);
            RaiseItemsChanged(_items.Count);
            return;
        }
        
        _emptyItem.Subtitle = newSearch;
        var results = _lookup.Query(newSearch);
        lock (_resultsLock)
        {
            _items.Clear();
            if (results.Count == 0)
            {
                _items.Add(_emptyItem);
            }
            else
            {
                var a = new Result("a", 0, [], [], 2);

                _items.AddRange(
                    results
                        .Where(result => result.Choices.Count > 0)
                        .Select(_makeItem)
                );
            }
            RaiseItemsChanged(_items.Count);
        }
    }
}
