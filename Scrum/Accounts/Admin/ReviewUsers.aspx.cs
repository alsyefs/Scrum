using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Admin
{
    public partial class ReviewUsers : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            int countNewUsers = getTotalNewUsers();
            if (countNewUsers > 0)
            {
                lblMessage.Visible = false;
                createTable(countNewUsers);
            }
            else if (countNewUsers == 0)
            {
                lblMessage.Visible = true;
            }
        }
        protected int getTotalNewUsers()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //count the not-approved users:
            cmd.CommandText = "select count(*) from [Registrations]";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            return count;
        }
        protected void rebindValues()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string name = "", email = "", role = "";
            for (int row = 0; row < grdUsers.Rows.Count; row++)
            {
                int register_roleId = 0;
                if (role.Equals("Admin"))//1: Admin
                    register_roleId = 1;
                else if (role.Equals("Master")) //2: Master
                    register_roleId = 2;
                else if (role.Equals("Developer"))//3: Developer
                    register_roleId = 3;
                //Set links to review a user:
                name = grdUsers.Rows[row].Cells[0].Text;
                email = grdUsers.Rows[row].Cells[1].Text;
                role = grdUsers.Rows[row].Cells[2].Text;
                //Get the register ID:
                cmd.CommandText = "select [registerId] from [Registrations] where (register_firstname + ' ' + register_lastname) like '"+name+"' and " +
                    "register_email like '"+email+"' and register_role = '"+register_roleId+"' ";
                string id = cmd.ExecuteScalar().ToString();
                HyperLink nameLink = new HyperLink();
                HyperLink emailLink = new HyperLink();
                HyperLink roleLink = new HyperLink();
                nameLink.Text = name + " ";
                emailLink.Text = email + " ";
                roleLink.Text = email + " ";
                nameLink.NavigateUrl = "ReviewUser.aspx?id="+id;
                emailLink.NavigateUrl = "ReviewUser.aspx?id=" + id;
                roleLink.NavigateUrl = "ReviewUser.aspx?id=" + id;
                grdUsers.Rows[row].Cells[0].Controls.Add(nameLink);
                grdUsers.Rows[row].Cells[1].Controls.Add(emailLink);
                grdUsers.Rows[row].Cells[2].Controls.Add(roleLink);
            }
            connect.Close();
        }
        protected void createTable(int count)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Role", typeof(string));
            string id = "", name = "", email = "", role = "";
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            for (int i = 1; i <= count; i++)
            {
                //Get the register ID:
                cmd.CommandText = "select [registerId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY registerId ASC), *FROM [Registrations]) as t where rowNum = '" + i + "'";
                id = cmd.ExecuteScalar().ToString();
                //Get first name:
                cmd.CommandText = "select register_firstname from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY registerId ASC), *FROM [Registrations]) as t where rowNum = '" + i + "'";
                name = cmd.ExecuteScalar().ToString();
                //Get last name and add it to the end of the first name:
                cmd.CommandText = "select register_lastname from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY registerId ASC), *FROM [Registrations]) as t where rowNum = '" + i + "'";
                name = name + " " + cmd.ExecuteScalar().ToString();
                //Get email:
                cmd.CommandText = "select register_email from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY registerId ASC), *FROM [Registrations]) as t where rowNum = '" + i + "'";
                email = cmd.ExecuteScalar().ToString();
                //Get role (1 = Admin, 2 = Master, 3 = Developer):
                cmd.CommandText = "select register_roleId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY registerId ASC), *FROM [Registrations]) as t where rowNum = '" + i + "'";
                int tempRole = Convert.ToInt32(cmd.ExecuteScalar());
                if (tempRole == 1)
                    role = "Admin";
                else if (tempRole == 2)
                    role = "Master";
                else// if (tempRole == 3)
                    role = "Developer";
                dt.Rows.Add(name, email, role);
            }
            connect.Close();
            grdUsers.DataSource = dt;
            grdUsers.DataBind();
            rebindValues();
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
        protected void grdUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grdUsers.PageIndex = e.NewPageIndex;
            grdUsers.DataBind();
            rebindValues();
        }
    }
}