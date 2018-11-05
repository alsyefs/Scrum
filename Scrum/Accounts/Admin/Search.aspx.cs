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

namespace Scrum.Accounts.Admin
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
            if (int_roleId != 1)
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
                    createUserStoriesTable();
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
                    createUserStoriesTable();
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
            if (grdResults.Rows.Count == 0)
                lblResultsMessage.Visible = true;
            else
                lblResultsMessage.Visible = false;
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
                rebindValuesForProjects();
            else if (drpSearch.SelectedIndex == 6)//Searching for user stories within a time period
                rebindValuesForUserStories();
            else if (drpSearch.SelectedIndex == 7)//Searching for sprint tasks within a time period
                rebindValuesForSprintTasks();
            else if (drpSearch.SelectedIndex == 8)//Searching for test cases within a time period
                rebindValuesForTestCases();
            else if (drpSearch.SelectedIndex == 9)//Searching for users by keywords in full name
                rebindValuesForUsers();
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
        protected void createProjectsTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Project Name", typeof(string));
            dt.Columns.Add("Created by", typeof(string));
            dt.Columns.Add("Created time", typeof(string));
            dt.Columns.Add("Creator ID", typeof(string));
            string searchString = txtSearch.Text.Replace("'", "''");
            string[] words = searchString.Split(' ');
            SortedSet<string> set_results = new SortedSet<string>();
            string id = "", project_name = "", createdBy = "", createdOn = "", creatorId = "";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    cmd.CommandText = "select count(*) from Projects where project_isDeleted = 0 and project_name like '%" + word + "%' ";
                    int countMatchingWord = Convert.ToInt32(cmd.ExecuteScalar());
                    for (int i = 1; i <= countMatchingWord; i++)
                    {
                        cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY projectId ASC), * FROM [Projects] where project_isDeleted = 0 and project_name like '%" + word + "%' ) as t where rowNum = '" + i + "'";
                        string tempId = cmd.ExecuteScalar().ToString();
                        set_results.Add(tempId);
                    }
                }
            }
            int totalCount = set_results.Count;
            for (int i = 0; i < totalCount; i++)
            {
                //Get the project ID:
                id = set_results.ElementAt(i);
                //Get project name:
                cmd.CommandText = "select project_name from Projects where projectId = '" + id + "' ";
                project_name = cmd.ExecuteScalar().ToString();
                //Get the project's creator User ID:
                cmd.CommandText = "select project_createdBy from Projects where projectId = '" + id + "' ";
                creatorId = cmd.ExecuteScalar().ToString();
                //Convert the User ID to a name:
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + creatorId + "' ";
                createdBy = cmd.ExecuteScalar().ToString();
                //Get project creation date:
                cmd.CommandText = "select project_createdDate from Projects where projectId = '" + id + "' ";
                createdOn = cmd.ExecuteScalar().ToString();
                dt.Rows.Add(project_name, createdBy, Layouts.getTimeFormat(createdOn), creatorId);
                //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            rebindValuesForProjects();
        }
        protected void rebindValuesForProjects()
        {
            if (grdResults.Rows.Count > 0)
            {
                //Hide the header called "User ID":
                grdResults.HeaderRow.Cells[3].Visible = false;
                //Hide IDs column and content which are located in column index 3:
                for (int i = 0; i < grdResults.Rows.Count; i++)
                {
                    grdResults.Rows[i].Cells[3].Visible = false;
                }
            }
            connect.Open();
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                string project_name = "", createdBy = "", createdOn = "", creatorId = "";
                for (int row = 0; row < grdResults.Rows.Count; row++)
                {
                    //Set links to review a user:
                    project_name = grdResults.Rows[row].Cells[0].Text;
                    createdBy = grdResults.Rows[row].Cells[1].Text;
                    createdOn = grdResults.Rows[row].Cells[2].Text;
                    creatorId = grdResults.Rows[row].Cells[3].Text;
                    //Get the Project ID:
                    cmd.CommandText = "select [projectId] from [Projects] where project_name like '" + project_name + "' and " +
                        "project_createdDate = '" + Layouts.getOriginalTimeFormat(createdOn) + "' and project_createdBy = '" + creatorId + "' ";
                    string id = cmd.ExecuteScalar().ToString();
                    //string linkToReviewUser = "ReviewUser.aspx?id=" + id;
                    HyperLink projectLink = new HyperLink();
                    HyperLink userLink = new HyperLink();
                    HyperLink dateLink = new HyperLink();
                    projectLink.Text = project_name + " ";
                    userLink.Text = createdBy + " ";
                    dateLink.Text = Layouts.getTimeFormat(createdOn) + " ";
                    projectLink.NavigateUrl = "ViewProject.aspx?id=" + id;
                    userLink.NavigateUrl = "Profile.aspx?id=" + creatorId;
                    dateLink.NavigateUrl = "ViewProject.aspx?id=" + id;
                    grdResults.Rows[row].Cells[0].Controls.Add(projectLink);
                    grdResults.Rows[row].Cells[1].Controls.Add(userLink);
                    grdResults.Rows[row].Cells[2].Controls.Add(dateLink);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
            connect.Close();
        }
        protected void createUserStoriesTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("User story ID", typeof(double));
            dt.Columns.Add("As a (type of user)", typeof(string));
            dt.Columns.Add("I want to (some goal)", typeof(string));
            dt.Columns.Add("So that (reason)", typeof(string));
            dt.Columns.Add("Date introduced", typeof(string));
            dt.Columns.Add("Date considered for implementation", typeof(string));
            dt.Columns.Add("Developers responsible for", typeof(string));
            dt.Columns.Add("Current status", typeof(string));
            dt.Columns.Add("Creator User ID", typeof(string));
            dt.Columns.Add("DB User Story ID", typeof(string));
            dt.Columns.Add("Archive User Story", typeof(string));
            string id = "", userStoryId = "", asARole = "", iWant = "", soThat = "", dateIntroduced = "", dateConsidered = "",
                developersResponsible = "", currentStatus = "";
            string creatorId = "", removeUserStoryCommand = " Archive ";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string searchString = txtSearch.Text.Replace("'", "''");
            string[] words = searchString.Split(' ');
            SortedSet<string> set_results = new SortedSet<string>();
            int int_roleId = Convert.ToInt32(roleId);
            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    if (int_roleId == 1)//If an admin is searching, get everything:
                    {
                        cmd.CommandText = "select count(*) from UserStories where (userStory_asARole like '%" + word + "%' or userStory_iWantTo like '%" + word + "%' or userStory_uniqueId like '" + word + "' or userStory_soThat like '%" + word + "%' " +
                              "or userStory_currentStatus like '%" + word + "%') ";
                        int countMatchingWord = Convert.ToInt32(cmd.ExecuteScalar());
                        for (int i = 1; i <= countMatchingWord; i++)
                        {
                            cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where " +
                                "(userStory_asARole like '%" + word + "%' or userStory_iWantTo like '%" + word + "%' or userStory_uniqueId like '" + word + "' or userStory_soThat like '%" + word + "%' " +
                                "or userStory_currentStatus like '%" + word + "%') ) as t where rowNum = '" + i + "'";
                            string tempId = cmd.ExecuteScalar().ToString();
                            set_results.Add(tempId);
                        }
                    }
                    else//If a master or a developer is searching, get the related results:
                    {
                        cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                        string userId = cmd.ExecuteScalar().ToString();
                        cmd.CommandText = "select count(*) from UsersForProjects where userId = '" + userId + "' ";
                        int totalProjectsForUser = Convert.ToInt32(cmd.ExecuteScalar());
                        for (int j = 1; j <= totalProjectsForUser; j++)
                        {
                            cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForProjectsId ASC), * FROM [UsersForProjects] where userId = '" + userId + "') as t where rowNum = '" + j + "'";
                            string projectId = cmd.ExecuteScalar().ToString();
                            //Check if the selected project is deleted:
                            cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
                            int project_isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                            if (project_isDeleted == 0)//If the project is not deleted, fetch its user stories: (0 = false)
                            {
                                cmd.CommandText = "select count(*) from UserStories where projectId = '" + projectId + "' and (userStory_asARole like '%" + word + "%' or userStory_iWantTo like '%" + word + "%' or userStory_uniqueId like '" + word + "' or userStory_soThat like '%" + word + "%' " +
                                    "or userStory_currentStatus like '%" + word + "%') ";
                                int countMatchingWord = Convert.ToInt32(cmd.ExecuteScalar());
                                for (int i = 1; i <= countMatchingWord; i++)
                                {
                                    cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where projectId = '" + projectId + "' and " +
                                        "(userStory_asARole like '%" + word + "%' or userStory_iWantTo like '%" + word + "%' or userStory_uniqueId like '" + word + "' or userStory_soThat like '%" + word + "%' " +
                                        "or userStory_currentStatus like '%" + word + "%') ) as t where rowNum = '" + i + "'";
                                    string tempId = cmd.ExecuteScalar().ToString();
                                    set_results.Add(tempId);
                                }
                            }
                        }
                    }
                }
            }
            int totalCount = set_results.Count;
            for (int i = 0; i < totalCount; i++)
            {
                //Get the user story ID:
                id = set_results.ElementAt(i);
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + id + "' ";
                userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_asARole from UserStories where userStoryId = '" + id + "' ";
                asARole = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_iWantTo from UserStories where userStoryId = '" + id + "' ";
                iWant = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_soThat from UserStories where userStoryId = '" + id + "' ";
                soThat = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_dateIntroduced from UserStories where userStoryId = '" + id + "' ";
                dateIntroduced = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_dateConsideredForImplementation from UserStories where userStoryId = '" + id + "' ";
                dateConsidered = cmd.ExecuteScalar().ToString();
                //Loop through the developers for the selected User Story:
                cmd.CommandText = "select count(*) from UsersForUserStories where userStoryId = '" + id + "' ";
                int usersForUserStory = Convert.ToInt32(cmd.ExecuteScalar());
                for (int j = 1; j <= usersForUserStory; j++)
                {
                    cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForUserStoriesId ASC), * FROM [UsersForUserStories] where userStoryId = '" + id + "' ) as t where rowNum = '" + j + "'";
                    string developerId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + developerId + "' ";
                    if (j == 1)
                        developersResponsible = cmd.ExecuteScalar().ToString();
                    else
                        developersResponsible = developersResponsible + ", " + cmd.ExecuteScalar().ToString();
                }
                cmd.CommandText = "select userStory_currentStatus from UserStories where userStoryId = '" + id + "' ";
                currentStatus = cmd.ExecuteScalar().ToString();
                //Creator's info, which will be hidden:
                cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + id + "' ";
                creatorId = cmd.ExecuteScalar().ToString();
                dt.Rows.Add(userStoryId, asARole, iWant, soThat, Layouts.getTimeFormat(dateIntroduced), Layouts.getTimeFormat(dateConsidered), developersResponsible, currentStatus, creatorId, id, removeUserStoryCommand);
                //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            //Sort by "User story ID" column in "ASC" ascending order:
            if (dt != null)
            {
                DataView dv = new DataView(dt);
                dv.Sort = "User story ID" + " " + "ASC";
                grdResults.DataSource = dv;
                grdResults.DataBind();
            }
            rebindValuesForUserStories();
        }
        protected void rebindValuesForUserStories()
        {
            int int_roleId = Convert.ToInt32(roleId);
            if (grdResults.Rows.Count > 0)
            {
                //Hide the headers called "User ID" and "User Story ID":
                grdResults.HeaderRow.Cells[8].Visible = false;
                grdResults.HeaderRow.Cells[9].Visible = false;
                //Hide IDs column and content which are located in column index 8:
                for (int i = 0; i < grdResults.Rows.Count; i++)
                {
                    grdResults.Rows[i].Cells[8].Visible = false;
                    grdResults.Rows[i].Cells[9].Visible = false;
                }
            }
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            string userStoryId = "", asARole = "", iWant = "", soThat = "", dateIntroduced = "", dateConsidered = "",
                   developersResponsible = "", currentStatus = "";
            string creatorId = "", db_userStoryId = "";
            for (int row = 0; row < grdResults.Rows.Count; row++)
            {
                //Set links to review a user:
                userStoryId = grdResults.Rows[row].Cells[0].Text;
                asARole = grdResults.Rows[row].Cells[1].Text;
                iWant = grdResults.Rows[row].Cells[2].Text;
                soThat = grdResults.Rows[row].Cells[3].Text;
                dateIntroduced = grdResults.Rows[row].Cells[4].Text;
                dateConsidered = grdResults.Rows[row].Cells[5].Text;
                developersResponsible = grdResults.Rows[row].Cells[6].Text;
                currentStatus = grdResults.Rows[row].Cells[7].Text;
                creatorId = grdResults.Rows[row].Cells[8].Text;
                db_userStoryId = grdResults.Rows[row].Cells[9].Text;
                string removeUserStoryCommand = grdResults.Rows[row].Cells[10].Text;
                //Loop through the developers for the selected User Story:
                cmd.CommandText = "select count(*) from UsersForUserStories where userStoryId = '" + db_userStoryId + "' ";
                int usersForUserStory = Convert.ToInt32(cmd.ExecuteScalar());
                HyperLink developerResponsibleLink = new HyperLink();
                for (int j = 1; j <= usersForUserStory; j++)
                {
                    HyperLink participantLink = new HyperLink();
                    cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForUserStoriesId ASC), * FROM [UsersForUserStories] where userStoryId = '" + db_userStoryId + "' ) as t where rowNum = '" + j + "'";
                    string developerId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + developerId + "' ";
                    string developer_name = cmd.ExecuteScalar().ToString();
                    participantLink.Text = developer_name + " ";
                    participantLink.NavigateUrl = "Profile.aspx?id=" + developerId;
                    grdResults.Rows[row].Cells[6].Controls.Add(participantLink);
                    if (usersForUserStory > 1)
                    {
                        HyperLink temp = new HyperLink();
                        temp.Text = "<br/>";
                        grdResults.Rows[row].Cells[6].Controls.Add(temp);
                    }
                }
                //Get the User Story ID:
                string id = db_userStoryId;
                string userStoryUrl = "ViewUserStory.aspx?id=" + id;
                HyperLink idLink = new HyperLink();
                HyperLink asARoleLink = new HyperLink();
                HyperLink iWantLink = new HyperLink();
                HyperLink soThatLink = new HyperLink();
                HyperLink dateIntroducedLink = new HyperLink();
                HyperLink dateConsideredLink = new HyperLink();
                HyperLink currentStatusLink = new HyperLink();
                idLink.Text = userStoryId + " ";
                asARoleLink.Text = asARole + " ";
                iWantLink.Text = iWant + " ";
                soThatLink.Text = soThat + " ";
                dateIntroducedLink.Text = Layouts.getTimeFormat(dateIntroduced) + " ";
                dateConsideredLink.Text = Layouts.getTimeFormat(dateConsidered) + " ";
                currentStatusLink.Text = currentStatus + " ";
                idLink.NavigateUrl = userStoryUrl;
                asARoleLink.NavigateUrl = userStoryUrl;
                iWantLink.NavigateUrl = userStoryUrl;
                soThatLink.NavigateUrl = userStoryUrl;
                dateIntroducedLink.NavigateUrl = userStoryUrl;
                dateConsideredLink.NavigateUrl = userStoryUrl;
                currentStatusLink.NavigateUrl = userStoryUrl;
                //User Story remove button:
                LinkButton removeUserStoryLink = new LinkButton();
                removeUserStoryLink.Text = removeUserStoryCommand + " ";
                removeUserStoryLink.Command += new CommandEventHandler(RemoveUserStoryLink_Click);
                removeUserStoryLink.CommandName = id;
                removeUserStoryLink.CommandArgument = userStoryId;
                removeUserStoryLink.Enabled = true;
                removeUserStoryLink.CssClass = "removeUserStoryButton";
                //Check if the user story has been deleted already, if so, disable the button:
                cmd.CommandText = "select userStory_isDeleted from UserStories where userStoryId = '" + id + "' ";
                int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (isDeleted == 1)
                    removeUserStoryLink.Enabled = false;
                int int_creatorId = Convert.ToInt32(creatorId);
                cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                int int_userId = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select count(userStory_editedBy) from UserStories where userStoryId = '" + id + "' ";
                int isEditedBySomeone = Convert.ToInt32(cmd.ExecuteScalar());
                if (isEditedBySomeone > 0)
                {
                    cmd.CommandText = "select userStory_editedBy from UserStories where userStoryId = '" + id + "' ";
                    int int_editorId = Convert.ToInt32(cmd.ExecuteScalar());
                    if (int_userId != int_creatorId && int_roleId != 2 && int_roleId != 1 && int_userId != int_editorId)
                        removeUserStoryLink.Enabled = false;
                }
                else
                {
                    if (int_userId != int_creatorId && int_roleId != 2 && int_roleId != 1)
                        removeUserStoryLink.Enabled = false;
                }
                if (!removeUserStoryLink.Enabled)
                {
                    removeUserStoryLink.CssClass = "disabledRemoveUserStoryButton";
                }
                grdResults.Rows[row].Cells[0].Controls.Add(idLink);
                grdResults.Rows[row].Cells[1].Controls.Add(asARoleLink);
                grdResults.Rows[row].Cells[2].Controls.Add(iWantLink);
                grdResults.Rows[row].Cells[3].Controls.Add(soThatLink);
                grdResults.Rows[row].Cells[4].Controls.Add(dateIntroducedLink);
                grdResults.Rows[row].Cells[5].Controls.Add(dateConsideredLink);
                grdResults.Rows[row].Cells[7].Controls.Add(currentStatusLink);
                grdResults.Rows[row].Cells[10].Controls.Add(removeUserStoryLink);
            }
            connect.Close();
        }
        protected void RemoveUserStoryLink_Click(object sender, CommandEventArgs e)
        {
            string userStoryId = e.CommandName;
            string userStoryUniqueId = e.CommandArgument.ToString();
            lblRemoveUserStoryMessage.Text = "Are you sure you want to archive the user story# " + userStoryUniqueId + "?";
            lblUserStoryId.Text = userStoryId;
            divRemoveUserStory.Visible = true;
        }
        protected void btnConfirmRemoveUserStory_Click(object sender, EventArgs e)
        {
            string userStoryIdToRemove = lblUserStoryId.Text;
            //Delete the selected user story:
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "update UserStories set userStory_isDeleted = 1 where userStoryId = '" + userStoryIdToRemove + "'  ";
            cmd.ExecuteScalar();
            //Update all sprint tasks related to the deleted user story:
            cmd.CommandText = "update SprintTasks set sprintTask_isDeleted = 1 where userStoryId = '" + userStoryIdToRemove + "'  ";
            cmd.ExecuteScalar();
            //Update all test cases related to the deleted sprint tasks of the selected user story:
            cmd.CommandText = "select count(*) from SprintTasks where userStoryId = '" + userStoryIdToRemove + "' ";
            int totalTestCases = Convert.ToInt32(cmd.ExecuteScalar());
            for (int i = 1; i <= totalTestCases; i++)
            {
                cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] where userStoryId = '" + userStoryIdToRemove + "' ) as t where rowNum = '" + i + "'";
                string temp_sprintTaskId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "update TestCases set testCase_isDeleted = 1 where sprintTaskId = '" + temp_sprintTaskId + "'  ";
                cmd.ExecuteScalar();
            }
            connect.Close();
            divRemoveUserStory.Visible = false;
            Page.Response.Redirect(Page.Request.Url.ToString(), true);
        }
        protected void btnCancelRemoveUserStory_Click(object sender, EventArgs e)
        {
            divRemoveUserStory.Visible = false;
        }
        protected void createSprintTasksTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Sprint task ID", typeof(double));
            dt.Columns.Add("User story ID", typeof(string));
            dt.Columns.Add("Task description", typeof(string));
            dt.Columns.Add("Date introduced", typeof(string));
            dt.Columns.Add("Date considered for implementation", typeof(string));
            dt.Columns.Add("Date completed", typeof(string));
            dt.Columns.Add("Developers responsible for", typeof(string));
            dt.Columns.Add("Current status", typeof(string));
            dt.Columns.Add("Creator User ID", typeof(string));
            dt.Columns.Add("DB Sprint Task ID", typeof(string));
            dt.Columns.Add("Archive Sprint Task", typeof(string));
            string id = "", sprintTask_uniqueId = "", userStoryUniqueId = "", taskDescription = "", dateIntroduced = "", dateConsidered = "", dateCompleted = "",
                developersResponsible = "", currentStatus = "";
            string creatorId = "", removeSprintTaskCommand = " Archive ";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string searchString = txtSearch.Text.Replace("'", "''");
            string[] words = searchString.Split(' ');
            SortedSet<string> set_results = new SortedSet<string>();
            int int_roleId = Convert.ToInt32(roleId);
            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    if (int_roleId == 1)//If an admin is searching, get everything:
                    {
                        cmd.CommandText = "select count(*) from SprintTasks where and (sprintTask_uniqueId like '" + word + "' or sprintTask_taskDescription like '%" + word + "%' or sprintTask_currentStatus like '%" + word + "%') ";
                        int countMatchingWord = Convert.ToInt32(cmd.ExecuteScalar());
                        for (int i = 1; i <= countMatchingWord; i++)
                        {
                            cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] where " +
                                " and (sprintTask_uniqueId like '" + word + "' or sprintTask_taskDescription like '%" + word + "%' or sprintTask_currentStatus like '%" + word + "%')  ) as t where rowNum = '" + i + "'";
                            string tempId = cmd.ExecuteScalar().ToString();
                            set_results.Add(tempId);
                        }
                    }
                    else//If a master or a developer is searching, get the related results:
                    {
                        cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                        string userId = cmd.ExecuteScalar().ToString();
                        cmd.CommandText = "select count(*) from UsersForProjects where userId = '" + userId + "' ";
                        int totalProjectsForUser = Convert.ToInt32(cmd.ExecuteScalar());
                        for (int j = 1; j <= totalProjectsForUser; j++)
                        {
                            cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForProjectsId ASC), * FROM [UsersForProjects] where userId = '" + userId + "') as t where rowNum = '" + j + "'";
                            string projectId = cmd.ExecuteScalar().ToString();
                            //Check if the selected project is deleted:
                            cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
                            int project_isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                            if (project_isDeleted == 0)//If the project is not deleted, fetch its sprint tasks: (0 = false)
                            {
                                cmd.CommandText = "select count(*) from UserStories where projectId = '" + projectId + "' ";
                                int totalUserStoriesForProject = Convert.ToInt32(cmd.ExecuteScalar());
                                for (int i = 1; i <= totalUserStoriesForProject; i++)
                                {
                                    cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where projectId = '" + projectId + "') " +
                                        "as t where rowNum = '" + i + "'";
                                    string userStoryId = cmd.ExecuteScalar().ToString();
                                    cmd.CommandText = "select count(*) from SprintTasks where userStoryId = '" + userStoryId + "' and (sprintTask_uniqueId like '" + word + "' or sprintTask_taskDescription like '%" + word + "%' or sprintTask_currentStatus like '%" + word + "%') ";
                                    int countMatchingWord = Convert.ToInt32(cmd.ExecuteScalar());
                                    for (int k = 1; k <= countMatchingWord; k++)
                                    {
                                        cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] where userStoryId = '" + userStoryId + "' and  " +
                                            "  (sprintTask_uniqueId like '" + word + "' or sprintTask_taskDescription like '%" + word + "%' or sprintTask_currentStatus like '%" + word + "%')  ) as t where rowNum = '" + k + "'";
                                        string tempId = cmd.ExecuteScalar().ToString();
                                        set_results.Add(tempId);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int totalCount = set_results.Count;
            for (int i = 0; i < totalCount; i++)
            {
                //Get the sprint task ID:
                id = set_results.ElementAt(i);
                cmd.CommandText = "select userStoryId from SprintTasks where sprintTaskId = '" + id + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
                userStoryUniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + id + "' ";
                sprintTask_uniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_taskDescription from SprintTasks where sprintTaskId = '" + id + "' ";
                taskDescription = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_dateIntroduced from SprintTasks where sprintTaskId = '" + id + "' ";
                dateIntroduced = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_dateConsideredForImplementation from SprintTasks where sprintTaskId = '" + id + "' ";
                dateConsidered = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select count(sprintTask_dateCompleted) from SprintTasks where sprintTaskId = '" + id + "' ";
                int thereExists_dateComepleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (thereExists_dateComepleted > 0)
                {
                    cmd.CommandText = "select sprintTask_dateCompleted from SprintTasks where sprintTaskId = '" + id + "' ";
                    dateCompleted = cmd.ExecuteScalar().ToString();
                }
                //Loop through the developers for the selected Sprint Task:
                cmd.CommandText = "select count(*) from UsersForSprintTasks where sprintTaskId = '" + id + "' ";
                int usersForSprintTask = Convert.ToInt32(cmd.ExecuteScalar());
                for (int j = 1; j <= usersForSprintTask; j++)
                {
                    cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForSprintTasksId ASC), * FROM [UsersForSprintTasks] where sprintTaskId = '" + id + "' ) as t where rowNum = '" + j + "'";
                    string developerId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + developerId + "' ";
                    if (j == 1)
                        developersResponsible = cmd.ExecuteScalar().ToString();
                    else
                        developersResponsible = developersResponsible + ", " + cmd.ExecuteScalar().ToString();
                }
                cmd.CommandText = "select sprintTask_currentStatus from SprintTasks where sprintTaskId = '" + id + "' ";
                currentStatus = cmd.ExecuteScalar().ToString();
                //Creator's info, which will be hidden:
                cmd.CommandText = "select sprintTask_createdBy from SprintTasks where sprintTaskId = '" + id + "' ";
                creatorId = cmd.ExecuteScalar().ToString();
                dt.Rows.Add(sprintTask_uniqueId, userStoryUniqueId, taskDescription, Layouts.getTimeFormat(dateIntroduced), Layouts.getTimeFormat(dateConsidered), Layouts.getTimeFormat(dateCompleted), developersResponsible, currentStatus, creatorId, id, removeSprintTaskCommand);
                //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            //Sort by "Sprint task ID" column in "ASC" ascending order:
            if (dt != null)
            {
                DataView dv = new DataView(dt);
                dv.Sort = "Sprint task ID" + " " + "ASC";
                grdResults.DataSource = dv;
                grdResults.DataBind();
            }
            rebindValuesForSprintTasks();
        }
        protected void rebindValuesForSprintTasks()
        {
            int int_roleId = Convert.ToInt32(roleId);
            if (grdResults.Rows.Count > 0)
            {
                //Hide the headers called "User ID" and "Sprint Task ID":
                grdResults.HeaderRow.Cells[8].Visible = false;
                grdResults.HeaderRow.Cells[9].Visible = false;
                //Hide IDs column and content which are located in column index 8:
                for (int i = 0; i < grdResults.Rows.Count; i++)
                {
                    grdResults.Rows[i].Cells[8].Visible = false;
                    grdResults.Rows[i].Cells[9].Visible = false;
                }
            }
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            string sprintTaskUniqueId = "", userStoryUniqueId = "", taskDescription = "", dateIntroduced = "", dateConsidered = "", dateCompleted = "",
                   developersResponsible = "", currentStatus = "";
            string creatorId = "", db_sprintTaskId = "";
            for (int row = 0; row < grdResults.Rows.Count; row++)
            {
                //Set links to review a user:
                sprintTaskUniqueId = grdResults.Rows[row].Cells[0].Text;
                userStoryUniqueId = grdResults.Rows[row].Cells[1].Text;
                taskDescription = grdResults.Rows[row].Cells[2].Text;
                dateIntroduced = grdResults.Rows[row].Cells[3].Text;
                dateConsidered = grdResults.Rows[row].Cells[4].Text;
                dateCompleted = grdResults.Rows[row].Cells[5].Text;
                developersResponsible = grdResults.Rows[row].Cells[6].Text;
                currentStatus = grdResults.Rows[row].Cells[7].Text;
                creatorId = grdResults.Rows[row].Cells[8].Text;
                db_sprintTaskId = grdResults.Rows[row].Cells[9].Text;
                string removeSprintTaskCommand = grdResults.Rows[row].Cells[10].Text;
                //Loop through the developers for the selected Sprint Tasks:
                cmd.CommandText = "select count(*) from UsersForSprintTasks where sprintTaskId = '" + db_sprintTaskId + "' ";
                int usersForSprintTask = Convert.ToInt32(cmd.ExecuteScalar());
                HyperLink developerResponsibleLink = new HyperLink();
                for (int j = 1; j <= usersForSprintTask; j++)
                {
                    HyperLink participantLink = new HyperLink();
                    cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForSprintTasksId ASC), * FROM [UsersForSprintTasks] where sprintTaskId = '" + db_sprintTaskId + "' ) as t where rowNum = '" + j + "'";
                    string developerId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + developerId + "' ";
                    string developer_name = cmd.ExecuteScalar().ToString();
                    participantLink.Text = developer_name + " ";
                    participantLink.NavigateUrl = "Profile.aspx?id=" + developerId;
                    grdResults.Rows[row].Cells[6].Controls.Add(participantLink);
                    if (usersForSprintTask > 1)
                    {
                        HyperLink temp = new HyperLink();
                        temp.Text = "<br/>";
                        grdResults.Rows[row].Cells[6].Controls.Add(temp);
                    }
                }
                //Get the Sprint Task ID:
                string id = db_sprintTaskId;
                cmd.CommandText = "select userStoryId from SprintTasks where sprintTaskId = '" + id + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select projectId from UserStories where userStoryId = '" + userStoryId + "' ";
                string projectId = cmd.ExecuteScalar().ToString();
                string sprintTaskUrl = "ViewSprintTask.aspx?id=" + id + "&userStoryId=" + userStoryId + "&projectId=" + projectId;
                HyperLink sprintTaskUIdLink = new HyperLink();
                HyperLink userStoryUIdLink = new HyperLink();
                HyperLink taskDescriptionLink = new HyperLink();
                HyperLink dateIntroducedLink = new HyperLink();
                HyperLink dateConsideredLink = new HyperLink();
                HyperLink dateCompletedLink = new HyperLink();
                HyperLink currentStatusLink = new HyperLink();
                sprintTaskUIdLink.Text = sprintTaskUniqueId + " ";
                userStoryUIdLink.Text = userStoryUniqueId + " ";
                taskDescriptionLink.Text = taskDescription + " ";
                dateIntroducedLink.Text = Layouts.getTimeFormat(dateIntroduced) + " ";
                dateConsideredLink.Text = Layouts.getTimeFormat(dateConsidered) + " ";
                dateCompletedLink.Text = (string.IsNullOrEmpty(Layouts.getTimeFormat(dateCompleted))) ? "Not completed" : Layouts.getTimeFormat(dateCompleted);
                currentStatusLink.Text = currentStatus + " ";
                sprintTaskUIdLink.NavigateUrl = sprintTaskUrl;
                userStoryUIdLink.NavigateUrl = sprintTaskUrl;
                taskDescriptionLink.NavigateUrl = sprintTaskUrl;
                dateIntroducedLink.NavigateUrl = sprintTaskUrl;
                dateConsideredLink.NavigateUrl = sprintTaskUrl;
                dateCompletedLink.NavigateUrl = sprintTaskUrl;
                currentStatusLink.NavigateUrl = sprintTaskUrl;
                //Sprint Task remove button:
                LinkButton removeSprintTaskLink = new LinkButton();
                removeSprintTaskLink.Text = removeSprintTaskCommand + " ";
                removeSprintTaskLink.Command += new CommandEventHandler(RemoveSprintTaskLink_Click);
                removeSprintTaskLink.CommandName = id;
                removeSprintTaskLink.CommandArgument = Convert.ToString(row + 1);
                removeSprintTaskLink.Enabled = true;
                removeSprintTaskLink.CssClass = "removeUserStoryButton";
                //Check if the sprint task has been deleted already, if so, disable the button:
                cmd.CommandText = "select sprintTask_isDeleted from SprintTasks where sprintTaskId = '" + id + "' ";
                int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (isDeleted == 1)
                    removeSprintTaskLink.Enabled = false;
                int int_creatorId = Convert.ToInt32(creatorId);
                cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                int int_userId = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select count(sprintTask_editedBy) from SprintTasks where sprintTaskId = '" + id + "' ";
                int isEditedBySomeone = Convert.ToInt32(cmd.ExecuteScalar());
                if (isEditedBySomeone > 0)
                {
                    cmd.CommandText = "select sprintTask_editedBy from SprintTasks where sprintTaskId = '" + id + "' ";
                    int int_editorId = Convert.ToInt32(cmd.ExecuteScalar());
                    if (int_userId != int_creatorId && int_roleId != 2 && int_roleId != 1 && int_userId != int_editorId)
                        removeSprintTaskLink.Enabled = false;
                }
                else
                {
                    if (int_userId != int_creatorId && int_roleId != 2 && int_roleId != 1)
                        removeSprintTaskLink.Enabled = false;
                }
                if (!removeSprintTaskLink.Enabled)
                {
                    removeSprintTaskLink.CssClass = "disabledRemoveUserStoryButton";
                }
                grdResults.Rows[row].Cells[0].Controls.Add(sprintTaskUIdLink);
                grdResults.Rows[row].Cells[1].Controls.Add(userStoryUIdLink);
                grdResults.Rows[row].Cells[2].Controls.Add(taskDescriptionLink);
                grdResults.Rows[row].Cells[3].Controls.Add(dateIntroducedLink);
                grdResults.Rows[row].Cells[4].Controls.Add(dateConsideredLink);
                grdResults.Rows[row].Cells[5].Controls.Add(dateCompletedLink);
                grdResults.Rows[row].Cells[7].Controls.Add(currentStatusLink);
                grdResults.Rows[row].Cells[10].Controls.Add(removeSprintTaskLink);
            }
            connect.Close();
        }
        protected void RemoveSprintTaskLink_Click(object sender, CommandEventArgs e)
        {
            string sprintTaskId = e.CommandName;
            string sprintTaskUniqueId = e.CommandArgument.ToString();
            lblRemoveSprintTaskMessage.Text = "Are you sure you want to archive the sprint task# " + sprintTaskUniqueId + "?";
            lblSprintTaskId.Text = sprintTaskId;
            divRemoveSprintTask.Visible = true;
        }
        protected void btnConfirmRemoveSprintTask_Click(object sender, EventArgs e)
        {
            string sprintTaskIdToRemove = lblSprintTaskId.Text;
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //delete the selected sprint task:
            cmd.CommandText = "update SprintTasks set sprintTask_isDeleted = 1 where sprintTaskId = '" + sprintTaskIdToRemove + "'  ";
            cmd.ExecuteScalar();
            //Update all test cases related to the deleted sprint task:
            cmd.CommandText = "update TestCases set testCase_isDeleted = 1 where sprintTaskId = '" + sprintTaskIdToRemove + "'  ";
            cmd.ExecuteScalar();
            connect.Close();
            divRemoveSprintTask.Visible = false;
            Page.Response.Redirect(Page.Request.Url.ToString(), true);
        }
        protected void btnCancelRemoveSprintTask_Click(object sender, EventArgs e)
        {
            divRemoveSprintTask.Visible = false;
        }
        protected void createTestCasesTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Test case ID", typeof(double));
            dt.Columns.Add("User story ID", typeof(string));
            dt.Columns.Add("Sprint task ID", typeof(string));
            dt.Columns.Add("Test case scenario", typeof(string));
            dt.Columns.Add("Input parameters", typeof(string));
            dt.Columns.Add("Expected output", typeof(string));
            dt.Columns.Add("Current status", typeof(string));
            dt.Columns.Add("Creator User ID", typeof(string));
            dt.Columns.Add("DB Test Case ID", typeof(string));
            dt.Columns.Add("Archive Test Case", typeof(string));
            string testCase_uniqueId = "", userStoryUniqueId = "", sprintTask_uniqueId = "", testCaseScenario = "", inputParameters = "", expectedOutput = "", currentStatus = "";
            string testCase_creatorId = "", testCaseId = "", removeTestCaseCommand = " Archive ";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string searchString = txtSearch.Text.Replace("'", "''");
            string[] words = searchString.Split(' ');
            SortedSet<string> set_results = new SortedSet<string>();
            int int_roleId = Convert.ToInt32(roleId);
            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    if (int_roleId == 1)//If an admin is searching, get everything:
                    {
                        cmd.CommandText = "select count(*) from TestCases where (testCase_uniqueId like '" + word + "' or testCase_Scenario like '%" + word + "%' or testCase_expectedOutput like '%" + word + "%' or testCase_currentStatus like '%" + word + "%') ";
                        int countMatchingWord = Convert.ToInt32(cmd.ExecuteScalar());
                        for (int i = 1; i <= countMatchingWord; i++)
                        {
                            cmd.CommandText = "select [testCaseId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY testCaseId ASC), * FROM [TestCases] where " +
                                " (testCase_uniqueId like '" + word + "' or testCase_Scenario like '%" + word + "%' or testCase_expectedOutput like '%" + word + "%' or testCase_currentStatus like '%" + word + "%')" +
                                " ) as t where rowNum = '" + i + "'";
                            string tempId = cmd.ExecuteScalar().ToString();
                            set_results.Add(tempId);
                        }
                    }
                    else//If a master or a developer is searching, get the related results:
                    {
                        cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                        string userId = cmd.ExecuteScalar().ToString();
                        cmd.CommandText = "select count(*) from UsersForProjects where userId = '" + userId + "' ";
                        int totalProjectsForUser = Convert.ToInt32(cmd.ExecuteScalar());
                        for (int j = 1; j <= totalProjectsForUser; j++)
                        {
                            cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForProjectsId ASC), * FROM [UsersForProjects] where userId = '" + userId + "') as t where rowNum = '" + j + "'";
                            string projectId = cmd.ExecuteScalar().ToString();
                            //Check if the selected project is deleted:
                            cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
                            int project_isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                            if (project_isDeleted == 0)//If the project is not deleted, fetch its sprint tasks: (0 = false)
                            {
                                cmd.CommandText = "select count(*) from UserStories where projectId = '" + projectId + "' ";
                                int totalUserStoriesForProject = Convert.ToInt32(cmd.ExecuteScalar());
                                for (int i = 1; i <= totalUserStoriesForProject; i++)
                                {
                                    cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where projectId = '" + projectId + "') " +
                                        "as t where rowNum = '" + i + "'";
                                    string userStoryId = cmd.ExecuteScalar().ToString();
                                    cmd.CommandText = "select count(*) from SprintTasks where userStoryId = '" + userStoryId + "' ";
                                    int totalSprintTasks = Convert.ToInt32(cmd.ExecuteScalar());
                                    for (int k = 1; k <= totalSprintTasks; k++)
                                    {
                                        cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] where userStoryId = '" + userStoryId + "'  " +
                                            " ) as t where rowNum = '" + k + "'";
                                        string sprintTaskId = cmd.ExecuteScalar().ToString();
                                        cmd.CommandText = "select count(*) from TestCases where sprintTaskId = '" + sprintTaskId + "' and (testCase_uniqueId like '" + word + "' or testCase_testCaseScenario like '%" + word + "%' or testCase_expectedOutput like '%" + word + "%' or testCase_currentStatus like '%" + word + "%') ";
                                        int totalTestCases = Convert.ToInt32(cmd.ExecuteScalar());
                                        for (int m = 1; m <= totalTestCases; m++)
                                        {
                                            cmd.CommandText = "select [testCaseId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY testCaseId ASC), * FROM [TestCases] where sprintTaskId = '" + sprintTaskId + "' and " +
                                                " (testCase_uniqueId like '" + word + "' or testCase_testCaseScenario like '%" + word + "%' or testCase_expectedOutput like '%" + word + "%' or testCase_currentStatus like '%" + word + "%')" +
                                                " ) as t where rowNum = '" + m + "'";
                                            string tempId = cmd.ExecuteScalar().ToString();
                                            set_results.Add(tempId);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            int totalCount = set_results.Count;
            for (int i = 0; i < totalCount; i++)
            {
                //Get the test case ID:
                testCaseId = set_results.ElementAt(i);
                cmd.CommandText = "select sprintTaskId from TestCases where testCaseId = '" + testCaseId + "' ";
                string sprintTaskId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStoryId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
                userStoryUniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                sprintTask_uniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_testCaseScenario from TestCases where testCaseId = '" + testCaseId + "' ";
                testCaseScenario = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_expectedOutput from TestCases where testCaseId = '" + testCaseId + "' ";
                expectedOutput = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_currentStatus from TestCases where testCaseId = '" + testCaseId + "' ";
                currentStatus = cmd.ExecuteScalar().ToString();
                //Creator's info, which will be hidden:
                cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + testCaseId + "' ";
                testCase_creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_uniqueId from TestCases where testCaseId = '" + testCaseId + "' ";
                testCase_uniqueId = cmd.ExecuteScalar().ToString();
                //Loop through the developers for the selected Test Case:
                cmd.CommandText = "select count(*) from Parameters where testcaseId = '" + testCaseId + "' ";
                int parametersForTestCase = Convert.ToInt32(cmd.ExecuteScalar());
                for (int j = 1; j <= parametersForTestCase; j++)
                {
                    cmd.CommandText = "select parameter_name from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY parameterId ASC), * FROM Parameters where testCaseId = '" + testCaseId + "' ) as t where rowNum = '" + j + "'";
                    string parameter_name = cmd.ExecuteScalar().ToString();
                    if (j == 1)
                        inputParameters = parameter_name;
                    else
                        inputParameters = inputParameters + ", " + parameter_name;
                }
                dt.Rows.Add(testCase_uniqueId, userStoryUniqueId, sprintTask_uniqueId, testCaseScenario, inputParameters, expectedOutput, currentStatus, testCase_creatorId, testCaseId, removeTestCaseCommand);
                //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            //Sort by "Test case ID" column in "ASC" ascending order:
            if (dt != null)
            {
                DataView dv = new DataView(dt);
                dv.Sort = "Test case ID" + " " + "ASC";
                grdResults.DataSource = dv;
                grdResults.DataBind();
            }
            rebindValuesForTestCases();
        }
        protected void rebindValuesForTestCases()
        {
            int int_roleId = Convert.ToInt32(roleId);
            if (grdResults.Rows.Count > 0)
            {
                //Hide the headers called "User ID" and "Test Case ID":
                grdResults.HeaderRow.Cells[7].Visible = false;
                grdResults.HeaderRow.Cells[8].Visible = false;
                //Hide IDs column and content which are located in column index 8:
                for (int i = 0; i < grdResults.Rows.Count; i++)
                {
                    grdResults.Rows[i].Cells[7].Visible = false;
                    grdResults.Rows[i].Cells[8].Visible = false;
                }
            }
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            string testCaseUniqueId = "", sprintTaskUniqueId = "", userStoryUniqueId = "", testCaseScenario = "", inputParameters = "", expectedOutput = "", currentStatus = "";
            string creatorId = "", db_testCaseId = "";
            for (int row = 0; row < grdResults.Rows.Count; row++)
            {
                //Set links to review a user:
                testCaseUniqueId = grdResults.Rows[row].Cells[0].Text;
                userStoryUniqueId = grdResults.Rows[row].Cells[1].Text;
                sprintTaskUniqueId = grdResults.Rows[row].Cells[2].Text;
                testCaseScenario = grdResults.Rows[row].Cells[3].Text;
                inputParameters = grdResults.Rows[row].Cells[4].Text;
                expectedOutput = grdResults.Rows[row].Cells[5].Text;
                currentStatus = grdResults.Rows[row].Cells[6].Text;
                creatorId = grdResults.Rows[row].Cells[7].Text;
                db_testCaseId = grdResults.Rows[row].Cells[8].Text;
                string removeSprintTaskCommand = grdResults.Rows[row].Cells[9].Text;
                //Loop through the parameters for the selected Test Case:
                cmd.CommandText = "select count(*) from Parameters where testcaseId = '" + db_testCaseId + "' ";
                int totalParameters = Convert.ToInt32(cmd.ExecuteScalar());
                string parameters = "";
                for (int j = 1; j <= totalParameters; j++)
                {
                    cmd.CommandText = "select parameter_name from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY parameterId ASC), * FROM Parameters where testCaseId = '" + db_testCaseId + "' ) as t where rowNum = '" + j + "'";
                    string parameter = cmd.ExecuteScalar().ToString();
                    parameters = (j == 1) ? parameter : parameters + ", " + parameter;
                }
                //Get the test case ID:
                string id = db_testCaseId;
                cmd.CommandText = "select sprintTaskId from TestCases where testCaseId = '" + id + "' ";
                string sprintTaskId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStoryId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select projectId from UserStories where userStoryId = '" + userStoryId + "' ";
                string projectId = cmd.ExecuteScalar().ToString();
                string testCaseUrl = "ViewTestCase.aspx?id=" + id + "&sprintTaskId=" + sprintTaskId + "&userStoryId=" + userStoryId + "&projectId=" + projectId; ;
                HyperLink testCaseUIdLink = new HyperLink();
                HyperLink userStoryUIdLink = new HyperLink();
                HyperLink sprintTaskUIdLink = new HyperLink();
                HyperLink testCaseScenarioLink = new HyperLink();
                HyperLink parametersLink = new HyperLink();
                HyperLink expectedOutputLink = new HyperLink();
                HyperLink currentStatusLink = new HyperLink();

                testCaseUIdLink.Text = testCaseUniqueId + " ";
                userStoryUIdLink.Text = userStoryUniqueId + " ";
                sprintTaskUIdLink.Text = sprintTaskUniqueId + " ";
                testCaseScenarioLink.Text = testCaseScenario + " ";
                parametersLink.Text = parameters + " ";
                expectedOutputLink.Text = expectedOutput + " ";
                currentStatusLink.Text = currentStatus + " ";

                testCaseUIdLink.NavigateUrl = testCaseUrl;
                userStoryUIdLink.NavigateUrl = "ViewUserStory.aspx?id=" + userStoryId;
                sprintTaskUIdLink.NavigateUrl = testCaseUrl;
                testCaseScenarioLink.NavigateUrl = testCaseUrl;
                parametersLink.NavigateUrl = testCaseUrl;
                expectedOutputLink.NavigateUrl = testCaseUrl;
                currentStatusLink.NavigateUrl = testCaseUrl;
                //Test Case remove button:
                LinkButton removeTestCaseLink = new LinkButton();
                removeTestCaseLink.Text = removeSprintTaskCommand + " ";
                removeTestCaseLink.Command += new CommandEventHandler(RemoveTestCaseLink_Click);
                removeTestCaseLink.CommandName = id;
                removeTestCaseLink.CommandArgument = Convert.ToString(row + 1);
                removeTestCaseLink.Enabled = true;
                removeTestCaseLink.CssClass = "removeUserStoryButton";
                //Check if the test case has been deleted already, if so, disable the button:
                cmd.CommandText = "select testCase_isDeleted from TestCases where testCaseId = '" + id + "' ";
                int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (isDeleted == 1)
                    removeTestCaseLink.Enabled = false;
                int int_creatorId = Convert.ToInt32(creatorId);
                cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                int int_userId = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select count(testCase_editedBy) from TestCases where testCaseId = '" + id + "' ";
                int isEditedBySomeone = Convert.ToInt32(cmd.ExecuteScalar());
                if (isEditedBySomeone > 0)
                {
                    cmd.CommandText = "select testCase_editedBy from TestCases where testCaseId = '" + id + "' ";
                    int int_editorId = Convert.ToInt32(cmd.ExecuteScalar());
                    if (int_userId != int_creatorId && int_roleId != 2 && int_roleId != 1 && int_userId != int_editorId)
                        removeTestCaseLink.Enabled = false;
                }
                else
                {
                    if (int_userId != int_creatorId && int_roleId != 2 && int_roleId != 1)
                        removeTestCaseLink.Enabled = false;
                }
                if (!removeTestCaseLink.Enabled)
                {
                    removeTestCaseLink.CssClass = "disabledRemoveUserStoryButton";
                }
                grdResults.Rows[row].Cells[0].Controls.Add(testCaseUIdLink);
                grdResults.Rows[row].Cells[1].Controls.Add(userStoryUIdLink);
                grdResults.Rows[row].Cells[2].Controls.Add(sprintTaskUIdLink);
                grdResults.Rows[row].Cells[3].Controls.Add(testCaseScenarioLink);
                grdResults.Rows[row].Cells[4].Controls.Add(parametersLink);
                grdResults.Rows[row].Cells[5].Controls.Add(expectedOutputLink);
                grdResults.Rows[row].Cells[6].Controls.Add(currentStatusLink);
                grdResults.Rows[row].Cells[9].Controls.Add(removeTestCaseLink);
            }
            connect.Close();
        }
        protected void RemoveTestCaseLink_Click(object sender, CommandEventArgs e)
        {
            string testCaseId = e.CommandName;
            string testCaseUniqueId = e.CommandArgument.ToString();
            lblRemoveTestCaseMessage.Text = "Are you sure you want to archive the test case# " + testCaseUniqueId + "?";
            lblTestCaseId.Text = testCaseId;
            divRemoveTestCase.Visible = true;
        }
        protected void btnConfirmRemoveTestCase_Click(object sender, EventArgs e)
        {
            string testCaseIdToRemove = lblTestCaseId.Text;
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //delete the selected test case:
            cmd.CommandText = "update TestCases set testCase_isDeleted = 1 where testCaseId = '" + testCaseIdToRemove + "'  ";
            cmd.ExecuteScalar();
            connect.Close();
            divRemoveTestCase.Visible = false;
            Page.Response.Redirect(Page.Request.Url.ToString(), true);//Refresh the page.
        }
        protected void btnCancelRemoveTestCase_Click(object sender, EventArgs e)
        {
            divRemoveTestCase.Visible = false;
        }
        protected void createProjectsTimeTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Project Name", typeof(string));
            dt.Columns.Add("Created by", typeof(string));
            dt.Columns.Add("Created time", typeof(string));
            dt.Columns.Add("Creator ID", typeof(string));
            SortedSet<string> set_results = new SortedSet<string>();
            string id = "", project_name = "", createdBy = "", createdOn = "", creatorId = "";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            DateTime start_time = calFrom.SelectedDate, end_time = calTo.SelectedDate;
            int countMatchingResults = 0;
            if (!start_time.ToString().Equals("1/1/0001 12:00:00 AM") && !end_time.ToString().Equals("1/1/0001 12:00:00 AM"))
            {
                cmd.CommandText = "select count(*) from Projects where project_isDeleted = 0 and project_createdDate >= '" + start_time + "' and project_createdDate <= '" + end_time + "' ";
                countMatchingResults = Convert.ToInt32(cmd.ExecuteScalar());
            }
            for (int i = 1; i <= countMatchingResults; i++)
            {
                cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY projectId ASC), * FROM [Projects] where project_isDeleted = 0 and project_createdDate >= '" + start_time + "' and project_createdDate <= '" + end_time + "' ) as t where rowNum = '" + i + "'";
                string tempId = cmd.ExecuteScalar().ToString();
                set_results.Add(tempId);
            }
            int totalCount = set_results.Count;
            for (int i = 0; i < totalCount; i++)
            {
                //Get the project ID:
                id = set_results.ElementAt(i);
                //Get project name:
                cmd.CommandText = "select project_name from Projects where projectId = '" + id + "' ";
                project_name = cmd.ExecuteScalar().ToString();
                //Get the project's creator User ID:
                cmd.CommandText = "select project_createdBy from Projects where projectId = '" + id + "' ";
                creatorId = cmd.ExecuteScalar().ToString();
                //Convert the User ID to a name:
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + creatorId + "' ";
                createdBy = cmd.ExecuteScalar().ToString();
                //Get project creation date:
                cmd.CommandText = "select project_createdDate from Projects where projectId = '" + id + "' ";
                createdOn = cmd.ExecuteScalar().ToString();
                dt.Rows.Add(project_name, createdBy, Layouts.getTimeFormat(createdOn), creatorId);
                //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            rebindValuesForProjects();
        }
        protected void createUserStoriesTimeTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("User story ID", typeof(double));
            dt.Columns.Add("As a (type of user)", typeof(string));
            dt.Columns.Add("I want to (some goal)", typeof(string));
            dt.Columns.Add("So that (reason)", typeof(string));
            dt.Columns.Add("Date introduced", typeof(string));
            dt.Columns.Add("Date considered for implementation", typeof(string));
            dt.Columns.Add("Developers responsible for", typeof(string));
            dt.Columns.Add("Current status", typeof(string));
            dt.Columns.Add("Creator User ID", typeof(string));
            dt.Columns.Add("DB User Story ID", typeof(string));
            dt.Columns.Add("Archive User Story", typeof(string));
            string id = "", userStoryId = "", asARole = "", iWant = "", soThat = "", dateIntroduced = "", dateConsidered = "",
                developersResponsible = "", currentStatus = "";
            string creatorId = "", removeUserStoryCommand = " Archive ";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            SortedSet<string> set_results = new SortedSet<string>();
            DateTime start_time = calFrom.SelectedDate, end_time = calTo.SelectedDate;
            int countMatchingResults = 0;
            if (!start_time.ToString().Equals("1/1/0001 12:00:00 AM") && !end_time.ToString().Equals("1/1/0001 12:00:00 AM"))
            {
                int int_roleId = Convert.ToInt32(roleId);
                if (int_roleId == 1)//If an admin is searching, get everything:
                {
                    cmd.CommandText = "select count(*) from UserStories where userStory_createdDate >= '" + start_time + "' and userStory_createdDate <= '" + end_time + "' ";
                    countMatchingResults = Convert.ToInt32(cmd.ExecuteScalar());
                    for (int i = 1; i <= countMatchingResults; i++)
                    {
                        cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where  " +
                            " userStory_createdDate >= '" + start_time + "' and userStory_createdDate <= '" + end_time + "' ) as t where rowNum = '" + i + "'";
                        string tempId = cmd.ExecuteScalar().ToString();
                        set_results.Add(tempId);
                    }
                }
                else//If a master or a developer is searching, get the related results:
                {
                    cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                    string userId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select count(*) from UsersForProjects where userId = '" + userId + "' ";
                    int totalProjectsForUser = Convert.ToInt32(cmd.ExecuteScalar());
                    for (int j = 1; j <= totalProjectsForUser; j++)
                    {
                        cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForProjectsId ASC), * FROM [UsersForProjects] where userId = '" + userId + "') as t where rowNum = '" + j + "'";
                        string projectId = cmd.ExecuteScalar().ToString();
                        //Check if the selected project is deleted:
                        cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
                        int project_isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                        if (project_isDeleted == 0)//If the project is not deleted, fetch its user stories: (0 = false)
                        {
                            cmd.CommandText = "select count(*) from UserStories where userStory_createdDate >= '" + start_time + "' and userStory_createdDate <= '" + end_time + "' ";
                            countMatchingResults = Convert.ToInt32(cmd.ExecuteScalar());
                            for (int i = 1; i <= countMatchingResults; i++)
                            {
                                cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where " +
                                    " userStory_createdDate >= '" + start_time + "' and userStory_createdDate <= '" + end_time + "' ) as t where rowNum = '" + i + "'";
                                string tempId = cmd.ExecuteScalar().ToString();
                                set_results.Add(tempId);
                            }
                        }
                    }
                }
            }


            int totalCount = set_results.Count;
            for (int i = 0; i < totalCount; i++)
            {
                //Get the user story ID:
                id = set_results.ElementAt(i);
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + id + "' ";
                userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_asARole from UserStories where userStoryId = '" + id + "' ";
                asARole = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_iWantTo from UserStories where userStoryId = '" + id + "' ";
                iWant = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_soThat from UserStories where userStoryId = '" + id + "' ";
                soThat = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_dateIntroduced from UserStories where userStoryId = '" + id + "' ";
                dateIntroduced = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_dateConsideredForImplementation from UserStories where userStoryId = '" + id + "' ";
                dateConsidered = cmd.ExecuteScalar().ToString();
                //Loop through the developers for the selected User Story:
                cmd.CommandText = "select count(*) from UsersForUserStories where userStoryId = '" + id + "' ";
                int usersForUserStory = Convert.ToInt32(cmd.ExecuteScalar());
                for (int j = 1; j <= usersForUserStory; j++)
                {
                    cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForUserStoriesId ASC), * FROM [UsersForUserStories] where userStoryId = '" + id + "' ) as t where rowNum = '" + j + "'";
                    string developerId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + developerId + "' ";
                    if (j == 1)
                        developersResponsible = cmd.ExecuteScalar().ToString();
                    else
                        developersResponsible = developersResponsible + ", " + cmd.ExecuteScalar().ToString();
                }
                cmd.CommandText = "select userStory_currentStatus from UserStories where userStoryId = '" + id + "' ";
                currentStatus = cmd.ExecuteScalar().ToString();
                //Creator's info, which will be hidden:
                cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + id + "' ";
                creatorId = cmd.ExecuteScalar().ToString();
                dt.Rows.Add(userStoryId, asARole, iWant, soThat, Layouts.getTimeFormat(dateIntroduced), Layouts.getTimeFormat(dateConsidered), developersResponsible, currentStatus, creatorId, id, removeUserStoryCommand);
                //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            //Sort by "User story ID" column in "ASC" ascending order:
            if (dt != null)
            {
                DataView dv = new DataView(dt);
                dv.Sort = "User story ID" + " " + "ASC";
                grdResults.DataSource = dv;
                grdResults.DataBind();
            }
            rebindValuesForUserStories();
        }
        protected void createSprintTasksTimeTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Sprint task ID", typeof(double));
            dt.Columns.Add("User story ID", typeof(string));
            dt.Columns.Add("Task description", typeof(string));
            dt.Columns.Add("Date introduced", typeof(string));
            dt.Columns.Add("Date considered for implementation", typeof(string));
            dt.Columns.Add("Date completed", typeof(string));
            dt.Columns.Add("Developers responsible for", typeof(string));
            dt.Columns.Add("Current status", typeof(string));
            dt.Columns.Add("Creator User ID", typeof(string));
            dt.Columns.Add("DB Sprint Task ID", typeof(string));
            dt.Columns.Add("Archive Sprint Task", typeof(string));
            string id = "", sprintTask_uniqueId = "", userStoryUniqueId = "", taskDescription = "", dateIntroduced = "", dateConsidered = "", dateCompleted = "",
                developersResponsible = "", currentStatus = "";
            string creatorId = "", removeSprintTaskCommand = " Archive ";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            SortedSet<string> set_results = new SortedSet<string>();
            int int_roleId = Convert.ToInt32(roleId);
            DateTime start_time = calFrom.SelectedDate, end_time = calTo.SelectedDate;
            int countMatchingResults = 0;
            if (!start_time.ToString().Equals("1/1/0001 12:00:00 AM") && !end_time.ToString().Equals("1/1/0001 12:00:00 AM"))
            {
                if (int_roleId == 1)//If an admin is searching, get everything:
                {
                    cmd.CommandText = "select count(*) from SprintTasks where sprintTask_createdDate >= '" + start_time + "' and sprintTask_createdDate <= '" + end_time + "' ";
                    countMatchingResults = Convert.ToInt32(cmd.ExecuteScalar());
                    for (int i = 1; i <= countMatchingResults; i++)
                    {
                        cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] where " +
                            " sprintTask_createdDate >= '" + start_time + "' and sprintTask_createdDate <= '" + end_time + "' ) as t where rowNum = '" + i + "'";
                        string tempId = cmd.ExecuteScalar().ToString();
                        set_results.Add(tempId);
                    }
                }
                else//If a master or a developer is searching, get the related results:
                {
                    cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                    string userId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select count(*) from UsersForProjects where userId = '" + userId + "' ";
                    int totalProjectsForUser = Convert.ToInt32(cmd.ExecuteScalar());
                    for (int j = 1; j <= totalProjectsForUser; j++)
                    {
                        cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForProjectsId ASC), * FROM [UsersForProjects] where userId = '" + userId + "') as t where rowNum = '" + j + "'";
                        string projectId = cmd.ExecuteScalar().ToString();
                        //Check if the selected project is deleted:
                        cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
                        int project_isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                        if (project_isDeleted == 0)//If the project is not deleted, fetch its sprint tasks: (0 = false)
                        {
                            cmd.CommandText = "select count(*) from UserStories where projectId = '" + projectId + "' ";
                            int totalUserStoriesForProject = Convert.ToInt32(cmd.ExecuteScalar());
                            for (int i = 1; i <= totalUserStoriesForProject; i++)
                            {
                                cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where projectId = '" + projectId + "') " +
                                    "as t where rowNum = '" + i + "'";
                                string userStoryId = cmd.ExecuteScalar().ToString();
                                cmd.CommandText = "select count(*) from SprintTasks where sprintTask_createdDate >= '" + start_time + "' and sprintTask_createdDate <= '" + end_time + "' ";
                                countMatchingResults = Convert.ToInt32(cmd.ExecuteScalar());
                                for (int k = 1; k <= countMatchingResults; k++)
                                {
                                    cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] where " +
                                        " sprintTask_createdDate >= '" + start_time + "' and sprintTask_createdDate <= '" + end_time + "' ) as t where rowNum = '" + k + "'";
                                    string tempId = cmd.ExecuteScalar().ToString();
                                    set_results.Add(tempId);
                                }
                            }
                        }
                    }
                }
            }

            int totalCount = set_results.Count;
            for (int i = 0; i < totalCount; i++)
            {
                //Get the sprint task ID:
                id = set_results.ElementAt(i);
                cmd.CommandText = "select userStoryId from SprintTasks where sprintTaskId = '" + id + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
                userStoryUniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + id + "' ";
                sprintTask_uniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_taskDescription from SprintTasks where sprintTaskId = '" + id + "' ";
                taskDescription = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_dateIntroduced from SprintTasks where sprintTaskId = '" + id + "' ";
                dateIntroduced = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_dateConsideredForImplementation from SprintTasks where sprintTaskId = '" + id + "' ";
                dateConsidered = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select count(sprintTask_dateCompleted) from SprintTasks where sprintTaskId = '" + id + "' ";
                int thereExists_dateComepleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (thereExists_dateComepleted > 0)
                {
                    cmd.CommandText = "select sprintTask_dateCompleted from SprintTasks where sprintTaskId = '" + id + "' ";
                    dateCompleted = cmd.ExecuteScalar().ToString();
                }
                //Loop through the developers for the selected Sprint Task:
                cmd.CommandText = "select count(*) from UsersForSprintTasks where sprintTaskId = '" + id + "' ";
                int usersForSprintTask = Convert.ToInt32(cmd.ExecuteScalar());
                for (int j = 1; j <= usersForSprintTask; j++)
                {
                    cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForSprintTasksId ASC), * FROM [UsersForSprintTasks] where sprintTaskId = '" + id + "' ) as t where rowNum = '" + j + "'";
                    string developerId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + developerId + "' ";
                    if (j == 1)
                        developersResponsible = cmd.ExecuteScalar().ToString();
                    else
                        developersResponsible = developersResponsible + ", " + cmd.ExecuteScalar().ToString();
                }
                cmd.CommandText = "select sprintTask_currentStatus from SprintTasks where sprintTaskId = '" + id + "' ";
                currentStatus = cmd.ExecuteScalar().ToString();
                //Creator's info, which will be hidden:
                cmd.CommandText = "select sprintTask_createdBy from SprintTasks where sprintTaskId = '" + id + "' ";
                creatorId = cmd.ExecuteScalar().ToString();
                dt.Rows.Add(sprintTask_uniqueId, userStoryUniqueId, taskDescription, Layouts.getTimeFormat(dateIntroduced), Layouts.getTimeFormat(dateConsidered), Layouts.getTimeFormat(dateCompleted), developersResponsible, currentStatus, creatorId, id, removeSprintTaskCommand);
                //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            //Sort by "Sprint task ID" column in "ASC" ascending order:
            if (dt != null)
            {
                DataView dv = new DataView(dt);
                dv.Sort = "Sprint task ID" + " " + "ASC";
                grdResults.DataSource = dv;
                grdResults.DataBind();
            }
            rebindValuesForSprintTasks();
        }
        protected void createTestCasesTimeTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Test case ID", typeof(double));
            dt.Columns.Add("User story ID", typeof(string));
            dt.Columns.Add("Sprint task ID", typeof(string));
            dt.Columns.Add("Test case scenario", typeof(string));
            dt.Columns.Add("Input parameters", typeof(string));
            dt.Columns.Add("Expected output", typeof(string));
            dt.Columns.Add("Current status", typeof(string));
            dt.Columns.Add("Creator User ID", typeof(string));
            dt.Columns.Add("DB Test Case ID", typeof(string));
            dt.Columns.Add("Archive Test Case", typeof(string));
            string testCase_uniqueId = "", userStoryUniqueId = "", sprintTask_uniqueId = "", testCaseScenario = "", inputParameters = "", expectedOutput = "", currentStatus = "";
            string testCase_creatorId = "", testCaseId = "", removeTestCaseCommand = " Archive ";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            SortedSet<string> set_results = new SortedSet<string>();
            int int_roleId = Convert.ToInt32(roleId);
            DateTime start_time = calFrom.SelectedDate, end_time = calTo.SelectedDate;
            int countMatchingResults = 0;
            if (!start_time.ToString().Equals("1/1/0001 12:00:00 AM") && !end_time.ToString().Equals("1/1/0001 12:00:00 AM"))
            {
                if (int_roleId == 1)//If an admin is searching, get everything:
                {
                    cmd.CommandText = "select count(*) from TestCases where testCase_createdDate >= '" + start_time + "' and testCase_createdDate <= '" + end_time + "' ";
                    countMatchingResults = Convert.ToInt32(cmd.ExecuteScalar());
                    for (int i = 1; i <= countMatchingResults; i++)
                    {
                        cmd.CommandText = "select [testCaseId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY testCaseId ASC), * FROM [TestCases] where " +
                            " testCase_createdDate >= '" + start_time + "' and testCase_createdDate <= '" + end_time + "' " +
                            " ) as t where rowNum = '" + i + "'";
                        string tempId = cmd.ExecuteScalar().ToString();
                        set_results.Add(tempId);
                    }
                }
                else//If a master or a developer is searching, get the related results:
                {
                    cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                    string userId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select count(*) from UsersForProjects where userId = '" + userId + "' ";
                    int totalProjectsForUser = Convert.ToInt32(cmd.ExecuteScalar());
                    for (int j = 1; j <= totalProjectsForUser; j++)
                    {
                        cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForProjectsId ASC), * FROM [UsersForProjects] where userId = '" + userId + "') as t where rowNum = '" + j + "'";
                        string projectId = cmd.ExecuteScalar().ToString();
                        //Check if the selected project is deleted:
                        cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
                        int project_isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                        if (project_isDeleted == 0)//If the project is not deleted, fetch its sprint tasks: (0 = false)
                        {
                            cmd.CommandText = "select count(*) from UserStories where  projectId = '" + projectId + "' ";
                            int totalUserStoriesForProject = Convert.ToInt32(cmd.ExecuteScalar());
                            for (int i = 1; i <= totalUserStoriesForProject; i++)
                            {
                                cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where projectId = '" + projectId + "') " +
                                    "as t where rowNum = '" + i + "'";
                                string userStoryId = cmd.ExecuteScalar().ToString();
                                cmd.CommandText = "select count(*) from SprintTasks where userStoryId = '" + userStoryId + "'  ";
                                int totalSprintTasks = Convert.ToInt32(cmd.ExecuteScalar());
                                for (int k = 1; k <= totalSprintTasks; k++)
                                {
                                    cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] where userStoryId = '" + userStoryId + "'   " +
                                        "  ) as t where rowNum = '" + k + "'";
                                    string sprintTaskId = cmd.ExecuteScalar().ToString();
                                    cmd.CommandText = "select count(*) from TestCases where  testCase_createdDate >= '" + start_time + "' and testCase_createdDate <= '" + end_time + "' ";
                                    countMatchingResults = Convert.ToInt32(cmd.ExecuteScalar());
                                    for (int m = 1; m <= countMatchingResults; m++)
                                    {
                                        cmd.CommandText = "select [testCaseId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY testCaseId ASC), * FROM [TestCases] where " +
                                            " testCase_createdDate >= '" + start_time + "' and testCase_createdDate <= '" + end_time + "' " +
                                            " ) as t where rowNum = '" + m + "'";
                                        string tempId = cmd.ExecuteScalar().ToString();
                                        set_results.Add(tempId);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            int totalCount = set_results.Count;
            for (int i = 0; i < totalCount; i++)
            {
                //Get the test case ID:
                testCaseId = set_results.ElementAt(i);
                cmd.CommandText = "select sprintTaskId from TestCases where testCaseId = '" + testCaseId + "' ";
                string sprintTaskId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStoryId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
                userStoryUniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                sprintTask_uniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_testCaseScenario from TestCases where testCaseId = '" + testCaseId + "' ";
                testCaseScenario = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_expectedOutput from TestCases where testCaseId = '" + testCaseId + "' ";
                expectedOutput = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_currentStatus from TestCases where testCaseId = '" + testCaseId + "' ";
                currentStatus = cmd.ExecuteScalar().ToString();
                //Creator's info, which will be hidden:
                cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + testCaseId + "' ";
                testCase_creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_uniqueId from TestCases where testCaseId = '" + testCaseId + "' ";
                testCase_uniqueId = cmd.ExecuteScalar().ToString();
                //Loop through the developers for the selected Test Case:
                cmd.CommandText = "select count(*) from Parameters where testcaseId = '" + testCaseId + "' ";
                int parametersForTestCase = Convert.ToInt32(cmd.ExecuteScalar());
                for (int j = 1; j <= parametersForTestCase; j++)
                {
                    cmd.CommandText = "select parameter_name from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY parameterId ASC), * FROM Parameters where testCaseId = '" + testCaseId + "' ) as t where rowNum = '" + j + "'";
                    string parameter_name = cmd.ExecuteScalar().ToString();
                    if (j == 1)
                        inputParameters = parameter_name;
                    else
                        inputParameters = inputParameters + ", " + parameter_name;
                }
                dt.Rows.Add(testCase_uniqueId, userStoryUniqueId, sprintTask_uniqueId, testCaseScenario, inputParameters, expectedOutput, currentStatus, testCase_creatorId, testCaseId, removeTestCaseCommand);
                //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
            }
            connect.Close();
            grdResults.DataSource = dt;
            grdResults.DataBind();
            //Sort by "Test case ID" column in "ASC" ascending order:
            if (dt != null)
            {
                DataView dv = new DataView(dt);
                dv.Sort = "Test case ID" + " " + "ASC";
                grdResults.DataSource = dv;
                grdResults.DataBind();
            }
            rebindValuesForTestCases();
        }
    }
}