using System;
using System.Data;
using System.Data.SqlClient;
using Nest;

namespace ConsoleAppES
{
    public static class un_list_to_es
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
                cmd.CommandText = "App_GetUNListforEs";
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    count = AppendUnListRecords(reader, descriptor, lastDate, ref recordId);
                }
            }

            return descriptor;
        }

        private static int AppendUnListRecords(SqlDataReader reader, BulkDescriptor descriptor, DateTime lastDate, ref int recordId)
        {
            int rowCount = 0;
            while (reader.Read())
            {
                recordId++;
                rowCount++;
                int currentRecordId = recordId;
                descriptor.Index<Person>(op => op.Document(MapUnListRowToPerson(reader, lastDate, currentRecordId)));
            }
            return rowCount;
        }

        private static Person MapUnListRowToPerson(SqlDataReader reader, DateTime lastDate, int recordId)
        {
            return new Person
            {
                final_id = recordId,
                SourceId = helper.CleanInput(reader["DATAID"].ToString()),
                FirstName = helper.CleanInput(""),
                LastName = helper.CleanInput(""),
                midName = helper.CleanInput(""),
                AltName = helper.CleanInput(""),
                OrgName = helper.CleanInput(reader["fullName"].ToString()),
                ListType = helper.CleanInput(reader["Type"].ToString()),
                Comment = helper.CleanInput(reader["COMMENT"].ToString()),
                ListSource = helper.CleanInput("UN"),
                ListSourceID = helper.CleanInput("4"),
                Location = helper.CleanInput(reader["NATIONALITY"].ToString()),
                Address = helper.CleanInput(reader["SancAddress"].ToString()),
                City = helper.CleanInput(""),
                Country = helper.CleanInput(reader["country"].ToString()),
                ListedOn = lastDate,
                LastUpdateDate = lastDate,
            };
        }

        public static void AppendToIndex(BulkDescriptor list)
        {
            es_helper.esClient().Bulk(list);
        }

        public static void AddUnListToEs(bool isLoadAll)
        {
            int count = 0;
            BulkDescriptor list = GetAppendlist(out count);
            AppendToIndex(list);
            helper.OutMsg("append to UN list record count " + count);
        }
    }
}
