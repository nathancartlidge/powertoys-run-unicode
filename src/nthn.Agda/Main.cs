using ManagedCommon;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Wox.Plugin;

namespace nthn.Agda
{
    public class Main : IPlugin
    {
        private string IconPath { get; set; }
        private AgdaLookup _lookup = new AgdaLookup();

        private PluginInitContext Context { get; set; }
        public string Name => "Agda";

        public string Description => "Agda Unicode Input";

        public static string PluginID => "778f24fc48714097b30303f83d5bed6a";

        private Result MakeResult(string symbol, int score)
        {
            return new Result
            {
                Title = symbol,
                SubTitle = "Copy this symbol to the clipboard",
                IcoPath = IconPath,
                Score = score,
                Action = e =>
                {
                    Clipboard.SetText(symbol);
                    return true;
                },
            };
        }
        
        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();

            // Fetch the non-keyword part of the query
            string q;
            if (string.IsNullOrEmpty(query.ActionKeyword))
            {
                q = query.RawQuery.Trim();
            }
            else
            {
                q = query.RawQuery.Substring(query.ActionKeyword.Length).Trim();
            }

            // Exact matching - agda has a key, we provide that key
            string[] exactMatches = _lookup.ExactMatches(q);
            for (int i = 0; i < exactMatches.Length; i++)
            {
                results.Add(
                    MakeResult(exactMatches[i], i == 0 ? 1 : -1)
                );
            }

            // Number-indexed matching support
            string numberMatch = _lookup.NumberMatch(q);
            if (numberMatch != null)
            {
                results.Add(
                    MakeResult(numberMatch, 0)
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
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                IconPath = "images/agda.light.png";
            }
            else
            {
                IconPath = "images/agda.dark.png";
            }
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }
    }
}
