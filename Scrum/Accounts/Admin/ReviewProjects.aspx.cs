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
    public partial class ReviewProjects : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            int countNewProjects = getTotalNewProjects();
            if (countNewProjects > 0)
            {
                lblMessage.Visible = false;
                createTable(countNewProjects);
            }
            else if (countNewProjects == 0)
            {
                lblMessage.Visible = true;
            }
        }
        protected void initialPageAccess()
        {
            Configuration config = new Configuration();
            conn = config.getConnectionString();
            connect = new SqlConnection(conn);
            getSession();
            CheckSession session = new CheckSession();
            bool correctSession = session.sessionIsCorrect(username, roleId, token);
            if (!correctSession)
                clearSession();

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
        protected void grdProjects_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdProjects.PageIndex = e.NewPageIndex;
            grdProjects.DataBind();
            rebindValues();
        }
        protected void rebindValues()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string creator = "", project_name="";
            for (int row = 0; row < grdProjects.Rows.Count; row++)
            {
                //Set a link to review a project:
                project_name = grdProjects.Rows[row].Cells[0].Text;
                cmd.CommandText = "select projectId from Projects where project_name like '" + project_name + "' and project_isApproved = 0 and project_isDenied = 0 and project_isTerminated = 0 and project_isDeleted = 0";
                string projectId = cmd.ExecuteScalar().ToString();
                HyperLink projectLink = new HyperLink();
                projectLink.Text = project_name + " ";
                projectLink.NavigateUrl = "ReviewProject.aspx?id=" + projectId;
                grdProjects.Rows[row].Cells[0].Controls.Add(projectLink);
                //Set the creator's link

                //Get creator's ID:
                cmd.CommandText = "select [project_createdBy] from Projects where projectId = '" + projectId + "' ";
                string creatorId = cmd.ExecuteScalar().ToString();
                HyperLink creatorLink = new HyperLink();
                creatorLink.Text = creator + " ";
                creatorLink.NavigateUrl = "Profile.aspx?id=" + creatorId;
                grdProjects.Rows[row].Cells[2].Controls.Add(creatorLink);
            }
            connect.Close();
        }
        protected void createTable(int count)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Created on", typeof(string));
            dt.Columns.Add("Creator", typeof(string));
            string id = "", project_name = "", project_createdDate = "", creator = "";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            for (int i = 1; i <= count; i++)
            {
                //Get the Project ID:
                cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY projectId ASC), * FROM [Projects] where project_isApproved = 0 and project_isDenied = 0 and project_isTerminated = 0 and project_isDeleted = 0) as t where rowNum = '" + i + "'";
                id = cmd.ExecuteScalar().ToString();
                //Get Name:
                cmd.CommandText = "select [project_name] from Projects where projectId = '"+id+"' ";
                project_name = cmd.ExecuteScalar().ToString();
                //Get date:
                cmd.CommandText = "select [project_createdDate] from Projects where projectId = '" + id + "' ";
                project_createdDate = cmd.ExecuteScalar().ToString();
                //Get creator's ID:
                cmd.CommandText = "select [project_createdBy] from Projects where projectId = '" + id + "' ";
                string creatorId = cmd.ExecuteScalar().ToString();
                //Get creator's name:
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where userId = '" + creatorId + "' ";
                creator = cmd.ExecuteScalar().ToString();
                dt.Rows.Add(project_name, Layouts.getTimeFormat(project_createdDate), creator);
            }
            connect.Close();
            grdProjects.DataSource = dt;
            grdProjects.DataBind();
            rebindValues();
        }
        protected int getTotalNewProjects()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count the not-approved topics:
            cmd.CommandText = "select count(*) from Projects where project_isApproved = 0 and project_isDenied = 0 and project_isTerminated = 0 and project_isDeleted = 0";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return count;
        }
    }
}