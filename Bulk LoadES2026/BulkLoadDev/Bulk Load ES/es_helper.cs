using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Elasticsearch;

namespace ConsoleAppES
{
    static class es_helper
    {
        static ElasticClient es_client = new ElasticClient();
        //static string defaultIndex = "";// "cgi_sanc";

        public static string getDefaultIndex()
        {
            string defaultIndex = System.Configuration.ConfigurationManager.AppSettings["sanc_index"].ToString();
            return defaultIndex;
        }

        public static ElasticClient esClient()
        {
            string url_es = System.Configuration.ConfigurationManager.AppSettings["es_url"].ToString();
            var settings = new ConnectionSettings(new Uri(url_es))
                .DefaultIndex(getDefaultIndex())
                .RequestTimeout(TimeSpan.FromMinutes(30));
            es_client = new ElasticClient(settings);
            return es_client;
        }

        private static void AddIndex(int finalid, string id,  string company, string fname, string lname, string listType,
                                 string location, string note, string source, string listedOn, string  altName,string MidName)
        {
            var person = new Person
            {
                final_id= finalid,
                SourceId = id,
                FirstName = fname,
                LastName = lname,
                Location = location,
                LastUpdateDate = DateTime.Now,
                Comment = note,
                ListSource = source,
                OrgName = company,
                AltName = altName,
                midName=MidName,
                ListType = listType,
                ListedOn = helper.ToDate( listedOn)


            };
            var indexResponse = esClient().IndexDocument(person);
        }


        public static void createIndex(string indexName)
        {
            var client = esClient();
            var response = client.Indices.Create(indexName,
                    index => index.Map<Person>(
                        x => x.AutoMap()
                    ));

            if (!response.IsValid)
            {
                helper.OutMsg(" FAILED to create index: " + indexName);
                helper.OutMsg(" " + response.DebugInformation);
                if (response.ServerError != null)
                    helper.OutMsg(" " + response.ServerError.Error.Reason);
                return;
            }

            helper.OutMsg(" completed create index: " + indexName);
        }
        public static List<Person> search_sanc_fname(string _text)
        {

            var searchResponse = es_helper.esClient().Search<Person>(s => s
                    .From(0)
                    .Size(50)

                    .Query(q => q
                         .Match(m => m
                            .Field(f => f.FirstName).Query(_text)
                            .Field(f2 => f2.LastName).Query(_text)
                            .Field(f3 => f3.OrgName).Query(_text)
                            .Fuzziness(Fuzziness.Auto)
                         )
                    )
                    );

            var people = searchResponse.Documents;
            return people.ToList();
        }

  
        
        public static void dropIndex(string _indexName)
        {
            var client = esClient();

            helper.OutMsg(" start DROP INDEX " + _indexName + " ... " + DateTime.Now);

            var existsResponse = client.Indices.Exists(_indexName);
            if (!existsResponse.IsValid)
            {
                helper.OutMsg(" FAILED to check index existence: " + _indexName);
                helper.OutMsg(" " + existsResponse.DebugInformation);
                return;
            }

            if (!existsResponse.Exists)
            {
                helper.OutMsg(" index not found (nothing to drop): " + _indexName);
                return;
            }

            var deleteResponse = client.Indices.Delete(_indexName);
            if (!deleteResponse.IsValid)
            {
                helper.OutMsg(" FAILED to drop index: " + _indexName);
                helper.OutMsg(" " + deleteResponse.DebugInformation);
                if (deleteResponse.ServerError != null)
                    helper.OutMsg(" " + deleteResponse.ServerError.Error.Reason);
                return;
            }

            var stillExists = client.Indices.Exists(_indexName);
            if (stillExists.Exists)
            {
                helper.OutMsg(" WARNING: index still exists after delete: " + _indexName);
                return;
            }

            helper.OutMsg(" completed DROP INDEX " + _indexName + " ... " + DateTime.Now);
        }
        public static bool DeletDocByDateGraterThan(DateTime deleteFrom)
        {
            var queryResponse =
                      esClient().DeleteByQuery<Person > (
                        s => s.Query(
                            q => q
                    .DateRange(r => r
                        .Field(f => f.ListedOn)
                        .GreaterThanOrEquals(new DateTime(deleteFrom.Year, deleteFrom.Month, deleteFrom.Day))

                        )
                      ));

           return queryResponse.IsValid;
        }

        public static bool DeletByListISourceDES(int listSourceID)
        {
            string listSourceIdValue = listSourceID.ToString();
            helper.OutMsg(" start delete by listSourceID " + listSourceIdValue + " ... " + DateTime.Now);

            var queryResponse = esClient().DeleteByQuery<Person>(del => del
                .Index(getDefaultIndex())
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Bool(bb => bb
                                .Should(
                                    s => s.Term(t => t.Field("listSourceID.keyword").Value(listSourceIdValue)),
                                    s => s.Term(t => t.Field("listSourceID").Value(listSourceIdValue))
                                )
                                .MinimumShouldMatch(1)
                            )
                        )
                    )
                )
                .WaitForCompletion()
                .Refresh(true)
            );

            if (!queryResponse.IsValid)
            {
                helper.OutMsg(" FAILED to delete by listSourceID: " + listSourceIdValue);
                helper.OutMsg(" " + queryResponse.DebugInformation);
                if (queryResponse.ServerError != null)
                    helper.OutMsg(" " + queryResponse.ServerError.Error.Reason);
                return false;
            }

            helper.OutMsg(" deleted " + queryResponse.Deleted + " records for listSourceID " + listSourceIdValue + " ... " + DateTime.Now);
            return true;
        }
    
        public static void DeleteAllDoc()
        {
            helper.OutMsg("start delete existing ..." +DateTime.Now);
            esClient().DeleteByQuery<Person>(del => del
            .Query(q => q.QueryString(qs => qs.Query("*"))
        ).WaitForCompletion());
            helper.OutMsg("Completed delete existing ..." + DateTime.Now);
          
        }


    }
}
