using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Admin
{
    public partial class ViewSprintTask : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        string previousPage = "";
        static string g_sprintTaskId = "";
        static string g_userStoryId = "";
        static string g_projectId = "";
        //static ArrayList searchedUsers = new ArrayList();
        //static SortedSet<string> usersToAdd = new SortedSet<string>();
        static List<HttpPostedFile> files;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                if (Request.QueryString["id"] != null)
                {
                    g_sprintTaskId = Request.QueryString["id"];
                    if (!check.checkSprintTaskAccess(g_sprintTaskId, loginId))
                        goBack();
                    if (Request.QueryString["userStoryId"] != null)
                    {
                        g_userStoryId = Request.QueryString["userStoryId"];
                    }
                    if (Request.QueryString["projectId"] != null)
                    {
                        g_projectId = Request.QueryString["projectId"];
                    }
                }
                else
                    goBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                goBack();
            }
            if (!IsPostBack)
            {
                files = new List<HttpPostedFile>();
                getSprintTaskInfo();
                showView();
            }
            if (!AddNewTestCase.Visible)
            {
                try
                {
                    createTable();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.ToString());
                }
            }
            //The below to be used whenever needed in the other page:
            Session.Add("projectId", g_projectId);
            Session.Add("userStoryId", g_userStoryId);
            Session.Add("sprintTaskId", g_sprintTaskId);
            checkIfSprintTaskDeleted();
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
            previousPage = previous_page;
            //Get current time:
            DateTime currentTime = DateTime.Now;
            //Get user's IP:
            string userIP = GetIPAddress();
            CheckSession session = new CheckSession();
            bool correctSession = session.sessionIsCorrect(username, roleId, token, current_page, previous_page, currentTime, userIP);
            if (!correctSession)
                clearSession();
            int int_roleId = Convert.ToInt32(roleId);
            if (int_roleId != 1)//1 = Admin role.
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
            try
            {
                Session.Add("projectId", g_projectId);
                Session.Add("userStoryId", g_userStoryId);
                Session.Add("sprintTaskId", g_sprintTaskId);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }
        protected void getSession()
        {
            username = (string)(Session["username"]);
            roleId = (string)(Session["roleId"]);
            loginId = (string)(Session["loginId"]);
            token = (string)(Session["token"]);
            try
            {
                g_projectId = (string)(Session["projectId"]);
                g_userStoryId = (string)(Session["userStoryId"]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }
        protected void goBack()
        {
            addSession();
            string backLink = "";
            try
            {
                g_userStoryId = (string)(Session["userStoryId"]);
                backLink = "ViewUserStory.aspx?id=" + g_userStoryId;
            }
            catch (Exception e)
            {
                backLink = "Home.aspx";
                Console.WriteLine("Error: " + e);
            }
            Response.Redirect(backLink);
        }
        protected void getSprintTaskInfo()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select [userStoryId] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            string userStoryId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select [sprintTask_createdBy] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTask_createdBy = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select [sprintTask_createdDate] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTask_createdDate = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select [sprintTask_uniqueId] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTask_uniqueId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select [sprintTask_taskDescription] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTask_taskDescription = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select [sprintTask_dateIntroduced] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTask_dateIntroduced = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select [sprintTask_dateConsideredForImplementation] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTask_dateConsideredForImplementation = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select [sprintTask_isDeleted] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            int sprintTask_isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select [sprintTask_hasImage] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            int sprintTask_hasImage = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select sprintTask_currentStatus from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTask_currentStatus = cmd.ExecuteScalar().ToString();
            string sprintTask_dateCompleted = "";// "Not completed";
            string sprintTask_editedBy = "";//"Not edited";
            string sprintTask_editedDate = "";// "Not edited";
            string sprintTask_previousVersion = "";// "No previous version";
            cmd.CommandText = "select count(sprintTask_dateCompleted) from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            int thereExists_dateCompleted = Convert.ToInt32(cmd.ExecuteScalar());
            if (thereExists_dateCompleted > 0)
            {
                cmd.CommandText = "select [sprintTask_dateCompleted] from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
                sprintTask_dateCompleted = cmd.ExecuteScalar().ToString();
            }
            cmd.CommandText = "select count(sprintTask_editedBy) from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            int thereExists_editedBy = Convert.ToInt32(cmd.ExecuteScalar());
            if (thereExists_editedBy > 0)
            {
                cmd.CommandText = "select sprintTask_editedBy from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
                sprintTask_editedBy = cmd.ExecuteScalar().ToString();
            }
            cmd.CommandText = "select count(sprintTask_editedDate) from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            int thereExists_editedDate = Convert.ToInt32(cmd.ExecuteScalar());
            if (thereExists_editedDate > 0)
            {
                cmd.CommandText = "select sprintTask_editedDate from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
                sprintTask_editedDate = cmd.ExecuteScalar().ToString();
            }
            cmd.CommandText = "select count(sprintTask_previousVersion) from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
            int thereExists_previousVersion = Convert.ToInt32(cmd.ExecuteScalar());
            if (thereExists_previousVersion > 0)
            {
                cmd.CommandText = "select sprintTask_previousVersion from [SprintTasks] where sprintTaskId = '" + g_sprintTaskId + "' ";
                sprintTask_previousVersion = cmd.ExecuteScalar().ToString();
            }
            //Convert the createdByUserId to a name:
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + sprintTask_createdBy + "' ";
            string createdBy = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            string imagesHTML = "";
            if (sprintTask_hasImage == 1)
            {
                cmd.CommandText = "select count(*) from ImagesForSprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
                int totalImages = Convert.ToInt32(cmd.ExecuteScalar());
                for (int i = 1; i <= totalImages; i++)
                {
                    cmd.CommandText = "select [imageId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY imageId ASC), * FROM [ImagesForSprintTasks] where sprintTaskId = '" + g_sprintTaskId + "') as t where rowNum = '" + i + "'";
                    string imageId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select image_name from Images where imageId = '" + imageId + "' ";
                    string image_name = cmd.ExecuteScalar().ToString();
                    imagesHTML = imagesHTML + "<a href='../../images/" + image_name + "' target=\"_blank\">" + image_name + "</a> <br />";
                }
            }
            cmd.CommandText = "select count(*) from UsersForSprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
            int totalUsers = Convert.ToInt32(cmd.ExecuteScalar());
            string[] sprintTaskMembers = new string[totalUsers];
            for (int i = 1; i <= totalUsers; i++)
            {
                cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY UsersForSprintTasksId ASC), * FROM [UsersForSprintTasks] where sprintTaskId = '" + g_sprintTaskId + "') as t where rowNum = '" + i + "'";
                sprintTaskMembers[i - 1] = cmd.ExecuteScalar().ToString();
            }
            cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '"+ userStoryId + "' ";
            string userStoryUId = cmd.ExecuteScalar().ToString();
            connect.Close();
            //Construct an HTML output to post it:
            lblSprintTaskInfo.Text = Layouts.sprintTaskHeader(g_sprintTaskId, roleId, loginId, userId, userStoryId, sprintTask_createdBy,
                sprintTask_createdDate, createdBy, sprintTask_uniqueId, sprintTask_taskDescription, sprintTask_dateIntroduced, sprintTask_dateConsideredForImplementation,
                sprintTask_isDeleted, imagesHTML, sprintTask_hasImage, sprintTask_currentStatus, sprintTaskMembers,userStoryUId, 
                sprintTask_dateCompleted, sprintTask_editedBy, sprintTask_editedDate, sprintTask_previousVersion);
        }
        protected void showView()
        {
            View.Visible = true;
            AddNewTestCase.Visible = false;
        }
        protected void showNewTestCase()
        {
            View.Visible = false;
            AddNewTestCase.Visible = true;
        }
        protected void checkIfSprintTaskDeleted()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //Count the existance of the sprint task:
            cmd.CommandText = "select count(*) from SprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count > 0)//if count > 0, then the project ID exists in DB.
            {
                cmd.CommandText = "select sprintTask_createdBy from SprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
                string actual_creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_isDeleted from SprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
                int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (isDeleted == 1)
                {
                    //Hide the submit buttons:
                    btnAddNewTestCase.Visible = false;
                    //btnAddUserToList.Visible = false;
                    btnSaveTestCase.Visible = false;
                    btnUpload.Visible = false;
                }
            }
            connect.Close();
        }
        protected void rebindValues()
        {
            int int_roleId = Convert.ToInt32(roleId);
            if (grdTestCases.Rows.Count > 0)
            {
                //Hide the headers called "User ID" and "Test Case ID":
                grdTestCases.HeaderRow.Cells[7].Visible = false;
                grdTestCases.HeaderRow.Cells[8].Visible = false;
                //Hide IDs column and content which are located in column index 8:
                for (int i = 0; i < grdTestCases.Rows.Count; i++)
                {
                    grdTestCases.Rows[i].Cells[7].Visible = false;
                    grdTestCases.Rows[i].Cells[8].Visible = false;
                }
            }
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            string testCaseUniqueId = "", sprintTaskUniqueId = "", userStoryUniqueId = "", testCaseScenario = "", inputParameters = "", expectedOutput = "", currentStatus = "";
            string creatorId = "", db_testCaseId = "";
            for (int row = 0; row < grdTestCases.Rows.Count; row++)
            {
                //Set links to review a user:
                testCaseUniqueId = grdTestCases.Rows[row].Cells[0].Text;
                userStoryUniqueId = grdTestCases.Rows[row].Cells[1].Text;
                sprintTaskUniqueId = grdTestCases.Rows[row].Cells[2].Text;
                testCaseScenario = grdTestCases.Rows[row].Cells[3].Text;
                inputParameters = grdTestCases.Rows[row].Cells[4].Text;
                expectedOutput = grdTestCases.Rows[row].Cells[5].Text;
                currentStatus = grdTestCases.Rows[row].Cells[6].Text;
                creatorId = grdTestCases.Rows[row].Cells[7].Text;
                db_testCaseId = grdTestCases.Rows[row].Cells[8].Text;
                string removeSprintTaskCommand = grdTestCases.Rows[row].Cells[9].Text;
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
                string testCaseUrl = "ViewTestCase.aspx?id=" + id;
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
                userStoryUIdLink.NavigateUrl = "ViewUserStory.aspx?id=" + g_userStoryId;
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
                removeTestCaseLink.CommandArgument = testCaseUniqueId;
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
                grdTestCases.Rows[row].Cells[0].Controls.Add(testCaseUIdLink);
                grdTestCases.Rows[row].Cells[1].Controls.Add(userStoryUIdLink);
                grdTestCases.Rows[row].Cells[2].Controls.Add(sprintTaskUIdLink);
                grdTestCases.Rows[row].Cells[3].Controls.Add(testCaseScenarioLink);
                grdTestCases.Rows[row].Cells[4].Controls.Add(parametersLink);
                grdTestCases.Rows[row].Cells[5].Controls.Add(expectedOutputLink);
                grdTestCases.Rows[row].Cells[6].Controls.Add(currentStatusLink);
                grdTestCases.Rows[row].Cells[9].Controls.Add(removeTestCaseLink);
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
        protected int getTotalTestCases()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from TestCases where sprintTaskId = '" + g_sprintTaskId + "' ";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return count;
        }
        protected void createTable()
        {
            int countTestCases = getTotalTestCases();
            if (countTestCases == 0)
            {
                lblMessage.Visible = true;
            }
            else if (countTestCases > 0)
            {
                lblMessage.Visible = false;
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
                for (int i = 1; i <= countTestCases; i++)
                {
                    //Get the test case ID:
                    cmd.CommandText = "select testCaseId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY testCaseId ASC), * FROM TestCases where sprintTaskId = '" + g_sprintTaskId + "' ) as t where rowNum = '" + i + "'";
                    testCaseId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + g_userStoryId + "' ";
                    userStoryUniqueId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
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
                grdTestCases.DataSource = dt;
                grdTestCases.DataBind();
                //Sort by "Test case ID" column in "ASC" ascending order:
                if (dt != null)
                {
                    DataView dv = new DataView(dt);
                    dv.Sort = "Test case ID" + " " + "ASC";
                    grdTestCases.DataSource = dv;
                    grdTestCases.DataBind();
                }
                rebindValues();
            }
        }
        protected void grdTestCases_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdTestCases.PageIndex = e.NewPageIndex;
            grdTestCases.DataBind();
            rebindValues();
        }
        protected void calDateIntroduced_SelectionChanged(object sender, EventArgs e)
        {

        }
        protected void calDateConsidered_SelectionChanged(object sender, EventArgs e)
        {

        }
        protected void btnAddNewTestCase_Click(object sender, EventArgs e)
        {
            showNewTestCase();
            //count the number of the current test cases, then add one to the total
            //So the new test case Unique ID will the resulting number:
            updateUniqueId();
            string userStoryUID = "";
            string sprintTaskUID = "";
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + g_userStoryId + "' ";
                userStoryUID = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
                sprintTaskUID = cmd.ExecuteScalar().ToString();
                connect.Close();
            }
            catch (Exception ex)
            {
                connect.Close();
                Console.WriteLine("Error: " + ex);
            }
            txtUniqueUserStoryID.Text = userStoryUID;
            txtUniqueSprintTaskID.Text = sprintTaskUID;
            clearNewTestCaseInputs();
            hideErrorLabels();
        }
        protected void updateUniqueId()
        {
            string newId = "";
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();

                cmd.CommandText = "select top(1) testCase_uniqueId from TestCases where sprintTaskId = '" + g_sprintTaskId + "' order by testCaseId desc ";
                newId = cmd.ExecuteScalar().ToString();
                connect.Close();
            }
            catch (Exception e)
            {
                connect.Close();
                Console.WriteLine("Error: " + e);
            }
            try
            {
                if (newId.Contains("."))
                {
                    List<string> theId = newId.Split('.').ToList<string>();
                    int tempId = Convert.ToInt32(theId.ElementAt(1));
                    ++tempId;
                    int result = Convert.ToInt32(theId.ElementAt(0)) + 1;
                    newId = result.ToString();
                }
                else
                {
                    int tempId = Convert.ToInt32(newId.Replace(" ", ""));
                    ++tempId;
                    newId = tempId.ToString();
                }
            }
            catch (Exception e)
            {
                newId = "1";
                Console.WriteLine("Error: " + e);
            }
            txtUniqueTestCaseID.Text = newId;
            drpCurrentStatus.SelectedIndex = 1;
            drpCurrentStatus.Enabled = false;
        }
        protected void clearNewTestCaseInputs()
        {
            try
            {
                txtExpectedOutput.Text = "";
                txtInputParameters.Text = "";
                txtTestCaseScenario.Text = "";
                drpCurrentStatus.SelectedIndex = 1;
                drpInputParametersList.Items.Clear();
                if (files != null)
                    files.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
        }
        protected void hideErrorLabels()
        {
            lblTestCaseScenarioError.Visible = false;
            lblInputParametersListError.Visible = false;
            lblInputParametersError.Visible = false;
            lblExpectedOutputError.Visible = false;
            lblUniqueSprintTaskIDError.Visible = false;
            lblUniqueUserStoryIDError.Visible = false;
            lblCurrentStatusError.Visible = false;
            fileNames.InnerHtml = "";
        }
        protected void btnSaveTestCase_Click(object sender, EventArgs e)
        {
            hideErrorLabels();
            if (correctInput())
            {
                //storeInDB();
                addNewEntry();
                sendEmail();
                clearNewTestCaseInputs();
            }
            updateUniqueId();
        }
        protected void sendEmail()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            Email email = new Email();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select user_email from Users where userId like '" + userId + "' ";
            string emailTo = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId like '" + userId + "' ";
            string name = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_name from Projects where projectId = '" + g_projectId + "' ";
            string project_name = cmd.ExecuteScalar().ToString();
            connect.Close();
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your test case#("+txtUniqueTestCaseID.Text+") for sprint task#(" + txtUniqueSprintTaskID.Text + ") has been successfully submitted for the project (" + project_name + ") under the user story#(" + txtUniqueUserStoryID.Text + ").\n" +
                "\n\nBest regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
            email.sendEmail(emailTo, messageBody);
        }
        protected void addNewEntry()
        {
            int hasImage = 0;
            if (files.Count > 0)
            {
                storeImagesInServer();
                hasImage = 1;
            }
            //Store new test case as neither approved nor denied and return its ID:
            string new_testCaseId = storeTestCase(hasImage);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessTestCase(new_testCaseId);
            storeImagesInDB(new_testCaseId, hasImage, files);
            lblAddTestCaseMessage.Visible = true;
            lblAddTestCaseMessage.ForeColor = System.Drawing.Color.Green;
            lblAddTestCaseMessage.Text = "The test case has been successfully submitted and an email notification has been sent to you. <br/>";
            wait.Visible = false;
        }
        protected void allowUserAccessTestCase(string new_testCaseId)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Note: by adding the user to this table, he/she is given access to the newly-created table.
            //Add the creator to UsersForTestCase table.
            //Add the current user to the test case:
            cmd.CommandText = "insert into UsersForTestCases (userId, testCaseId, usersForTestCases_isNotified) values " +
                    "('" + userId + "', '" + new_testCaseId + "', '0')";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected string storeTestCase(int hasImage)
        {
            string new_testCaseId = "";
            DateTime createdDate = DateTime.Now;
            string testCaseUId = txtUniqueTestCaseID.Text.Replace(" ", "");
            testCaseUId = testCaseUId.Replace("'", "''");
            string sprintTaskUId = txtUniqueSprintTaskID.Text.Replace(" ", "");
            sprintTaskUId = sprintTaskUId.Replace("'", "''");
            string userStoryUId = txtUniqueUserStoryID.Text.Replace(" ", "");
            userStoryUId = userStoryUId.Replace("'", "''");
            string testScenario = txtTestCaseScenario.Text.Replace("'", "''");
            string expectedOutput = txtExpectedOutput.Text.Replace("'", "''");
            string currentStatus = drpCurrentStatus.SelectedValue;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string createdBy = cmd.ExecuteScalar().ToString();
            //Store the new user story in the database:
            cmd.CommandText = "insert into TestCases (sprintTaskId, testCase_createdBy, testCase_createdDate, testCase_uniqueId, testCase_testCaseScenario, testCase_expectedOutput, " +
                "testCase_hasImage, testCase_currentStatus) values " +
               "('"+g_sprintTaskId+"', '" + createdBy + "', '" + createdDate + "', '" + testCaseUId  + "', '" + testScenario + "', '" + expectedOutput + "',  " +
               " '" + hasImage + "',  '" + currentStatus + "') ";
            cmd.ExecuteScalar();
            //Get the ID of the newly stored test case from the database:
            cmd.CommandText = "select testCaseId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY testCaseId ASC), * FROM TestCases " +
                "where sprintTaskId = '" + g_sprintTaskId + "' and testCase_createdBy = '" + createdBy + "' and testCase_createdDate = '" + Layouts.getOriginalTimeFormat(createdDate.ToString()) + "' "
                + " and testCase_uniqueId like '" + testCaseUId + "'  "
                + " and testCase_hasImage = '" + hasImage + "' and testCase_currentStatus like '" + currentStatus + "' "
                + " and testCase_isDeleted = '0' "
                + " ) as t where rowNum = '1'";
            new_testCaseId = cmd.ExecuteScalar().ToString();
            //Store the parameters:
            foreach (var p in drpInputParametersList.Items)
            {
                cmd.CommandText = "insert into Parameters (testCaseId, parameter_name) values ('" + new_testCaseId + "', '"+p.ToString().Replace("'", "''")+"')";
                cmd.ExecuteScalar();
            }
            connect.Close();
            return new_testCaseId;
        }
        protected void storeImagesInDB(string testcaseId, int hasImage, List<HttpPostedFile> files)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Check if there is an image:
            if (hasImage == 1)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string imageName = files[i].FileName.ToString().Replace("'", "''");
                    //Add to Images:
                    cmd.CommandText = "insert into Images (image_name) values ('" + imageName + "')";
                    cmd.ExecuteScalar();
                    //Get the image ID:
                    cmd.CommandText = "select imageId from Images where image_name like '" + imageName + "' ";
                    string imageId = cmd.ExecuteScalar().ToString();
                    //Add in ImagesForUserStories:
                    cmd.CommandText = "insert into ImagesForTestCases (imageId, testcaseId) values ('" + imageId + "', '" + testcaseId + "')";
                    cmd.ExecuteScalar();
                }
            }
            connect.Close();
        }
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (Request.Files.Count > 0)
            {
                int fileCount = Request.Files.Count;
                for (int i = 0; i < fileCount; i++)
                {
                    HttpPostedFile file = Request.Files[i];
                    if (checkFile(file))
                        files.Add(file);
                }
                if (fileCount == 1)
                    fileNames.InnerHtml = "You have successfully uploaded your file!";
                else if (fileCount > 1)
                    fileNames.InnerHtml = "You have successfully uploaded your files!";
                else
                    fileNames.InnerHtml = "You have not uploaded any files!";
            }
        }
        protected bool checkFile(HttpPostedFile file)
        {
            bool correct = true;
            if (file.ContentLength > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(file.FileName);
                int filesize = FileUpload1.PostedFile.ContentLength;
                string filename = file.FileName;
                if (fileExtension.ToLower() != ".jpg" && fileExtension.ToLower() != ".tiff" && fileExtension.ToLower() != ".jpeg" &&
                    fileExtension.ToLower() != ".png" && fileExtension.ToLower() != ".gif" && fileExtension.ToLower() != ".bmp" &&
                    fileExtension.ToLower() != ".tif" && fileExtension.ToLower() != ".txt" && fileExtension.ToLower() != ".pdf"
                    && fileExtension.ToLower() != ".html")
                {
                    correct = false;
                    lblImageError.Visible = true;
                    lblImageError.Text = "File Error: The supported formats for files are: jpg, jpeg, tif, tiff, png, gif, bmp, " +
                        "txt, pdf, and HTML.";
                }

                if (filesize > 5242880)
                {
                    correct = false;
                    lblImageError.Visible = true;
                    lblImageError.Text = "File Error: The size of any uploaded file needs to be less than 5MB.";
                }
                if (string.IsNullOrWhiteSpace(filename))
                {
                    correct = false;
                    lblImageError.Visible = true;
                    lblImageError.Text = "File Error: The file you are trying to upload must have a name.";
                }
            }
            else if (file.ContentLength == 0 && file == null)
            {
                correct = false;
                lblImageError.Visible = true;
                lblImageError.Text = "File Error: Please select at least one file to upload.";
            }
            return correct;
        }
        protected void storeImagesInServer()
        {
            //Loop through images and store each one of them:
            for (int i = 0; i < files.Count; i++)
            {
                string path = Server.MapPath("~/images/" + files[i].FileName);
                string fileExtension = System.IO.Path.GetExtension(files[i].FileName);
                if (fileExtension.ToLower() == ".jpg" || fileExtension.ToLower() == ".tiff" || fileExtension.ToLower() == ".jpeg" ||
                    fileExtension.ToLower() == ".png" || fileExtension.ToLower() == ".gif" || fileExtension.ToLower() == ".bmp" ||
                    fileExtension.ToLower() == ".tif")
                {
                    try
                    {
                        System.Drawing.Bitmap image = new System.Drawing.Bitmap(files[i].InputStream);
                        System.Drawing.Bitmap image_copy = new System.Drawing.Bitmap(image);
                        System.Drawing.Image img = RezizeImage(System.Drawing.Image.FromStream(files[i].InputStream), 500, 500);
                        img.Save(path, ImageFormat.Jpeg);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.ToString());
                    }
                }
                else
                {
                    //If the file is not an image, just save it as it is:
                    files[i].SaveAs(path);
                }
            }
        }
        private MemoryStream BytearrayToStream(byte[] arr)
        {
            return new MemoryStream(arr, 0, arr.Length);
        }
        private System.Drawing.Image RezizeImage(System.Drawing.Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = Math.Max(xRatio, yRatio);
                int nnx = (int)Math.Floor(img.Width / ratio);
                int nny = (int)Math.Floor(img.Height / ratio);
                Bitmap cpy = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }
        protected void btnAddParameter_Click(object sender, EventArgs e)
        {
            lblInputParametersError.Visible = false;
            //Check text for paramter:
            if (string.IsNullOrWhiteSpace(txtInputParameters.Text))
            {
                lblInputParametersError.Visible = true;
                lblInputParametersError.Text = "Invalid input: Please type something for the input paramters before adding it.";
            }
            else
            {
                bool noDuplicateParameter = true;
                string parameter = txtInputParameters.Text;
                parameter = parameter.TrimStart(' ').TrimEnd(' ');
                //check if there is another duplicate parameter:
                foreach (var p in drpInputParametersList.Items)
                {
                    if (p.ToString().Equals(parameter))
                    {
                        noDuplicateParameter = false;
                    }
                }
                if (noDuplicateParameter)
                {
                    //If passed the check, add the newly entered text:
                    parameter = parameter.Replace("'", "''");
                    drpInputParametersList.Items.Add(parameter);
                    //Clear the text entered for parameters and leave the second copy in the ListBox
                    txtInputParameters.Text = "";
                }
                else
                {
                    lblInputParametersError.Visible = true;
                    lblInputParametersError.Text = "Invalid input: The parameter you entered already exists, please type another parameter.";
                }
            }
        }
        protected void btnRemoveParameter_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = drpInputParametersList.Items.Count; i >= 0; i--)
                {
                    int indexToRemove = drpInputParametersList.SelectedIndex;
                    if (indexToRemove > -1)
                    {
                        drpInputParametersList.Items.RemoveAt(indexToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
        protected bool correctInput()
        {
            bool correct = true;
            if (string.IsNullOrWhiteSpace(txtExpectedOutput.Text))
            {
                correct = false;
                lblExpectedOutputError.Text = "Invalid input: Please type something for the Expected output .";
                lblExpectedOutputError.Visible = true;
            }
            if (string.IsNullOrWhiteSpace(txtTestCaseScenario.Text))
            {
                correct = false;
                lblTestCaseScenarioError.Text = "Invalid input: Please type something for the test case scenario .";
                lblTestCaseScenarioError.Visible = true;
            }
            if (drpCurrentStatus.SelectedIndex == 0)
            {
                correct = false;
                lblCurrentStatusError.Visible = true;
                lblCurrentStatusError.Text = "Invalid input: Please select a status for this test case.";
            }

            return correct;
        }
        protected void btnGoBack_Click(object sender, EventArgs e)
        {
            addSession();
            //Response.Redirect("Home");
            goBack();
        }
        protected void btnGoBackToListOfTestCases_Click(object sender, EventArgs e)
        {
            showView();
            createTable();
        }
        protected void grdTestCases_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        protected static bool isSprintTaskCorrect(string sprintTaskId, string creatorId)
        {
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            //check if id contains a special character:
            if (!errors.isDigit(sprintTaskId))
                correct = false;
            //check if id contains an id that does not exist in DB:
            else if (errors.ContainsSpecialChars(sprintTaskId))
                correct = false;
            if (correct)
            {
                Configuration config = new Configuration();
                SqlConnection connect = new SqlConnection(config.getConnectionString());
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();
                //Count the existance of the Sprint Task:
                cmd.CommandText = "select count(*) from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)//if count > 0, then the Sprint Task ID exists in DB.
                {
                    cmd.CommandText = "select sprintTask_createdBy from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    string actual_creatorId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select sprintTask_isDeleted from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                    if (isDeleted == 1)
                        correct = false;
                }
                else
                    correct = false; // means that the test case ID does not exists in DB.
                connect.Close();
            }
            return correct;
        }
        [WebMethod]
        [ScriptMethod()]
        public static void removeSprintTask_Click(string sprintTaskId, string entry_creatorId)
        {
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool sprintTaskIdExists = isSprintTaskCorrect(sprintTaskId, entry_creatorId);
            if (sprintTaskIdExists)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //update the DB and set isDeleted = true:
                cmd.CommandText = "update SprintTasks set sprintTask_isDeleted = 1, sprintTask_currentStatus = 'Archived' where sprintTaskId = '" + sprintTaskId + "' ";
                cmd.ExecuteScalar();
                //Update all test cases related to the deleted sprint task:
                cmd.CommandText = "update TestCases set testCase_isDeleted = 1, testCase_currentStatus = 'Archived'  where sprintTaskId = '" + sprintTaskId + "'  ";
                cmd.ExecuteScalar();
                //Email the Sprint task creator about the project being deleted:
                cmd.CommandText = "select sprintTask_createdBy from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string sprintTaskUID = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStoryId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStoryUID = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select projectId from UserStories where userStoryId = '" + userStoryId + "' ";
                string projectId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select project_name from Projects where projectId = '" + projectId + "' ";
                string project_name = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select user_firstname from Users where userId = '" + creatorId + "' ";
                string name = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select user_lastname from Users where userId = '" + creatorId + "' ";
                name = name + " " + cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select user_email from Users where userId = '" + creatorId + "' ";
                string emailTo = cmd.ExecuteScalar().ToString();
                connect.Close();
                string emailBody = "Hello " + name + ",\n\n" +
                    "This email is to inform you that your sprint task#(" + sprintTaskUID + ") for user story#(" + userStoryUID + ") in the project (" + project_name + ") has been deleted. If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                    "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                Email email = new Email();
                email.sendEmail(emailTo, emailBody);
            }
        }
        [WebMethod]
        [ScriptMethod()]
        public static void updateSprintTaskStatus_Click(string sprintTaskId, string entry_creatorId, string newStatus)
        {
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool userStoryIdExists = isSprintTaskCorrect(sprintTaskId, entry_creatorId);
            if (userStoryIdExists)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //update the DB and set isDeleted = true:
                cmd.CommandText = "update SprintTasks set sprintTask_currentStatus = '" + newStatus.Replace("'", "''") + "'  where sprintTaskId = '" + sprintTaskId + "' ";
                cmd.ExecuteScalar();
                //Email the Sprint task creator about the project being deleted:
                cmd.CommandText = "select sprintTask_createdBy from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string sprintTaskUID = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStoryId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStoryUID = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select projectId from UserStories where userStoryId = '" + userStoryId + "' ";
                string projectId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select project_name from Projects where projectId = '" + projectId + "' ";
                string project_name = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select user_firstname from Users where userId = '" + creatorId + "' ";
                string name = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select user_lastname from Users where userId = '" + creatorId + "' ";
                name = name + " " + cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select user_email from Users where userId = '" + creatorId + "' ";
                string emailTo = cmd.ExecuteScalar().ToString();
                connect.Close();
                string emailBody = "Hello " + name + ",\n\n" +
                    "This email is to inform you that the status of your sprint task#(" + sprintTaskUID + ") for user story#(" + userStoryUID + ") in the project (" + project_name + ") has been updated to (" + newStatus + "). If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                    "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                Email email = new Email();
                email.sendEmail(emailTo, emailBody);
            }
        }
    }
}