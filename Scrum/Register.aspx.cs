using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //check input
            //if input = true, send it to database code to store it
        }
        
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            bool correctInput = checkInput();
            if (correctInput)
            {
                //send it all to database code:
                storeInput();

                //prompt a message that everything was successful:
                lblResult.ForeColor = System.Drawing.Color.Green;
                lblResult.Visible = true;
                lblResult.Text = "Your application has been successfully submitted!";
                clearInputs(Page);
            }
            else
                lblResult.Visible = false;
        }
        protected void clearInputs(Control p1)
        {
            foreach (Control ctrl in p1.Controls)
            {
                if (ctrl is TextBox)
                {
                    TextBox t = ctrl as TextBox;
                    if (t != null)
                        t.Text = String.Empty;
                }
                else
                {
                    if (ctrl.Controls.Count > 0)
                        clearInputs(ctrl);
                    else
                    {
                        drpRole.ClearSelection();
                    }
                }
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //Go home:
            Response.Redirect("~/");
        }
        protected void hideAllErrorMessages()
        {
            lblFirstnameError.Visible = false;
            lblLastnameError.Visible = false;
            lblEmailError.Visible = false;
            lblPhoneError.Visible = false;
            lblRoleError.Visible = false;
        }
        protected bool checkInput()
        {
            bool correct = true;
            hideAllErrorMessages();
            //First, check if there is any special character within any field:
            CheckErrors error = new CheckErrors();
            //Check first name:
            string firstnameError = "";
            if (!error.validFirstName(txtFirstname.Text, out firstnameError))
            {
                correct = false;
                lblFirstnameError.Visible = true;
                lblFirstnameError.Text = firstnameError;
                txtFirstname.Focus();
            }
            //Check last name:
            string lastnameError = "";
            if (!error.validLastName(txtLastname.Text, out lastnameError))
            {
                correct = false;
                lblLastnameError.Visible = true;
                lblLastnameError.Text = lastnameError;
                txtLastname.Focus();
            }
            //Check email:
            string emailError = "";
            if (!error.validEmail(txtEmail.Text, out emailError))
            {
                correct = false;
                lblEmailError.Visible = true;
                lblEmailError.Text = emailError;
                txtEmail.Focus();
            }
            
            //Check phone:
            string phoneError = "";
            if (!error.validPhone(txtPhone.Text, out phoneError))
                {
                    correct = false;
                    lblPhoneError.Visible = true;
                    lblPhoneError.Text = phoneError;
                    txtPhone.Focus();
                }
            
            //Check selected role:
            string roleError = "";
            if (!error.validRole(drpRole.SelectedIndex, out roleError))
            {
                correct = false;
                lblRoleError.Visible = true;
                lblRoleError.Text = roleError;
                drpRole.Focus();
            }
            return correct;
        }

        //The below is the database code which can be separated in another class if needed:
        protected void storeInput()
        {
            //store role as an int to the temp table in the database: Admin = 1, Physician = 2, and Patient = 3.
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            connect.Open();
            //If special characters are needed, then the below can be used for all strings
            //to replace a special character which can cause SQL errors.
            txtFirstname.Text = txtFirstname.Text.Replace("'", "''");
            txtLastname.Text = txtLastname.Text.Replace("'", "''");
            txtEmail.Text = txtEmail.Text.Replace("'", "''");
            txtPhone.Text = txtPhone.Text.Replace("'", "''");
            txtPhone.Text = txtPhone.Text.Replace(" ", ""); //Replace empty spaces.
            txtPhone.Text = txtPhone.Text.Replace("'", "''");//Replace the single quotation character
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "insert into Registrations(register_firstname, register_lastname, register_email, register_roleId, register_phone)" +
                " values ('" + txtFirstname.Text + "', '" + txtLastname.Text + "', '" + txtEmail.Text + "'," +
                " '" + drpRole.SelectedIndex + "','" + txtPhone.Text + "' ) ";
            cmd.ExecuteScalar();
            connect.Close();
        }

        protected void drpRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
    }
}