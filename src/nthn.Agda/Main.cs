using ManagedCommon;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public string Description => "Agda-style Unicode Input";
        public int MaxResults => 8;

        public static string PluginID => "778f24fc48714097b30303f83d5bed6a";

        private Result MakeResult(string prefix, List<string> choices, List<char> nextChar, int score)
        {
            var titleStringBuilder = new StringBuilder();
            titleStringBuilder.Append(prefix);
            if (choices.Count == 0) // we do not have a match {
            {
                // no result, but return a preview anyway
                return new Result
                {
                    Title = prefix,
                    SubTitle = nextChar.Count == 0
                        ? "No match found"
                        : "No match found yet - keep typing! " + _arrayToString(nextChar),
                    IcoPath = IconPath,
                    Score = nextChar.Count == 0 ? score - 5 : score - 1,
                    Action = e => false,
                };
            }

            titleStringBuilder.Append(" \u2192 ");
            titleStringBuilder.Append(choices[0]);

            var subtitleStringBuilder = new StringBuilder();
            subtitleStringBuilder.Append("Copy this symbol to the clipboard");
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
                Action = e =>
                {
                    Clipboard.SetText(choices[0]);
                    return true;
                },
            };
        }

        private string _arrayToString(List<char> l, string separator = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            
            for (var i = 0; i < l.Count; i++)
            {
                sb.Append(l[i]);
                if (i != l.Count - 1)
                {
                    sb.Append(separator);
                }
            }

            sb.Append("]");
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
            // Fetch the non-keyword part of the query
            string q;
            if (string.IsNullOrEmpty(query.ActionKeyword))
            {
                q = query.RawQuery.Trim();
            }
            else
            {
                q = query.RawQuery[query.ActionKeyword.Length..].Trim();
            }

            List<Result> results = [];
            
            // Exact matching - agda has a key, we provide that key
            var exactMatches = AgdaLookup.ExactMatches(q);
            (List<char> validChars, List<string> partialMatches) = _lookup.PartialMatch(q);
            
            results.Add(
                item: MakeResult(
                    prefix:   q,
                    choices:  exactMatches,
                    nextChar: validChars,
                    score:    0
                )
            );

            // Number-indexed matching support
            var (k, i, numberMatch) = _lookup.NumberMatch(q);
            if (numberMatch != null)
            {
                results.Add(
                    item: MakeResult(
                        prefix:   k + _subscriptNumber(i + 1),
                        choices:  [numberMatch],
                        nextChar: [],
                        score:    0
                    )
                );
            }

            // Partial Match candidates (to fill remaining slots)
            var remainingSlots = int.Max(0, MaxResults - results.Count);
            if (remainingSlots <= 0) return results; // early stopping

            results.AddRange(
                collection: partialMatches
                    .OrderBy(it => it.Length)
                    .Take(remainingSlots)
                    .Select(s => MakeResult(
                        prefix: s,
                        choices: AgdaLookup.ExactMatches(s),
                        nextChar: [],
                        score: partialMatches.Count == 1 ? 0 : -1
                    )
                )
            );

            // Number-indexed alternatives (to fill remaining slots
            remainingSlots = int.Max(0, MaxResults - results.Count);
            if (remainingSlots <= 0) return results; // early stopping
            
            int jStart;
            string searchKey;
            List<string> options;

            // which number should we start from?
            // - if our search was for a particular number, show subsequent options
            // - otherwise, start from 1
            // - if neither of these conditions apply, just return
            if (numberMatch != null)
            {
                options = AgdaLookup.ExactMatches(k)[(i + 1)..];
                searchKey = k;
                jStart = i + 1;
            }
            else if (exactMatches.Count > 1)
            {
                options = exactMatches[1..];
                searchKey = q;
                jStart = 1;
            }
            else return results;
            
            for (var j = 0; j < int.Min(remainingSlots, options.Count); j++)
            {
                results.Add(
                    item: MakeResult(
                        prefix:   searchKey + _subscriptNumber(j + jStart + 1),
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
