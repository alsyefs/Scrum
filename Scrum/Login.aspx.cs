using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum
{
    public partial class Login : System.Web.UI.Page
    {
        //string username = "", roleId = "", token = "";
        string loginId = "";
        static string connection_string = "";
        SqlConnection connect = new SqlConnection(connection_string);
        protected void Page_Load(object sender, EventArgs e)
        {
            Configuration config = new Configuration();
            connection_string = config.getConnectionString();
            connect = new SqlConnection(connection_string);
        }
        protected void getLoginId(string username)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select loginId from Logins where login_username like '" + username + "' ";
            loginId = cmd.ExecuteScalar().ToString();
            connect.Close();
        }
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            string username = txtUsername.Text.Replace("'", "''");
            string password = txtPassword.Text;
            check(username, password);
        }
        protected bool checkIfLocked(string username)
        {
            bool active = true;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select login_isActive from Logins where login_username like '" + username + "' ";
            int isActive = Convert.ToInt32(cmd.ExecuteScalar());
            if (isActive == 0)
                active = false;
            connect.Close();
            return active;
        }
        protected void check(string username, string password)
        {
            string errorMessage = "Value Error: Make sure you have entered the correct username and password.";
            int flag = 1;// flag 1 means everything is good.
            flag = checkIfEmpty();
            if (flag == 1)//if input is correct.
            {
                Boolean exists = checkIfExists(username); //check if username in DB.
                if (exists)//if user exists in the DB.
                {
                    getLoginId(username);
                    bool active = checkIfLocked(username);
                    if (active)
                    {
                        addNumberOfTries();
                        Boolean correctPassword = checkPassword(username, password);
                        //check if password is correct.
                        //Boolean correctEndDate = checkEndDate(username);
                        //if (correctPassword && correctEndDate)
                        if (correctPassword)
                        {
                            string roleId = findRole(username); //find the roleId.
                            setNumberOfTriesToZero();
                            success(username, roleId);
                        }
                        else if (!correctPassword)//if password incorrect, display the same message for security reasons.
                        {
                            int attempts = getNumberOfTries();
                            if (attempts > 2)
                            {
                                lockUser(loginId);
                                lblError.Visible = true;
                                lblError.Text = "This account has been locked. Please, contact the support to unluck it.";
                            }
                            else
                            {
                                lblError.Visible = true;
                                lblError.Text = errorMessage;
                            }
                        }
                    }
                    else
                    {
                        lblError.Visible = true;
                        lblError.Text = "This account has been locked. Please, contact the support to unluck it.";
                    }

                }
                else // if user does not exist in DB.
                {
                    lblError.Visible = true;
                    lblError.Text = errorMessage;
                }

            }
        }
        protected int checkIfEmpty()
        {
            int flag = 1;
            if (string.IsNullOrWhiteSpace(txtUsername.Text))//if user leaves blank.
            {
                flag = 0;
                lblUsernameError.Visible = true;
                lblUsernameError.Text = "Input Error: Type a username.";
            }
            else
            {
                lblUsernameError.Visible = false;
                lblUsernameError.Text = "";//clear text in case for another try username is filled but password not filled.
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                flag = 0;
                lblPasswordError.Visible = true;
                lblPasswordError.Text = "Input Error: Type a password.";
            }
            else
            {
                lblPasswordError.Visible = false;
                lblPasswordError.Text = "";
            }
            return flag;
        }
        protected Boolean checkIfExists(string username)
        {
            Boolean exists = true;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from logins where login_username like '" + username + "' ";
            int countUser = Convert.ToInt32(cmd.ExecuteScalar());
            if (countUser < 1) //user does not exist.
            {
                exists = false;
                lblError.Visible = true;
                lblError.Text = username + " does not exist";
            }
            connect.Close();
            return exists;
        }
        protected Boolean checkPassword(string username, string password)
        {
            Boolean correct = true;
            string hashed = Encryption.hash(password);
            password = "";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count (*) from logins where login_username like '" + username + "' and login_password like '" + hashed + "' ";
            int correctCombination = Convert.ToInt32(cmd.ExecuteScalar()); //count matching. result either 0 or 1.
            if (correctCombination == 0)
            {
                correct = false;
            }
            connect.Close();
            return correct;
        }
        protected string findRole(string username)
        {
            string roleId = "";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(roleId) from logins where login_username like '" + username + "'";
            int isThereRoleInDB = Convert.ToInt32(cmd.ExecuteScalar());
            if (isThereRoleInDB > 0) //means the user has a stored roleId in DB. This is to prevent DB error.
            {
                cmd.CommandText = "select roleId from logins where login_username like '" + username + "'";
                roleId = cmd.ExecuteScalar().ToString();
            }
            else //DB error: roleId was not stored for user. It is an extreme case, but it can happen somehow.
            {
                lblError.Visible = true;
                lblError.Text = "Database Error: Username has no role. Please contact the support.";
            }
            connect.Close();
            return roleId;
        }
        protected bool checkIfUserHasSecurityQuestions()
        {
            bool hasThem = true;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select loginId from Logins where login_username like '" + txtUsername.Text + "' ";
            string loginId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select count(*) from SecurityQuestions where loginId = '" + loginId + "' ";
            int questionsCount = Convert.ToInt32(cmd.ExecuteScalar());
            if (questionsCount < 3)
                hasThem = false;
            connect.Close();
            return hasThem;
        }
        protected void success(string username, string roleId)
        {
            //Generate a Token:
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            //Store the token in DB:
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select loginId from Logins where login_username like '" + txtUsername.Text + "' ";
            string loginId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "update logins set login_token = '" + token + "' where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
            token = token.Replace("'", "''");
            //addSession();
            Session.Add("username", username);
            Session.Add("roleId", roleId);
            Session.Add("loginId", loginId);
            Session.Add("token", token);
            //check if user has three security questions:
            bool hasSecurityQuestions = checkIfUserHasSecurityQuestions();
            //if the user has three security questions and changed password:
            if (hasSecurityQuestions)
                Response.Redirect("~/SecurityQuestions.aspx");
            //if the user doesn't have three security questions, create them:
            else
                Response.Redirect("~/CreateSecurityQuestions.aspx");
        }
        protected void clearSession()
        {

            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            Response.Redirect("~/");
        }
        protected int getNumberOfTries()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select login_attempts from Logins where loginId = '" + loginId + "' ";
            int attempts = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return attempts;
        }
        protected void addNumberOfTries()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "update Logins set login_attempts = login_attempts + 1  from Logins where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected void setNumberOfTriesToZero()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "update Logins set login_attempts = 0  from Logins where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected void lockUser(string id)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "update Logins set login_isActive = 0 where loginId like '" + id + "' ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //Go home:
            clearSession();
        }
    }
}