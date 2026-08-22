using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppBulckMatch
{
    public class SancDoc
    {
        public string OrgName { get; set; }
        public string ListSource { get; set; }
        public string comment { get; set; }
        public string country { get; set; }



    }
    public class Person
        {
            public string _id;

            public int final_id;

            public string SourceId;
            public string OrgName;
            public string FirstName;
            public string midName;
            public string LastName;
            public string AltName;
            public string Location;
            public string Comment;
            public string ListType;
            public string ListSource;
            public string ListSourceID;
            public DateTime LastUpdateDate;
            public DateTime ListedOn;
            public string RefNumber;
            public string Address;
            public string City;
            public string Country;
            public string IsActive;
        }


        public class ExtSanctionDatadelete
        {


            public string TITLE { get; set; }
            public string id { get; set; }
            public string Name { get; set; }

            public string Type { get; set; }

            public string ALTNAME { get; set; }
            public string NOTE { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string midName { get; set; }

            public string ListSource { get; set; }
            public string ListSourceID { get; set; }

            public string Address { get; set; }
            public string City { get; set; }
            public string Country { get; set; }

        }

    
}
