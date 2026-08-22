using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Nest;

namespace ConsoleAppES
{
    public static class wikiPep_to_es
    {
        public static bool IS_LOAD_ALL=false;
        public static void addPepToESbyCountry(bool isLoadall)
        {
            IS_LOAD_ALL = isLoadall;
            string sql = " select   pepCountry  from   [dbo].[PEPCOUNTRY]";
            DataSet ds = new DbClass().GetDataSet(sql);
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string countryParam = dr["pepCountry"].ToString();
                if (isLoadall)
                {
                   // addSancToEs(countryParam);
                   // Console.WriteLine("Add all sanc " + countryParam);
                }
               
                    BulkDescriptor list = GetAppendlist(countryParam);
                    AppendToIndex(list); 
                    Console.WriteLine("Appended " + countryParam);
               
            }
        }
        public static void AppendToIndex(BulkDescriptor Descriptlist)
        {
            //es_helper.DeletDocByDateGraterThan(helper.getLastdate());
            es_helper.esClient().Bulk(Descriptlist);
        }
        public static BulkDescriptor GetAppendlist(string countryParam)
        {
            DateTime lastDate = helper.getLastdate();

            List<Person> data = new List<Person>();
            SqlDataReader reader;
            var descriptor = new BulkDescriptor(es_helper.getDefaultIndex());
            int i = 0;
            // DateTime lastDate = helper.getLastdate();
            using (SqlConnection conn = new SqlConnection(new DbClass().ConnStr()))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 0;
                // cmd.ResetCommandTimeout();// = 0;
                conn.Open();
                cmd.CommandText = "App_GetPEPForES_inc";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("country", countryParam));

                //DataSet ds= new DbClass().RunStoredProcQueryDS("App_GetSancForES", param);

                reader = cmd.ExecuteReader();
                
                if (reader.HasRows)
                {
                    // Obtain a row from the query result.
                    while (reader.Read())
                    {
                        i++;
                        descriptor.Index<Person>(op => op
                        .Document(new Person
                        {
                            SourceId = helper.CleanInput(reader["pepcode"].ToString()),
                            FirstName = helper.CleanInput(""),
                            LastName = helper.CleanInput(""),
                            midName = helper.CleanInput(""),
                            AltName = helper.CleanInput(reader["pepFullName"].ToString()),

                            OrgName = helper.CleanInput(reader["pepFullName"].ToString()),
                            Comment = helper.CleanInput(reader["pepDetail"].ToString()),
                            ListSource = helper.CleanInput(reader["ListSource"].ToString()),
                            ListSourceID = helper.CleanInput(reader["SourceID"].ToString()),
                            Location = helper.CleanInput(""),
                            Address = helper.CleanInput(""),
                            City = helper.CleanInput(""),
                            Country = helper.CleanInput(reader["Country"].ToString()),
                            ListedOn = DateTime.Now,
                            LastUpdateDate = DateTime.Now
                        })

                    );
                    }

                }
            }
            Console.WriteLine("total record appended: "+i.ToString());
            return descriptor;
        }

        public static List<ExtSanctionData> addSancToEs(string countryParam)
        {
            Console.WriteLine("start read from db "+ countryParam + DateTime.Now.ToString());
            List<ExtSanctionData> exNameList = new List<ExtSanctionData>();

            SqlDataReader reader;


            var descriptor = new BulkDescriptor("cgi_sanc");

            using (SqlConnection conn = new SqlConnection(new DbClass().ConnStr()))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 0;
                // cmd.ResetCommandTimeout();// = 0;
                conn.Open();
                cmd.CommandText = "App_GetPEPForES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("country", countryParam));
                reader = cmd.ExecuteReader();
                int i = 0;
                if (reader.HasRows)
                {
                    // Obtain a row from the query result.
                    while (reader.Read())
                    {
                        i++;
                        //   string dbg = helper.CleanInput(reader["ListSource"].ToString());
                        //  if (dbg.Contains("PEP_WORLD"))
                        //  dbg = dbg;

                        if (!String.IsNullOrEmpty(reader["pepFullName"].ToString()))
                        {
                            descriptor.Index<Person>(op => op
                               .Document(new Person
                               {
                                   SourceId = helper.CleanInput(reader["pepcode"].ToString()),
                                   FirstName = helper.CleanInput(""),
                                   LastName = helper.CleanInput(""),
                                   midName = helper.CleanInput(""),
                                   AltName = helper.CleanInput(reader["pepFullName"].ToString()),

                                   OrgName = helper.CleanInput(reader["pepFullName"].ToString()),
                                   Comment = helper.CleanInput(reader["pepDetail"].ToString()),
                                   ListSource = helper.CleanInput(reader["ListSource"].ToString()),
                                   ListSourceID = helper.CleanInput(reader["SourceID"].ToString()),
                                   Location = helper.CleanInput(""),
                                   Address = helper.CleanInput(""),
                                   City = helper.CleanInput(""),
                                   Country = helper.CleanInput(reader["Country"].ToString()),
                                   ListedOn =  DateTime.Now,
                                   LastUpdateDate =  DateTime.Now
                               })
                           );
                            // es_helper.esClient().IndexDocument(descriptor);
                        }

                    }
                    reader.Close();

                    var result = es_helper.esClient().Bulk(descriptor);
                    Console.WriteLine("bulk load completed ");

                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                Console.WriteLine("Number of records " + i);

            }
            return exNameList;
        }

    }
}

