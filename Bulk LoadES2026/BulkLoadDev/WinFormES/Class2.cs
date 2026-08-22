using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
namespace WinFormES
{
    public class SancDoc
    {
        public string OrgName { get; set; }
        public string ListSource { get; set; }
        public string comment { get; set; }
        public string country { get; set; }



    }
    class Class2
    {
        public List<SancDoc> FilterByNameContainmentInv(
    List<SancDoc> matched,
    string name)
        {
            if (matched == null || matched.Count == 0 || string.IsNullOrWhiteSpace(name))
                return new List<SancDoc>();

            string fullName = name.Trim();

            return matched
                .Where(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.OrgName))
                        return false;

                    string org = x.OrgName.Trim();

            // Ensure safe length (org substring slightly shorter)
            int targetLength = Math.Max(org.Length - 2, 1);

                    if (org.Length < targetLength)
                        return false;

            // Take substring FROM OrgName
            string search = org.Substring(0, targetLength);

            // INVERTED: fullName contains org fragment
            return fullName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .ToList();
        }


        public List<SancDoc> FilterByNameContainment(
    List<SancDoc> matched,
    string name)
        {
            if (matched == null || matched.Count == 0 || string.IsNullOrWhiteSpace(name))
                return new List<SancDoc>();

            string fullName = name.Trim();

            return matched
                .Where(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.OrgName))
                        return false;

                    string org = x.OrgName.Trim();

            // Ensure we don't go out of bounds
            int targetLength = Math.Max(org.Length - 2, 1);

                    if (fullName.Length < targetLength)
                        return false;

            // Take substring of input name
            string search = fullName.Substring(0, targetLength);

                    return org.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .ToList();
        }

        public List<SancDoc> FilterByNameContainment222(
            List<SancDoc> matched,
            string name)
        {
            if (matched == null || matched.Count == 0 || string.IsNullOrWhiteSpace(name))
                return new List<SancDoc>();

            string search = name.Trim();

            return matched
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.OrgName) &&
                    x.OrgName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                )
                .ToList();
        }
        public List<SancDoc> SearchSanctions(
                
                string searchText,
                int size = 100
)
        {
            var response = es_helper.esClient().Search<SancDoc>(s => s
                .Index("cgi_sanc")
                .Size(size)
                .Query(q => q
                    .Wildcard(w => w
                        .Field(f => f.OrgName.Suffix("keyword"))
                        .Value($"*{searchText}*")
                        .CaseInsensitive(true)
                    )
                )
            );

            if (!response.IsValid)
            {
                // log response.DebugInformation if needed
                throw new Exception(response.OriginalException?.Message ?? "Elasticsearch search failed");
            }

            return response.Documents.ToList();
        }


        private void getdata()
        {
    
        var searchText = "Alicia Cabrera";          // your input
        var indexName = "cgi_sanc";

        var response = es_helper.esClient().Search<SancDoc>(s => s
            .Index(indexName)
            .Size(0)
            .Query(q => q
                .Wildcard(w => w
                    .Field(f => f.OrgName.Suffix("keyword"))
                    .Value($"*{searchText}*")
                    .CaseInsensitive(true) // if your NEST version supports it
                )
            )
            .Aggregations(a => a
                .Terms("distinct_listSource", t => t
                    .Field(f => f.ListSource.Suffix("keyword"))
                    .Size(10)
                )
            )
        );
    }

    }
}
