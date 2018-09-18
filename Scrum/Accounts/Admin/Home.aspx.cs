using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Admin
{
    public partial class Home : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
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
        }
        protected void getSession()
        {
            username = (string)(Session["username"]);
            roleId = (string)(Session["roleId"]);
            loginId = (string)(Session["loginId"]);
            token = (string)(Session["token"]);
        }
        protected void rebindValues()
        {
            //Hide the header called "User ID":
            grdProjects.HeaderRow.Cells[1].Visible = false;
            //Hide IDs column and content which are located in column index 3:
            for (int i = 0; i < grdProjects.Rows.Count; i++)
            {
                grdProjects.Rows[i].Cells[3].Visible = false;
            }
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string project_name = "", createdBy = "", createdOn = "", creatorId = "";
            for (int row = 0; row < grdProjects.Rows.Count; row++)
            {
                //Set links to review a user:
                project_name = grdProjects.Rows[row].Cells[0].Text;
                createdBy = grdProjects.Rows[row].Cells[1].Text;
                createdOn = grdProjects.Rows[row].Cells[2].Text;
                creatorId = grdProjects.Rows[row].Cells[3].Text;
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
                grdProjects.Rows[row].Cells[0].Controls.Add(projectLink);
                grdProjects.Rows[row].Cells[1].Controls.Add(userLink);
                grdProjects.Rows[row].Cells[2].Controls.Add(dateLink);
            }
            connect.Close();
        }
        protected int getTotalNewUsers()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count the not-approved users:
            cmd.CommandText = "select count(*) from [Projects]";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return count;
        }
        protected void createTable()
        {
            int countProjects = getTotalNewUsers();
            if (countProjects == 0)
            {
                lblMessage.Visible = true;
            }
            else if (countProjects > 0)
            {
                lblMessage.Visible = false;
                DataTable dt = new DataTable();
                dt.Columns.Add("Project Name", typeof(string));
                dt.Columns.Add("Created by", typeof(string));
                dt.Columns.Add("Created time", typeof(string));
                dt.Columns.Add("Creator ID", typeof(string));
                string id = "", project_name = "", createdBy = "", createdOn = "", creatorId = "";
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                for (int i = 1; i <= countProjects; i++)
                {
                    //Get the project ID:
                    cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY projectId ASC), *FROM [Projects]) as t where rowNum = '" + i + "'";
                    id = cmd.ExecuteScalar().ToString();
                    //Get project name:
                    cmd.CommandText = "select project_name from Projects where projectId = '"+id+"' ";
                    project_name = cmd.ExecuteScalar().ToString();
                    //Get the project's creator User ID:
                    cmd.CommandText = "select project_createdBy from Projects where projectId = '" + id + "' ";
                    creatorId = cmd.ExecuteScalar().ToString();
                    //Convert the User ID to a name:
                    cmd.CommandText = "select (user_firstname + ' '  + user_lastname) from Users where userId = '"+ creatorId + "' ";
                    createdBy = cmd.ExecuteScalar().ToString();
                    //Get project creation date:
                    cmd.CommandText = "select project_createdDate from Projects where projectId = '" + id + "' ";
                    createdOn = cmd.ExecuteScalar().ToString();
                    dt.Rows.Add(project_name, createdBy, Layouts.getTimeFormat(createdOn), creatorId);
                    //Creator ID is not needed here, but it's used to uniquely identify the names in the system in case we have duplicate names.
                }
                connect.Close();
                grdProjects.DataSource = dt;
                grdProjects.DataBind();
                rebindValues();
            }
        }
        protected void grdProjects_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdProjects.PageIndex = e.NewPageIndex;
            grdProjects.DataBind();
            rebindValues();
        }
    }
}