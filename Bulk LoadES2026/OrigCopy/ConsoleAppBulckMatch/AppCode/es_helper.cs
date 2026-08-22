using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Elasticsearch;

namespace ConsoleAppBulckMatch
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

            //string defaultIndex = System.Configuration.ConfigurationManager.AppSettings["sanc_index"].ToString();


            var settings = new ConnectionSettings(new Uri(url_es)).DefaultIndex(getDefaultIndex());

            try
            {
                es_client = new ElasticClient(settings);
            }
            catch { return es_client; }


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
            es_client = esClient();
            var response = es_client.Indices.Create(indexName,
                    index => index.Map<Person>(
                        x => x.AutoMap()
                    ));

            
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
            es_client = esClient();

            Console.Write(" start DROP INDEX ..." + DateTime.Now);

            es_client.Indices.DeleteAsync(new DeleteIndexRequest(Indices.Index(_indexName))).Wait();

          //  es_client.Indices.Delete(new DeleteIndexRequest(Indices.Index(_indexName)));
            //es_client.Indices.DeleteAsync(getDefaultIndex()).Wait();

                Console.Write(" completed DROP INDEX ..." + DateTime.Now);
           
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
    
        public static void DeleteAllDoc()
        {
            Console.Write("start delete existing ..." +DateTime.Now);
            esClient().DeleteByQuery<Person>(del => del
            .Query(q => q.QueryString(qs => qs.Query("*"))
        ).WaitForCompletion());
            Console.Write("Completed delete existing ..." + DateTime.Now);
          
        }


    }
}
