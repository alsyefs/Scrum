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

namespace Scrum.Accounts.Master
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
                fillInputs();
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
                //If the user in the list is not me, add him/her to the set:
                if (int_loginId != temp_loginId)
                {
                    usersToAdd.Add(temp_userId);
                }
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
            //Check if user is the creator of this user story:
            cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + userStoryId + "' ";
            int userStory_creatorId = Convert.ToInt32(cmd.ExecuteScalar());
            int int_roleId = Convert.ToInt32(roleId);
            //If the user trying to edit is not the creator and not an admin and not the user story creator: 
            //(Note, the creator of the project must be a master)
            if (userId != project_creatorId && userId != userStory_creatorId && int_roleId != 1)
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
            Boolean correct = checkInput();
            if (correct)
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
        protected void allowUserAccessUserStory(string userStoryId)
        {
            DateTime currentTime = DateTime.Now;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Delete all the users assigned for this project:
            cmd.CommandText = "delete from UsersForUserStories where userStoryId = '" + userStoryId + "' ";
            cmd.ExecuteScalar();
            //Note: by adding the user to this table, he/she is given access to the newly-created table.
            //Add the creator to UsersForProjects table: 
            cmd.CommandText = "insert into UsersForUserStories (userId, userStoryId, usersForUserStories_isNotified) values " +
                "('" + userId + "', '" + userStoryId + "', '" + 0 + "')";
            cmd.ExecuteScalar();
            //Add the list of selected developers to the user story:
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                string developerResponsible_userId = usersToAdd.ElementAt(i);
                cmd.CommandText = "insert into UsersForUserStories (userId, userStoryId, usersForUserStories_isNotified) values " +
                    "('" + developerResponsible_userId + "', '" + userStoryId + "', '" + 0 + "')";
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
            //Store new topic as neither approved nor denied and return its ID:
            string new_userStoryId = storeUserStory(hasImage);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessUserStory(new_userStoryId);
            //storeImagesInDB(projectId, hasImage, files);
            lblError.Visible = true;
            lblError.ForeColor = System.Drawing.Color.Green;
            lblError.Text = "The user story has been successfully updated and an email notification has been sent to you. <br/>";
            wait.Visible = false;
        }
        protected string storeUserStory(int hasImage)
        {
            string newUserStoryId = userStoryId;
            DateTime entryTime = DateTime.Now;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
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
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "update UserStories set userStory_asARole = '" + asARole + "', userStory_iWantTo ='" + iWant + "', userStory_soThat = '"+soThat+"'," +
                "userStory_currentStatus = '"+currentStatus+ "' where userStoryId = '" + userStoryId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
            return newUserStoryId;
        }
        protected Boolean checkInput()
        {
            Boolean correct = true;
            //Check for blank title:
            if (string.IsNullOrWhiteSpace(txtIWantTo.Text))
            {
                correct = false;
                lblIWantToError.Visible = true;
                lblIWantToError.Text = "Input Error: Please type something for \"I want to\".";
            }
            //Check for blank description:
            if (string.IsNullOrWhiteSpace(txtSoThat.Text))
            {
                correct = false;
                lblSoThatError.Visible = true;
                lblSoThatError.Text = "Input Error: Please type something for \"So that\".";
            }
            if(drpCurrentStatus.SelectedIndex == 0)
            {
                correct = false;
                lblCurrentStatusError.Visible = true;
                lblCurrentStatusError.Text = "Invalid input: Please select a status.";
            }
            if (drpAsRole.SelectedIndex == 0)
            {
                correct = false;
                lblAsRoleError.Text = "Invalid input: Please select a role.";
                lblAsRoleError.Visible = true;
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
                //add the searched user if his/her account is active, and the user is not the current user searching, and this user is a member of the project:
                if (temp_isActive == 1 && int_loginId != temp_loginId && isMemberOfProject == 1)
                {
                    searchedUsers.Add(temp_userId);
                    drpFindUser.Items.Add(++counter + " " + temp_user);
                }

            }
            connect.Close();
        }
    }
}