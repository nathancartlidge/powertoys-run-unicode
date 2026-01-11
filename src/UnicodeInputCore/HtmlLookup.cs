#nullable enable
using System;
using System.Collections.Generic;

namespace UnicodeInput.Core;

public class HtmlLookup(Dictionary<string, string> mappings) : BaseLookup(mappings)
{
    private static string RemoveHtml(string key)
    {
        if (key.StartsWith('&'))
        {
            key = key[1..];
        }

        if (key.EndsWith(';'))
        {
            key = key[..^1];
        }

        return key;
    }

    public static string? NumericMatch(string key)
    {
        key = RemoveHtml(key);
        key = key.Split("#")[1];
        try
        {
            var keyValue = key.StartsWith('x') ? Convert.ToInt32(key[1..], 16) : Convert.ToInt32(key, 10);
            return keyValue > 12 ? char.ConvertFromUtf32(keyValue) : null;
        }
        catch (ArgumentOutOfRangeException) {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
    
    public new List<string> ExactMatches(string key)
    {
        return base.ExactMatches(RemoveHtml(key));
    }
    
    public new (List<char>, List<string>) PartialMatches(string key)
    {
        return base.PartialMatches(RemoveHtml(key));
    }

    public new string? Get(string key)
    {
        return base.Get(RemoveHtml(key));
    }
}