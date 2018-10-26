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
    public partial class ViewProject : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        string previousPage = "";
        string projectId = "";
        static ArrayList searchedUsers = new ArrayList();
        static SortedSet<string> usersToAdd = new SortedSet<string>();
        static List<HttpPostedFile> files;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                projectId = Request.QueryString["id"];
                if (!check.checkProjectAccess(projectId, loginId))
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
                getProjectInfo();
                showView();
            }
            createTable();
            checkIfProjectTerminated();
            //The below to be used whenever needed in the other page. Most likely to be used in ViewUserStory page:
            Session.Add("projectId", projectId);
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
            if (int_roleId != 3)//3 = Developer role.
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
        protected void goBack()
        {
            addSession();
            if (!string.IsNullOrWhiteSpace(previousPage)) Response.Redirect(previousPage);
            else Response.Redirect("Home.aspx");
        }
        protected void getProjectInfo()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select project_name from Projects where projectId = '" + projectId + "' ";
            string name = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_description from Projects where projectId = '" + projectId + "' ";
            string description = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_createdBy from Projects where projectId = '" + projectId + "' ";
            string createdByUserId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_createdDate from Projects where projectId = '" + projectId + "' ";
            string createdDate = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_isTerminated from Projects where projectId = '" + projectId + "' ";
            int isTerminated = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
            int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select project_hasImage from Projects where projectId = '" + projectId + "' ";
            int hasImage = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select project_startedDate from Projects where projectId = '" + projectId + "' ";
            string startDate = cmd.ExecuteScalar().ToString();
            //Convert the createdByUserId to a name:
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + createdByUserId + "' ";
            string createdBy = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            string imagesHTML = "";
            if (hasImage == 1)
            {
                cmd.CommandText = "select count(*) from ImagesForProjects where projectId = '" + projectId + "' ";
                int totalImages = Convert.ToInt32(cmd.ExecuteScalar());
                for (int i = 1; i <= totalImages; i++)
                {
                    cmd.CommandText = "select [imageId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY imageId ASC), * FROM [ImagesForProjects] where projectId = '" + projectId + "') as t where rowNum = '" + i + "'";
                    string imageId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select image_name from Images where imageId = '" + imageId + "' ";
                    string image_name = cmd.ExecuteScalar().ToString();
                    //imagesHTML = imagesHTML + "<img src='../../images/" + image_name + "'></img> <br />";
                    imagesHTML = imagesHTML + "<a href='../../images/" + image_name + "' target=\"_blank\">" + image_name + "</a> <br />";
                }
            }
            //Construct an HTML output to post it:
            lblProjectInfo.Text = Layouts.projectHeader(projectId, roleId, loginId, userId, name, description,
                createdByUserId, createdBy, createdDate, startDate, isTerminated, isDeleted, hasImage, imagesHTML);
            connect.Close();
        }
        protected void showView()
        {
            View.Visible = true;
            AddNewUserStory.Visible = false;
        }
        protected void showNewUserStory()
        {
            View.Visible = false;
            AddNewUserStory.Visible = true;
        }
        protected void checkIfProjectTerminated()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //Count the existance of the topic:
            cmd.CommandText = "select count(*) from Projects where projectId = '" + projectId + "' ";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count > 0)//if count > 0, then the project ID exists in DB.
            {
                cmd.CommandText = "select project_createdBy from Projects where projectId = '" + projectId + "' ";
                string actual_creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
                int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select project_isTerminated from Projects where projectId = '" + projectId + "' ";
                int isTerminated = Convert.ToInt32(cmd.ExecuteScalar());
                if (isTerminated == 1 || isDeleted == 1)
                {
                    //Hide the submit buttons:
                    btnAddNewUserStory.Visible = false;
                    btnAddUserToList.Visible = false;
                    btnSaveUserStory.Visible = false;
                    btnUpload.Visible = false;
                }
            }
            connect.Close();
        }
        protected void rebindValues()
        {
            int int_roleId = Convert.ToInt32(roleId);
            if (grdUserStories.Rows.Count > 0)
            {
                //Hide the headers called "User ID" and "User Story ID":
                grdUserStories.HeaderRow.Cells[8].Visible = false;
                grdUserStories.HeaderRow.Cells[9].Visible = false;
                //Hide IDs column and content which are located in column index 8:
                for (int i = 0; i < grdUserStories.Rows.Count; i++)
                {
                    grdUserStories.Rows[i].Cells[8].Visible = false;
                    grdUserStories.Rows[i].Cells[9].Visible = false;
                }
                //if (int_roleId == 3)//3: Developer
                //{
                //    //Hide the header for removing the User Story commands:
                //    grdUserStories.HeaderRow.Cells[10].Visible = false;
                //    for (int i = 0; i < grdUserStories.Rows.Count; i++)
                //    {
                //        grdUserStories.Rows[i].Cells[10].Visible = false;
                //    }
                //}
            }
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            string userStoryId = "", asARole = "", iWant = "", soThat = "", dateIntroduced = "", dateConsidered = "",
                   developersResponsible = "", currentStatus = "";
            string creatorId = "", db_userStoryId = "";
            for (int row = 0; row < grdUserStories.Rows.Count; row++)
            {
                //Set links to review a user:
                userStoryId = grdUserStories.Rows[row].Cells[0].Text;
                asARole = grdUserStories.Rows[row].Cells[1].Text;
                iWant = grdUserStories.Rows[row].Cells[2].Text;
                soThat = grdUserStories.Rows[row].Cells[3].Text;
                dateIntroduced = grdUserStories.Rows[row].Cells[4].Text;
                dateConsidered = grdUserStories.Rows[row].Cells[5].Text;
                developersResponsible = grdUserStories.Rows[row].Cells[6].Text;
                currentStatus = grdUserStories.Rows[row].Cells[7].Text;
                creatorId = grdUserStories.Rows[row].Cells[8].Text;
                db_userStoryId = grdUserStories.Rows[row].Cells[9].Text;
                string removeUserStoryCommand = grdUserStories.Rows[row].Cells[10].Text;
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
                    grdUserStories.Rows[row].Cells[6].Controls.Add(participantLink);
                    if (usersForUserStory > 1)
                    {
                        HyperLink temp = new HyperLink();
                        temp.Text = "<br/>";
                        grdUserStories.Rows[row].Cells[6].Controls.Add(temp);
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
                removeUserStoryLink.CommandArgument = Convert.ToString(row + 1);
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
                grdUserStories.Rows[row].Cells[0].Controls.Add(idLink);
                grdUserStories.Rows[row].Cells[1].Controls.Add(asARoleLink);
                grdUserStories.Rows[row].Cells[2].Controls.Add(iWantLink);
                grdUserStories.Rows[row].Cells[3].Controls.Add(soThatLink);
                grdUserStories.Rows[row].Cells[4].Controls.Add(dateIntroducedLink);
                grdUserStories.Rows[row].Cells[5].Controls.Add(dateConsideredLink);
                grdUserStories.Rows[row].Cells[7].Controls.Add(currentStatusLink);
                grdUserStories.Rows[row].Cells[10].Controls.Add(removeUserStoryLink);
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
        protected int getTotalUserStories()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count the not-approved users:
            cmd.CommandText = "select count(*) from [UserStories] where projectId = '" + projectId + "' ";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return count;
        }
        protected void createTable()
        {
            int countUserStories = getTotalUserStories();
            if (countUserStories == 0)
            {
                lblMessage.Visible = true;
            }
            else if (countUserStories > 0)
            {
                lblMessage.Visible = false;
                DataTable dt = new DataTable();
                dt.Columns.Add("User story ID", typeof(string));
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
                for (int i = 1; i <= countUserStories; i++)
                {
                    //Get the project ID:
                    cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] where projectId = '" + projectId + "' ) as t where rowNum = '" + i + "'";
                    id = cmd.ExecuteScalar().ToString();
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
                grdUserStories.DataSource = dt;
                grdUserStories.DataBind();
                rebindValues();
            }
        }
        protected void grdUserStories_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdUserStories.PageIndex = e.NewPageIndex;
            grdUserStories.DataBind();
            rebindValues();
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
        protected void btnAddNewUserStory_Click(object sender, EventArgs e)
        {
            showNewUserStory();
            //count the number of the current stories, then add one to the total
            //So the new user story Unique ID will the resulting number:
            updateUniqueId();
            clearNewUserStoryInputs();
            hideErrorLabels();
        }
        protected void updateUniqueId()
        {
            string newId = "";
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();
                cmd.CommandText = "select top(1) userStory_uniqueId from UserStories where projectId = '" + projectId + "' order by userStoryId desc ";
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
                    int result = Convert.ToInt32(theId.ElementAt(0)) + tempId;
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
            txtUniqueUserStoryID.Text = newId;
        }
        protected void clearNewUserStoryInputs()
        {
            try
            {
                drpAsRole.SelectedIndex = 0;
                txtIWantTo.Text = "";
                txtSoThat.Text = "";
                //calDateConsidered.SelectedDate = DateTime.Now;
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
            lblAsRoleError.Visible = false;
            lblIWantToError.Visible = false;
            lblSoThatError.Visible = false;
            lblDateConsideredError.Visible = false;
            lblDateIntroducedError.Visible = false;
            lblDeveloperResponsibleError.Visible = false;
            lblCurrentStatusError.Visible = false;
            lblFindUserResult.Visible = false;
            lblAddUserStoryMessage.Visible = false;
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
        protected void btnSaveUserStory_Click(object sender, EventArgs e)
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
                clearNewUserStoryInputs();
            }
            //count the number of the current stories, then add one to the total
            //So the new user story Unique ID will the resulting number:
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
            cmd.CommandText = "select project_name from Projects where projectId = '" + projectId + "' ";
            string project_name = cmd.ExecuteScalar().ToString();
            connect.Close();
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your user story#(" + txtUniqueUserStoryID.Text + ") has been successfully submitted for the project (" + project_name + ").\n" +
                "\n\nBest regards,\nScrum Support\nScrum.UWL@gmail.com";
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
                string temp_messageBody = "Hello " + temp_name + ",\nThis email is to notify you that you have been added to the list of developers in user story#(" + txtUniqueUserStoryID.Text + ") for the project (" + project_name + ").\n" +
                "\n\nBest regards,\nScrum Support\nScrum.UWL@gmail.com";
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
            //Store new topic as neither approved nor denied and return its ID:
            string userStoryId = storeUserStory(hasImage);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessUserStory(userStoryId);
            storeImagesInDB(userStoryId, hasImage, files);
            lblAddUserStoryMessage.Visible = true;
            lblAddUserStoryMessage.ForeColor = System.Drawing.Color.Green;
            lblAddUserStoryMessage.Text = "The user story has been successfully submitted and an email notification has been sent to you. <br/>";
            wait.Visible = false;
        }
        protected void allowUserAccessUserStory(string userStoryId)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Note: by adding the user to this table, he/she is given access to the newly-created table.
            ////Add the creator to UsersForProjects table.
            ////Add the current user to the user story:
            //cmd.CommandText = "insert into UsersForUserStories (userId, userStoryId, usersForUserStories_isNotified) values " +
            //        "('" + userId + "', '" + userStoryId + "', '0')";
            //cmd.ExecuteScalar();
            //Add the list of selected developers to the user story:
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                string developerResponsible_userId = usersToAdd.ElementAt(i);
                cmd.CommandText = "insert into UsersForUserStories (userId, userStoryId, usersForUserStories_isNotified) values " +
                    "('" + developerResponsible_userId + "', '" + userStoryId + "', '0')";
                cmd.ExecuteScalar();
            }
            connect.Close();
        }
        protected string storeUserStory(int hasImage)
        {
            string userStoryId = "";
            DateTime createdDate = DateTime.Now;
            string uniqueId = txtUniqueUserStoryID.Text.Replace(" ", "");
            uniqueId = txtUniqueUserStoryID.Text.Replace("'", "''");
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
            string iWantTo = txtIWantTo.Text.Replace("'", "''");
            string soThat = txtSoThat.Text.Replace("'", "''");
            string dateIntroduced = calDateIntroduced.SelectedDate.ToString();
            string dateConsidered = calDateConsidered.SelectedDate.ToString();
            //string developerResponsible = drpFindUser.SelectedValue;
            string currentStatus = drpCurrentStatus.SelectedValue;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string createdBy = cmd.ExecuteScalar().ToString();
            //Store the new user story in the database:
            cmd.CommandText = "insert into UserStories (projectId, userStory_createdBy, userStory_createdDate, userStory_uniqueId, userStory_asARole, userStory_iWantTo, " +
                "userStory_soThat, userStory_dateIntroduced, userStory_dateConsideredForImplementation," +
                " userStory_hasImage, userStory_currentStatus) values " +
               "('" + projectId + "', '" + createdBy + "', '" + createdDate + "', '" + uniqueId + "', '" + asARole + "', '" + iWantTo + "', '" + soThat + "', '" + dateIntroduced + "'," +
               " '" + dateConsidered + "',  '" + hasImage + "', '" + currentStatus + "') ";
            cmd.ExecuteScalar();
            //Get the ID of the newly stored User Story from the database:
            cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), * FROM [UserStories] " +
                "where projectId = '" + projectId + "' and userStory_createdBy = '" + createdBy + "' and userStory_createdDate = '" + Layouts.getOriginalTimeFormat(createdDate.ToString()) + "' "
                + " and userStory_asARole like '" + asARole + "' and userStory_iWantTo like '" + iWantTo + "' and userStory_soThat like '" + soThat + "' "
                + " and userStory_dateIntroduced = '" + Layouts.getOriginalTimeFormat(dateIntroduced.ToString()) + "' "
                + " and userStory_dateConsideredForImplementation = '" + Layouts.getOriginalTimeFormat(dateConsidered.ToString()) + "' "
                + " and userStory_hasImage = '" + hasImage + "' and userStory_currentStatus like '" + currentStatus + "' "
                + " and userStory_isDeleted = '0' "
                + " ) as t where rowNum = '1'";
            userStoryId = cmd.ExecuteScalar().ToString();
            connect.Close();
            return userStoryId;
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
            cmd.CommandText = "select project_startedDate from Projects where projectId = '" + projectId + "' ";
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
        protected void btnGoBack_Click(object sender, EventArgs e)
        {
            addSession();
            Response.Redirect("Home");
            //goBack();
        }
        protected void btnGoBackToListOfUserStories_Click(object sender, EventArgs e)
        {
            showView();
            createTable();
        }
        protected void grdUserStories_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        [WebMethod]
        [ScriptMethod()]
        public static void terminateProject_Click(string projectId, string entry_creatorId)
        {

            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool projectIdExists = isProjectCorrect(projectId, entry_creatorId);
            if (projectIdExists)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //update the DB and set project_isTerminated = true:
                cmd.CommandText = "update Projects set project_isTerminated = 1 where projectId = '" + projectId + "' ";
                cmd.ExecuteScalar();
                //Email the topic creator about the topic being deleted:
                cmd.CommandText = "select project_createdBy from Projects where projectId = '" + projectId + "' ";
                string creatorId = cmd.ExecuteScalar().ToString();
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
                    "This email is to inform you that your project with the name (" + project_name + ") has been terminated. If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                    "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                Email email = new Email();
                email.sendEmail(emailTo, emailBody);
            }
        }
        protected static bool isProjectCorrect(string projectId, string creatorId)
        {
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            //check if id contains a special character:
            if (!errors.isDigit(projectId))
                correct = false;
            //check if id contains an id that does not exist in DB:
            else if (errors.ContainsSpecialChars(projectId))
                correct = false;
            if (correct)
            {
                Configuration config = new Configuration();
                SqlConnection connect = new SqlConnection(config.getConnectionString());
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();
                //Count the existance of the topic:
                cmd.CommandText = "select count(*) from Projects where projectId = '" + projectId + "' ";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)//if count > 0, then the project ID exists in DB.
                {
                    cmd.CommandText = "select project_createdBy from Projects where projectId = '" + projectId + "' ";
                    string actual_creatorId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select project_isDeleted from Projects where projectId = '" + projectId + "' ";
                    int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.CommandText = "select project_isTerminated from Projects where projectId = '" + projectId + "' ";
                    int isTerminated = Convert.ToInt32(cmd.ExecuteScalar());
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
        public static void removeProject_Click(string projectId, string entry_creatorId)
        {
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool projectIdExists = isProjectCorrect(projectId, entry_creatorId);
            if (projectIdExists)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //update the DB and set isDeleted = true:
                cmd.CommandText = "update Projects set project_isDeleted = 1 where projectId = '" + projectId + "' ";
                cmd.ExecuteScalar();
                //Email the project creator about the project being deleted:
                cmd.CommandText = "select project_createdBy from Projects where projectId = '" + projectId + "' ";
                string creatorId = cmd.ExecuteScalar().ToString();
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
                    "This email is to inform you that your project with the name (" + project_name + ") has been deleted. If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                    "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                Email email = new Email();
                email.sendEmail(emailTo, emailBody);
            }
        }
    }
}