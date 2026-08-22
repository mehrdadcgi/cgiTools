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

public sealed class DbClass
    {
    public void ExecuteSQL(string sql, Dictionary<string, object> param)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL statement cannot be empty.", nameof(sql));

        using (var conn = new SqlConnection(ConnStr()))
        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 120;

            if (param != null)
            {
                foreach (var p in param)
                {
                    cmd.Parameters.AddWithValue(
                        p.Key,
                        p.Value ?? DBNull.Value
                    );
                }
            }

            conn.Open();
            cmd.ExecuteNonQuery();
        }


    }
    public DataSet GetSQLDataSet(string sql, Dictionary<string, object> param)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL cannot be null or empty.", nameof(sql));

        var ds = new DataSet();

        using (var conn = new SqlConnection(ConnStr()))
        using (var cmd = new SqlCommand(sql, conn))
        using (var da = new SqlDataAdapter(cmd))
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 120;

            // Add parameters safely
            if (param != null)
            {
                foreach (var p in param)
                {
                    cmd.Parameters.AddWithValue(
                        p.Key,
                        p.Value ?? DBNull.Value
                    );
                }
            }

            conn.Open();

            // Fill works even for UPDATE (returns empty DataSet)
            da.Fill(ds);
        }

        return ds;
    }

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
                try{
                    da.Fill(ds);
                }
                catch (Exception ex)
                {
                    ExceptionLogger.LogException(ex);

                }
                return ds;


            }
        }
    }

    public SqlDataReader GetDataReader(string sql)
    {
        using (SqlConnection myConnection=new SqlConnection(ConnStr())) { 
        SqlCommand myCommand;
        SqlDataReader myDataReader;

        myConnection.Open();

        //prepare sql statements
        myCommand = new SqlCommand(sql, myConnection);
        myDataReader = myCommand.ExecuteReader();
        return myDataReader;
            }

        }
        public string ConnStr()
        {

      //  var server = helper.ConfigByref("dbServer");// System.Configuration.ConfigurationManager.ConnectionStrings["dbServer"].ConnectionString;
      //  var db = helper.ConfigByref("dbName");
      //  var user = helper.ConfigByref("dbUser");  //System.Configuration.ConfigurationManager.ConnectionStrings["user"].ConnectionString;
       // var pass = helper.ConfigByref("dbPass");
        var connection = 
            System.Configuration.ConfigurationManager.ConnectionStrings["officeExDBConnectionString"].ConnectionString;
       // connection = "Data Source="+server+";Initial Catalog="+db+";User ID="+user+";Password="+pass+";Encrypt=False";
        return connection; 
         
        }

        public DataSet GetDataSet(string sql, string table)
        {
            ExceptionLogger.LogInfoApp(sql);

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
                if( helper.isSqlDebug())
                ExceptionLogger.LogException(ex);
            }
            myConnection.Close();
            return ds;
        }
        public DataSet GetDataSet(string sql)
        {
            ExceptionLogger.LogInfoApp(sql);
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
                if (helper.isSqlDebug())
                ExceptionLogger.LogException(ex+"\n cmd: "+sql);

        }
        myConnection.Close();
            return ds;

        }


        public void EXECUTE(string sql)
        {
            ExceptionLogger.LogInfoApp(sql);

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
            if (helper.isSqlDebug())
                ExceptionLogger.LogException(ex+" \n cmd: "+sql);

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
        ExceptionLogger.LogInfoApp(storedProcName);


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
                try{
                    da.Fill(ds);
                }catch(Exception ex)
                {
                    if (helper.isSqlDebug())
                        ExceptionLogger.LogException(ex+" :: \n cmd: "+cmd.CommandText);
                }
                return ds.Tables[0];


            }
        }
    }

    public void RunStoredProcNoQuery(string storedProcName, IDictionary<string, object> args)
    {
        ExceptionLogger.LogInfoApp(storedProcName);

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
            try
            {
                cmd.ExecuteNonQuery();
            }catch(Exception ex)
            {
                if (helper.isSqlDebug())
                    ExceptionLogger.LogException(ex);
                
            }
        }
    }

    public void RunStoredProcNoQuery(string storedProcName, IDictionary<string, object> args, out string error)
    {
        ExceptionLogger.LogInfoApp(storedProcName);
        error = "";
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
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (helper.isSqlDebug())
                    ExceptionLogger.LogException(ex);
                error="Error! "+ex.Message;
            }
        }
    }


}

