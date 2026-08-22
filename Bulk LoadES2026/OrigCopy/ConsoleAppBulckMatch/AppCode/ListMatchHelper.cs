using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ConsoleAppBulckMatch.AppCodes
{
   static class ListMatchHelper
    {

        private static string Normalize(string input)
        {
            return input
                .ToLowerInvariant()
                .Replace("-", " ")
                .Replace(",", " ")
                .Replace("'", "")
                .Replace(".", "")
                .Trim();
        }

        private static HashSet<string> NormalizeAndTokenize(string input)
        {
            return Normalize(input)
                .Split(' ', (char)StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 1)
                .ToHashSet();
        }
        public static List<SancDoc> GetTopMatches(
   List<SancDoc> list,
   string name,
   int maxResults = 2)
        {
            if (list == null || list.Count == 0 || string.IsNullOrWhiteSpace(name))
                return new List<SancDoc>();

            var inputTokens = NormalStringForES.NormalizeOrgName(name);

            return list
                .Where(x => !string.IsNullOrWhiteSpace(x.OrgName))
                .Select(x =>
                {
                    var orgTokens = NormalizeAndTokenize(x.OrgName);

            // Token overlap score
            int matchCount = orgTokens.Count(t => inputTokens.Contains(t));
                 //   orgTokens.Intersect(inputTokens).Count();

            // Bonus if one contains the other
            bool contains =
                        Normalize(name).Contains(Normalize(x.OrgName)) ||
                        Normalize(x.OrgName).Contains(Normalize(name));

                    int score = matchCount + (contains ? 2 : 0);

                    return new
                    {
                        Doc = x,
                        Score = score
                    };
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Doc.OrgName.Length)
                .Take(maxResults)
                .Select(x => x.Doc)
                .ToList();
        }

    }
}
