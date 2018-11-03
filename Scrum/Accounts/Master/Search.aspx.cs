using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Master
{
    public partial class Search : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            createSomeTable();
        }
        protected void initialPageAccess()
        {
            Configuration config = new Configuration();
            conn = config.getConnectionString();
            connect = new SqlConnection(conn);
            getSession();
            //Get from and to pages:
            string current_page = "", previous_page = "";
            if (HttpContext.Current.Request.Url.AbsoluteUri != null) current_page = HttpContext.Current.Request.Url.AbsoluteUri;
            if (Request.UrlReferrer != null) previous_page = Request.UrlReferrer.ToString();
            //Get current time:
            DateTime currentTime = DateTime.Now;
            //Get user's IP:
            string userIP = GetIPAddress();
            CheckSession session = new CheckSession();
            bool correctSession = session.sessionIsCorrect(username, roleId, token, current_page, previous_page, currentTime, userIP);
            if (!correctSession)
                clearSession();
            int int_roleId = Convert.ToInt32(roleId);
            if (int_roleId != 2)//2 = Master role.
                clearSession();
        }
        protected string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }
        protected void clearSession()
        {
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/");
        }
        protected void addSession()
        {
            Session.Add("username", username);
            Session.Add("roleId", roleId);
            Session.Add("loginId", loginId);
            Session.Add("token", token);
        }
        protected void getSession()
        {
            username = (string)(Session["username"]);
            roleId = (string)(Session["roleId"]);
            loginId = (string)(Session["loginId"]);
            token = (string)(Session["token"]);
        }
        protected void hideEverything()
        {
            lblResultsMessage.Visible = false;
            lblErrorMessage.Visible = false;
        }
        protected void createSomeTable()
        {
            hideEverything();
            bool correct = checkInput();
            if (correct)
            {
                grdResults.Visible = true;
                lblResultsMessage.Visible = false;
                //call a method to create a table for the selected criteria:
                if (drpSearch.SelectedIndex == 1)//Searching for projects
                    createProjectsTable();
                else if (drpSearch.SelectedIndex == 2)//Searching for user stories
                    createuserStoriesTable();
                else if (drpSearch.SelectedIndex == 3)//Searching for sprint tasks
                    createSprintTasksTable();
                else if (drpSearch.SelectedIndex == 4)//Searching for test cases
                    createTestCasesTable();
                else if (drpSearch.SelectedIndex == 5)//Searching for projects within a time period
                    createProjectsTimeTable();
                else if (drpSearch.SelectedIndex == 6)//Searching for user stories within a time period
                    createUserStoriesTimeTable();
                else if (drpSearch.SelectedIndex == 7)//Searching for sprint tasks within a time period
                    createSprintTasksTimeTable();
                else if (drpSearch.SelectedIndex == 8)//Searching for test cases within a time period
                    createTestCasesTimeTable();
                else if (drpSearch.SelectedIndex == 9)//Searching for users by keywords in full name
                    createSearchUsersTable();
            }
            hideEverything();
        }
        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }
        protected void showCalendars()
        {
            calFrom.Visible = true;
            calTo.Visible = true;
            lblFrom.Visible = true;
            lblTo.Visible = true;
            txtSearch.Visible = false;
            txtSearch.Text = "";
        }
        protected void hideCalendars()
        {
            calFrom.Visible = false;
            calTo.Visible = false;
            lblFrom.Visible = false;
            lblTo.Visible = false;
            txtSearch.Visible = true;
            txtSearch.Text = "";
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            hideEverything();
            bool correct = checkInput();
            if (correct)
            {
                grdResults.Visible = true;
                lblResultsMessage.Visible = false;
                //call a method to create a table for the selected criteria:
                if (drpSearch.SelectedIndex == 1)//Searching for projects
                    createProjectsTable();
                else if (drpSearch.SelectedIndex == 2)//Searching for user stories
                    createuserStoriesTable();
                else if (drpSearch.SelectedIndex == 3)//Searching for sprint tasks
                    createSprintTasksTable();
                else if (drpSearch.SelectedIndex == 4)//Searching for test cases
                    createTestCasesTable();
                else if (drpSearch.SelectedIndex == 5)//Searching for projects within a time period
                    createProjectsTimeTable();
                else if (drpSearch.SelectedIndex == 6)//Searching for user stories within a time period
                    createUserStoriesTimeTable();
                else if (drpSearch.SelectedIndex == 7)//Searching for sprint tasks within a time period
                    createSprintTasksTimeTable();
                else if (drpSearch.SelectedIndex == 8)//Searching for test cases within a time period
                    createTestCasesTimeTable();
                else if (drpSearch.SelectedIndex == 9)//Searching for users by keywords in full name
                    createSearchUsersTable();
            }
        }
        protected bool checkInput()
        {
            bool correct = true;
            lblErrorMessage.Text = "";
            if (drpSearch.SelectedIndex != 5 && drpSearch.SelectedIndex != 6 && drpSearch.SelectedIndex != 7 && drpSearch.SelectedIndex != 8)//These are time periods.
            {
                //check if search text is blank:
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    correct = false;
                    lblErrorMessage.Text += "Please type something in the search text field.<br/>";
                }
            }
            //if input has one quotation, replace it with a double quotaion to avoid SQL errors:
            txtSearch.Text = txtSearch.Text.Replace("'", "''");
            //check if no criteria was selected:
            if (drpSearch.SelectedIndex == 0)
            {
                correct = false;
                lblErrorMessage.Text += "Please select a search criteria.<br/>";
            }
            if (!correct)
                lblErrorMessage.Visible = true;
            else
                lblErrorMessage.Visible = false;
            return correct;
        }
        protected void drpSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            grdResults.Dispose();
            grdResults.DataSource = null;
            grdResults.Visible = false;
            if (drpSearch.SelectedIndex == 5 || drpSearch.SelectedIndex == 6 || drpSearch.SelectedIndex == 7 || drpSearch.SelectedIndex == 8)//These are time periods.
                showCalendars();
            else
                hideCalendars();
        }
        protected void grdResults_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdResults.PageIndex = e.NewPageIndex;
            grdResults.DataBind();
            if (drpSearch.SelectedIndex == 1)//Searching for projects
                rebindValuesForProjects();
            else if (drpSearch.SelectedIndex == 2)//Searching for user stories
                rebindValuesForUserStories();
            else if (drpSearch.SelectedIndex == 3)//Searching for sprint tasks
                rebindValuesForSprintTasks();
            else if (drpSearch.SelectedIndex == 4)//Searching for test cases
                rebindValuesForTestCases();
            else if (drpSearch.SelectedIndex == 5)//Searching for projects within a time period
                rebindValuesForProjectsTime();
            else if (drpSearch.SelectedIndex == 6)//Searching for user stories within a time period
                rebindValuesForUserStoriesTime();
            else if (drpSearch.SelectedIndex == 7)//Searching for sprint tasks within a time period
                rebindValuesForSprintTasksTime();
            else if (drpSearch.SelectedIndex == 8)//Searching for test cases within a time period
                rebindValuesForTestCasesTime();
            else if (drpSearch.SelectedIndex == 9)//Searching for users by keywords in full name
                rebindValuesForUsers();
        }
        protected void rebindValuesForUsers()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string searchString = txtSearch.Text.Replace("'", "''");
            if (grdResults.Rows.Count > 0)
            {
                //Hide the header called "User ID":
                grdResults.HeaderRow.Cells[0].Visible = false;
                //Hide IDs column and content which are located in column index 0:
                for (int i = 0; i < grdResults.Rows.Count; i++)
                {
                    grdResults.Rows[i].Cells[0].Visible = false;
                }
            }
            for (int row = 0; row < grdResults.Rows.Count; row++)
            {
                //Set the creator's link
                string creatorId = grdResults.Rows[row].Cells[0].Text;
                string creator = grdResults.Rows[row].Cells[1].Text;
                HyperLink creatorLink = new HyperLink();
                creatorLink.Text = creator + " ";
                creatorLink.NavigateUrl = "Profile.aspx?id=" + creatorId;
                grdResults.Rows[row].Cells[1].Controls.Add(creatorLink);
            }
            connect.Close();
        }
        protected void createSearchUsersTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("User ID", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            string id = "", name = "";
            string searchString = txtSearch.Text.Replace("'", "''");
            string[] words = searchString.Split(' ');
            SortedSet<string> set_results = new SortedSet<string>();
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    cmd.CommandText = "select count(*) from users where (user_firstname+ ' ' +user_lastname) like '%" + word + "%' ";
                    int countUsersMatchingWord = Convert.ToInt32(cmd.ExecuteScalar());
                    for (int i = 1; i <= countUsersMatchingWord; i++)
                    {
                        cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userId ASC), * FROM [Users] where (user_firstname+ ' ' +user_lastname) like '%" + word + "%' ) as t where rowNum = '" + i + "'";
                        string temp_userId = cmd.ExecuteScalar().ToString();
                        set_results.Add(temp_userId);
                    }
                }
            }
            int totalUsers = set_results.Count;
            for (int i = 0; i < totalUsers; i++)
            {
                id = set_results.ElementAt(i);
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + id + "' ";
                name = cmd.ExecuteScalar().ToString();
                dt.Rows.Add(id, name);
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            grdResults.Visible = true;
            rebindValuesForUsers();
        }
        protected void createProjectsTable() { }
        protected void rebindValuesForProjects() { }
        protected void createuserStoriesTable() { }
        protected void rebindValuesForUserStories() { }

        protected void createSprintTasksTable() { }
        protected void rebindValuesForSprintTasks() { }

        protected void createTestCasesTable() { }
        protected void rebindValuesForTestCases() { }

        protected void createProjectsTimeTable() { }
        protected void rebindValuesForProjectsTime() { }

        protected void createUserStoriesTimeTable() { }
        protected void rebindValuesForUserStoriesTime() { }

        protected void createSprintTasksTimeTable() { }
        protected void rebindValuesForSprintTasksTime() { }

        protected void createTestCasesTimeTable() { }
        protected void rebindValuesForTestCasesTime() { }

    }
}