using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace WinFormES
{
    public partial class Form2 : RadForm
    {
        public Form2()
        {
            InitializeComponent();
        }

        public List<SancDoc> GetMatched(string Name, string NameInv, List<Person> Matched1, out string distance)
        {
             distance = "0";
            //debuging
            if (Name.ToUpper().Contains("Filipchuk".ToUpper()))
                distance ="ddddd";
              
            List<SancDoc> Matched = new List<SancDoc>();

            foreach (Person person in Matched1)
            {
                SancDoc dc = new SancDoc();
                dc.OrgName = helper.CleanInput( person.OrgName);
                dc.ListSource = person.ListSource;
                dc.comment = person.Comment;
                dc.country = person.Country +", "+ person.Location;
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

            if(UMatched.Count==0 && Matched.Count>0 && Matched.Count<7)
            {
                UMatched = Matched;
                distance = "50-100%";
                return UMatched;
            }
            //  radGridView1.DataSource = Matched;
            //  radGridViewFiltered.DataSource = UMatched;
            return new List<SancDoc>();
        }
        public void RunBulk()
        {
            //string sql = @"select GUID, name, ListSource,  NOTE from [dbo].[stg_tt_analytic26]";

            string sql = "	 select GUID, name, Location, Country, concat(LastName , ' ', FirstName ) name2 from [dbo].[stg_tt_analytic26]";// where Name='Dmitry Yevgenyevich Shevtsov'";

            DataSet ds = new DbClass().GetDataSet(sql);

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string NameInv = helper.CleanInput(dr["Name2"].ToString());
                string Name = helper.CleanInput( dr["Name"].ToString());
                //Name = NormalStringForES.NormalizeOrgName(Name);
                List<Person> Matched1 = NestFuzzySearch.SearchByOrgNameFuzzy(Name);
                List<Person> Matched2 = NestFuzzySearch.SearchByOrgNameFuzzy(NameInv);

                List<Person> combined = new List<Person>();

                foreach (Person person in Matched1)
                {
                    combined.Add(person);
                }
                foreach (Person person in Matched2)
                {
                    combined.Add(person);
                }
    

                if (Name.ToUpper().Contains("Filipchuk".ToUpper()))
                {
                    string s = "";
                }
                string guid = dr["GUID"].ToString();
                string distance = "";
                List<SancDoc> mymatch = GetMatched(Name, NameInv, combined, out distance);
                //List<SancDoc> mymatch2 = GetMatched(NameInv, Name, Matched1, out distance);

                List<SancDoc> combinedP = new List<SancDoc>();
                foreach (SancDoc person in mymatch)
                {
                    combinedP.Add(person);
                }
           
                List<SancDoc> MatchedList = new List<SancDoc>();

                foreach (SancDoc person in combinedP)
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
                    comment += doc.comment;
                    names += doc.OrgName + ", ";
                    ListSource += doc.ListSource + ", ";
                    countryEs += doc.country + ", ";

                    resultCount++;


                }
                //debuging use onlt
                if (mymatch.Count != 0 && ((SancDoc)mymatch[0]).OrgName.Contains("HAMROUI"))
                {
                    string s = ((SancDoc)mymatch[0]).OrgName;
                }
                //93c14c4f896044d3bc6479dab630a826
                if (mymatch.Count > 0)
                {
                    sql = @"update[dbo].[stg_tt_analytic26] set Country2='"+helper.CleanInput(countryEs+"',  ListSource='" + helper.CleanInput( ListSource) + "',  NOTE = '" + comment + "', SancMatched = '" + helper.CleanInput( names) + "', SancCount = '" + resultCount + "', Note2='"+distance+"'  ");
                    sql = sql + "where GUID = '" + guid + "'";
                    new DbClass().EXECUTE(sql);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // List<Person> Matched1 = NestFuzzySearch.test(this.radTextBox1.Text.ToString());
            //Name = NormalStringForES.NormalizeOrgName(this.radTextBox1.Text.ToString());
            Name =helper.CleanInput( this.radTextBox1.Text.ToString());
            List<Person> Matched1 = NestFuzzySearch.SearchByOrgNameFuzzy(Name);

            List<SancDoc> myList = new List<SancDoc>();
            foreach (Person doc in Matched1)
            {
                SancDoc MyMatch = new SancDoc();

                MyMatch.comment = doc.Comment;
                MyMatch.OrgName = doc.OrgName;
                MyMatch.ListSource = doc.ListSource;
                MyMatch.country = doc.Country;
                myList.Add(MyMatch);
            }
            string distance = "";
            List < SancDoc > matched = GetMatched(Name, Name, Matched1, out distance);

      
            radGridViewAll.DataSource = myList;
            radGridViewFiltered.DataSource = matched;
            this.label1.Text = distance;
            
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
        private void button2_Click(object sender, EventArgs e)
        {
            RunBulk();
        }
    }
}
