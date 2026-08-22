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
    class uk_hm
    {
        public void addUK()
        {
            var descriptor = new BulkDescriptor(es_helper.getDefaultIndex());
           // string sql = @"select s.nameAlias_Id id, s.gender title,  'EU_UNION' ListSource, '10' ListSourceID , s.wholeName OrgName  
                       //     from[dbo].[STG_EUR_ALINAME] s where s.wholeName like '%Aleksej Fjodorovitj Lavrinenko%'";

         
                //string sql = "select listSourceId, Listsource from View_ES_SanctionSourceIDs ";
            //SqlDataReader reader = new DbClass().GetDataReader (sql);
            descriptor=adddesc();

            sancDb_to_es.AppendToIndex(descriptor);

        }

        private BulkDescriptor adddesc()
        {
            int i = 0;
            var descriptor = new BulkDescriptor(es_helper.getDefaultIndex());
            string sql = @"App_GetUKForES";
            Dictionary<string, object> param = new Dictionary<string, object>();
            //string sql = "select listSourceId, Listsource from View_ES_SanctionSourceIDs ";
            SqlDataReader reader = new DbClass().ExecProcGetReader(sql, param);
           // descriptor = adddesc(reader, descriptor);
            if (reader.HasRows)
            {
                // Obtain a row from the query result.
                while (reader.Read())
                {
                    string dbg = helper.CleanInput(reader["fullName"].ToString());
                        //if (dbg.Contains("ALICIA CABRERA"))
                     //   dbg = dbg;
                    i++;
                    descriptor.Index<Person>(op => op
                    .Document(new Person
                    {
                        final_id = helper.CInt(i.ToString()),
                        SourceId = helper.CleanInput(reader["id"].ToString()),
                        FirstName = helper.CleanInput(""),
                        LastName = helper.CleanInput(""),
                        midName = helper.CleanInput(""),
                        AltName = helper.CleanInput(""),

                        OrgName = helper.CleanInput(reader["fullName"].ToString()),
                        Comment = helper.CleanInput(reader["Note"].ToString()),
                        ListSource = helper.CleanInput("UK"),
                        ListSourceID = helper.CleanInput("5"),
                        Location = helper.CleanInput(reader["Nationality"].ToString()),
                        Address = helper.CleanInput(""),
                        City = helper.CleanInput(""),
                        Country = helper.CleanInput(reader["country"].ToString()),
                        ListedOn = DateTime.Now,
                        LastUpdateDate = DateTime.Now,
                    })
                );
                }
            }
            return descriptor;
        }
    }
}
