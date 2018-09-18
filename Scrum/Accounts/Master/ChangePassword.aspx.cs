using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Master
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        string username = "", roleId = "", loginId = "", token = "";
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        static string previousPage = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            initialAccess();
            if (!IsPostBack)
            {
                if (Request.UrlReferrer != null)
                    previousPage = Request.UrlReferrer.ToString();
                else
                    previousPage = "Home.aspx";
            }
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
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            bool correctInput = checkInputs();
            if (correctInput)
            {
                store();
                showSuccessfulMessage();
                clearInputs();
            }
        }
        protected void clearInputs()
        {
            txtP1.Text = "";
            txtP2.Text = "";
        }
        protected void showSuccessfulMessage()
        {
            lblError.Visible = false;
            lblSuccess.Visible = true;
            lblSuccess.Text = "You have successfully changed your password!";
            btnSubmit.Visible = false;
        }
        protected void store()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select loginId from Logins where login_username like '" + username + "' ";
            string temp_loginId = cmd.ExecuteScalar().ToString();
            //Hash the new password:
            string hashedPassword = Encryption.hash(txtP1.Text);
            //Update new password:
            cmd.CommandText = "update Logins set login_password = '" + hashedPassword + "' where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            //Set the status as NOT the initial login:
            cmd.CommandText = "update Logins set login_initial = 0 where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected bool checkInputs()
        {
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            string p1, p2, p3;
            if (!errors.validPassword(txtP1.Text, out p1))
            {
                correct = false;
                lblP1Error.Visible = true;
                lblP1Error.Text = p1;
                txtP1.Focus();
            }
            if (!errors.validPassword(txtP2.Text, out p2))
            {
                correct = false;
                lblP2Error.Visible = true;
                lblP2Error.Text = p2;
                txtP2.Focus();
            }
            if (!errors.passwordsMatch(txtP1.Text, txtP2.Text, out p3))
            {
                correct = false;
                lblError.Visible = true;
                lblError.Text = p3;
                txtP1.Focus();
            }
            return correct;
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            goBack();
        }
        protected void goBack()
        {
            addSession();
            if (!string.IsNullOrWhiteSpace(previousPage))
                Response.Redirect(previousPage);
            else
                Response.Redirect("Home.aspx");
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