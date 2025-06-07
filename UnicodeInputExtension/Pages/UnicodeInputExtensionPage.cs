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
            5);
        
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

    private static ListItem MakeItem(Result result)
    {
        return new ListItem(new NoOpCommand())
        {
            Title = result.UserInput + " -> " + result.Choices[0],
            Subtitle = _arrayToString(result.ValidNextChars),
            Icon = IconHelpers.FromRelativePath("Assets\\logo.png"),
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
                        .Select(MakeItem)
                );
            }
            RaiseItemsChanged(_items.Count);
        }
    }
}
