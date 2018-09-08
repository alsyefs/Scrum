using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum
{
    public partial class SecurityQuestions : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        static string g_id = "", g_token = "";
        //static string g_create_id = "";
        static int g_q1, g_q2, g_q3, g_final_question;
        static int pageRefreshes = 0, g_numOfTries = 0;
        string username, roleId, loginId, token;

        protected void Page_Load(object sender, EventArgs e)
        {
            Configuration config = new Configuration();
            conn = config.getConnectionString();
            connect = new SqlConnection(conn);
            //get number of wrong attempts from the database:
            g_numOfTries = getNumberOfTries();
            getSession();
            if (!IsPostBack)
            {
                CheckSession session = new CheckSession();
                bool correctSession = session.sessionIsCorrect(username, roleId, token);
                if (!correctSession)
                    clearSession();
                else
                {
                    updateToken();
                }
                pageRefreshes = 0;
                getQuestions();
            }
        }
        protected void updateToken()
        {
            //Generate a Token:
            byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            token = Convert.ToBase64String(time.Concat(key).ToArray());
            token = token.Replace("'", "''");
            //Store the token in DB:
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
        protected void goHome()
        {
            //addSession();
            //Generate a Token:
            updateToken();
            addSession();
            //addSession();
            //Session.Add("username", username);
            //Session.Add("roleId", roleId);
            //Session.Add("loginId", loginId);
            //Session.Add("token", token);
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
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select login_initial from Logins where loginId = '" + loginId + "' ";
            int initialValue = Convert.ToInt32(cmd.ExecuteScalar());
            if (initialValue == 0)
                initial = false;
            connect.Close();
            return initial;
        }
        protected void getQuestions()
        {
            if (!string.IsNullOrWhiteSpace(loginId))
                g_id = loginId;
            if (!string.IsNullOrWhiteSpace(token))
                g_token = token;
            pageRefreshes++;
            /////The below new code is to generate a random number between 0 - 2 at each call:
            Random random = new Random();
            int random_index = random.Next(3);
            getThreeQuestions(random_index);
            lblQ.Text = getQuestion(g_id, g_q1);
            //
            //g_q1 = questionIds[index.Next(1, 4) - 1];
            //The below guarantees us to have three non-redundant different random questions:
            //if (pageRefreshes == 1)
            //{
            //    getThreeQuestions(0);
            //    lblQ.Text = getQuestion(g_id, g_q1);  //db.Questions.Find(g_q1).QuestionText;
            //    //g_final_question = g_q1;
            //}
            //else if (pageRefreshes == 2)
            //{
            //    getThreeQuestions(1);
            //    lblQ.Text = getQuestion(g_id, g_q2); //db.Questions.Find(g_q2).QuestionText;
            //    //g_final_question = g_q2;
            //}
            //else
            //{
            //    getThreeQuestions(2);
            //    lblQ.Text = getQuestion(g_id, g_q3);
            //    pageRefreshes = 0;
            //    //g_final_question = g_q3;
            //}
        }
        protected string getQuestion(string g_id, int questionId)
        {
            string question = "";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select question_text from questions where questionID = '" + questionId + "' ";
            question = cmd.ExecuteScalar().ToString();
            //Get the questionId for the question text:
            cmd.CommandText = "select questionId from Questions where question_text like '" + question.Replace("'", "''") + "' ";
            string tempQuestionId = cmd.ExecuteScalar().ToString();
            g_final_question = Convert.ToInt32(tempQuestionId);
            connect.Close();
            return question;
        }
        protected void getThreeQuestions(int ran)
        {
            int[] questionIds = new int[3];
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select questionID from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY questionID ASC), * FROM SecurityQuestions where loginId like '" + g_id + "') as t  where rowNum = '" + 1 + "'";
            questionIds[0] = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select questionID from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY questionID ASC), * FROM SecurityQuestions where loginId like '" + g_id + "') as t  where rowNum = '" + 2 + "'";
            questionIds[1] = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select questionID from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY questionID ASC), * FROM SecurityQuestions where loginId like '" + g_id + "') as t  where rowNum = '" + 3 + "'";
            questionIds[2] = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            g_q1 = questionIds[ran];
            if (g_q1 == questionIds[3 - 1])
            {
                g_q2 = questionIds[1 - 1];
                g_q3 = questionIds[2 - 1];
            }
            else if (g_q1 == questionIds[2 - 1])
            {
                g_q2 = questionIds[3 - 1];
                g_q3 = questionIds[1 - 1];
            }
            else if (g_q1 == questionIds[1 - 1])
            {
                g_q2 = questionIds[2 - 1];
                g_q3 = questionIds[3 - 1];
            }
        }
        protected int getNumberOfTries()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select login_securityQuestionsAttempts from Logins where loginId = '" + loginId + "' ";
            int attempts = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return attempts;
        }
        protected void addNumberOfTries()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "update Logins set login_securityQuestionsAttempts = login_securityQuestionsAttempts + 1  from Logins where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected void setNumberOfTriesToZero()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "update Logins set login_securityQuestionsAttempts = 0  from Logins where loginId = '" + loginId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            //Add the number of tries to the database:
            addNumberOfTries();
            g_numOfTries = getNumberOfTries();
            Boolean correct = checkAnswer(g_final_question, txtA.Text, g_id);
            string id = g_id;
            if (correct)
            {
                pageRefreshes = 0;
                g_numOfTries = 0;
                setNumberOfTriesToZero();
                goHome();
            }
            else
            {
                if (g_numOfTries > 2)
                {
                    g_numOfTries = 0;
                    lblError.Visible = true;
                    lblError.Text = "Input Error: Your account has been locked for entering three incorrect answers.";
                    lockUser(id);
                    lblQ.Visible = false;
                    txtA.Visible = false;
                    btnSubmit.Visible = false;
                    //clearSession();
                    //Response.Redirect("~/");
                }
                else
                {
                    pageRefreshes++;
                    //The below guarantees us to have three non-redundant different random questions:
                    if (pageRefreshes == 1)
                    {
                        //getThreeQuestions();
                        lblQ.Text = getQuestion(g_id, g_q1);  //db.Questions.Find(g_q1).QuestionText;
                        //g_final_question = g_q1;
                    }
                    else if (pageRefreshes == 2)
                    {
                        lblQ.Text = getQuestion(g_id, g_q2); //db.Questions.Find(g_q2).QuestionText;
                        //g_final_question = g_q2;
                    }
                    else
                    {
                        lblQ.Text = getQuestion(g_id, g_q3);  //db.Questions.Find(g_q3).QuestionText;
                        pageRefreshes = 0;
                        //g_final_question = g_q3;
                    }
                    lblError.Visible = true;
                    lblError.Text = "Input Error: You have entered the wrong answer.";
                    //Response.Redirect(Request.RawUrl);
                    //return RedirectToAction("Index", "SecurityQuestions", new { id });                    
                }
            }

            //bool correctInput = checkInputs();
            //bool correctAnswer = checkAnswer();
            //if (correctInput)
            //{
            //    if (!correctAnswer)
            //        getAnotherQuestion();
            //    else
            //        goHome();
            //}
        }
        protected void lockUser(string id)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "update Logins set login_isActive = 0 where loginId like '" + id + "' ";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected Boolean checkAnswer(int question, string answer, string id)
        {
            Boolean correct = true;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            answer = answer.ToLower();
            //Encrypt new answers:
            string a_encrypted = Encryption.encrypt(answer);
            cmd.CommandText = "select count(*) from SecurityQuestions where loginId like '" + id + "' and questionID = '" + question + "' and securityQuestion_answer like '" + a_encrypted + "' ";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count == 0)
                correct = false;
            connect.Close();
            return correct;
        }
        protected bool checkInputs()
        {
            bool correct = true;
            CheckErrors errors = new CheckErrors();
            string a;
            if (!errors.validQuestion(txtA.Text, out a))
            {
                correct = false;
                lblAError.Visible = true;
                lblAError.Text = a;
                txtA.Focus();
            }
            return correct;
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            clearSession();
        }
    }
}