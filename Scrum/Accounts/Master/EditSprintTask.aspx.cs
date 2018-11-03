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

namespace Scrum.Accounts.Master
{
    public partial class EditSprintTask : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        string previousPage = "";
        static string g_userStoryId = "";
        static string g_projectId = "";
        static string g_sprintTaskId = "";
        static ArrayList searchedUsers = new ArrayList();
        static SortedSet<string> usersToAdd = new SortedSet<string>();
        static List<HttpPostedFile> files;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                g_sprintTaskId = Request.QueryString["sprintTaskId"];
                if (!check.checkSprintTaskAccess(g_sprintTaskId, loginId))
                    goBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                goBack();
            }
            //Check if the user editing is the master of this project or an admin:
            if (!isAccessAllowed())
                goBack();
            if (!IsPostBack)
            {
                fillInputs();
                files = new List<HttpPostedFile>();
            }
            //The below to be used whenever needed in the other page. Most likely to be used in ViewUserStory page:
            Session.Add("projectId", g_projectId);
            Session.Add("userStoryId", g_userStoryId);
            Session.Add("sprintTaskId", g_sprintTaskId);
            updateUniqueId();
            checkIfSprintTaskDeleted();
        }
        protected void fillInputs()
        {
            usersToAdd.Clear();
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
            for (int i = 1; i <= totalUsers; i++)
            {
                cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY UsersForSprintTasksId ASC), * FROM [UsersForSprintTasks] where sprintTaskId = '" + g_sprintTaskId + "') as t where rowNum = '" + i + "'";
                string temp_userId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select loginId from Users where userId = '" + temp_userId + "' ";
                int temp_loginId = Convert.ToInt32(cmd.ExecuteScalar());
                int int_loginId = Convert.ToInt32(loginId);
                usersToAdd.Add(temp_userId);
            }
            //The second is to guarantee that the sorted set has been filled without duplicates, then we fill the below list:
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                string temp_userId = usersToAdd.ElementAt(i);
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + temp_userId + "'  ";
                string temp_user_name = cmd.ExecuteScalar().ToString();
                drpSelectedUsers.Items.Add(temp_user_name);
            }
            cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStoryUId = cmd.ExecuteScalar().ToString();
            connect.Close();
            txtUniqueUserStoryID.Text = userStoryUId;
            txtTaskDescription.Text = sprintTask_taskDescription;
            calDateIntroduced.SelectedDate = DateTime.Now;
            calDateConsidered.SelectedDate = DateTime.Now;
            calDateIntroduced.Visible = false;
        }
        protected void btnRemoveSelectedUser_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = drpSelectedUsers.Items.Count; i >= 0; i--)
                {
                    int indexToRemove = drpSelectedUsers.SelectedIndex;
                    if (indexToRemove > -1)
                    {
                        drpSelectedUsers.Items.RemoveAt(indexToRemove);
                        usersToAdd.Remove(usersToAdd.ElementAt(indexToRemove));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
        protected bool isAccessAllowed()
        {
            bool allowed = true;
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            int userId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select project_createdBy from Projects where projectId = '" + g_projectId + "' ";
            int project_creatorId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(userId) from usersForSprintTasks where userId = '" + userId + "' and sprintTaskId = '" + g_sprintTaskId + "' ";
            int isMember = Convert.ToInt32(cmd.ExecuteScalar());
            int int_roleId = Convert.ToInt32(roleId);
            //If the user trying to edit is not the project creator, and not an admin, and not the user story creator, and he/she is not a member of the sprint task: 
            //(Note, the creator of the project must be a master)
            if (userId != project_creatorId && int_roleId != 1 && isMember == 0)
                allowed = false;
            connect.Close();
            return allowed;
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
            try
            {
                Session.Add("projectId", g_projectId);
                Session.Add("userStoryId", g_userStoryId);
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
                g_projectId = (string)(Session["projectId"]);
                g_userStoryId = (string)(Session["userStoryId"]);
                backLink = "ViewSprintTask.aspx?id=" + g_sprintTaskId;
            }
            catch (Exception e)
            {
                backLink = "Home.aspx";
                Console.WriteLine("Error: " + e);
            }
            Response.Redirect(backLink);
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
                    btnAddUserToList.Visible = false;
                    btnUpload.Visible = false;
                    btnRemoveSelectedUser.Visible = false;
                    btnSaveSprintTask.Visible = false;
                }
            }
            connect.Close();
        }
        protected void calDateIntroduced_SelectionChanged(object sender, EventArgs e)
        {

        }
        protected void calDateConsidered_SelectionChanged(object sender, EventArgs e)
        {

        }
        protected void dayRender(object sender, DayRenderEventArgs e)
        {
            if (e.Day.Date < DateTime.Now.Date)
            {
                e.Day.IsSelectable = false;
                e.Cell.ForeColor = System.Drawing.Color.Gray;
            }
        }
        protected void updateUniqueId()
        {
            string newId = "";
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();

                cmd.CommandText = "select top(1) sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + g_sprintTaskId + "' order by sprintTaskId desc ";
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
                    string result = Convert.ToInt32(theId.ElementAt(0)) + "." + tempId;
                    newId = result.ToString();
                }
                else
                {
                    int tempId = Convert.ToInt32(newId.Replace(" ", ""));
                    newId = tempId.ToString() + ".1";
                    newId = newId.Replace(" ", "");
                }
            }
            catch (Exception e)
            {
                newId = "1";
                Console.WriteLine("Error: " + e);
            }
            txtUniqueSprintTaskID.Text = newId;
            //drpCurrentStatus.SelectedIndex = 1;
            //drpCurrentStatus.Enabled = false;
        }
        protected void clearNewSprintTaskInputs()
        {
            try
            {
                txtTaskDescription.Text = "";
                calDateIntroduced.SelectedDates.Clear();
                calDateConsidered.SelectedDates.Clear();
                calDateIntroduced.SelectedDate = DateTime.Now;
                txtDeveloperResponsible.Text = "";
                drpFindUser.Items.Clear();
                lblFindUserResult.Text = "";
                lblListOfUsers.Text = "";
                drpCurrentStatus.SelectedIndex = 0;
                if (searchedUsers != null)
                    searchedUsers.Clear();
                if (usersToAdd != null)
                    usersToAdd.Clear();
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
            lblTaskDescriptionError.Visible = false;
            lblUniqueSprintTaskIDError.Visible = false;
            lblUniqueUserStoryIDError.Visible = false;
            lblDateConsideredError.Visible = false;
            lblDateIntroducedError.Visible = false;
            lblDeveloperResponsibleError.Visible = false;
            lblCurrentStatusError.Visible = false;
            lblFindUserResult.Visible = false;
            lblAddSprintTaskMessage.Visible = false;
            lblListOfUsers.Visible = false;
            fileNames.InnerHtml = "";
            //Hide the calendar for Date Introduced:
            lblDateIntroduced.Visible = false;
            calDateIntroduced.Visible = false;
        }
        protected void drpFindUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            int userIndex = drpFindUser.SelectedIndex;
            string selectedUser = drpFindUser.SelectedValue;
            var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' };
            string result = selectedUser.TrimStart(digits);
            lblFindUserResult.Text = "Selected user: " + (userIndex + 1) + " " + result;
            lblFindUserResult.Visible = true;
        }
        protected void txtDeveloperResponsible_TextChanged(object sender, EventArgs e)
        {
            drpFindUser.Items.Clear();
            searchedUsers.Clear();
            int counter = 0;
            string searchKeyword = txtDeveloperResponsible.Text.Replace("'", "''");
            string[] words = searchKeyword.Split(' ');
            SortedSet<string> set_results = new SortedSet<string>();
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select projectId from UserStories where userStoryId = '" + g_userStoryId + "' ";
            string projectId = cmd.ExecuteScalar().ToString();
            foreach (string word in words)
            {
                cmd.CommandText = "select userId from Users where (user_firstname + ' ' + user_lastname) like '%" + word + "%'  ";
                string temp_Id = cmd.ExecuteScalar().ToString();
                set_results.Add(temp_Id);
            }
            for (int i = 0; i < set_results.Count; i++)
            {
                string temp_userId = set_results.ElementAt(i);
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where userId = '" + temp_userId + "' ";
                string temp_user = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select loginId from Users where userId = '" + temp_userId + "' ";
                int temp_loginId = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select login_isActive from Logins where loginId = '" + temp_loginId + "' ";
                int temp_isActive = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select roleId from Logins where loginId = '" + temp_loginId + "' ";
                int temp_roleId = Convert.ToInt32(cmd.ExecuteScalar());
                int int_loginId = Convert.ToInt32(loginId);
                int int_roleId = Convert.ToInt32(roleId);
                cmd.CommandText = "select count(*) from UsersForProjects where projectId = '" + projectId + "' and userId = '" + temp_userId + "'  ";
                int memberOfProject = Convert.ToInt32(cmd.ExecuteScalar());
                if (memberOfProject == 1)//If the user searched for is a member of the current project
                {
                    //add the searched user if his/her account is active //, and the user is not the current user searching:
                    if (temp_isActive == 1)// && int_loginId != temp_loginId)
                    {
                        searchedUsers.Add(temp_userId);
                        drpFindUser.Items.Add(++counter + " " + temp_user);
                    }
                }

            }
            connect.Close();
        }
        protected void btnSaveSprintTask_Click(object sender, EventArgs e)
        {
            hideErrorLabels();
            if (!string.IsNullOrWhiteSpace(lblListOfUsers.Text))
            {
                lblListOfUsers.Visible = true;
            }
            if (correctInput())
            {
                //storeInDB();
                addNewEntry();
                sendEmail();
                clearNewSprintTaskInputs();
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
            cmd.CommandText = "select projectId from UserStories where userStoryId = '" + g_userStoryId + "' ";
            string projectId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_name from Projects where projectId = '" + projectId + "' ";
            string project_name = cmd.ExecuteScalar().ToString();
            connect.Close();
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your sprint task#(" + txtUniqueSprintTaskID.Text + ") has been successfully updated for the project (" + project_name + ") under the user story#(" + txtUniqueUserStoryID.Text + ").\n" +
                "\n\nBest regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
            email.sendEmail(emailTo, messageBody);
            //Email every developer who has been added to this user story:
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                string temp_id = usersToAdd.ElementAt(i);
                connect.Open();
                cmd.CommandText = "select user_email from Users where userId like '" + temp_id + "' ";
                string temp_emailTo = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId like '" + temp_id + "' ";
                string temp_name = cmd.ExecuteScalar().ToString();
                connect.Close();
                string temp_messageBody = "Hello " + temp_name + ",\nThis email is to notify you that you have been added to the list of developers in sprint task#(" + txtUniqueSprintTaskID.Text + ") for the project (" + project_name + ") under the user story#(" + txtUniqueUserStoryID.Text + ").\n" +
                "\n\nBest regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                email.sendEmail(temp_emailTo, temp_messageBody);
            }
        }
        protected void addNewEntry()
        {
            int hasImage = 0;
            if (files.Count > 0)
            {
                storeImagesInServer();
                hasImage = 1;
            }
            //Store new sprint task as neither approved nor denied and return its ID:
            string sprintTaskId = storeSprintTask(hasImage);
            updateAllSubEntries(g_sprintTaskId, sprintTaskId);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessSprintTask(sprintTaskId);
            storeImagesInDB(sprintTaskId, hasImage, files);
            lblAddSprintTaskMessage.Visible = true;
            lblAddSprintTaskMessage.ForeColor = System.Drawing.Color.Green;
            lblAddSprintTaskMessage.Text = "The sprint task has been successfully updated and an email notification has been sent to you. <br/>";
            wait.Visible = false;
        }
        protected void updateAllSubEntries(string oldId, string newId)
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //Search for everything related and update the ID:
            cmd.CommandText = "update TestCases set sprintTaskId = '" + newId + "' where sprintTaskId = '" + oldId + "'  ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected void allowUserAccessSprintTask(string sprintTaskId)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Delete all the users assigned for this project:
            cmd.CommandText = "delete from UsersForSprintTasks where sprintTaskId = '" + sprintTaskId + "' ";
            cmd.ExecuteScalar();
            //Add the list of selected developers to the sprint task:
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                string developerResponsible_userId = usersToAdd.ElementAt(i);
                cmd.CommandText = "insert into UsersForSprintTasks (userId, sprintTaskId, usersForSprintTasks_isNotified) values " +
                    "('" + developerResponsible_userId + "', '" + sprintTaskId + "', '0')";
                cmd.ExecuteScalar();
            }
            connect.Close();
        }
        protected string storeSprintTask(int hasImage)
        {
            string sprintTaskId = "";
            DateTime createdDate = DateTime.Now;
            string sprintTaskUId = txtUniqueSprintTaskID.Text.Replace(" ", "");
            sprintTaskUId = txtUniqueSprintTaskID.Text.Replace("'", "''");
            string userStoryUId = txtUniqueUserStoryID.Text.Replace(" ", "");
            userStoryUId = txtUniqueUserStoryID.Text.Replace("'", "''");
            string taskDescription = txtTaskDescription.Text.Replace("'", "''");
            string dateIntroduced = calDateIntroduced.SelectedDate.ToString();
            string dateConsidered = calDateConsidered.SelectedDate.ToString();
            string currentStatus = drpCurrentStatus.SelectedValue;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string createdBy = cmd.ExecuteScalar().ToString();
            //Mark to original Sprint Task as deleted:
            cmd.CommandText = "update SprintTasks set sprintTask_currentStatus = 'Revised', sprintTask_editedBy = '" + createdBy + "', " +
                "sprintTask_editedDate = '" + createdDate + "', sprintTask_isDeleted = '1'  " +
                "where sprintTaskId = '" + g_sprintTaskId + "' ";
            cmd.ExecuteScalar();
            //Store the new user story in the database:
            cmd.CommandText = "insert into SprintTasks (userStoryId, sprintTask_createdBy, sprintTask_createdDate, sprintTask_uniqueId, sprintTask_taskDescription, sprintTask_dateIntroduced, " +
                "sprintTask_dateConsideredForImplementation, sprintTask_hasImage, sprintTask_currentStatus, sprintTask_previousVersion) values " +
               "('" + g_userStoryId + "', '" + createdBy + "', '" + createdDate + "', '" + sprintTaskUId + "', '" + taskDescription + "', '" + dateIntroduced + "', '" + dateConsidered + "', " +
               " '" + hasImage + "',  '" + currentStatus + "', '"+g_sprintTaskId+"') ";
            cmd.ExecuteScalar();
            //Get the ID of the newly stored Sprint task from the database:
            cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] " +
                "where userStoryId = '" + g_userStoryId + "' and sprintTask_createdBy = '" + createdBy + "' and sprintTask_createdDate = '" + Layouts.getOriginalTimeFormat(createdDate.ToString()) + "' "
                + " and sprintTask_uniqueId like '" + sprintTaskUId + "'  "
                + " and sprintTask_dateIntroduced = '" + Layouts.getOriginalTimeFormat(dateIntroduced.ToString()) + "' "
                + " and sprintTask_dateConsideredForImplementation = '" + Layouts.getOriginalTimeFormat(dateConsidered.ToString()) + "' "
                + " and sprintTask_hasImage = '" + hasImage + "' and sprintTask_currentStatus like '" + currentStatus + "' "
                + " and sprintTask_isDeleted = '0' "
                + " ) as t where rowNum = '1'";
            sprintTaskId = cmd.ExecuteScalar().ToString();
            connect.Close();
            return sprintTaskId;
        }
        protected void storeImagesInDB(string sprintTaskId, int hasImage, List<HttpPostedFile> files)
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
                    cmd.CommandText = "insert into ImagesForSprintTasks (imageId, sprintTaskId) values ('" + imageId + "', '" + sprintTaskId + "')";
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
        protected void btnGoBackToListOfSprintTasks_Click(object sender, EventArgs e)
        {
            addSession();
            goBack();
        }
        protected void btnAddUserToList_Click(object sender, EventArgs e)
        {
            SqlCommand cmd = connect.CreateCommand();
            lblListOfUsers.Text = "Added to the list:<br/>";
            int userIndex = drpFindUser.SelectedIndex;
            string selectedDeveloper_fullname = drpFindUser.SelectedValue;
            var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' };
            selectedDeveloper_fullname = selectedDeveloper_fullname.TrimStart(digits);
            string developerResponsible_userId = searchedUsers[userIndex].ToString();
            usersToAdd.Add(developerResponsible_userId);
            connect.Open();
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where userId = '" + usersToAdd.ElementAt(i) + "' ";
                string temp_name = cmd.ExecuteScalar().ToString();
                lblListOfUsers.Text += temp_name + "<br/>";
            }
            connect.Close();
            lblListOfUsers.Visible = true;
        }
        protected bool correctInput()
        {
            bool correct = true;
            if (string.IsNullOrWhiteSpace(txtTaskDescription.Text))
            {
                correct = false;
                lblTaskDescriptionError.Text = "Invalid input: Please type something for \"Task description\" .";
                lblTaskDescriptionError.Visible = true;
            }
            //Check the start date of the project:
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select project_startedDate from Projects where projectId = '" + g_projectId + "' ";
            DateTime project_startedDate = DateTime.Parse(cmd.ExecuteScalar().ToString());
            cmd.CommandText = "select userStory_dateIntroduced from UserStories where userStoryId = '" + g_userStoryId + "' ";
            DateTime userStory_dateIntroduced = DateTime.Parse(cmd.ExecuteScalar().ToString());
            cmd.CommandText = "select userStory_dateConsideredForImplementation from UserStories where userStoryId = '" + g_userStoryId + "' ";
            DateTime userStory_dateConsideredForImplementation = DateTime.Parse(cmd.ExecuteScalar().ToString());
            connect.Close();
            int differenceInDaysFromNow = (calDateIntroduced.SelectedDate - DateTime.Now).Days;
            int differenceFromProjectStartDateToDateIntroduced = (calDateIntroduced.SelectedDate - project_startedDate).Days;
            int differenceFromProjectStartDateToDateConsidered = (calDateConsidered.SelectedDate - project_startedDate).Days;
            int differenceFromUserStoryDateConsideredToSprintTaskDateConsidered = (calDateConsidered.SelectedDate - userStory_dateConsideredForImplementation).Days;
            int differenceFromUserStoryDateIntroducedDateToSprintTaskDateIntroduced = (calDateIntroduced.SelectedDate - userStory_dateIntroduced).Days;
            if (differenceFromProjectStartDateToDateConsidered < 0)
            {
                correct = false;
                lblDateConsideredError.Visible = true;
                lblDateConsideredError.Text = "Invalid input: Please select a date starting after the project's start date (" + Layouts.getTimeFormat(project_startedDate.ToString()) + ").";
            }
            if (differenceFromProjectStartDateToDateIntroduced < 0)
            {
                correct = false;
                lblDateConsideredError.Visible = true;
                lblDateConsideredError.Text = "Please wait until the project's start date (" + Layouts.getTimeFormat(project_startedDate.ToString()) + ").";
            }
            if (differenceFromProjectStartDateToDateIntroduced > 0 && differenceFromProjectStartDateToDateConsidered > 0)
            {
                if (differenceFromUserStoryDateConsideredToSprintTaskDateConsidered < 0)
                {
                    correct = false;
                    lblDateConsideredError.Visible = true;
                    lblDateConsideredError.Text = "Invalid input: Please select a date starting after the user story's considered date (" + Layouts.getTimeFormat(userStory_dateConsideredForImplementation.ToString()) + ").";
                }
                if (differenceFromUserStoryDateIntroducedDateToSprintTaskDateIntroduced < 0)
                {
                    correct = false;
                    lblDateConsideredError.Visible = true;
                    lblDateConsideredError.Text = "Please wait until the user story's start date (" + Layouts.getTimeFormat(userStory_dateIntroduced.ToString()) + ").";
                }
                if (differenceFromUserStoryDateConsideredToSprintTaskDateConsidered > 0 && differenceFromUserStoryDateIntroducedDateToSprintTaskDateIntroduced > 0)
                {
                    if (differenceInDaysFromNow < 0)
                    {
                        correct = false;
                        lblDateIntroducedError.Visible = true;
                        lblDateIntroducedError.Text = "Invalid input: Please select a date starting from now.";
                    }
                    differenceInDaysFromNow = (calDateConsidered.SelectedDate - DateTime.Now).Days;
                    if (differenceInDaysFromNow < 0)
                    {
                        correct = false;
                        lblDateConsideredError.Visible = true;
                        lblDateConsideredError.Text = "Invalid input: Please select a date starting from now.";
                    }
                }
            }
            if (drpCurrentStatus.SelectedIndex == 0)
            {
                correct = false;
                lblCurrentStatusError.Visible = true;
                lblCurrentStatusError.Text = "Invalid input: Please select a status for this sprint task.";
            }
            if (usersToAdd.Count == 0)
            {
                correct = false;
                lblDeveloperResponsibleError.Visible = true;
                lblDeveloperResponsibleError.Text = "Invalid input: Please add at least one developer.";
            }
            return correct;
        }
        protected void btnGoBack_Click(object sender, EventArgs e)
        {
            addSession();
            //Response.Redirect("Home");
            goBack();
        }
    }
}