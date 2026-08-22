using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;

namespace ConsoleAppES
{
    public static class helper
    {
        public static void OutMsg(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void OutMsg(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public static string GetConfigByKey(string key)
        {
            string val = System.Configuration.ConfigurationManager.AppSettings[key].ToString();
            return val;
        }
        public static void updatLastdate()
        {
            string sql = "insert into APP_ESLastUpdate (APP_NAMES,EsLastUpdateDate) VALUES ('Elastic Bulk','"+DateTime.Now.AddDays(-1).ToString()+"')";
            new DbClass().EXECUTE(sql);

        }
        public static DateTime getLastdate()
        {
            string sql = "select Max(EsLastUpdateDate) EsLastUpdateDate from APP_ESLastUpdate";
             System.Data.DataSet ds= new DbClass().GetDataSet(sql);

            string date = "";
            try
            {
                date = ds.Tables[0].Rows[0][0].ToString();
            }
            catch { }

            DateTime mydate = helper.ToDate(date);

            return mydate;


        }
        public static int ToInt(int? str)
        {
            int i = 0;
            if (str != null)
                Int32.TryParse(str.ToString(), out i);
            return i;
        }

        public static int ToInt(string str)
        {
            int i = 0;
            Int32.TryParse(str, out i);
            return i;
        }
        public static DateTime ToDate(string str)
        {
            DateTime d = new DateTime();
            DateTime.TryParse(str, out d);
            return d;
        }
        public static bool isDate(string str)
        {
            bool isD = true;
            DateTime d;
            if (!DateTime.TryParse(str, out d))
                isD = false;
            return isD;
        }

        public static String CleanInput(string strIn)
        {
            try
            {
         
                if (strIn.Trim().Length == 0)
                {
                    return String.Empty;
                }
                strIn = Regex.Replace(strIn, @"'", "").Trim();
                strIn = Regex.Replace(strIn, @"\n", "").Trim();
                strIn = Regex.Replace(strIn, @"\r", "").Trim();
                strIn = Regex.Replace(strIn, @"\t", "").Trim();
                strIn = Regex.Replace(strIn, @"\r\n", "").Trim();
                return strIn;
            }
            catch { }

            return String.Empty;
        }
        public static bool IsWholeNumber(String strNumber)
        {
            Regex objNotWholePattern = new Regex("[^0-9]");
            return !objNotWholePattern.IsMatch(strNumber);
        }
        public static string supperAdmin
        {
            get { return ConfigurationManager.AppSettings["supperAdmin"]; }
        }

        public static bool CBool(string str)
        {
            bool isTrue = false;
            bool.TryParse(str, out isTrue);
            return isTrue;
        }
        public static bool IsInteger(string str)
        {
            str = str.Trim();
            return (Regex.IsMatch(str, @"^[\+\-]?\d+$"));
        }


        public static int CInt(object str)
        {
            int i = 0;
            Int32.TryParse(str.ToString(), out i);
            return i;
        }
        public static double CDouble(object str)
        {
            double i = 0;
            Double.TryParse(str.ToString(), out i);
            return i;
        }
        public static decimal CDecimal(object str)
        {
            Decimal i = 0;
            Decimal.TryParse(str.ToString(), out i);
            return i;
        }
        public static string FormatNumber2(string str)
        {
            str = str.Replace(",", String.Empty);
            return String.Format("{0:#,#.00}", CDouble(str));

        }
        public static string FormatNumber2(object nbr)
        {
            return FormatNumber2(nbr.ToString());
        }
        public static string FormatNumber6(string str)
        {
            return String.Format("{0:N8}", CDouble(str));
        }
        public static string FormatNumber6(object nbr)
        {
            return FormatNumber6(nbr.ToString());
        }
        public static string FormatNumber0(object str)
        {
            return String.Format("{0:#,#.##}", CDouble(str.ToString()));
        }
        public static string CDate(string date)
        {
            DateTime mydate = DateTime.Now;//.ToString("MM/dd/yyyy");
            DateTime.TryParse(date, out mydate);
            return mydate.ToString("MM/dd/yyyy");
        }

        public static int CDateInt(object date)
        {
            try
            {
                DateTime date2 = Convert.ToDateTime(date);
                string s = date2.Year.ToString() + date2.Month.ToString("00") + date2.Day.ToString("00");
                return Convert.ToInt32(s);
            }
            catch { return 0; }
        }

    }
}

