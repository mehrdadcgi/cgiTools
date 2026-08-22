using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace WinFormES
{
   public static class NestFuzzySearch
    {
        public static List<Person> test_delete(string name)
        {
                        var response = es_helper.esClient().Search<Person>(s => s
                .Size(20)
                .Query(q => q
                    .Bool(b => b
                        .Must(mu => mu
                            .MatchPhrase(mp => mp
                                .Field(f => f.OrgName)
                                .Query("ABD AL RAHMAN BIN UMAYR")
                            )
                        )
                        .Should(
                            sh => sh.Match(m => m
                                .Field(f => f.OrgName)
                                .Query("AL NUAYMI")
                            ),
                            sh => sh.Match(m => m
                                .Field(f => f.OrgName)
                                .Query("AL JABER")
                            )
                        )
                    )
                )
            );

            return response.Documents.ToList();
        }
        public static List<Person> SearchByOrgNameFuzzy(string Name)
        {
            string name= NormalStringForES.NormalizeOrgName(Name);
            if (string.IsNullOrWhiteSpace(name))
                return new List<Person>();

            var searchResponse = es_helper.esClient().Search<Person>(s => s
       .From(0)
       .Size(50)

       .Query(q => q
            .Match(m => m
               .Field(f => f.OrgName).Query(name)
               .Operator(Operator.And)
               .Fuzziness(Fuzziness.Auto)
            )
       )
        );

            return searchResponse.Documents.ToList();
            
        }
        public static List<Person> SearchByOrgNameFuzzy2222(
     
    string name,
    int size = 50
)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<Person>();

            var response = es_helper.esClient().Search<Person>(s => s
                .Index("cgi_sanc")
                .Size(size)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.OrgName)
                        .Query(name)
                        .Fuzziness(Fuzziness.Auto)   // edit distance auto
                        .Operator(Operator.And)      // all terms must match
                        .PrefixLength(1)              // first char exact
                        .MaxExpansions(50)
                    )
                )
            );

            if (!response.IsValid)
                throw new Exception(response.DebugInformation);

            return response.Documents.ToList();
        }

    }
}
