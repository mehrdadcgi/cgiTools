using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ConsoleAppBulckMatch
{
 

    static class FindMatch
    {
        public static List<SancDoc> GetMatched(string Name, string NameInv, List<Person> Matched1, out string distance)
        {
            distance = "0";
            //debuging
            // if (Name.ToUpper().Contains("Francis Smith".ToUpper()))
            //    distance ="ddddd";

            List<SancDoc> Matched = new List<SancDoc>();

            foreach (Person person in Matched1)
            {
                SancDoc dc = new SancDoc();
                dc.OrgName = person.OrgName;
                dc.ListSource = person.ListSource;
                dc.comment = person.Comment;
                dc.country = person.Country + ", " + person.Location;
                Matched.Add(dc);
            }

            List<SancDoc> MatchedFiltered = new List<SancDoc>();



            MatchedFiltered = NameMatcher.FilterByLevenshtein95(Matched, Name);

            if (MatchedFiltered.Count > 0)
            {
                distance = "95-100%";
                return MatchedFiltered;

            }

            List<SancDoc> UMatched = new List<SancDoc>();
            // UMatched = RemoveDuplicatesByName(Matched);
            UMatched = AppCodes.ListMatchHelper.GetTopMatches(Matched, Name);

            if (MatchedFiltered.Count == 0)
            {
                UMatched = AppCodes.ListMatchHelper.GetTopMatches(Matched, Name);

                if (UMatched.Count > 0)
                {
                    distance = "60-100%";
                    return UMatched;
                }



            }

            if (UMatched.Count == 0 && Matched.Count > 0 && Matched.Count < 7)
            {
                UMatched = Matched;
                distance = "50-100%";
                return UMatched;
            }
            //  radGridView1.DataSource = Matched;
            //  radGridViewFiltered.DataSource = UMatched;
            return new List<SancDoc>();
        }
        public static void RunBulk()
        {
            //string sql = @"select GUID, name, ListSource,  NOTE from [dbo].[stg_tt_analytic26]";

            //  string sql = "	 select  GUID, name, Location, Country, concat(LastName , ' ', FirstName ) name2 from [dbo].[mill_analytic_part2]";// where Name='Dmitry Yevgenyevich Shevtsov'";

            //DataSet ds = new DbClass().GetDataSet(sql);
            string sql = "App_MillSancTest";
            DataSet ds = new DbClass().RunStoredProcQueryDS(sql, new Dictionary<string, object>());

            Console.WriteLine("start: "+DateTime.Now.ToString());
            int counter = 0;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                counter++;
                //label2.Text = counter.ToString()+;

                string NameInv = helper.CleanInput(dr["Name2"].ToString());
                string Name = helper.CleanInput(dr["Name"].ToString());
                Console.WriteLine( counter.ToString() + " :: " + Name);
                //Name = NormalStringForES.NormalizeOrgName(Name);
                List<Person> Matched1 = NestFuzzySearch.SearchByOrgNameFuzzy(Name);
                //if (Name.ToUpper().Contains("GAFFAR"))
                //{
                //    string s = "";
                //}
                string guid = dr["GUID"].ToString();
                string distance = "";
                List<SancDoc> mymatch = GetMatched(Name, NameInv, Matched1, out distance);

                List<SancDoc> MatchedList = new List<SancDoc>();

                foreach (SancDoc person in mymatch)
                {
                    SancDoc dc = new SancDoc();
                    dc.OrgName = person.OrgName;
                    dc.ListSource = person.ListSource;
                    dc.comment = person.comment;
                    dc.country = person.country;
                    MatchedList.Add(dc);
                }



                string comment = "";
                string names = "";
                string ListSource = "";
                string countryEs = "";
                int resultCount = 0;
                foreach (SancDoc doc in RemoveDuplicatesByList(mymatch))
                {
                    comment += helper.CleanInput(doc.comment);
                    names += helper.CleanInput(doc.OrgName) + ", ";
                    ListSource += helper.CleanInput(doc.ListSource) + ", ";
                    countryEs += helper.CleanInput(doc.country) + ", ";

                    resultCount++;


                }
                if (comment.Length > 4000)
                    comment = comment.Substring(1, 4000 - 2);
                //debuging use onlt
                if (mymatch.Count != 0 && ((SancDoc)mymatch[0]).OrgName.Contains("HAMROUI"))
                {
                    string s = ((SancDoc)mymatch[0]).OrgName;
                }




                Dictionary<string, object> param = new Dictionary<string, object>();
                param.Add("Country", helper.CleanInput(countryEs));
                param.Add("source_GUID", helper.CleanInput(guid));
                param.Add("comment", helper.CleanInput(comment));
                param.Add("MatchedNames", names);
                param.Add("MeatchedPerc", helper.CleanInput(distance));

                param.Add("SancCount", resultCount);
                param.Add("ListSource", ListSource);


                //new DbClass().RunStoredProcNoQuery("App_add_Mill_Tracker", param);

                //93c14c4f896044d3bc6479dab630a826
                /*
               if (mymatch.Count > 0)
                {
                    sql = @"update [dbo].[mill_analytic_part2] set Country2='" + helper.CleanInput(countryEs)+"',  ListSource='" + helper.CleanInput( ListSource) + "',  NOTE = '" + comment + "', SancMatched = '" + helper.CleanInput( names) + "', SancCount = '" + resultCount + "', Note2='"+distance+"'  ";
                    sql = sql + "where GUID = '" + guid + "'";
                    new DbClass().EXECUTE(sql);
                }
                
                    sql = "insert into Mill_Tracker (source_GUID, SancCount)values("+ guid + ", "+ resultCount + ")";
                    new DbClass().EXECUTE(sql);
                */

            }
           
        }
        public static List<SancDoc> RemoveDuplicatesByName(
               List<SancDoc> matchedNames)
        {
            return matchedNames
                .GroupBy(x => x.OrgName?.Trim().ToLower())
                .Select(g => g.First())
                .ToList();
        }
        public static List<SancDoc> RemoveDuplicatesByList(
            List<SancDoc> matchedNames)
        {
            return matchedNames
                .GroupBy(x => x.ListSource?.Trim().ToLower())
                .Select(g => g.First())
                .ToList();
        }
    }
}
