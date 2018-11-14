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

namespace Scrum.Accounts.Developer
{
    public partial class PrintProject : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        string previousPage = "";
        string projectId = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                if (!check.checkProjectAccess(projectId, loginId))
                    goBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                goBack();
            }
            printInfo();
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
            if (int_roleId != 3)
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
            projectId = (string)(Session["projectId"]);
        }
        protected void goBack()
        {
            addSession();
            if (!string.IsNullOrWhiteSpace(previousPage)) Response.Redirect(previousPage);
            else Response.Redirect("Home.aspx");
        }
        protected void printInfo()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select project_name from Projects where projectId = '" + projectId + "' ";
            string project_name = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_description from Projects where projectId = '" + projectId + "' ";
            string project_description = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select count(*) from UsersForProjects where projectId = '" + projectId + "' ";
            int totalUsersForProject = Convert.ToInt32(cmd.ExecuteScalar());
            List<string> names = new List<string>();
            for (int i = 1; i <= totalUsersForProject; i++)
            {
                cmd.CommandText = "select userId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userId ASC), * FROM usersForProjects where projectId = '" + projectId + "' ) as t where rowNum = '" + i + "' ";
                string userId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + userId + "'  ";
                string name = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select loginId from Users where userId = '" + userId + "' ";
                string loginId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select roleId from Logins where loginId = '" + loginId + "' ";
                int int_roleId = Convert.ToInt32(cmd.ExecuteScalar());
                if (int_roleId == 2)
                    name = "Project Master: " + name + "";
                else
                    name = "Developer: " + name + "";
                names.Add(name);
            }
            string projectHeader = "<h1>" + project_name + "</h1></br>" + project_description + "</br></br>";
            foreach (string n in names)
                projectHeader += n + "</br>";
            projectHeader += "</br></br></br>";
            string userStoriesTable =
                "<h3>User Stories</h3>" +
                "<table border='1' style='width:100%;' >" +
                "<tr><th>Unique user story ID</th>" + "<th>As a \"type of user\"</th>" + "<th>I want to \"some goal\"</th>" + "<th>so that \"reason\"</th>" + "<th>Date introduced</th>" + "<th>Date considered for implementation</th>" + "<th>Developer responsible for</th>" + "<th>Current Status</th></tr>";
            string sprintTasksTable =
                "<h3>Sprint Tasks</h3>" +
                "<table border='1' style='width:100%;' >" +
                "<tr><th>Unique sprint task ID</th>" + "<th>Unique user story ID</th>" + "<th>Task description</th>" + "<th>Date introduced</th>" + "<th>Date considered for implementation</th>" + "<th>Date completed</th>" + "<th>Developer responsible for</th>" + "<th>Current Status</th></tr>";
            string testCasesTables =
                "<h3>Test Cases</h3>";
            cmd.CommandText = "select count(*) from UserStories where projectId = '" + projectId + "' ";
            int totalUserStories = Convert.ToInt32(cmd.ExecuteScalar());
            int totalSprintTasks = 0;
            int totalTestCases = 0;
            //int sprintTaskCounter = 0;
            for (int i = 1; i <= totalUserStories; i++)
            {
                cmd.CommandText = "select userStoryId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY CAST(userStory_uniqueId AS float) ASC), * FROM UserStories where projectId = '" + projectId + "' ) as t where rowNum = '" + i + "' ";
                string userStoryId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStory_uniqueId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_asARole from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStory_asARole = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_iWantTo from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStory_iWantTo = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_soThat from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStory_soThat = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_dateIntroduced from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStory_dateIntroduced = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_dateConsideredForImplementation from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStory_dateConsideredForImplementation = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_currentStatus from UserStories where userStoryId = '" + userStoryId + "' ";
                string userStory_currentStatus = cmd.ExecuteScalar().ToString();
                //Now get the list of developers:
                cmd.CommandText = "select count(*) from UsersForUserStories where userStoryId = '" + userStoryId + "' ";
                int totalUsersForUserStory = Convert.ToInt32(cmd.ExecuteScalar());
                string usersForUserStories = "";
                for (int j = 1; j <= totalUsersForUserStory; j++)
                {
                    cmd.CommandText = "select userId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForUserStoriesId ASC), * FROM UsersForUserStories where userStoryId = '" + userStoryId + "' ) as t where rowNum = '" + j + "' ";
                    string temp_userId = cmd.ExecuteScalar().ToString();
                    //Translate the user ID to a user name:
                    cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + temp_userId + "' ";
                    string temp_name = cmd.ExecuteScalar().ToString();
                    if (j == 1)
                        usersForUserStories = temp_name;
                    else
                        usersForUserStories = usersForUserStories + ",</br>" + temp_name;
                }
                userStoriesTable +=
                    "<tr><td>" + userStory_uniqueId + "</td><td>" + userStory_asARole + "</td><td>" + userStory_iWantTo + "</td><td>" + userStory_soThat + "</td><td>" + Layouts.getTimeFormat(userStory_dateIntroduced) + "</td><td>" + Layouts.getTimeFormat(userStory_dateConsideredForImplementation) + "</td><td>" + usersForUserStories + "</td><td>" + userStory_currentStatus + "</td></tr>";
                cmd.CommandText = "select count(*) from SprintTasks where userStoryId = '" + userStoryId + "' ";
                int temp_totalSprintTasks = Convert.ToInt32(cmd.ExecuteScalar());
                totalSprintTasks += temp_totalSprintTasks;
                for (int j = 1; j <= temp_totalSprintTasks; j++)
                {
                    //sprintTaskCounter++;
                    cmd.CommandText = "select sprintTaskId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY CAST(sprintTask_uniqueId AS float) ASC), * FROM SprintTasks where userStoryId = '" + userStoryId + "' ) as t where rowNum = '" + j + "' ";
                    string sprintTaskId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    string sprintTask_uniqueId = cmd.ExecuteScalar().ToString();
                    //userStory_uniqueId
                    cmd.CommandText = "select sprintTask_taskDescription from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    string sprintTask_taskDescription = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select sprintTask_dateIntroduced from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    string sprintTask_dateIntroduced = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select sprintTask_dateConsideredForImplementation from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    string sprintTask_dateConsideredForImplementation = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select count(sprintTask_dateCompleted) from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    int isCompleted = Convert.ToInt32(cmd.ExecuteScalar());
                    string sprintTask_dateCompleted = "";
                    if (isCompleted > 0)
                    {
                        cmd.CommandText = "select sprintTask_dateCompleted from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                        sprintTask_dateCompleted = cmd.ExecuteScalar().ToString();
                        sprintTask_dateCompleted = Layouts.getTimeFormat(sprintTask_dateCompleted);
                    }
                    else
                        sprintTask_dateCompleted = "Not completed";
                    //Now get the list of developers:
                    cmd.CommandText = "select count(*) from UsersForSprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    int totalUsersForSprintTask = Convert.ToInt32(cmd.ExecuteScalar());
                    string usersForSprintTasks = "";
                    for (int k = 1; k <= totalUsersForSprintTask; k++)
                    {
                        cmd.CommandText = "select userId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY usersForSprintTasksId ASC), * FROM UsersForSprintTasks where sprintTaskId = '" + sprintTaskId + "' ) as t where rowNum = '" + k + "' ";
                        string temp_userId = cmd.ExecuteScalar().ToString();
                        //Translate the user ID to a user name:
                        cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + temp_userId + "' ";
                        string temp_name = cmd.ExecuteScalar().ToString();
                        if (j == 1)
                            usersForSprintTasks = temp_name;
                        else
                            usersForSprintTasks = usersForSprintTasks + ",</br>" + temp_name;
                    }
                    cmd.CommandText = "select sprintTask_currentStatus from SprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
                    string sprintTask_currentStatus = cmd.ExecuteScalar().ToString();
                    sprintTasksTable +=
                        "<tr><td>" + sprintTask_uniqueId + "</td><td>" + userStory_uniqueId + "</td><td>" + sprintTask_taskDescription + "</td><td>" + Layouts.getTimeFormat(sprintTask_dateIntroduced) + "</td><td>" + Layouts.getTimeFormat(sprintTask_dateConsideredForImplementation) + "</td><td>" + sprintTask_dateCompleted + "</td><td>" + usersForSprintTasks + "</td><td>" + sprintTask_currentStatus + "</td></tr>";
                    string testCaseTable =
                        "<h3>Test case for (user story ID: " + userStory_uniqueId + ", sprint task ID: " + sprintTask_uniqueId + ") </h3>" +
                        "<table border='1' style='width:100%;' >" +
                        "<tr><th>Unique test case ID</th>" + "<th>Unique user story ID</th>" + "<th>Sprint task ID</th>" + "<th>Test case scenario</th>" + "<th>Input Parameters</th>" + "<th>Expected output</th>" + "<th>Current Status</th></tr>";
                    cmd.CommandText = "select count(*) from TestCases where sprintTaskId = '" + sprintTaskId + "' ";
                    int temp_totalTestCases = Convert.ToInt32(cmd.ExecuteScalar());
                    totalTestCases += temp_totalTestCases;
                    for (int m = 1; m <= temp_totalTestCases; m++)
                    {
                        cmd.CommandText = "select testCaseId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY CAST(testCase_uniqueId AS float) ASC), * FROM TestCases where sprintTaskId = '" + sprintTaskId + "' ) as t where rowNum = '" + m + "' ";
                        string testCaseId = cmd.ExecuteScalar().ToString();
                        cmd.CommandText = "select testCase_uniqueId from TestCases where testCaseId = '" + testCaseId + "' ";
                        string testCase_uniqueId = cmd.ExecuteScalar().ToString();
                        //userStory_uniqueId
                        //sprintTask_uniqueId
                        cmd.CommandText = "select testCase_testCaseScenario from TestCases where testCaseId = '" + testCaseId + "' ";
                        string testCase_testCaseScenario = cmd.ExecuteScalar().ToString();
                        //Get the input parameters:
                        cmd.CommandText = "select count(*) from Parameters where testCaseId = '" + testCaseId + "' ";
                        int totalParameters = Convert.ToInt32(cmd.ExecuteScalar());
                        string inputParameters = "";
                        for (int n = 1; n <= totalParameters; n++)
                        {
                            cmd.CommandText = "select parameter_name from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY parameterId ASC), * FROM Parameters where testCaseId = '" + testCaseId + "' ) as t where rowNum = '" + n + "' ";
                            string parameter = cmd.ExecuteScalar().ToString();
                            if (n == 1)
                                inputParameters = parameter;
                            else
                                inputParameters = inputParameters + ",</br>" + parameter;
                        }
                        cmd.CommandText = "select testCase_expectedOutput from TestCases where testCaseId = '" + testCaseId + "' ";
                        string testCase_expectedOutput = cmd.ExecuteScalar().ToString();
                        cmd.CommandText = "select testCase_currentStatus from TestCases where testCaseId = '" + testCaseId + "' ";
                        string testCase_currentStatus = cmd.ExecuteScalar().ToString();
                        testCaseTable +=
                            "<tr><td>" + testCase_uniqueId + "</td><td>" + userStory_uniqueId + "</td><td>" + sprintTask_uniqueId + "</td><td>" + testCase_testCaseScenario + "</td><td>" + inputParameters + "</td><td>" + testCase_expectedOutput + "</td><td>" + testCase_currentStatus + "</td></tr>";
                    }
                    testCaseTable = (temp_totalTestCases > 0) ? testCaseTable + "</table></br>" : "";
                    testCasesTables += testCaseTable;
                }
            }
            userStoriesTable = (totalUserStories > 0) ? userStoriesTable + "</table></br>" : "";
            sprintTasksTable = (totalSprintTasks > 0) ? sprintTasksTable + "</table></br>" : "";
            testCasesTables = (totalTestCases > 0) ? testCasesTables + "</br>" : "";
            connect.Close();
            string result =
                projectHeader +
                userStoriesTable +
                sprintTasksTable +
                testCasesTables;
            lblContent.Text = result;

        }
    }
}