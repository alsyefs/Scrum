using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum
{
    public partial class Admin : System.Web.UI.MasterPage
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialAccess();
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
            lblAlerts.Text = "Alerts (" + countTotalAlerts() + ")";
            lblName.Text = getName();
        }
        protected string getName()
        {
            string name = "";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select count(*) from Users where loginId = '" + loginId + "' ";
            int countLogins = Convert.ToInt32(cmd.ExecuteScalar());
            if (countLogins > 0)
            {
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where loginId = '" + loginId + "' ";
                name = cmd.ExecuteScalar().ToString();
            }
            else
                name = "Account";
            connect.Close();
            return name;
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
        public int countTotalAlerts()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count users to be approved:
            cmd.CommandText = "select count(*) from registrations";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            //Maybe later add more alerts:
            //count += someValue;
            connect.Close();
            return count;
        }
    }
}