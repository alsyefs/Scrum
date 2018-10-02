using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Master
{
    public partial class ViewProject : System.Web.UI.Page
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
            projectId = Request.QueryString["id"];
            if(!check.checkProjectAccess(projectId, loginId))
                goBack();
            createTable();
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
        protected void rebindValues()
        {
            //Hide the header called "User ID":
            grdUserStories.HeaderRow.Cells[8].Visible = false;
            //Hide IDs column and content which are located in column index 8:
            for (int i = 0; i < grdUserStories.Rows.Count; i++)
            {
                grdUserStories.Rows[i].Cells[8].Visible = false;
            }
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string userStoryId = "", asARole = "", iWant = "", soThat = "", dateIntroduced = "", dateConsidered = "",
                   developerResponsible = "", currentStatus = "";
            string creatorId = "";
            for (int row = 0; row < grdUserStories.Rows.Count; row++)
            {
                //Set links to review a user:
                userStoryId = grdUserStories.Rows[row].Cells[0].Text;
                asARole = grdUserStories.Rows[row].Cells[1].Text;
                iWant = grdUserStories.Rows[row].Cells[2].Text;
                soThat = grdUserStories.Rows[row].Cells[3].Text;
                dateIntroduced = grdUserStories.Rows[row].Cells[4].Text;
                dateConsidered = grdUserStories.Rows[row].Cells[5].Text;
                developerResponsible = grdUserStories.Rows[row].Cells[6].Text;
                currentStatus = grdUserStories.Rows[row].Cells[7].Text;
                creatorId = grdUserStories.Rows[row].Cells[8].Text;
                cmd.CommandText = "select userId from Users where (user_firstname + ' ' + user_lastname) like '" + developerResponsible + "' ";
                string developerId = cmd.ExecuteScalar().ToString();
                //Get the User Story ID:
                cmd.CommandText = "select [userStoryId] from [UserStories] where userStory_createdBy like '" + creatorId + "' and " +
                    "userStory_uniqueId = '"+userStoryId+"' and userStory_asARole = '"+asARole+"' and userStory_iWantTo = '"+iWant+"' and  userStory_soThat = '"+soThat+"'  " +
                    "userStory_dateIntroduced = '" + Layouts.getOriginalTimeFormat(dateIntroduced) + "' and userStory_dateConsideredForImplementation = '" + Layouts.getOriginalTimeFormat(dateConsidered) + "' " +
                    "and userStory_developerResponsibleUserId = '"+ developerId + "' and userStory_currentStatus = '"+currentStatus+"' ";
                string id = cmd.ExecuteScalar().ToString();
                string userStoryUrl = "ViewUserStory.aspx?id=" + id;
                HyperLink idLink = new HyperLink();
                HyperLink asARoleLink = new HyperLink();
                HyperLink iWantLink = new HyperLink();
                HyperLink soThatLink = new HyperLink();
                HyperLink dateIntroducedLink = new HyperLink();
                HyperLink dateConsideredLink = new HyperLink();
                HyperLink developerResponsibleLink = new HyperLink();
                HyperLink currentStatusLink = new HyperLink();
                idLink.Text = userStoryId + " ";
                asARoleLink.Text = asARole + " ";
                iWantLink.Text = iWant + " ";
                soThatLink.Text = soThat + " ";
                dateIntroducedLink.Text = Layouts.getTimeFormat(dateIntroduced) + " ";
                dateConsideredLink.Text = Layouts.getTimeFormat(dateConsidered) + " ";
                developerResponsibleLink.Text = developerResponsible + " ";
                currentStatusLink.Text = currentStatus + " ";
                idLink.NavigateUrl = userStoryUrl;
                asARoleLink.NavigateUrl = userStoryUrl;
                iWantLink.NavigateUrl = userStoryUrl;
                soThatLink.NavigateUrl = userStoryUrl;
                dateIntroducedLink.NavigateUrl = userStoryUrl;
                dateConsideredLink.NavigateUrl = userStoryUrl;
                developerResponsibleLink.NavigateUrl = "Profile.aspx?id=" + creatorId;
                currentStatusLink.NavigateUrl = userStoryUrl;
                grdUserStories.Rows[row].Cells[0].Controls.Add(idLink);
                grdUserStories.Rows[row].Cells[1].Controls.Add(asARoleLink);
                grdUserStories.Rows[row].Cells[2].Controls.Add(iWantLink);
                grdUserStories.Rows[row].Cells[3].Controls.Add(soThatLink);
                grdUserStories.Rows[row].Cells[4].Controls.Add(dateIntroducedLink);
                grdUserStories.Rows[row].Cells[5].Controls.Add(dateConsideredLink);
                grdUserStories.Rows[row].Cells[6].Controls.Add(developerResponsibleLink);
                grdUserStories.Rows[row].Cells[7].Controls.Add(currentStatusLink);
            }
            connect.Close();
        }
        protected int getTotalUserStories()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count the not-approved users:
            cmd.CommandText = "select count(*) from [UserStories] where projectId = '"+projectId+"' ";
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
                dt.Columns.Add("As a <<type of user>>", typeof(string));
                dt.Columns.Add("I want to <<some goal>>", typeof(string));
                dt.Columns.Add("So that <<reason>>", typeof(string));
                dt.Columns.Add("Date introduced", typeof(string));
                dt.Columns.Add("Date considered for implementation", typeof(string));
                dt.Columns.Add("Developer responsible for", typeof(string));
                dt.Columns.Add("Current status", typeof(string));
                dt.Columns.Add("Developer User ID", typeof(string));
                string id = "", userStoryId = "", asARole = "", iWant = "", soThat = "", dateIntroduced = "", dateConsidered = "",
                    developerResponsible = "", currentStatus = "";
                string creatorId = "";
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                for (int i = 1; i <= countUserStories; i++)
                {
                    //Get the project ID:
                    cmd.CommandText = "select [userStoryId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY userStoryId ASC), *FROM [UserStories]) as t where rowNum = '" + i + "'";
                    id = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + id + "' ";
                    userStoryId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_asARole from UserStories where userStoryId = '"+id+"' ";
                    asARole = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_iWantTo from UserStories where userStoryId = '" + id + "' ";
                    iWant = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_soThat from UserStories where userStoryId = '" + id + "' ";
                    soThat = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_dateIntroduced from UserStories where userStoryId = '" + id + "' ";
                    dateIntroduced = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_dateConsideredForImplementation from UserStories where userStoryId = '" + id + "' ";
                    dateConsidered = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_developerResponsibleUserId from UserStories where userStoryId = '" + id + "' ";
                    string developerId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + developerId + "' ";
                    developerResponsible = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select userStory_currentStatus from UserStories where userStoryId = '" + id + "' ";
                    currentStatus = cmd.ExecuteScalar().ToString();
                    //Creator's info, which will be hidden:
                    cmd.CommandText = "select userStory_createdBy from UserStories where userStoryId = '" + id + "' ";
                    creatorId = cmd.ExecuteScalar().ToString();
                    dt.Rows.Add(userStoryId, asARole, iWant, soThat, Layouts.getTimeFormat(dateIntroduced), Layouts.getTimeFormat(dateConsidered), creatorId);
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
    }
}