using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Scrum
{
    public class Configuration
    {
        //The below is Saleh's personal connection string:
        //static string connectionString = "data source=R14\\SALEH;initial catalog=ScrumDB;MultipleActiveResultSets=True;user id=sa; password=Saleh.Alsyefi1988 ";
        //The server's connection string:
        static string connectionString = "data source=Murcap02;initial catalog=ScrumDB;MultipleActiveResultSets=True; user id=sa; password=Saleh.Alsyefi1988";
        static string dbId = setDBID();
        static string dbPassword = setDBPassword();
        public string getConnectionString()
        {
            //System.Configuration.ConnectionStringSettings theString = staticConnectionString();
            SqlConnection connect = new SqlConnection(connectionString);
            return connectionString;
        }
        //Note that Gmail service might not allow to send emails, to solve that, you need to login to the below email 
        //regulary using the computer then check and verify the activity. Afterwards, the emailing service will work in the application.
        //This is a restriction from Gmail.
        string email = setEmail();
        //The password cannot be hashed as password are already in Gmail database; therefore, sending a hashed password will result rehashing the 
        // hash and password will be wrong. to verify password, Gmail does: hash(password) = hashedPassword
        //if we send the hashed password, Gmail would do: hash(hashedPassword) != hashedPassword
        string password = setPassword();
        protected static string setDBPassword()
        {
            string str_dbPassword = "";
            SqlConnection connect = new SqlConnection(connectionString);
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select TOP 1 key_DBPassword from keys";
            str_dbPassword = cmd.ExecuteScalar().ToString();
            connect.Close();
            return str_dbPassword;
        }
        protected static string setDBID()
        {
            string str_id = "";
            SqlConnection connect = new SqlConnection(connectionString);
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select TOP 1 key_DBID from keys";
            str_id = cmd.ExecuteScalar().ToString();
            connect.Close();
            return str_id;
        }
        protected static string setEmail()
        {
            string str_email = "";
            SqlConnection connect = new SqlConnection(connectionString);
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select TOP 1 key_email from keys";
            str_email = cmd.ExecuteScalar().ToString();
            connect.Close();
            return str_email;
        }
        protected static string setPassword()
        {
            string str_password = "";
            SqlConnection connect = new SqlConnection(connectionString);
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select TOP 1 key_password from keys";
            str_password = cmd.ExecuteScalar().ToString();
            connect.Close();
            return str_password;
        }
        public string getEmail()
        {
            return email;
        }
        public string getPassword()
        {
            return password;
        }
    }
}