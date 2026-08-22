using System;
using System.Web;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;


public class DbClass
    {

    public DataSet RunStoredProcQueryDS(string storedProcName, IDictionary<string, object> args)
    {
        
        using (SqlConnection conn = new SqlConnection(new DbClass().ConnStr()))
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = storedProcName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                conn.Open();
                foreach (KeyValuePair<string, object> kvp in args)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds;


            }
        }
    }

    public SqlDataReader GetDataReader(string sql)
    {
        SqlConnection myConnection = new SqlConnection(ConnStr());
        SqlCommand myCommand;
        SqlDataReader myDataReader;

        myConnection.Open();

        //prepare sql statements
        myCommand = new SqlCommand(sql, myConnection);
        myDataReader = myCommand.ExecuteReader();
        return myDataReader;
            

        }
        public string ConnStr()
        {
        //CGIDESIGN.CA
        //return  "workstation id=officeex;packet size=4096;user id=mehrdadabdi;data source='65.17.229.1';persist security info=True;initial catalog=officeex_data;password=546160";
        //IC-PACIFIC.COM
        //return  "workstation id=MEHRDAD;packet size=4096;user id=ic-pacific;data source='65.17.228.2';persist security info=True;initial catalog=officeex_data;password=my546160";
        //return "workstation id=MEHRDAD;packet size=4096;integrated security=SSPI;data source=mehrdad;persist security info=False;initial catalog=OfficeExWeb";
       // var connection =
       // System.Configuration.ConfigurationManager.
        //ConnectionStrings["ConnectionString"].ConnectionString;
        return  System.Configuration.ConfigurationSettings.AppSettings["ConnectionString"];
          
        }

        public DataSet GetDataSet(string sql, string table)
        {
            SqlConnection myConnection;
            SqlCommand myCommand;
            SqlDataAdapter sqlAdap;
            DataSet ds = new DataSet();

            string connStr = ConnStr();
            myConnection = new SqlConnection(connStr);
            myConnection.Open();
            try
            {
                myCommand = new SqlCommand(sql, myConnection);
                sqlAdap = new SqlDataAdapter(sql, connStr);
                sqlAdap.Fill(ds, table);
            }
            catch (Exception ex)
            {
                //sqlError=ex.ToString()+"<br>"+sql;

                // HttpContext.Current.Response.Write(sqlError);
            }
            myConnection.Close();
            return ds;
        }
        public DataSet GetDataSet(string sql)
        {

            SqlConnection myConnection;
            SqlCommand myCommand;
            SqlDataAdapter sqlAdap;
            DataSet ds = new DataSet();

            string connStr = ConnStr();
            myConnection = new SqlConnection(connStr);
            myConnection.Open();
            try
            {
                myCommand = new SqlCommand(sql, myConnection);
                sqlAdap = new SqlDataAdapter(sql, connStr);
                sqlAdap.Fill(ds, "Table1");
            }
            catch (Exception ex)
            {
               // new ExceptionOx(ex);
                //sqlError=ex.ToString()+"<br>"+sql;
                Console.WriteLine(ex.Message);
                // HttpContext.Current.Response.Write(sqlError);
            }
            myConnection.Close();
            return ds;

        }


        public void EXECUTE(string sql)
        {
            SqlConnection myConnection;
            SqlCommand myCommand;

            string connStr = ConnStr();
            myConnection = new SqlConnection(connStr);
            myConnection.Open();
            try
            {
                myCommand = new SqlCommand(sql, myConnection);
                myCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
            Console.WriteLine(ex.Message);
              //  File.AppendAllText(helper.ConfigByref("ErrorLog"), ex.Message);
                 //new OXReportWeb2019.AppCode()
                // HttpContext.Current.Response.Write(ex.InnerException+"<br>"+ex.Message);			
            }
            myConnection.Close();
        }

        public String CleanInputQaut(string strIn)
        {
            // Replace single Quote characters with 2 single Quote strings.
            try
            {
                if (strIn.Trim().Length == 0)
                {
                    return "''";
                }

                strIn = Regex.Replace(strIn, @"'", "`").Trim();
                return "'" + strIn + "'";
                //strIn=Regex.Replace(strIn, @" \""); 
            }
            catch
            {


            }

            return "''";
        }

        public String CleanInput(string strIn)
        {
            // Replace single Quote characters with 2 single Quote strings.
            try
            {
                if (strIn.Trim().Length == 0)
                {
                    return String.Empty;
                }

                strIn = Regex.Replace(strIn, @"'", "").Trim();
                return strIn;
                //strIn=Regex.Replace(strIn, @" \""); 
            }
            catch { }

            return String.Empty;
        }

        public String CleanInputP(object strIn)
        {
        if (strIn == null)
            return "";
        string str = strIn.ToString();
            try
            {
                if (str.Trim().Length == 0)
                {
                    return null;
                }
                if (str.Trim() == "0")
                {
                    return null;
                }

                strIn = Regex.Replace(str, @"'", "").Trim();
                return str;

            }
            catch
            {


            }

            return null;
        }

        public string FormatNumberText(double number)
        {
            return String.Format("{0:N2}", number);
        }
    public DataTable RunStoredProcQuery(string storedProcName, IDictionary<string, object> args)
    {
        using (SqlConnection conn = new SqlConnection(new DbClass().ConnStr()))
        {
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = storedProcName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection = conn;
                conn.Open();
                foreach (KeyValuePair<string, object> kvp in args)
                {
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                da.Fill(ds);
                return ds.Tables[0];


            }
        }
    }

    public SqlDataReader ExecProcGetReader(string storedProcName, IDictionary<string, object> args)
    {
        SqlDataReader dr ;
        SqlConnection conn = new SqlConnection(new DbClass().ConnStr());
            SqlCommand cmd = conn.CreateCommand();
        
            cmd.CommandText = storedProcName;
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (KeyValuePair<string, object> kvp in args)
            {
                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
            }
            cmd.CommandTimeout = 300;
           
            conn.Open();
           dr= cmd.ExecuteReader();
            
        return dr;
    }

    public void RunStoredProcNoQuery(string storedProcName, IDictionary<string, object> args)
    {
        using (SqlConnection conn = new SqlConnection(new DbClass().ConnStr()))
        using (SqlCommand cmd = conn.CreateCommand())
        {
            cmd.CommandText = storedProcName;
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (KeyValuePair<string, object> kvp in args)
            {
                cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
            }

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

}

