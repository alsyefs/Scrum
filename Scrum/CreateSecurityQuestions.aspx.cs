using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum
{
    public partial class CreateSecurityQuestions : System.Web.UI.Page
    {
        string username = "", roleId = "", loginId = "", token = "";
        Configuration config = new Configuration();
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            bool correctInput = checkInputs();
            if (correctInput)
            {
                store();
                goHome();
            }
        }
        protected void goHome()
        {
            ////addSession();
            //Session.Add("username", username);
            //Session.Add("roleId", roleId);
            //Session.Add("loginId", loginId);
            //Session.Add("token", token);
            //Generate a Token:
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            token = Convert.ToBase64String(time.Concat(key).ToArray());
            token = token.Replace("'", "''");
            //Store the token in DB:
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "update logins set login_token = '" + token + "' where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
            //addSession();
            Session.Add("username", username);
            Session.Add("roleId", roleId);
            Session.Add("loginId", loginId);
            Session.Add("token", token);
            //check if this is the user's initial login:
            bool initialLogin = checkIfUsersInitialLogin();
            if (initialLogin)
            {
                Response.Redirect("~/ChangePassword.aspx");
            }
            else
            {
                if (roleId.Equals("1"))
                {
                    //Admin.
                    Response.Redirect("~/Accounts/Admin/Home.aspx");
                }
                else if (roleId.Equals("2"))
                {
                    //Master.
                    Response.Redirect("~/Accounts/Master/Home.aspx");
                }
                else if (roleId.Equals("3"))
                {
                    //Developer.
                    Response.Redirect("~/Accounts/Developer/Home.aspx");
                }
            }
        }
        protected bool checkIfUsersInitialLogin()
        {
            bool initial = true;
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select login_initial from Logins where loginId = '" + loginId + "' ";
            int initialValue = Convert.ToInt32(cmd.ExecuteScalar());
            if (initialValue == 0)
                initial = false;
            connect.Close();
            return initial;
        }
        protected void hideAllErrors()
        {
            lblQ1Error.Visible = false;
            lblA1Error.Visible = false;
            lblQ2Error.Visible = false;
            lblA2Error.Visible = false;
            lblQ3Error.Visible = false;
            lblA3Error.Visible = false;
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            clearSession();
        }
        protected void updateToken()
        {
            //Generate a Token:
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            token = Convert.ToBase64String(time.Concat(key).ToArray());
            token = token.Replace("'", "''");
            //Store the token in DB:
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "update logins set login_token = '" + token + "' where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
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

        protected void Page_Load(object sender, EventArgs e)
        {
            getSession();
            if (!IsPostBack)
            {
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
                else
                {
                    updateToken();
                }
                fillDropLists();
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
        protected void fillDropLists()
        {
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from Questions";
            int totalQuestions = Convert.ToInt32(cmd.ExecuteScalar());
            string message = "Please select a question";
            drpQ1.Items.Add(message);
            drpQ2.Items.Add(message);
            drpQ3.Items.Add(message);
            for (int i = 1; i <= totalQuestions; i++)
            {
                cmd.CommandText = "select [question_text] from (SELECT rowNum = ROW_NUMBER() OVER(ORDER BY questionId ASC), * FROM [Questions] ) as t where rowNum = '" + i + "'";
                string question = cmd.ExecuteScalar().ToString();
                drpQ1.Items.Add(question);
                drpQ2.Items.Add(question);
                drpQ3.Items.Add(question);
            }
            connect.Close();
        }
        protected void store()
        {
            string q1 = drpQ1.SelectedValue.Replace("'", "''");
            string q2 = drpQ2.SelectedValue.Replace("'", "''");
            string q3 = drpQ3.SelectedValue.Replace("'", "''");
            txtA1.Text = txtA1.Text.Replace("'", "''");
            txtA2.Text = txtA2.Text.Replace("'", "''");
            txtA3.Text = txtA3.Text.Replace("'", "''");
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select loginId from Logins where login_username like '" + username + "' ";
            string temp_loginId = cmd.ExecuteScalar().ToString();
            //g_loginId = loginId;
            //Get questions' IDs:
            cmd.CommandText = "select questionId from questions where question_text like '" + q1 + "' ";
            string q1Id = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select questionId from questions where question_text like '" + q2 + "' ";
            string q2Id = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select questionId from questions where question_text like '" + q3 + "' ";
            string q3Id = cmd.ExecuteScalar().ToString();
            //Encrypt new answers:
            string a1_encrypted = Encryption.encrypt(txtA1.Text);
            string a2_encrypted = Encryption.encrypt(txtA2.Text);
            string a3_encrypted = Encryption.encrypt(txtA3.Text);
            //Insert answers with their questions' IDs:
            cmd.CommandText = "insert into SecurityQuestions (loginId, questionId, securityQuestion_answer) values " +
                "('" + temp_loginId + "', '" + q1Id + "', '" + a1_encrypted + "')";
            cmd.ExecuteScalar();
            cmd.CommandText = "insert into SecurityQuestions (loginId, questionId, securityQuestion_answer) values " +
                "('" + temp_loginId + "', '" + q2Id + "', '" + a2_encrypted + "')";
            cmd.ExecuteScalar();
            cmd.CommandText = "insert into SecurityQuestions (loginId, questionId, securityQuestion_answer) values " +
                "('" + temp_loginId + "', '" + q3Id + "', '" + a3_encrypted + "')";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected bool checkInputs()
        {
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            string a1, a2, a3;
            if (drpQ1.SelectedIndex == 0)
            {
                correct = false;
                lblQ1Error.Visible = true;
                lblQ1Error.Text = "Invalid input: Please select a question";
                drpQ1.Focus();
            }
            if (drpQ2.SelectedIndex == 0)
            {
                correct = false;
                lblQ2Error.Visible = true;
                lblQ2Error.Text = "Invalid input: Please select a question";
                drpQ2.Focus();
            }
            if (drpQ3.SelectedIndex == 0)
            {
                correct = false;
                lblQ3Error.Visible = true;
                lblQ3Error.Text = "Invalid input: Please select a question";
                drpQ3.Focus();
            }
            if (!errors.validAnswer(txtA1.Text, out a1))
            {
                correct = false;
                lblA1Error.Visible = true;
                lblA1Error.Text = a1;
                txtA1.Focus();
            }
            if (!errors.validAnswer(txtA2.Text, out a2))
            {
                correct = false;
                lblA2Error.Visible = true;
                lblA2Error.Text = a2;
                txtA2.Focus();
            }
            if (!errors.validAnswer(txtA3.Text, out a3))
            {
                correct = false;
                lblA3Error.Visible = true;
                lblA3Error.Text = a3;
                txtA3.Focus();
            }
            //Check specific questions if they are selected:
            if (drpQ1.SelectedValue.Equals("In what year were you born?") || drpQ1.SelectedValue.Equals("Which year did you graduate from high school?")
                || drpQ1.SelectedValue.Equals("In which year your immediate elder sibling was born?") || drpQ1.SelectedValue.Equals("In which year your immediate younger sibling was born?")
                || drpQ1.SelectedValue.Equals("What are the last four digits of your current phone number?") || drpQ1.SelectedValue.Equals("When did you graduate from the bachelor school?"))
            {
                if (!txtA1.Text.All(char.IsDigit) || txtA1.Text.Length != 4)
                {
                    correct = false;
                    lblA1Error.Visible = true;
                    lblA1Error.Text = "The answer must be a four-digits.";
                    txtA1.Focus();
                }
            }
            if (drpQ2.SelectedValue.Equals("In what year were you born?") || drpQ2.SelectedValue.Equals("Which year did you graduate from high school?")
                || drpQ2.SelectedValue.Equals("In which year your immediate elder sibling was born?") || drpQ2.SelectedValue.Equals("In which year your immediate younger sibling was born?")
                || drpQ2.SelectedValue.Equals("What are the last four digits of your current phone number?") || drpQ2.SelectedValue.Equals("When did you graduate from the bachelor school?"))
            {
                if (!txtA2.Text.All(char.IsDigit) || txtA2.Text.Length != 4)
                {
                    correct = false;
                    lblA2Error.Visible = true;
                    lblA2Error.Text = "The answer must be a four-digits.";
                    txtA2.Focus();
                }
            }
            if (drpQ3.SelectedValue.Equals("In what year were you born?") || drpQ3.SelectedValue.Equals("Which year did you graduate from high school?")
                || drpQ3.SelectedValue.Equals("In which year your immediate elder sibling was born?") || drpQ3.SelectedValue.Equals("In which year your immediate younger sibling was born?")
                || drpQ3.SelectedValue.Equals("What are the last four digits of your current phone number?") || drpQ3.SelectedValue.Equals("When did you graduate from the bachelor school?"))
            {
                if (!txtA3.Text.All(char.IsDigit) || txtA3.Text.Length != 4)
                {
                    correct = false;
                    lblA3Error.Visible = true;
                    lblA3Error.Text = "The answer must be a four-digits.";
                    txtA3.Focus();
                }
            }
            if (drpQ1.SelectedValue.Equals("What was your birth month and date?"))
            {
                if (!txtA1.Text.All(char.IsDigit) || txtA1.Text.Length != 4)
                {
                    correct = false;
                    lblA1Error.Visible = true;
                    lblA1Error.Text = "The answer must be four-digits, 2-digits month and 2-digits year.";
                    txtA1.Focus();
                }
            }
            if (drpQ2.SelectedValue.Equals("What was your birth month and date?"))
            {
                if (!txtA2.Text.All(char.IsDigit) || txtA2.Text.Length != 4)
                {
                    correct = false;
                    lblA2Error.Visible = true;
                    lblA2Error.Text = "The answer must be four-digits, 2-digits month and 2-digits year.";
                    txtA2.Focus();
                }
            }
            if (drpQ3.SelectedValue.Equals("What was your birth month and date?"))
            {
                if (!txtA3.Text.All(char.IsDigit) || txtA3.Text.Length != 4)
                {
                    correct = false;
                    lblA3Error.Visible = true;
                    lblA3Error.Text = "The answer must be four-digits, 2-digits month and 2-digits year.";
                    txtA3.Focus();
                }
            }
            return correct;
        }
        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            drpQ1.Items.Clear();
            drpQ2.Items.Clear();
            drpQ3.Items.Clear();
            fillDropLists();

            hideAllErrors();
            drpQ1.ClearSelection();
            txtA1.Text = "";
            drpQ2.ClearSelection();
            txtA2.Text = "";
            drpQ3.ClearSelection();
            txtA3.Text = "";
        }
        protected void disableSelectedQuestionFromOtherLists()
        {
            foreach (ListItem i in drpQ1.Items)
            {
                i.Enabled = true;
                if (drpQ2.SelectedValue.Equals(i.Text) || drpQ3.SelectedValue.Equals(i.Text))
                    drpQ1.Items.FindByText(i.Text).Enabled = false;
            }
            foreach (ListItem i in drpQ2.Items)
            {
                i.Enabled = true;
                if (drpQ1.SelectedValue.Equals(i.Text) || drpQ3.SelectedValue.Equals(i.Text))
                    drpQ2.Items.FindByText(i.Text).Enabled = false;
            }
            foreach (ListItem i in drpQ3.Items)
            {
                i.Enabled = true;
                if (drpQ1.SelectedValue.Equals(i.Text) || drpQ2.SelectedValue.Equals(i.Text))
                    drpQ3.Items.FindByText(i.Text).Enabled = false;
            }
            drpQ1.Items[0].Enabled = true;
            drpQ2.Items[0].Enabled = true;
            drpQ3.Items[0].Enabled = true;
        }
        protected void drpQ1_SelectedIndexChanged(object sender, EventArgs e)
        {
            disableSelectedQuestionFromOtherLists();
        }
        protected void drpQ2_SelectedIndexChanged(object sender, EventArgs e)
        {
            disableSelectedQuestionFromOtherLists();
        }
        protected void drpQ3_SelectedIndexChanged(object sender, EventArgs e)
        {
            disableSelectedQuestionFromOtherLists();
        }
    }
}