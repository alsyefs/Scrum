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
        protected void showAllFields()
        {
            btnNewUsers.Visible = true;
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