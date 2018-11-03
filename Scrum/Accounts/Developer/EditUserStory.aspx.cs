using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Developer
{
    public partial class EditUserStory : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        string previousPage = "";
        static string userStoryId = "";
        static string g_projectId = "";
        static ArrayList searchedUsers = new ArrayList();
        static SortedSet<string> usersToAdd = new SortedSet<string>();
        static List<HttpPostedFile> files;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                if (!string.IsNullOrWhiteSpace(Request.QueryString["userStoryId"]))
                {
                    userStoryId = Request.QueryString["userStoryId"];
                    if (!check.checkUserStoryAccess(userStoryId, loginId))
                        goBack();
                }
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
            Session.Add("userStoryId", userStoryId);
            updateUniqueId();
            checkIfUserStoryDeleted();
        }
        protected void checkIfUserStoryDeleted()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //Count the existance of the user story:
            cmd.CommandText = "select count(*) from UserStories where userStoryId = '" + userStoryId + "' ";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count > 0)//if count > 0, then the project ID exists in DB.
            {
                cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + userStoryId + "' ";
                string actual_creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select userStory_isDeleted from UserStories where userStoryId = '" + userStoryId + "' ";
                int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (isDeleted == 1)
                {
                    //Hide the submit buttons:
                    btnAddUserToList.Visible = false;
                    btnUpload.Visible = false;
                    btnRemoveUserStoryUser.Visible = false;
                    btnSubmit.Visible = false;
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
        protected void fillInputs()
        {
            usersToAdd.Clear();
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select userStory_asARole from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_asARole = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_iWantTo from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_iWantTo = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_soThat from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_soThat = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_currentStatus from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_currentStatus = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_asARole from UserStories where userStoryId = '" + userStoryId + "' ";
            string notParsed_roles = cmd.ExecuteScalar().ToString().Replace(" ", "");
            cmd.CommandText = "select count(*) from UsersForUserStories where userStoryId = '" + userStoryId + "' ";
            int countUsersForUserStories = Convert.ToInt32(cmd.ExecuteScalar());
            for (int i = 1; i <= countUsersForUserStories; i++)
            {
                //Add the user IDs of the user stories users:
                cmd.CommandText = "select[userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY UsersForUserStoriesId ASC), * FROM [UsersForUserStories] where userStoryId = '" + userStoryId + "' ) as t where rowNum = '" + i + "'";
                string temp_userId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select loginId from Users where userId = '" + temp_userId + "' ";
                int temp_loginId = Convert.ToInt32(cmd.ExecuteScalar());
                int int_loginId = Convert.ToInt32(loginId);
                ////If the user in the list is not me, add him/her to the set:
                //if (int_loginId != temp_loginId)
                //{
                    usersToAdd.Add(temp_userId);
                //}
            }
            //The second is to guarantee that the sorted set has been filled without duplicates, then we fill the below list:
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                string temp_userId = usersToAdd.ElementAt(i);
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + temp_userId + "'  ";
                string temp_user_name = cmd.ExecuteScalar().ToString();
                drpUserStoryUsers.Items.Add(temp_user_name);
            }
            connect.Close();
            List<string> roles = notParsed_roles.Split(',').ToList<string>();
            for (int i = 0; i< drpAsRole.Items.Count; i++)
            {
                string temp = drpAsRole.Items[i].ToString();
                if (roles.Contains(temp))
                {
                    drpAsRole.Items[i].Selected = true;
                    //drpAsRole.SelectedIndex = i;
                }
            }
            txtIWantTo.Text = userStory_iWantTo;
            txtSoThat.Text = userStory_soThat;
            ListItem selectedListItem = drpCurrentStatus.Items.FindByValue(userStory_currentStatus);
            if (selectedListItem != null)
            {
                selectedListItem.Selected = true;
            }
            //drpCurrentStatus.SelectedValue = userStory_currentStatus;
            calDateIntroduced.SelectedDate = DateTime.Now;
            calDateConsidered.SelectedDate = DateTime.Now;
        }
        protected void drpProjectUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        protected void btnRemoveUserStoryUser_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = drpUserStoryUsers.Items.Count; i >= 0; i--)
                {
                    int indexToRemove = drpUserStoryUsers.SelectedIndex;
                    if (indexToRemove > -1)
                    {
                        drpUserStoryUsers.Items.RemoveAt(indexToRemove);
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
            ////Check if user is the creator of this user story:
            //cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + userStoryId + "' ";
            //int userStory_creatorId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(userId) from usersForUserStories where userId = '"+userId+ "' and userStoryId = '" + userStoryId + "' ";
            int isMember = Convert.ToInt32(cmd.ExecuteScalar());
            int int_roleId = Convert.ToInt32(roleId);
            //If the user trying to edit is not the project creator, and not an admin, and he/she is not a member of the user story: 
            //(Note, the creator of the project must be a master)
            if (userId != project_creatorId && int_roleId != 1 && isMember == 0)
                allowed = false;
            connect.Close();
            return allowed;
        }
        protected void goBack()
        {
            addSession();
            //if (!string.IsNullOrWhiteSpace(previousPage)) Response.Redirect(previousPage);
            //else Response.Redirect("Home.aspx");
            Response.Redirect("ViewUserStory.aspx?id=" + userStoryId);
        }
        protected void initialAccess()
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
            try
            {
                Session.Add("projectId", g_projectId);
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
            }catch(Exception e)
            {
                Console.WriteLine("Error: "+e);
            }
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            hideErrorLabels();
            if (correctInput())
            {
                addNewEntry();
                clearInputs();
                sendEmail();
            }
        }
        protected void clearInputs()
        {
            txtIWantTo.Text = "";
            txtSoThat.Text = "";
            drpAsRole.SelectedIndex = 0;
            drpCurrentStatus.SelectedIndex = 0;
        }
        protected void allowUserAccessUserStory(string newUserStoryId)
        {
            DateTime currentTime = DateTime.Now;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Delete all the users assigned for this project:
            cmd.CommandText = "delete from UsersForUserStories where userStoryId = '" + newUserStoryId + "' ";
            cmd.ExecuteScalar();
            //Note: by adding the user to this table, he/she is given access to the newly-created table.
            ////Add the creator to UsersForProjects table: 
            //cmd.CommandText = "insert into UsersForUserStories (userId, userStoryId, usersForUserStories_isNotified) values " +
            //    "('" + userId + "', '" + newUserStoryId + "', '" + 0 + "')";
            //cmd.ExecuteScalar();
            //Add the list of selected developers to the user story:
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                string developerResponsible_userId = usersToAdd.ElementAt(i);
                cmd.CommandText = "insert into UsersForUserStories (userId, userStoryId, usersForUserStories_isNotified) values " +
                    "('" + developerResponsible_userId + "', '" + newUserStoryId + "', '" + 0 + "')";
                cmd.ExecuteScalar();
            }
            connect.Close();
        }
        protected void sendEmail()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select user_email from Users where userId like '" + userId + "' ";
            string emailTo = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId like '" + userId + "' ";
            string name = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_name from Projects where projectId = '" + g_projectId+"' ";
            string project_title = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStoryUID = cmd.ExecuteScalar().ToString();
            connect.Close();
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your user story#("+ userStoryUID + ") in the project (" + project_title + ") has been successfully updated.\n" +
                "\n\nBest regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
            Email email = new Email();
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
            //Store new topic as neither approved nor denied and return its ID:
            string new_userStoryId = storeUserStory(hasImage);
            updateAllSubEntries(userStoryId, new_userStoryId);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessUserStory(new_userStoryId);
            storeImagesInDB(new_userStoryId, hasImage, files);
            lblError.Visible = true;
            lblError.ForeColor = System.Drawing.Color.Green;
            lblError.Text = "The user story has been successfully updated and an email notification has been sent to you. <br/>";
            wait.Visible = false;
        }
        protected void updateAllSubEntries(string oldUserStoryId, string newUserStoryId)
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //Search for everything related to the old user story ID and change its ID to link to the new user story ID:
            cmd.CommandText = "update SprintTasks set userStoryId = '"+newUserStoryId+"' where userStoryId = '"+oldUserStoryId+"'  ";
            cmd.ExecuteScalar();
            //The test cases will still be linked to the same sprint tasks.
            connect.Close();
        }
        protected void storeImagesInDB(string userStoryId, int hasImage, List<HttpPostedFile> files)
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
                    cmd.CommandText = "insert into ImagesForUserStories (imageId, userStoryId) values ('" + imageId + "', '" + userStoryId + "')";
                    cmd.ExecuteScalar();
                }
            }
            connect.Close();
        }
        protected string storeUserStory(int hasImage)
        {
            string newUserStoryId = "";
            DateTime entryTime = DateTime.Now;
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            string newUniqueId = txtUniqueUserStoryID.Text.Replace(" ", "");
            newUniqueId = txtUniqueUserStoryID.Text.Replace("'", "''");
            string asARole = "";
            int counter = 0;
            foreach (ListItem listItem in drpAsRole.Items)
            {
                if (listItem.Selected)
                {
                    counter++;
                    var val = listItem.Value;
                    var txt = listItem.Text;
                    if (counter == 1)
                        asARole += val.ToString();
                    else
                        asARole = asARole + ", " + val.ToString();
                }
            }
            string iWant = txtIWantTo.Text.Replace("'", "''");
            string soThat = txtSoThat.Text.Replace("'", "''");
            string currentStatus = drpCurrentStatus.SelectedValue;
            string dateIntroduced = calDateIntroduced.SelectedDate.ToString();
            string dateConsidered = calDateConsidered.SelectedDate.ToString();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Mark to original User Story as deleted:
            cmd.CommandText = "update UserStories set userStory_currentStatus = 'Revised', userStory_editedBy = '"+userId+"', " +
                "userStory_editedDate = '"+entryTime+ "', userStory_isDeleted = '1'  " +
                "where userStoryId = '" + userStoryId + "' ";
            cmd.ExecuteScalar();
            //Add a new user story with another unique ID according to the format x.x:
            cmd.CommandText = "insert into UserStories (projectId, userStory_createdBy, userStory_createdDate, userStory_uniqueId, userStory_asARole, userStory_iWantTo, " +
                "userStory_soThat, userStory_dateIntroduced, userStory_dateConsideredForImplementation," +
                " userStory_hasImage, userStory_currentStatus, userStory_previousVersion) values " +
               "('" + g_projectId + "', '" + userId + "', '" + entryTime + "', '" + newUniqueId + "', '" + asARole + "', '" + iWant + "', '" + soThat + "', '" + dateIntroduced + "'," +
               " '" + dateConsidered + "',  '" + hasImage + "', '" + currentStatus + "', '"+ userStoryId + "') ";
            cmd.ExecuteScalar();
            //Get the ID of the newly stored User Story from the database:
            cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] " +
                "where projectId = '" + g_projectId + "' and userStory_createdBy = '" + userId + "' and userStory_createdDate = '" + Layouts.getOriginalTimeFormat(entryTime.ToString()) + "' "
                + " and userStory_asARole like '" + asARole + "' and userStory_iWantTo like '" + iWant + "' and userStory_soThat like '" + soThat + "' "
                + " and userStory_dateIntroduced = '" + Layouts.getOriginalTimeFormat(dateIntroduced.ToString()) + "' "
                + " and userStory_dateConsideredForImplementation = '" + Layouts.getOriginalTimeFormat(dateConsidered.ToString()) + "' "
                + " and userStory_hasImage = '" + hasImage + "' and userStory_currentStatus like '" + currentStatus + "' "
                + " and userStory_isDeleted = '0' "
                + " ) as t where rowNum = '1'";
            newUserStoryId = cmd.ExecuteScalar().ToString();
            cmd.ExecuteScalar();
            connect.Close();
            return newUserStoryId;
        }
        protected void updateUniqueId()
        {
            string newId = "";
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();
                cmd.CommandText = "select top(1) userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' order by userStoryId desc ";
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
                    //++tempId;
                    newId = tempId.ToString() + ".1";
                    newId = newId.Replace(" ", "");
                }
            }
            catch (Exception e)
            {
                newId = "1";
                Console.WriteLine("Error: " + e);
            }
            txtUniqueUserStoryID.Text = newId;
            //drpCurrentStatus.Enabled = false;
            //drpCurrentStatus.SelectedIndex = 1;
        }
        protected bool correctInput()
        {
            bool correct = true;
            if (drpAsRole.SelectedIndex == 0)
            {
                correct = false;
                lblAsRoleError.Text = "Invalid input: Please select a role.";
                lblAsRoleError.Visible = true;
            }
            if (string.IsNullOrEmpty(txtIWantTo.Text))
            {
                correct = false;
                lblIWantToError.Visible = true;
                lblIWantToError.Text = "Invalid input: Please type something for \"I want to...\" ";
            }
            if (string.IsNullOrWhiteSpace(txtSoThat.Text))
            {
                correct = false;
                lblSoThatError.Visible = true;
                lblSoThatError.Text = "Invalid input: Please type something for \"So that ...\" ";
            }
            //Check the start date of the project:
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select project_startedDate from Projects where projectId = '" + g_projectId + "' ";
            DateTime project_startedDate = DateTime.Parse(cmd.ExecuteScalar().ToString());
            connect.Close();
            int differenceInDays = (calDateIntroduced.SelectedDate - DateTime.Now).Days;
            int differenceFromProjectStartDateToDateIntroduced = (calDateIntroduced.SelectedDate - project_startedDate).Days;
            int differenceFromProjectStartDateToDateConsidered = (calDateConsidered.SelectedDate - project_startedDate).Days;
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
                if (differenceInDays < 0)
                {
                    correct = false;
                    lblDateIntroducedError.Visible = true;
                    lblDateIntroducedError.Text = "Invalid input: Please select a date starting from now.";
                }
                differenceInDays = (calDateConsidered.SelectedDate - DateTime.Now).Days;
                if (differenceInDays < 0)
                {
                    correct = false;
                    lblDateConsideredError.Visible = true;
                    lblDateConsideredError.Text = "Invalid input: Please select a date starting from now.";
                }
            }
            if (drpCurrentStatus.SelectedIndex == 0)
            {
                correct = false;
                lblCurrentStatusError.Visible = true;
                lblCurrentStatusError.Text = "Invalid input: Please select a status for this user story.";
            }
            if (usersToAdd.Count == 0)
            {
                correct = false;
                lblDeveloperResponsibleError.Visible = true;
                lblDeveloperResponsibleError.Text = "Invalid input: Please add at least one developer.";
            }
            return correct;
        }
        protected void hideErrorLabels()
        {
            lblAsRoleError.Visible = false;
            lblCurrentStatusError.Visible = false;
            lblSoThatError.Visible = false;
            lblError.Visible = false;
            lblIWantToError.Visible = false;
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            addSession();
            goBack();
            //Response.Redirect("Home");
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
            //This is to clear the list of users, and then fill it later from a set to avoid duplicates:
            drpUserStoryUsers.Items.Clear();
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where userId = '" + usersToAdd.ElementAt(i) + "' ";
                string temp_name = cmd.ExecuteScalar().ToString();
                lblListOfUsers.Text += temp_name + "<br/>";
                //Fill the list
                drpUserStoryUsers.Items.Add(temp_name);
            }
            connect.Close();
            lblListOfUsers.Visible = true;
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
                //Check if the user is part of the this project:
                cmd.CommandText = "select count(*) from UsersForProjects where projectId = '"+g_projectId+"' and userId = '"+temp_userId+"'  ";
                int isMemberOfProject = Convert.ToInt32(cmd.ExecuteScalar());
                //add the searched user if his/her account is active, and this user is a member of the project, //and the user is not the current user searching:
                if (temp_isActive == 1 && isMemberOfProject == 1)// && int_loginId != temp_loginId)
                {
                    searchedUsers.Add(temp_userId);
                    drpFindUser.Items.Add(++counter + " " + temp_user);
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
    }
}