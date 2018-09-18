using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Admin
{
    public partial class ReviewUser : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        static string previousPage = "";
        static string currentPage = "";
        string registerId = "";
        //Globals for "Users" table:
        string g_firstName, g_lastName, g_email, g_phone;
        //Globals for "Logins" table:
        int g_roleId;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (HttpContext.Current.Request.Url.AbsoluteUri != null) currentPage = HttpContext.Current.Request.Url.AbsoluteUri;
                else currentPage = "Home.aspx";
                if (Request.UrlReferrer != null) previousPage = Request.UrlReferrer.ToString();
                else previousPage = "Home.aspx";
                if (currentPage.Equals(previousPage))
                    previousPage = "Home.aspx";
            }
            initialPageAccess();
            registerId = Request.QueryString["id"];
            showUserInformation();
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
        protected void showUserInformation()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Check if the ID exists in the database:
            cmd.CommandText = "select count(*) from registrations where registerId = '" + registerId.Replace("'", "''") + "' ";
            int countUser = Convert.ToInt32(cmd.ExecuteScalar());
            if (countUser > 0)//if ID exists, countUser = 1
            {
                //Get first name:
                cmd.CommandText = "select register_firstname from [Registrations] where [registerId] = '" + registerId + "' ";
                string firstName = cmd.ExecuteScalar().ToString();
                //Get last name and add it to the first name:
                cmd.CommandText = "select register_lastname from [Registrations] where [registerId] = '" + registerId + "' ";
                string lastName = " " + cmd.ExecuteScalar().ToString();
                //Get email:
                cmd.CommandText = "select register_email from [Registrations] where [registerId] = '" + registerId + "' ";
                string email = cmd.ExecuteScalar().ToString();
                //Get role ID as int:
                cmd.CommandText = "select register_roleId from [Registrations] where [registerId] = '" + registerId + "' ";
                int int_roleId = Convert.ToInt32(cmd.ExecuteScalar());
                //Convert role ID to string:
                string role = "";
                if (int_roleId == 1)
                    role = "Admin";
                else if (int_roleId == 2)
                    role = "Master";
                else
                    role = "Developer";
                //Get phone:
                cmd.CommandText = "select register_phone from [Registrations] where [registerId] = '" + registerId + "' ";
                string phone = cmd.ExecuteScalar().ToString();
                //Create an informative message containing all information for the selected user:
                lblUserInformation.Text =
                    "<table>" +
                    "<tr><td>Name: </td><td>" + firstName + " " + lastName + "</td></tr>" +
                    "<tr><td>Email: </td><td>" + email + "</td></tr>" +
                    "<tr><td>Phone#: </td><td>" + phone + "</td></tr>" +
                    "<tr><td>Role: </td><td>" + role + "</td></tr>";
                lblUserInformation.Text += "</table>";
                lblUserInformation.Visible = true;
                //Copy values to globals:
                g_firstName = firstName; g_lastName = lastName; g_email = email; g_phone = phone; g_roleId = int_roleId;
            }
            else
            {
                goBack();
            }
            connect.Close();
        }
        protected string createUsername()
        {
            string generatedUsername = "";
            //generatedUsername = g_firstName + g_lastName + g_roleId + registerId;
            generatedUsername = g_firstName + "." + g_lastName;// + g_roleId + registerId;
            generatedUsername = generatedUsername.Replace(" ", "");
            generatedUsername = generatedUsername.Replace("'", "");
            //Make sure the new username doesn't match another username in the system:
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from Logins where login_username like '" + generatedUsername + "' ";
            int countDuplicateUsernames = Convert.ToInt32(cmd.ExecuteScalar());
            if (countDuplicateUsernames > 0)
            {
                //If the username exists, add the role ID at the end of it:
                generatedUsername = generatedUsername + g_roleId;
                cmd.CommandText = "select count(*) from Logins where login_username like '" + generatedUsername + "' ";
                int countDuplicateUsernames_2 = Convert.ToInt32(cmd.ExecuteScalar());
                if (countDuplicateUsernames_2 > 0)
                {
                    //If the username exists, add the register ID at the end of it:
                    generatedUsername = generatedUsername + registerId;
                    cmd.CommandText = "select count(*) from Logins where login_username like '" + generatedUsername + "' ";
                    int countDuplicateUsernames_3 = Convert.ToInt32(cmd.ExecuteScalar());
                    if (countDuplicateUsernames_3 > 0)
                    {
                        Random rnd = new Random();
                        int addUniqueness = rnd.Next(1, 999);
                        //If the username exists, add a random integer at the end of it:
                        generatedUsername = generatedUsername + addUniqueness;
                        cmd.CommandText = "select count(*) from Logins where login_username like '" + generatedUsername + "' ";
                        int countDuplicateUsernames_4 = Convert.ToInt32(cmd.ExecuteScalar());
                        if (countDuplicateUsernames_4 > 0)
                        {
                            //In an extreme case, if that generated username duplicates with another one, add the login ID + 1 from the last login ID:
                            cmd.CommandText = "select top 1 loginId from logins order by loginId desc";
                            int lastLoginId = Convert.ToInt32(cmd.ExecuteScalar());
                            lastLoginId++;
                            generatedUsername = generatedUsername + lastLoginId;
                        }
                    }
                }
            }
            connect.Close();
            return generatedUsername;
        }
        protected string createPassword()
        {
            //The below will generate a password of 8 characters having at least 4 non-alphanumeric characters:
            string generatedPassword = Membership.GeneratePassword(8, 4);
            return generatedPassword;
        }
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            //Hide the success message:
            lblMessage.Visible = false;
            //Create a new unique username:
            string newUsername = createUsername();
            //Create an initial password:
            string newPassword = createPassword();
            //Hash the password:
            string hashedPassword = Encryption.hash(newPassword);
            //Set login_attempts = 0, login_securityQuestionsAttempts = 0, login_initial = 1 and login_isActive = 1: (1 in bit = true)
            //Store the previous information into the table "Logins":
            g_phone = g_phone.Replace(" ", "");
            g_phone = g_phone.Replace("'", "''");
            //Create an email message to be sent:
            string emailMessage = "Hello " + g_firstName + " " + g_lastName + ",\n\n" +
                "This email is to inform you that your account has been approved for Scrum. To access the site, you need the following information:\n" +
                "username: " + newUsername + "\n" +
                "password: " + newPassword + "\n" +
                "Remeber, your provided password is a temporary password and you must change it once you login to the site.\n\n" +
                "Best regards,\nScrum Support\nScrum.UWL@gmail.com";
            //Send an email notification the user using the entered email:
            Email emailClass = new Email();
            emailClass.sendEmail(g_email, emailMessage);
            //Display a success message:
            lblMessage.Visible = true;
            lblMessage.Text = "The selected user has been successfully approved and the new information has been emailed to the user!";
            //Hide "Approve" and "Deny" buttons:
            hideApproveDeny();
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "insert into Logins (login_username, login_password, roleId, login_attempts, login_securityQuestionsAttempts, login_initial, login_isActive) values " +
                "('" + newUsername + "', '" + hashedPassword + "', '" + g_roleId + "', 0, 0, 1, 1)";
            cmd.ExecuteScalar();
            //Get the loginID of the user just created using the username:
            cmd.CommandText = "select loginId from Logins where login_username like '" + newUsername + "' ";
            string newLoginId = cmd.ExecuteScalar().ToString();
            //Store the user's information into the "Users" table:
            cmd.CommandText = "insert into Users (user_firstname, user_lastname, user_email, user_phone, loginId) values " +
                "('" + g_firstName + "', '" + g_lastName + "', '" + g_email + "', '" + g_phone + "', '" + newLoginId + "') ";
            cmd.ExecuteScalar();
            //Delete user information from "Registrations" table:
            cmd.CommandText = "delete from [Registrations] where registerId = '" + registerId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected void hideApproveDeny()
        {
            btnApprove.Visible = false;
            btnDeny.Visible = false;
        }
        protected void btnDeny_Click(object sender, EventArgs e)
        {
            //Delete user information from "Registrations" table:
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "delete from [Registrations] where registerId = '" + registerId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
            //Create an email message to be sent:
            string emailMessage = "Hello " + g_firstName + " " + g_lastName + ",\n\n" +
                "This email is to inform you that your account has been denied for Scrum. For more information, please contact the support.\n\n" +
                "Best regards,\nScrum Support\nScrum.UWL@gmail.com";
            //Send an email notification the user using the entered email:
            Email emailClass = new Email();
            emailClass.sendEmail(g_email, emailMessage);
            //Show in a message that the user was denied:
            lblMessage.Visible = true;
            lblMessage.Text = "The selected user has been successfully denied, emailed and removed from the list of applied users!";
            //Hide "Approve" and "Deny" buttons:
            hideApproveDeny();
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            goBack();
        }
        protected void goBack()
        {
            addSession();
            if (!string.IsNullOrWhiteSpace(previousPage)) Response.Redirect(previousPage);
            else Response.Redirect("Home.aspx");
        }
    }
}