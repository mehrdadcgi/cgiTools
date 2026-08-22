using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace WinFormES
{
    public static class NameMatcher
    {
        public static List<SancDoc> FilterByLevenshtein95(
            List<SancDoc> matchedNames,
            string clientName,
            double threshold = 0.95
        )
        {
            if (matchedNames == null || string.IsNullOrWhiteSpace(clientName))
                return new List<SancDoc>();

            var clientNorm = NormalizeName(clientName);

            return matchedNames
                .Where(doc =>
                {
                    if (string.IsNullOrWhiteSpace(doc?.OrgName))
                        return false;

                    var candNorm = NormalizeName(doc.OrgName);

                    int dist = LevenshteinDistance(clientNorm, candNorm);
                    int maxLen = Math.Max(clientNorm.Length, candNorm.Length);

                    if (maxLen == 0) return false;

                    double similarity = 1.0 - (double)dist / maxLen;

                    return similarity >= threshold;
                })
                .ToList();


        }

        // ---------------- helpers ----------------

        private static string NormalizeName(string s)
        {
            s = s?.Trim() ?? "";
            s = Regex.Replace(s, @"\s+", " ");
            s = s.ToUpperInvariant();
            s = Regex.Replace(s, @"[^\p{L}\p{Nd}\s]", "");
            return s;
        }

        private static int LevenshteinDistance(string a, string b)
        {
            if (a.Length > b.Length)
                (a, b) = (b, a);

            int n = a.Length;
            int m = b.Length;

            if (n == 0) return m;
            if (m == 0) return n;

            var prev = new int[n + 1];
            var curr = new int[n + 1];

            for (int i = 0; i <= n; i++)
                prev[i] = i;

            for (int j = 1; j <= m; j++)
            {
                curr[0] = j;
                char bj = b[j - 1];

                for (int i = 1; i <= n; i++)
                {
                    int cost = (a[i - 1] == bj) ? 0 : 1;

                    curr[i] = Math.Min(
                        Math.Min(curr[i - 1] + 1, prev[i] + 1),
                        prev[i - 1] + cost
                    );
                }

                (prev, curr) = (curr, prev);
            }

            return prev[n];
        }
    }
}