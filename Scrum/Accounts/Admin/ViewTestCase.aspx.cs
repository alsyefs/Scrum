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
    public partial class ViewTestCase : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        string previousPage = "";
        static string g_sprintTaskId = "";
        static string g_userStoryId = "";
        static string g_projectId = "";
        static string g_testCaseId = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                g_testCaseId = Request.QueryString["id"];
                if (!check.checkTestCaseAccess(g_testCaseId, loginId))
                    goBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                goBack();
            }
            if (!IsPostBack)
            {
                connect.Open();
                try
                {
                    getTestCaseInfo();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.ToString());
                }
                connect.Close();
            }
            //The below to be used whenever needed in the other page. Most likely to be used in ViewSprintTask page:
            Session.Add("testCaseId", g_testCaseId);
            checkIfTestCaseTerminated();
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
            try
            {
                Session.Add("projectId", g_projectId);
                Session.Add("userStoryId", g_userStoryId);
                Session.Add("sprintTaskId", g_sprintTaskId);
                Session.Add("testCaseId", g_testCaseId);
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
                g_sprintTaskId = (string)(Session["sprintTaskId"]);
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
                g_sprintTaskId = (string)(Session["sprintTaskId"]);
                backLink = "ViewSprintTask.aspx?id=" + g_sprintTaskId;
            }
            catch (Exception e)
            {
                backLink = "Home.aspx";
                Console.WriteLine("Error: " + e);
            }
            Response.Redirect(backLink);
        }
        protected void getTestCaseInfo()
        {
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string createdByUserId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_createdDate from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string createddate = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_uniqueId from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string testCaseUID = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_testCaseScenario from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string testCaseScenario = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_expectedOutput from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string expectedOutput = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_isDeleted from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select testCase_hasImage from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int hasImage = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select testCase_currentStatus from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string currentStatus = cmd.ExecuteScalar().ToString();

            cmd.CommandText = "select count(testCase_editedBy) from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int countEditedBy = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(testCase_editedDate) from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int countEditedDate = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(testCase_previousVersion) from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int countPreviousVersion = Convert.ToInt32(cmd.ExecuteScalar());
            string testCase_editedBy="", testCase_editedDate="", testCase_previousVersion="";
            if (countEditedBy > 0)
            {
                cmd.CommandText = "select testCase_editedBy from TestCases where testCaseId = '" + g_testCaseId + "' ";
                testCase_editedBy = cmd.ExecuteScalar().ToString();
            }
            if(countEditedDate > 0)
            {
                cmd.CommandText = "select testCase_editedDate from TestCases where testCaseId = '" + g_testCaseId + "' ";
                testCase_editedDate = cmd.ExecuteScalar().ToString();
            }
            if(countPreviousVersion > 0)
            {
                cmd.CommandText = "select testCase_previousVersion from TestCases where testCaseId = '" + g_testCaseId + "' ";
                testCase_previousVersion = cmd.ExecuteScalar().ToString();
            }
            //Loop through the developers for the selected Test Case:
            cmd.CommandText = "select count(*) from Parameters where testcaseId = '" + g_testCaseId + "' ";
            int parametersForTestCase = Convert.ToInt32(cmd.ExecuteScalar());
            string inputParameters = "";
            for (int j = 1; j <= parametersForTestCase; j++)
            {
                cmd.CommandText = "select parameter_name from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY parameterId ASC), * FROM Parameters where testCaseId = '" + g_testCaseId + "' ) as t where rowNum = '" + j + "'";
                string parameter_name = cmd.ExecuteScalar().ToString();
                if (j == 1)
                    inputParameters = parameter_name;
                else
                    inputParameters = inputParameters + ",\n" + parameter_name;
            }
            //Convert the createdByUserId to a name:
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + createdByUserId + "' ";
            string createdByName = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            string imagesHTML = "";
            if (hasImage == 1)
            {
                cmd.CommandText = "select count(*) from ImagesForTestCases where testCaseId = '" + g_testCaseId + "' ";
                int totalImages = Convert.ToInt32(cmd.ExecuteScalar());
                for (int i = 1; i <= totalImages; i++)
                {
                    cmd.CommandText = "select [imageId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY imageId ASC), * FROM [ImagesForTestCases] where testCaseId = '" + g_testCaseId + "') as t where rowNum = '" + i + "'";
                    string imageId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select image_name from Images where imageId = '" + imageId + "' ";
                    string image_name = cmd.ExecuteScalar().ToString();
                    imagesHTML = imagesHTML + "<a href='../../images/" + image_name + "' target=\"_blank\">" + image_name + "</a> <br />";
                }
            }
            cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '"+g_userStoryId+"' ";
            string userStoryUId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTaskUId = cmd.ExecuteScalar().ToString();
            //Construct an HTML output to post it:
            lblTestCaseInfo.Text = Layouts.testCaseHeader(g_testCaseId, g_sprintTaskId, roleId, loginId, userId, createdByUserId,
            createddate, createdByName, testCaseUID, testCaseScenario, expectedOutput, inputParameters,
            isDeleted, imagesHTML, hasImage, currentStatus, userStoryUId, sprintTaskUId,
            testCase_editedBy, testCase_editedDate, testCase_previousVersion);
        }
        protected void checkIfTestCaseTerminated()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //Count the existance of the test case:
            cmd.CommandText = "select count(*) from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count > 0)//if count > 0, then the test case ID exists in DB.
            {
                cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + g_testCaseId + "' ";
                string actual_creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_isDeleted from TestCases where testCaseId = '" + g_testCaseId + "' ";
                int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (isDeleted == 1)
                {
                    lblMessage.Text = "This test case has been archived!";   
                }
            }
            connect.Close();
        }
        protected void btnGoBack_Click(object sender, EventArgs e)
        {
            addSession();
            goBack();
        }
        protected static bool isTestCaseCorrect(string testCaseId, string creatorId)
        {
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            //check if id contains a special character:
            if (!errors.isDigit(testCaseId))
                correct = false;
            //check if id contains an id that does not exist in DB:
            else if (errors.ContainsSpecialChars(testCaseId))
                correct = false;
            if (correct)
            {
                Configuration config = new Configuration();
                SqlConnection connect = new SqlConnection(config.getConnectionString());
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();
                //Count the existance of the topic:
                cmd.CommandText = "select count(*) from TestCases where testCaseId = '" + testCaseId + "' ";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)//if count > 0, then the project ID exists in DB.
                {
                    cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + testCaseId + "' ";
                    string actual_creatorId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select testCase_isDeleted from TestCases where testCaseId = '" + testCaseId + "' ";
                    int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                    if (isDeleted == 1)
                        correct = false;
                }
                else
                    correct = false; // means that the project ID does not exists in DB.
                connect.Close();
            }
            return correct;
        }
        [WebMethod]
        [ScriptMethod()]
        public static void removeTestCase_Click(string testCaseId, string entry_creatorId)
        {
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool testCaseIdExists = isTestCaseCorrect(testCaseId, entry_creatorId);
            if (testCaseIdExists)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //update the DB and set isDeleted = true:
                cmd.CommandText = "update TestCases set testCase_isDeleted = 1, testCase_currentStatus = 'Archived'  where testCaseId = '" + testCaseId + "' ";
                cmd.ExecuteScalar();
                //Email the test case creator about the test case being deleted:
                cmd.CommandText = "select testCase_uniqueId from TestCases where testCaseId = '" + testCaseId + "' ";
                string testCaseUID = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTaskId from TestCases where testCaseId = '"+testCaseId+"' ";
                string sprintTaskId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + testCaseId + "' ";
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
                    "This email is to inform you that your test case#("+testCaseUID+") under the user story#("+userStoryUID+") in the project with the name (" + project_name + ") has been deleted. If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                    "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                Email email = new Email();
                email.sendEmail(emailTo, emailBody);
            }
        }
        [WebMethod]
        [ScriptMethod()]
        public static void updateTestCaseStatus_Click(string testCaseId, string entry_creatorId, string newStatus)
        {
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool userStoryIdExists = isTestCaseCorrect(testCaseId, entry_creatorId);
            if (userStoryIdExists)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //update the DB and set isDeleted = true:
                cmd.CommandText = "update TestCases set testCase_currentStatus = '" + newStatus.Replace("'", "''") + "'  where testCaseId = '" + testCaseId + "' ";
                cmd.ExecuteScalar();
                //Email the Sprint task creator about the project being deleted:
                cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + testCaseId + "' ";
                string creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_uniqueId from TestCases where testCaseId = '" + testCaseId + "' ";
                string testCaseUID = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select sprintTaskId from TestCases where testCaseId = '"+testCaseId+"' ";
                string sprintTaskId = cmd.ExecuteScalar().ToString();
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
                    "This email is to inform you that the status of your test case#("+testCaseUID+") in sprint task#(" + sprintTaskUID + ") for user story#(" + userStoryUID + ") in the project (" + project_name + ") has been updated to (" + newStatus + "). If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                    "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                Email email = new Email();
                email.sendEmail(emailTo, emailBody);
            }
        }
    }
}