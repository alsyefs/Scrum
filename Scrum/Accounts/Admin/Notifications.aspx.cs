using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Admin
{
    public partial class Notifications : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            showAllFields();
            countNewUsers();
            countNewProjects();
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

            if (session.adminAlerts() == 0)
            {
                lblError.Visible = true;
                lblError.Text = "There are no new alerts!";
            }
            else
            {
                lblError.Visible = false;
            }
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
        protected void showAllFields()
        {
            btnNewUsers.Visible = true;
            btnNewProjects.Visible = true;
        }
        protected void countNewUsers()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from Registrations";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count == 0)
            {
                lblNewUsers.Text = "There are no new users to review.";
                btnNewUsers.Visible = false;
                //lblNewUsers.Visible = false;
            }
            else if (count == 1)
            {
                lblNewUsers.Text = "There is one new user to review.";
                btnNewUsers.Visible = true;
                lblNewUsers.Visible = true;
            }
            else
            {
                lblNewUsers.Text = "There are " + count + " new users to review.";
                btnNewUsers.Visible = true;
                lblNewUsers.Visible = true;
            }
            connect.Close();
        }
        protected void countNewProjects()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from [Projects] where project_isApproved = 0 and project_isDenied = 0 and project_isTerminated = 0 and project_isDeleted = 0";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count == 0)
            {
                lblNewProjects.Text = "There are no new projects to review.";
                btnNewProjects.Visible = false;
                //lblNewTopics.Visible = false;
            }
            else if (count == 1)
            {
                lblNewProjects.Text = "There is one new project to review.";
                btnNewProjects.Visible = true;
                lblNewProjects.Visible = true;
            }
            else
            {
                lblNewProjects.Text = "There are " + count + " new projects to review.";
                btnNewProjects.Visible = true;
                lblNewProjects.Visible = true;
            }
            connect.Close();
        }
        protected void btnNewUsers_Click(object sender, EventArgs e)
        {
            addSession();
            Response.Redirect("ReviewUsers");
        }
        protected void btnNewProjects_Click(object sender, EventArgs e)
        {
            addSession();
            Response.Redirect("ReviewProjects");
        }
    }
}