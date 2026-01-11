// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CommandPalette.Extensions.Toolkit;
using UnicodeInput.Core;
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
                _items.AddRange(
                    results
                        .Where(result => result.Choices.Count > 0)
                        .Select(ListItemHelper.MakeItem)
                );
            }
            RaiseItemsChanged(_items.Count);
        }
    }
}
