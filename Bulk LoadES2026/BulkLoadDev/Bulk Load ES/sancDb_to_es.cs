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
     
        public static BulkDescriptor GetAppendlist(out int count)
        {
            DateTime lastDate = helper.getLastdate();
            var descriptor = new BulkDescriptor(es_helper.getDefaultIndex());
            int recordId = 0;

            using (SqlConnection conn = new SqlConnection(new DbClass().ConnStr()))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 1800;
                conn.Open();
                cmd.CommandText = "App_GetSDNForES";
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int primaryCount = AppendPrimarySdnNames(reader, descriptor, lastDate, ref recordId);
                    reader.NextResult();
                    int altCount = AppendAlternateSdnNames(reader, descriptor, lastDate, ref recordId);
                    count = primaryCount + altCount;
                }
            }

            return descriptor;
        }

        private static int AppendPrimarySdnNames(SqlDataReader reader, BulkDescriptor descriptor, DateTime lastDate, ref int recordId)
        {
            int rowCount = 0;
            while (reader.Read())
            {
                recordId++;
                rowCount++;
                int currentRecordId = recordId;
                descriptor.Index<Person>(op => op.Document(MapSdnRowToPerson(reader, lastDate, currentRecordId)));
            }
            return rowCount;
        }

        private static int AppendAlternateSdnNames(SqlDataReader reader, BulkDescriptor descriptor, DateTime lastDate, ref int recordId)
        {
            int rowCount = 0;
            while (reader.Read())
            {
                recordId++;
                rowCount++;
                int currentRecordId = recordId;
                descriptor.Index<Person>(op => op.Document(MapSdnRowToPerson(reader, lastDate, currentRecordId)));
            }
            return rowCount;
        }

        private static Person MapSdnRowToPerson(SqlDataReader reader, DateTime lastDate, int recordId)
        {
            return new Person
            {
                final_id = recordId,
                SourceId = helper.CleanInput(reader["id"].ToString()),
                FirstName = helper.CleanInput(""),
                LastName = helper.CleanInput(""),
                midName = helper.CleanInput(""),
                AltName = helper.CleanInput(""),
                OrgName = helper.CleanInput(reader["fullName"].ToString()),
                Comment = helper.CleanInput(reader["Comment"].ToString()),
                ListSource = helper.CleanInput("SDN"),
                ListSourceID = helper.CleanInput("7"),
                Location = helper.CleanInput(""),
                Address = helper.CleanInput(reader["SanAddress"].ToString()),
                City = helper.CleanInput(""),
                Country = helper.CleanInput(reader["Country"].ToString()),
                ListedOn = lastDate,
                LastUpdateDate = lastDate,
            };
        }
        public static void AppendToIndex(BulkDescriptor list)
        {
            //es_helper.DeletDocByDateGraterThan(helper.getLastdate());
            es_helper.esClient().Bulk(list);
        }
        public static void AddSantionBySource(bool isLoadAll)
        {
            int count = 0;
            BulkDescriptor list = GetAppendlist(out count);
            AppendToIndex(list);
            helper.OutMsg("append record count " + count);
        }
        // this is not used
        public static void  addSancToEs(string listSourceIDParam)
        {
            DateTime lastDate = helper.getLastdate();

     
            helper.OutMsg("\n start read from db " + listSourceIDParam + DateTime.Now);
        //    List<ExtSanctionData> exNameList = new List<ExtSanctionData>();

            SqlDataReader reader;
            

            var descriptor = new BulkDescriptor(es_helper.getDefaultIndex());

            using (SqlConnection conn = new SqlConnection(new DbClass().ConnStr()))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandTimeout = 1800;
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
                                   ListSourceID = helper.CleanInput("7"),
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
                    helper.OutMsg("bulk load completed for "+ listSourceIDParam);

                }
                else
                {
                    helper.OutMsg("No rows found.");
                }
                helper.OutMsg("Number of records "+i);

            }
          //  return exNameList;
        }

    }
}

