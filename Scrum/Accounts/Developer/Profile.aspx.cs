using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Admin
{
    public partial class Profile : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        static string g_loginId = "";
        string profileId = "";
        static string previousPage = "";
        static string currentPage = "";
        static bool requestedTerminateOrUnlockAccount = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!requestedTerminateOrUnlockAccount)
                {
                    if (HttpContext.Current.Request.Url.AbsoluteUri != null) currentPage = HttpContext.Current.Request.Url.AbsoluteUri;
                    else currentPage = "Home.aspx";
                    if (Request.UrlReferrer != null) previousPage = Request.UrlReferrer.ToString();
                    else previousPage = "Home.aspx";
                    if (currentPage.Equals(previousPage))
                        previousPage = "Home.aspx";
                }
            }
            initialPageAccess();
            profileId = Request.QueryString["id"];
            g_loginId = loginId;
            bool userIdExists = isUserCorrect();
            if (!userIdExists)
                goHome();
            showInformation();
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            goBack();
        }
        protected void goBack()
        {
            addSession();
            requestedTerminateOrUnlockAccount = false;
            if (!string.IsNullOrWhiteSpace(previousPage)) Response.Redirect(previousPage);
            else Response.Redirect("Home.aspx");
        }
        protected void goHome()
        {
            Response.Redirect("Home.aspx");
        }
        protected bool isUserCorrect()
        {
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            //check if id contains a special character:
            if (!errors.isDigit(profileId))
                correct = false;
            //check if id contains an id that does not exist in DB:
            else if (errors.ContainsSpecialChars(profileId))
                correct = false;
            if (correct)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //Count the existance of the user:
                cmd.CommandText = "select count(*) from Users where userId = '" + profileId + "' ";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)//if count > 0, then the user ID exists in DB.
                {
                    //Get the current user's ID who is trying to access the profile:
                    cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
                    string current_userId = cmd.ExecuteScalar().ToString();
                    //Maybe later use the current user's ID to check if the current user has access to view the selected profile.
                }
                else
                    correct = false; // means that the user ID does not exists in DB.
                connect.Close();
            }
            return correct;
        }
        protected void showInformation()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID who is trying to access the profile:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string current_userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select loginId from users where userId = '" + profileId + "' ";
            string account_loginId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select login_isActive from Logins where loginId = '" + account_loginId + "' ";
            int isActive = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select roleId from logins where loginId = '" + account_loginId + "' ";
            int account_roleId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where userId = '" + profileId + "' ";
            string name = cmd.ExecuteScalar().ToString();
            connect.Close();
            if (account_loginId == loginId)
            {
                lblRow.Text = "This account belongs to you.";
            }
            else if(account_loginId != loginId && account_roleId == 1)//Another admin
                lblRow.Text = "This account belongs to '"+name+"' as an admin in the system";
            else if (account_loginId != loginId && account_roleId == 2)//Another Master
                lblRow.Text = "This account belongs to '" + name + "' as a master in the system";
            else if (account_loginId != loginId && account_roleId == 3)//Another Developer
                lblRow.Text = "This account belongs to '" + name + "' as a developer in the system";
            string terminateCommand = "<button id='terminate_button'type='button' onclick=\"terminateAccount('" + profileId + "')\">Terminate Account</button>";
            string unlockCommand = "<button id='unlock_button'type='button' onclick=\"unlockAccount('" + profileId + "')\">Unlock Account</button>";
            int int_roleId = Convert.ToInt32(roleId);
            if (isActive == 1 && account_loginId != loginId && int_roleId == 1)
                lblAdminCommands.Text += terminateCommand;
            else if (isActive == 0 && account_loginId != loginId && int_roleId == 1)
                lblAdminCommands.Text += unlockCommand;
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
            if (int_roleId != 1)
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
        protected static bool isAccountCorrect(string in_profileId, int terminateOrUnlock)
        {
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            //check if id contains a special character:
            if (!errors.isDigit(in_profileId))
                correct = false;
            //check if id contains an id that does not exist in DB:
            else if (errors.ContainsSpecialChars(in_profileId))
                correct = false;
            if (correct)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                //Count the existance of the user:
                cmd.CommandText = "select count(*) from Users where userId = '" + in_profileId + "' ";
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)//if count > 0, then the user ID exists in DB.
                {
                    //Get the current user's ID who is trying to access the profile:
                    cmd.CommandText = "select userId from Users where loginId = '" + g_loginId + "' ";
                    string current_userId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select loginId from users where userId = '" + in_profileId + "' ";
                    string account_loginId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select login_isActive from Logins where loginId = '" + account_loginId + "' ";
                    int isActive = Convert.ToInt32(cmd.ExecuteScalar());
                    if (terminateOrUnlock == 1)// if the command was to terminate:
                        if (isActive == 0)
                            correct = false;
                        else if (terminateOrUnlock == 2)// if the command was to unlock:
                            if (isActive == 1)
                                correct = false;
                    //Maybe later use the current user's ID to check if the current user has access to view the selected profile.
                    if (account_loginId == g_loginId)
                        correct = false;
                }
                else
                    correct = false; // means that the user ID does not exists in DB.
                connect.Close();
            }
            return correct;
        }
        [WebMethod]
        [ScriptMethod()]
        public static string terminateOrUnlockAccount(string in_profileId, int terminateOrUnlock)
        {
            string errorMessage = "";
            requestedTerminateOrUnlockAccount = true;
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            bool accountIdExists = isAccountCorrect(in_profileId, terminateOrUnlock);
            if (accountIdExists)
            {
                connect.Open();
                SqlCommand cmd = connect.CreateCommand();
                cmd.CommandText = "select loginId from users where userId = '" + in_profileId + "' ";
                string account_loginId = cmd.ExecuteScalar().ToString();
                connect.Close();
                if (terminateOrUnlock == 1)//1=terminate
                {
                    connect.Open();
                    //update the DB and set isActive = false:
                    cmd.CommandText = "update Logins set login_isActive = 0 where loginId = '" + account_loginId + "' ";
                    cmd.ExecuteScalar();
                    //Email the topic creator about the topic being deleted:
                    cmd.CommandText = "select user_firstname from Users where userId = '" + in_profileId + "' ";
                    string name = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select user_lastname from Users where userId = '" + in_profileId + "' ";
                    name = name + " " + cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select user_email from Users where userId = '" + in_profileId + "' ";
                    string emailTo = cmd.ExecuteScalar().ToString();
                    connect.Close();
                    string emailBody = "Hello " + name + ",\n\n" +
                        "This email is to inform you that your account has been terminated. If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                        "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                    Email email = new Email();
                    email.sendEmail(emailTo, emailBody);
                }
                else if (terminateOrUnlock == 2)//2 = Unlock
                {
                    connect.Open();
                    //update the DB and set isActive = true:
                    cmd.CommandText = "update Logins set login_isActive = 1 where loginId = '" + account_loginId + "' ";
                    cmd.ExecuteScalar();
                    //Email the topic creator about the topic being deleted:
                    cmd.CommandText = "select user_firstname from Users where userId = '" + in_profileId + "' ";
                    string name = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select user_lastname from Users where userId = '" + in_profileId + "' ";
                    name = name + " " + cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select user_email from Users where userId = '" + in_profileId + "' ";
                    string emailTo = cmd.ExecuteScalar().ToString();
                    connect.Close();
                    string emailBody = "Hello " + name + ",\n\n" +
                    "This email is to inform you that your account has been unlocked. If you think this happened by mistake, or you did not perform this action, plaese contact the support.\n\n" +
                        "Best regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
                    Email email = new Email();
                    email.sendEmail(emailTo, emailBody);
                }
            }
            return errorMessage;
        }
    }
}