using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Scrum
{
    public class Logs
    {
        static string connection_string = "";
        SqlConnection connect = new SqlConnection(connection_string);

        public Logs(string temp_username, string temp_roleId, string temp_token,
            string currentPage, string previousPage, DateTime currentTime, string userIP)
        {
            Configuration config = new Configuration();
            connection_string = config.getConnectionString();
            connect = new SqlConnection(connection_string);
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string loginId = "";
            string username = "";
            string roleId = "";
            string token = "";
            string cur_page = "";
            string pre_page = "";
            string ip = "";
            if (!string.IsNullOrWhiteSpace(temp_username))
            {
                cmd.CommandText = "select loginId from Logins where login_username like '" + temp_username + "' ";
                loginId = cmd.ExecuteScalar().ToString();
                username = temp_username;
            }
            if (!string.IsNullOrWhiteSpace(temp_roleId))
            {
                roleId = temp_roleId;
            }
            if (!string.IsNullOrWhiteSpace(temp_token))
            {
                token = temp_token;
            }
            if (!string.IsNullOrWhiteSpace(currentPage))
            {
                cur_page = currentPage;
            }
            if (!string.IsNullOrWhiteSpace(previousPage))
            {
                pre_page = previousPage;
            }
            if (!string.IsNullOrWhiteSpace(userIP))
            {
                ip = userIP;
            }
            cmd.CommandText = "insert into Logs(log_time, loginId, log_username, log_roleId, log_token, log_currentPage, log_previousPage, log_userIP) values " +
                "('" + currentTime + "', '" + loginId + "', '" + username + "', '" + roleId + "', '" + token + "', '" + cur_page + "', '" + pre_page + "', '" + ip + "')";
            cmd.ExecuteScalar();
            connect.Close();
        }
    }
}