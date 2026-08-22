using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nest;
using System.IO;
using System.Reflection;

namespace WinFormES
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static List<Person> search_sanc_async(string _text, string _listSourceID)
        {
          //  DocumentPath<Person> person = new DocumentPath<Person>(new Person());
            // var reservations = await es_client.SearchAsync<Person>(q => q .From(0) .Size(10) .Query(q=>q .Match(m=>m .Field(f=>f.FirstName).Query("ali")))));
            List<Person> peopleAsync = new List<Person>();
            var searchResult = es_helper.esClient().Search<Person>(s => s
                      .MatchAll()
                        .Size(10)
                        .Query(q => q.Bool(b => b
                           .Must(mu => mu
                                       .Match(m => m
                                               .Field(f => f.OrgName)
                                         .Query(_text)
                                         .Fuzziness(Fuzziness.EditDistance(0))
                                         .Operator(Operator.And)
                                         
                                          ) && q
                                        .Match(m => m
                                    .Field(f => f.ListSourceID)
                                            
                                           .Query(_listSourceID)
                                        )))));
            var pocoListWithIds = searchResult.Hits.Select(h =>
            {
                h.Source._id = h.Id;
                return h.Source;
            }).ToList();

            peopleAsync = searchResult.Documents.ToList();
            return peopleAsync;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string sql = @"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'finalSancNames'";
            DataSet ds = new DbClass().GetDataSet(sql);



           // PropertyInfo[] props = new Person().GetType().GetProperties();
           // radMultiColumnComboBox1.DataSource = ds.Tables[0];
           // radDropDownList1.DataSource = ds.Tables[0];
            radDropDownList1.ValueMember = "COLUMN_NAME";
            radDropDownList1.DisplayMember = "COLUMN_NAME";
        }
        public bool PartialUpdate(string id, object entity)
        {
            var result = es_helper.esClient().Update<Person, object>(DocumentPath<Person>.Id(id), i => i.Index(es_helper.getDefaultIndex()).Doc(entity));

            return result.IsValid;
        }

        private void bulkUpdate(string _ids, string _val, string filedName) 
        {
           // new Person().ListSourceID
            var updateByQueryResponse = es_helper.esClient().UpdateByQuery<object>(b => b
                        .Index(es_helper.getDefaultIndex())
                        .Query(q => q
                            .Ids(ids => ids
                                .Values(_ids)
                            )
                        )
                        .Script(s => s
                            .Source("ctx._source."+ filedName + " += params."+ filedName)
                            .Params(p => p
                                .Add(filedName, _val)
                            )
                        )
                    );
          bool stat=  updateByQueryResponse.IsValid;
        }

        private void update()
        {
            var client = new ElasticClient();

            var updateByQueryResponse = client.UpdateByQuery<object>(u => u
                .Index("usereventsreduced-*")
                .Script(s => s
                    .Source("ctx._source.AId = 1;")
                    .Lang("painless")
                )
                .Query(q => q
                    .Match(m => m
                        .Field("CId")
                        .Query("b60d505f-baf6-4522-b2a3-659509435c29")
                    )
                )
            );

        }
        private void button1_Click(object sender, EventArgs e)
        {
          
            // Create partial document with a dynamic
            dynamic updateDoc = new System.Dynamic.ExpandoObject();
            //updateDoc.Title = "My new title";
            List<Person> personList = search_sanc_async("Mehrdad", "7");

            foreach(Person P in personList)
            {
                P.ListSourceID = "mehrdad update";
                bulkUpdate(P._id, P.ListSourceID, "ListSource");
            }
             


          //  var countRequest = es_helper.esClient().Indices.Get(es_helper.getDefaultIndex());

           
        }
    }
}
