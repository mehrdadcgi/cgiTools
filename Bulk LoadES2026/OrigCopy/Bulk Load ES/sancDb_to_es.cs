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
    public static class sancDb_to_es
    {
     
        public static BulkDescriptor GetAppendlist(string listSourceIDParam, out int count)
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
                cmd.CommandText = "App_GetSancForES_Inc";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add( new SqlParameter("lisSourceID", helper.ToInt(listSourceIDParam)));


                //DataSet ds= new DbClass().RunStoredProcQueryDS("App_GetSancForES", param);

                reader = cmd.ExecuteReader();
               
                if (reader.HasRows)
                {
                    // Obtain a row from the query result.
                    while (reader.Read())
                    {
                        string dbg = helper.CleanInput(reader["name"].ToString());
                        if (dbg.Contains("ALICIA CABRERA"))
                            dbg = dbg;
                        i++;
                        descriptor.Index<Person>(op => op
                        .Document(new Person
                        {
                            final_id = helper.CInt(reader["final_id"].ToString()),
                            SourceId = helper.CleanInput(reader["id"].ToString()),
                            FirstName = helper.CleanInput(reader["FirstName"].ToString()),
                            LastName = helper.CleanInput(reader["LastName"].ToString()),
                            midName = helper.CleanInput(reader["midName"].ToString()),
                            AltName = helper.CleanInput(reader["ALTNAME"].ToString()),

                            OrgName = helper.CleanInput(reader["Name"].ToString()),
                            Comment = helper.CleanInput(reader["Note"].ToString()),
                            ListSource = helper.CleanInput(reader["ListSource"].ToString()),
                            ListSourceID = helper.CleanInput(reader["listSourceId"].ToString()),
                            Location = helper.CleanInput(reader["Location"].ToString()),
                            Address = helper.CleanInput(reader["Address"].ToString()),
                            City = helper.CleanInput(reader["City"].ToString()),
                            Country = helper.CleanInput(reader["Country"].ToString()),
                            ListedOn = lastDate,
                            LastUpdateDate = helper.ToDate(reader["LastUpdateOn"].ToString()),
                        })
                    ) ;
                    }
                    
                }
            }
            count = i;
            return descriptor;
        }
        public static void AppendToIndex(BulkDescriptor list)
        {
            //es_helper.DeletDocByDateGraterThan(helper.getLastdate());
            es_helper.esClient().Bulk(list);
        }
        public static void AddSantionBySource(bool isLoadAll)
        {
            string sql = " select listSourceId, Listsource  from dbo.SancLlistSource where listSourceId in (4,6,7,9)";
            DataSet dataSet = new DbClass().GetDataSet(sql);
            foreach (DataRow dr in dataSet.Tables[0].Rows)
            {
                string param = dr["listSourceId"].ToString();
                string source=  dr["Listsource"].ToString();
                Console.WriteLine("List source "+ param);
                if (isLoadAll)
                {
                  //  addSancToEs(param);
                   // Console.WriteLine("Add all sanc " + param);

                }
               
                    int count=0;
                    BulkDescriptor list =GetAppendlist(param, out count);
                    AppendToIndex(list);
                    Console.WriteLine("append to sanc " + param+" : "+ source+ " record count " +count);

                
            }
        }
        // this is not used
        public static void  addSancToEs(string listSourceIDParam)
        {
            DateTime lastDate = helper.getLastdate();

     
            Console.WriteLine("\n start read from db " + listSourceIDParam + DateTime.Now);
        //    List<ExtSanctionData> exNameList = new List<ExtSanctionData>();

            SqlDataReader reader;
            

            var descriptor = new BulkDescriptor(es_helper.getDefaultIndex());

            using (SqlConnection conn = new SqlConnection(new DbClass().ConnStr()))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 0;
               // cmd.ResetCommandTimeout();// = 0;
                conn.Open();
                cmd.CommandText = "App_GetSancForES";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add( new SqlParameter("lisSourceID", helper.ToInt(listSourceIDParam)));

                reader = cmd.ExecuteReader();
                int i = 0;
                if (reader.HasRows)
                {
                    // Obtain a row from the query result.
                    while (reader.Read())
                    {
                        i++;
                        string dbg = helper.CleanInput(reader["name"].ToString());
                       if (dbg.Contains("ALICIA CABRERA"))
                             dbg = dbg;

                        if (!String.IsNullOrEmpty(reader["Name"].ToString()))
                        {
                            descriptor.Index<Person>(op => op
                               .Document(new Person
                               {
                                   final_id = helper.CInt(reader["final_id"].ToString()),

                                   SourceId = helper.CleanInput(reader["id"].ToString()),
                                   FirstName = helper.CleanInput(reader["FirstName"].ToString()),
                                   LastName = helper.CleanInput(reader["LastName"].ToString()),
                                   midName = helper.CleanInput(reader["midName"].ToString()),
                                   AltName = helper.CleanInput(reader["ALTNAME"].ToString()),

                                   OrgName = helper.CleanInput(reader["Name"].ToString()),
                                   Comment = helper.CleanInput(reader["Note"].ToString()),
                                   ListSource = helper.CleanInput(reader["ListSource"].ToString()),
                                   ListSourceID = helper.CleanInput(reader["listSourceId"].ToString()),
                                   Location = helper.CleanInput(reader["Location"].ToString()),
                                   Address = helper.CleanInput(reader["Address"].ToString()),
                                   City = helper.CleanInput(reader["City"].ToString()),
                                   Country = helper.CleanInput(reader["Country"].ToString()),
                                   ListedOn = lastDate,
                                   LastUpdateDate = helper.ToDate(reader["LastUpdateOn"].ToString())
                               })
                           ) ;
                        }

                    }
                    reader.Close();
                    var result = es_helper.esClient().Bulk(descriptor);
                    Console.WriteLine("bulk load completed for "+ listSourceIDParam);

                }
                else
                {
                    Console.WriteLine("No rows found.");
                }
                Console.WriteLine("Number of records "+i);

            }
          //  return exNameList;
        }

    }
}

