﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Master
{
    public partial class CreateProject : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        static ArrayList searchedUsers = new ArrayList();
        static SortedSet<string> usersToAdd = new SortedSet<string>();
        static List<HttpPostedFile> files;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            if (!IsPostBack)
            {
                files = new List<HttpPostedFile>();
                calStartDate.SelectedDate = DateTime.Today;
                fileNames.InnerHtml = " ";
            }
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
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            hideErrorLabels();
            Boolean correct = checkInput();
            if (correct)
            {
                addNewEntry();
                clearInputs();
                sendEmail();
            }
        }
        protected void clearInputs()
        {
            txtTitle.Text = "";
            txtDescription.Text = "";
            lblImageError.Text = "";
            calStartDate.SelectedDate = DateTime.Today;
            fileNames.InnerHtml = " ";
        }
        protected void allowUserAccessProject(string projectId)
        {
            DateTime currentTime = DateTime.Now;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Note: by adding the user to this table, he/she is given access to the newly-created table.
            //Add the creator to UsersForProjects table: 
            cmd.CommandText = "insert into UsersForProjects (userId, projectId, usersForProjects_joinedTime) values " +
                "('" + userId + "', '" + projectId + "', '" + currentTime + "')";
            cmd.ExecuteScalar();
            //Add the list of selected developers to the user story:
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                string developerResponsible_userId = usersToAdd.ElementAt(i);
                cmd.CommandText = "insert into UsersForProjects (userId, projectId, usersForProjects_joinedTime) values " +
                    "('" + developerResponsible_userId + "', '" + projectId + "', '"+ currentTime + "')";
                cmd.ExecuteScalar();
            }
            connect.Close();
        }
        protected void sendEmail()
        {
            //Process process = new Process();
            //process.sendEmail(loginId, txtTitle.Text, txtDescription.Text);
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select user_email from Users where userId like '" + userId + "' ";
            string emailTo = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId like '" + userId + "' ";
            string name = cmd.ExecuteScalar().ToString();
            connect.Close();
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your project (" + txtTitle.Text + ") has been successfully submitted.\n" +
                "The below is the description you typed: \n\n\"" + txtDescription.Text + "\"\n\nBest regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
            Email email = new Email();
            email.sendEmail(emailTo, messageBody);
        }
        protected void addNewEntry()
        {
            int hasImage = 0;
            if (files.Count > 0)
            {
                storeImagesInServer();
                hasImage = 1;
            }
            //Store new topic as neither approved nor denied and return its ID:
            string projectId = storeProject(hasImage);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessProject(projectId);
            storeImagesInDB(projectId, hasImage, files);
            lblError.Visible = true;
            lblError.ForeColor = System.Drawing.Color.Green;
            lblError.Text = "The project has been successfully submitted and an email notification has been sent to you. <br/>";
            wait.Visible = false;
            try
            {
                if(files != null)
                    files.Clear();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
            calStartDate.SelectedDate = DateTime.Now;

        }
        protected void dayRender(object sender, DayRenderEventArgs e)
        {
            if (e.Day.Date < DateTime.Now.Date)
            {
                e.Day.IsSelectable = false;
                e.Cell.ForeColor = System.Drawing.Color.Gray;
            }
        }
        protected void storeImagesInDB(string projectId, int hasImage, List<HttpPostedFile> files)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Check if there is an image:
            if (hasImage == 1)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    string imageName = files[i].FileName.ToString().Replace("'", "''");
                    //Add to Images:
                    cmd.CommandText = "insert into Images (image_name) values ('" + imageName + "')";
                    cmd.ExecuteScalar();
                    //Get the image ID:
                    cmd.CommandText = "select imageId from Images where image_name like '" + imageName + "' ";
                    string imageId = cmd.ExecuteScalar().ToString();
                    //Add ImagesForTopics:
                    cmd.CommandText = "insert into ImagesForProjects (imageId, projectId) values ('" + imageId + "', '" + projectId + "')";
                    cmd.ExecuteScalar();
                }
            }
            connect.Close();
        }
        protected void storeImagesInServer()
        {
            //Loop through images and store each one of them:
            for (int i = 0; i < files.Count; i++)
            {
                string path = Server.MapPath("~/images/" + files[i].FileName);
                string fileExtension = System.IO.Path.GetExtension(files[i].FileName);
                if (fileExtension.ToLower() == ".jpg" || fileExtension.ToLower() == ".tiff" || fileExtension.ToLower() == ".jpeg" ||
                    fileExtension.ToLower() == ".png" || fileExtension.ToLower() == ".gif" || fileExtension.ToLower() == ".bmp" ||
                    fileExtension.ToLower() == ".tif")
                {
                    try
                    {
                        ////System.Drawing.Image sourceimage = System.Drawing.Image.FromStream(files[i].InputStream);
                        ////System.Drawing.Bitmap image = new System.Drawing.Bitmap(sourceimage);
                        ////System.Drawing.Bitmap image = new System.Drawing.Bitmap(files[i].InputStream);
                        ////System.Drawing.Bitmap image_copy = new System.Drawing.Bitmap(image);
                        //System.Drawing.Image img = RezizeImage(System.Drawing.Image.FromStream(files[i].InputStream), 500, 500);
                        //img.Save(path, ImageFormat.Jpeg);
                        System.Drawing.Bitmap image = new System.Drawing.Bitmap(files[i].InputStream);
                        System.Drawing.Bitmap image_copy = new System.Drawing.Bitmap(image);
                        System.Drawing.Image img = RezizeImage(System.Drawing.Image.FromStream(files[i].InputStream), 500, 500);
                        img.Save(path, ImageFormat.Jpeg);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.ToString());
                    }
                }
                else
                {
                    //If the file is not an image, just save it as it is:
                    files[i].SaveAs(path);
                }
            }
        }
        private MemoryStream BytearrayToStream(byte[] arr)
        {
            return new MemoryStream(arr, 0, arr.Length);
        }
        private System.Drawing.Image RezizeImage(System.Drawing.Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = Math.Max(xRatio, yRatio);
                int nnx = (int)Math.Floor(img.Width / ratio);
                int nny = (int)Math.Floor(img.Height / ratio);
                Bitmap cpy = new Bitmap(nnx, nny, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }
        protected string storeProject(int hasImage)
        {
            string projectId = "";
            DateTime entryTime = DateTime.Now;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            string description = txtDescription.Text.Replace("'", "''");
            description = description.Replace("\n", "<br />");
            description = description.Replace("\r", "&nbsp;&nbsp;&nbsp;&nbsp;");
            string project_name = txtTitle.Text.Replace("'", "''");
            project_name = project_name.Replace("'", "''");
            project_name = project_name.Replace("\n", "");
            project_name = project_name.Replace("\r", "&nbsp;&nbsp;&nbsp;&nbsp;");
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "insert into Projects (project_name, project_description, project_createdBy, project_createdDate, project_isTerminated, project_isDeleted, project_startedDate, project_hasImage) values " +
                "('" + project_name + "', '" + description + "', '" + userId + "', '" + entryTime + "', 0, 0, '" + calStartDate.SelectedDate + "', '" + hasImage + "') ";
            cmd.ExecuteScalar();
            cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY projectId ASC), * FROM [Projects] " +
                "where project_createdBy = '" + userId + "' and project_name like '" + project_name + "' and project_createdDate = '" + Layouts.getOriginalTimeFormat(entryTime.ToString()) + "' "
                + " and project_startedDate = '" + Layouts.getOriginalTimeFormat(calStartDate.SelectedDate.ToString()) + "' "
                + " and project_hasImage = '" + hasImage +
                "' and project_isDeleted = '0' and project_isTerminated = '0' " +
                " ) as t where rowNum = '1'";
            projectId = cmd.ExecuteScalar().ToString();
            connect.Close();
            return projectId;
        }
        protected Boolean checkInput()
        {
            Boolean correct = true;
            //Check for blank title:
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                correct = false;
                lblTitleError.Visible = true;
                lblTitleError.Text = "Input Error: Please type something for the title.";
            }
            //Check for blank description:
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                correct = false;
                lblDescriptionError.Visible = true;
                lblDescriptionError.Text = "Input Error: Please type something for the description.";
            }
            //Check for the correct start date and ensure that it's in the future:
            int differenceInDays = (calStartDate.SelectedDate - DateTime.Now).Days;
            if (differenceInDays < 0)
            {
                correct = false;
                lblStartDateError.Visible = true;
                lblStartDateError.Text = "Input Error: Please select a current or future start date.";
            }
            return correct;
        }
        protected void hideErrorLabels()
        {
            lblTitleError.Visible = false;
            lblDescriptionError.Visible = false;
            lblImageError.Visible = false;
            lblError.Visible = false;
            lblStartDateError.Visible = false;
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            addSession();
            Response.Redirect("Home");
        }
        protected bool checkFile(HttpPostedFile file)
        {
            bool correct = true;
            if (file.ContentLength > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(file.FileName);
                int filesize = FileUpload1.PostedFile.ContentLength;
                string filename = file.FileName;
                if (fileExtension.ToLower() != ".jpg" && fileExtension.ToLower() != ".tiff" && fileExtension.ToLower() != ".jpeg" &&
                    fileExtension.ToLower() != ".png" && fileExtension.ToLower() != ".gif" && fileExtension.ToLower() != ".bmp" &&
                    fileExtension.ToLower() != ".tif" && fileExtension.ToLower() != ".txt" && fileExtension.ToLower() != ".pdf"
                    && fileExtension.ToLower() != ".html")
                {
                    correct = false;
                    lblImageError.Visible = true;
                    lblImageError.Text = "File Error: The supported formats for files are: jpg, jpeg, tif, tiff, png, gif, bmp, " +
                        "txt, pdf, and HTML.";
                }

                if (filesize > 5242880)
                {
                    correct = false;
                    lblImageError.Visible = true;
                    lblImageError.Text = "File Error: The size of any uploaded file needs to be less than 5MB.";
                }
                if (string.IsNullOrWhiteSpace(filename))
                {
                    correct = false;
                    lblImageError.Visible = true;
                    lblImageError.Text = "File Error: The file you are trying to upload must have a name.";
                }
            }
            else if(file.ContentLength == 0 && file == null)
            {
                correct = false;
                lblImageError.Visible = true;
                lblImageError.Text = "File Error: Please select at least one file to upload.";
            }
            return correct;
        }
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (Request.Files.Count > 0)
            {
                int fileCount = Request.Files.Count;
                for (int i = 0; i < fileCount; i++)
                {
                    HttpPostedFile file = Request.Files[i];
                    if (checkFile(file))
                        files.Add(file);
                }
                if (fileCount == 1)
                    fileNames.InnerHtml = "You have successfully uploaded your file!";
                else if (fileCount > 1)
                    fileNames.InnerHtml = "You have successfully uploaded your files!";
                else
                    fileNames.InnerHtml = "You have not uploaded any files!";
            }
        }
        protected void btnAddUserToList_Click(object sender, EventArgs e)
        {
            SqlCommand cmd = connect.CreateCommand();
            lblListOfUsers.Text = "Added to the list:<br/>";
            int userIndex = drpFindUser.SelectedIndex;
            string selectedDeveloper_fullname = drpFindUser.SelectedValue;
            var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' };
            selectedDeveloper_fullname = selectedDeveloper_fullname.TrimStart(digits);
            string developerResponsible_userId = searchedUsers[userIndex].ToString();
            usersToAdd.Add(developerResponsible_userId);
            connect.Open();
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where userId = '" + usersToAdd.ElementAt(i) + "' ";
                string temp_name = cmd.ExecuteScalar().ToString();
                lblListOfUsers.Text += temp_name + "<br/>";
            }
            connect.Close();
            lblListOfUsers.Visible = true;
        }
        protected void drpFindUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            int userIndex = drpFindUser.SelectedIndex;
            string selectedUser = drpFindUser.SelectedValue;
            var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' };
            string result = selectedUser.TrimStart(digits);
            lblFindUserResult.Text = "Selected user: " + (userIndex + 1) + " " + result;
            lblFindUserResult.Visible = true;
        }
        protected void txtDeveloperResponsible_TextChanged(object sender, EventArgs e)
        {
            drpFindUser.Items.Clear();
            searchedUsers.Clear();
            int counter = 0;
            string searchKeyword = txtDeveloperResponsible.Text.Replace("'", "''");
            string[] words = searchKeyword.Split(' ');
            SortedSet<string> set_results = new SortedSet<string>();
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            foreach (string word in words)
            {
                cmd.CommandText = "select userId from Users where (user_firstname + ' ' + user_lastname) like '%" + word + "%'  ";
                string temp_Id = cmd.ExecuteScalar().ToString();
                set_results.Add(temp_Id);
            }
            for (int i = 0; i < set_results.Count; i++)
            {
                string temp_userId = set_results.ElementAt(i);
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where userId = '" + temp_userId + "' ";
                string temp_user = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select loginId from Users where userId = '" + temp_userId + "' ";
                int temp_loginId = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select login_isActive from Logins where loginId = '" + temp_loginId + "' ";
                int temp_isActive = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select roleId from Logins where loginId = '" + temp_loginId + "' ";
                int temp_roleId = Convert.ToInt32(cmd.ExecuteScalar());
                int int_loginId = Convert.ToInt32(loginId);
                int int_roleId = Convert.ToInt32(roleId);
                //add the searched user if his/her account is active, and the user is not the current user searching:
                if (temp_isActive == 1 && int_loginId != temp_loginId)
                {
                    searchedUsers.Add(temp_userId);
                    drpFindUser.Items.Add(++counter + " " + temp_user);
                }

            }
            connect.Close();
        }
    }
}