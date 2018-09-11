using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Admin
{
    public partial class Account : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialAccess();
            showAllFields();
        }
        protected void showAllFields()
        {
            btnChangePassword.Visible = true;
            btnChangeSecurityQuestions.Visible = true;
        }
        protected void initialAccess()
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
        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            Response.Redirect("ChangePassword");
        }
        protected void btnChangeSecurityQuestions_Click(object sender, EventArgs e)
        {
            Response.Redirect("ChangeSecurityQuestions");
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
    }
}