// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using UnicodeInputExtension.Core;
using UnicodeInputExtension.Helpers;

namespace UnicodeInputExtension.Pages;

internal sealed partial class UnicodeInputDetailsExtensionPage : ListPage
{
    private readonly IListItem[] _items;
    
    public UnicodeInputDetailsExtensionPage(List<string> choices)
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Unicode Input";
        PlaceholderText = "Look up a symbol...";
        Name = "See All Variations";
        _items = choices.Select(choice =>
            new ListItem(new CopyTextCommand(choice))
            {
                Title = choice
            } // todo: extra lookup commands
        ).ToArray<IListItem>();
    }

    public override IListItem[] GetItems()
    {
        return _items;
    }
}
