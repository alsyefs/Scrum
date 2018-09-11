using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Scrum
{
    public class CheckSession
    {
        string username = "", roleId = "", token = "";
        Configuration config = new Configuration();
        public Boolean sessionIsCorrect(string temp_username, string temp_roleId, string temp_token)
        {
            username = temp_username;
            roleId = temp_roleId;
            token = temp_token;

            Boolean correctSession = true;
            Boolean isEmptySession = checkIfSessionIsEmpty();
            if (isEmptySession)
                correctSession = false;
            Boolean correctSessionValues = checkSeesionValues();
            if (correctSessionValues == false)
            {
                correctSession = false;
            }
            return correctSession;
        }
        protected Boolean checkSeesionValues()
        {
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            Boolean correct = true;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from logins where login_username like '" + username + "' and roleId = '" + roleId + "' ";
            int countValues = Convert.ToInt32(cmd.ExecuteScalar());
            if (countValues < 1)//session has wrong values for any role.
                correct = false;
            cmd.CommandText = "select count(*) from logins where login_token like '" + token + "' and roleId = '" + roleId + "' ";
            int countTokenValues = Convert.ToInt32(cmd.ExecuteScalar());
            if (countTokenValues < 1)//session has wrong values for any role with the recieved token.
                correct = false;
            connect.Close();
            return correct;
        }
        protected Boolean checkIfSessionIsEmpty()
        {
            Boolean itIsEmpty = false;
            if (string.IsNullOrWhiteSpace(username) || (!roleId.Equals("1") && !roleId.Equals("2") && !roleId.Equals("3")))//if no session values for either username or roleId, set to false.
            {
                itIsEmpty = true;
            }
            return itIsEmpty;
        }
        public int adminAlerts()
        {
            int count = 0;
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count users to be approved:
            cmd.CommandText = "select count(*) from registrations";
            count = Convert.ToInt32(cmd.ExecuteScalar());
            //Count new projects to be approved:
            cmd.CommandText = "select count(*) from Projects where project_isApproved = 0 and project_isDenied = 0 and project_isTerminated = 0 and project_isDeleted = 0";
            count = count + Convert.ToInt32(cmd.ExecuteScalar());
            ////count users to be approved for projects:
            //cmd.CommandText = "select count(*) from UsersForProjects where usersForProjects_isApproved = 0 ";
            //count = count + Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return count;
        }
        public int masterAlerts()
        {
            int count = 0;
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count users to be approved for projects:
            cmd.CommandText = "select count(*) from UsersForProjects where usersForProjects_isApproved = 0 ";
            count = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return count;
        }
        public int developerAlerts()
        {
            int count = 0;
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count new assignments for user stories:
            cmd.CommandText = "select count(*) from UsersForUserStories where usersForUserSroties_isNotified = 0 ";
            count = Convert.ToInt32(cmd.ExecuteScalar());
            //count new assignments for sprint tasks:
            cmd.CommandText = "select count(*) from UsersForSprintTasks where usersForSprintTasks_isNotified = 0 ";
            count += Convert.ToInt32(cmd.ExecuteScalar());
            //count new assignments for test cases:
            cmd.CommandText = "select count(*) from UsersForTestCases where usersForTestCases_isNotified = 0 ";
            count += Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return count;
        }
    }
}