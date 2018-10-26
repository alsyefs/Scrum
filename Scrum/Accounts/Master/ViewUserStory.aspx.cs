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
    public partial class ViewUserStory : System.Web.UI.Page
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
            initialPageAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                userStoryId = Request.QueryString["id"];
                if (!check.checkUserStoryAccess(userStoryId, loginId))
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
                getUserStoryInfo();
                showView();
            }
            createTable();
            updateUniqueId();
            //The below to be used whenever needed in the other page. Most likely to be used in ViewUserStory page:
            Session.Add("projectId", g_projectId);
            Session.Add("userStoryId", userStoryId);
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
                Session.Add("userStoryId", userStoryId);
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
                backLink = "ViewProject.aspx?id=" + g_projectId;
            }
            catch (Exception e)
            {
                backLink = "Home.aspx";
                Console.WriteLine("Error: " + e);
            }
            Response.Redirect(backLink);

        }
        protected void getUserStoryInfo()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + userStoryId + "' ";
            string createdByUserId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_uniqueId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_asARole from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_asARole = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_iWantTo from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_iWant = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_soThat from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_soThat = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_dateIntroduced from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_dateIntroduced = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_dateConsideredForImplementation from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_dateConsideredForImplementation = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userStory_hasImage from UserStories where userStoryId = '" + userStoryId + "' ";
            int userStory_hasImage = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select userStory_isDeleted from UserStories where userStoryId = '" + userStoryId + "' ";
            int userStory_isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select userStory_currentStatus from UserStories where userStoryId = '" + userStoryId + "' ";
            string userStory_currentStatus = cmd.ExecuteScalar().ToString();
            //Convert the createdByUserId to a name:
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + createdByUserId + "' ";
            string createdBy = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            string imagesHTML = "";
            if (userStory_hasImage == 1)
            {
                cmd.CommandText = "select count(*) from ImagesForUserStories where userStoryId = '" + userStoryId + "' ";
                int totalImages = Convert.ToInt32(cmd.ExecuteScalar());
                for (int i = 1; i <= totalImages; i++)
                {
                    cmd.CommandText = "select [imageId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY imageId ASC), * FROM [ImagesForUserStories] where userStoryId = '" + userStoryId + "') as t where rowNum = '" + i + "'";
                    string imageId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select image_name from Images where imageId = '" + imageId + "' ";
                    string image_name = cmd.ExecuteScalar().ToString();
                    //imagesHTML = imagesHTML + "<img src='../../images/" + image_name + "'></img> <br />";
                    imagesHTML = imagesHTML + "<a href='../../images/" + image_name + "' target=\"_blank\">" + image_name + "</a> <br />";
                }
            }
            cmd.CommandText = "select count(*) from UsersForUserStories where userStoryId = '" + userStoryId + "' ";
            int totalUsers = Convert.ToInt32(cmd.ExecuteScalar());
            string[] userStoryMembers = new string[totalUsers];
            for (int i = 1; i <= totalUsers; i++)
            {
                cmd.CommandText = "select [userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY UsersForUserStoriesId ASC), * FROM [UsersForUserStories] where userStoryId = '" + userStoryId + "') as t where rowNum = '" + i + "'";
                userStoryMembers[i - 1] = cmd.ExecuteScalar().ToString();
            }
            connect.Close();
            //Construct an HTML output to post it:
            lblUserStoryInfo.Text = Layouts.userStoryHeader(userStoryId, roleId, loginId, userId, userStory_uniqueId, userStory_asARole,
                createdByUserId, createdBy, userStory_iWant, userStory_soThat, userStory_dateIntroduced, userStory_dateConsideredForImplementation,
                userStory_hasImage, imagesHTML, userStory_isDeleted, userStory_currentStatus, userStoryMembers);
        }
        protected void showView()
        {
            View.Visible = true;
            AddNewSprintTask.Visible = false;
        }
        protected void showNewSprintTask()
        {
            View.Visible = false;
            AddNewSprintTask.Visible = true;
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
                    btnAddNewSprintTask.Visible = false;
                    btnAddUserToList.Visible = false;
                    btnSaveSprintTask.Visible = false;
                    btnUpload.Visible = false;
                }
            }
            connect.Close();
        }
        protected void rebindValues()
        {
            int int_roleId = Convert.ToInt32(roleId);
            if (grdSprintTasks.Rows.Count > 0)
            {
                //Hide the headers called "User ID" and "Sprint Task ID":
                grdSprintTasks.HeaderRow.Cells[8].Visible = false;
                grdSprintTasks.HeaderRow.Cells[9].Visible = false;
                //Hide IDs column and content which are located in column index 8:
                for (int i = 0; i < grdSprintTasks.Rows.Count; i++)
                {
                    grdSprintTasks.Rows[i].Cells[8].Visible = false;
                    grdSprintTasks.Rows[i].Cells[9].Visible = false;
                }
            }
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            string sprintTaskUniqueId = "", userStoryUniqueId = "", taskDescription = "", dateIntroduced = "", dateConsidered = "", dateCompleted = "",
                   developersResponsible = "", currentStatus = "";
            string creatorId = "", db_sprintTaskId = "";
            for (int row = 0; row < grdSprintTasks.Rows.Count; row++)
            {
                //Set links to review a user:
                sprintTaskUniqueId = grdSprintTasks.Rows[row].Cells[0].Text;
                userStoryUniqueId = grdSprintTasks.Rows[row].Cells[1].Text;
                taskDescription = grdSprintTasks.Rows[row].Cells[2].Text;
                dateIntroduced = grdSprintTasks.Rows[row].Cells[3].Text;
                dateConsidered = grdSprintTasks.Rows[row].Cells[4].Text;
                dateCompleted = grdSprintTasks.Rows[row].Cells[5].Text;
                developersResponsible = grdSprintTasks.Rows[row].Cells[6].Text;
                currentStatus = grdSprintTasks.Rows[row].Cells[7].Text;
                creatorId = grdSprintTasks.Rows[row].Cells[8].Text;
                db_sprintTaskId = grdSprintTasks.Rows[row].Cells[9].Text;
                string removeSprintTaskCommand = grdSprintTasks.Rows[row].Cells[10].Text;
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
                    grdSprintTasks.Rows[row].Cells[6].Controls.Add(participantLink);
                    if (usersForSprintTask > 1)
                    {
                        HyperLink temp = new HyperLink();
                        temp.Text = "<br/>";
                        grdSprintTasks.Rows[row].Cells[6].Controls.Add(temp);
                    }
                }
                //Get the Sprint Task ID:
                string id = db_sprintTaskId;
                string sprintTaskUrl = "ViewSprintTask.aspx?id=" + id;
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
                grdSprintTasks.Rows[row].Cells[0].Controls.Add(sprintTaskUIdLink);
                grdSprintTasks.Rows[row].Cells[1].Controls.Add(userStoryUIdLink);
                grdSprintTasks.Rows[row].Cells[2].Controls.Add(taskDescriptionLink);
                grdSprintTasks.Rows[row].Cells[3].Controls.Add(dateIntroducedLink);
                grdSprintTasks.Rows[row].Cells[4].Controls.Add(dateConsideredLink);
                grdSprintTasks.Rows[row].Cells[5].Controls.Add(dateCompletedLink);
                grdSprintTasks.Rows[row].Cells[7].Controls.Add(currentStatusLink);
                grdSprintTasks.Rows[row].Cells[10].Controls.Add(removeSprintTaskLink);
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
        protected int getTotalUserStories()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from [SprintTasks] where userStoryId = '" + userStoryId + "' ";
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
                dt.Columns.Add("Sprint task ID", typeof(string));
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
                for (int i = 1; i <= countUserStories; i++)
                {
                    //Get the project ID:
                    cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] where userStoryId = '" + userStoryId + "' ) as t where rowNum = '" + i + "'";
                    id = cmd.ExecuteScalar().ToString();
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
                grdSprintTasks.DataSource = dt;
                grdSprintTasks.DataBind();
                rebindValues();
            }
        }
        protected void grdSprintTasks_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdSprintTasks.PageIndex = e.NewPageIndex;
            grdSprintTasks.DataBind();
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
        protected void btnAddNewSprintTask_Click(object sender, EventArgs e)
        {
            showNewSprintTask();
            //count the number of the current sprint tasks, then add one to the total
            //So the new sprint task Unique ID will the resulting number:
            updateUniqueId();
            string userStoryUID = "";
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();
                cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + userStoryId + "' ";
                userStoryUID = cmd.ExecuteScalar().ToString();
                connect.Close();
            }
            catch (Exception ex)
            {
                connect.Close();
                Console.WriteLine("Error: "+ex);
            }
            txtUniqueUserStoryID.Text = userStoryUID;
            clearNewSprintTaskInputs();
            hideErrorLabels();
        }
        protected void updateUniqueId()
        {
            string newId = "";
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();

                cmd.CommandText = "select top(1) sprintTask_uniqueId from SprintTasks where userStoryId = '" + userStoryId + "' order by sprintTaskId desc ";
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
            txtUniqueSprintTaskID.Text = newId;
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
            cmd.CommandText = "select projectId from UserStories where userStoryId = '" + userStoryId + "' ";
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
            cmd.CommandText = "select projectId from UserStories where userStoryId = '" + userStoryId + "' ";
            string projectId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_name from Projects where projectId = '" + projectId + "' ";
            string project_name = cmd.ExecuteScalar().ToString();
            connect.Close();
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your sprint task#(" + txtUniqueSprintTaskID.Text + ") has been successfully submitted for the project (" + project_name + ") under the user story#(" + txtUniqueUserStoryID.Text + ").\n" +
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
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessSprintTask(sprintTaskId);
            storeImagesInDB(sprintTaskId, hasImage, files);
            lblAddSprintTaskMessage.Visible = true;
            lblAddSprintTaskMessage.ForeColor = System.Drawing.Color.Green;
            lblAddSprintTaskMessage.Text = "The sprint task has been successfully submitted and an email notification has been sent to you. <br/>";
            wait.Visible = false;
        }
        protected void allowUserAccessSprintTask(string sprintTaskId)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Note: by adding the user to this table, he/she is given access to the newly-created table.
            ////Add the creator to UsersForSprintTasks table.
            ////Add the current user to the sprint task:
            //cmd.CommandText = "insert into UsersForSprintTasks (userId, sprintTaskId, usersForSprintTasks_isNotified) values " +
            //        "('" + userId + "', '" + sprintTaskId + "', '0')";
            //cmd.ExecuteScalar();
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
            //Store the new user story in the database:
            cmd.CommandText = "insert into SprintTasks (userStoryId, sprintTask_createdBy, sprintTask_createdDate, sprintTask_uniqueId, sprintTask_taskDescription, sprintTask_dateIntroduced, " +
                "sprintTask_dateConsideredForImplementation, sprintTask_hasImage, sprintTask_currentStatus) values " +
               "('" + userStoryId + "', '" + createdBy + "', '" + createdDate + "', '" + sprintTaskUId + "', '" + taskDescription + "', '" + dateIntroduced + "', '" + dateConsidered + "', " +
               " '" + hasImage + "',  '" + currentStatus + "') ";
            cmd.ExecuteScalar();
            //Get the ID of the newly stored Sprint task from the database:
            cmd.CommandText = "select [sprintTaskId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY sprintTaskId ASC), * FROM [SprintTasks] " +
                "where userStoryId = '" + userStoryId + "' and sprintTask_createdBy = '" + createdBy + "' and sprintTask_createdDate = '" + Layouts.getOriginalTimeFormat(createdDate.ToString()) + "' "
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
            cmd.CommandText = "select userStory_dateIntroduced from UserStories where userStoryId = '" + userStoryId + "' ";
            DateTime userStory_dateIntroduced = DateTime.Parse(cmd.ExecuteScalar().ToString());
            cmd.CommandText = "select userStory_dateConsideredForImplementation from UserStories where userStoryId = '" + userStoryId + "' ";
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
                if(differenceFromUserStoryDateConsideredToSprintTaskDateConsidered < 0)
                {
                    correct = false;
                    lblDateConsideredError.Visible = true;
                    lblDateConsideredError.Text = "Invalid input: Please select a date starting after the user story's considered date (" + Layouts.getTimeFormat(userStory_dateConsideredForImplementation.ToString()) + ").";
                }
                if(differenceFromUserStoryDateIntroducedDateToSprintTaskDateIntroduced < 0)
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
        protected void btnGoBackToListOfSprintTasks_Click(object sender, EventArgs e)
        {
            showView();
            createTable();
        }
        protected void grdUserStories_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        protected static bool isUserStoryCorrect(string userStoryId, string creatorId)
        {
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            //check if id contains a special character:
            if (!errors.isDigit(userStoryId))
                correct = false;
            //check if id contains an id that does not exist in DB:
            else if (errors.ContainsSpecialChars(userStoryId))
                correct = false;
            if (correct)
            {
                Configuration config = new Configuration();
                SqlConnection connect = new SqlConnection(config.getConnectionString());
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();
                //Count the existance of the user story:
                cmd.CommandText = "select count(*) from UserStories where userStoryId = '" + userStoryId + "' ";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)//if count > 0, then the User Story ID exists in DB.
                {
                    cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + userStoryId + "' ";
                    string actual_creatorId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_isDeleted from UserStories where userStoryId = '" + userStoryId + "' ";
                    int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                    if (isDeleted == 1)
                        correct = false;
                }
                else
                    correct = false; // means that the user story ID does not exists in DB.
                connect.Close();
            }
            return correct;
        }
        [WebMethod]
        [ScriptMethod()]
        public static void removeUserStory_Click(string userStoryId, string entry_creatorId)
        {
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool userStoryIdExists = isUserStoryCorrect(userStoryId, entry_creatorId);
            if (userStoryIdExists)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //update the DB and set isDeleted = true:
                cmd.CommandText = "update UserStories set userStory_isDeleted = 1 where userStoryId = '" + userStoryId + "' ";
                cmd.ExecuteScalar();
                //Email the project creator about the project being deleted:
                cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + userStoryId + "' ";
                string creatorId = cmd.ExecuteScalar().ToString();
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
                    "This email is to inform you that your user story#(" + userStoryUID + ") in the project (" + project_name + ") has been deleted. If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                    "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                Email email = new Email();
                email.sendEmail(emailTo, emailBody);
            }
        }
    }
}