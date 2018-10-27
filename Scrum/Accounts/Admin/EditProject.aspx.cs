using System;
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

namespace Scrum.Accounts.Admin
{
    public partial class EditProject : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        string previousPage = "";
        string projectId = "";
        static ArrayList searchedUsers = new ArrayList();
        static SortedSet<string> usersToAdd = new SortedSet<string>();
        static List<HttpPostedFile> files;
        protected void Page_Load(object sender, EventArgs e)
        {
            initialAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                projectId = Request.QueryString["projectId"];
                if (!check.checkProjectAccess(projectId, loginId))
                    goBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                goBack();
            }
            //Check if the user editing is the master of this project or an admin:
            if (!isAccessAllowed())
                goBack();
            if (!IsPostBack)
            {
                files = new List<HttpPostedFile>();
                //calStartDate.SelectedDate = DateTime.Today;
                fileNames.InnerHtml = " ";
                try
                {
                    fillInputs();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: "+ex);
                }
            }
        }
        protected void fillInputs()
        {
            usersToAdd.Clear();
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select project_name from Projects where projectId = '"+projectId+"' ";
            string project_name = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_description from Projects where projectId = '" + projectId + "' ";
            string project_description = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_startedDate from Projects where projectId = '" + projectId + "' ";
            string project_startedDate = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_hasImage from Projects where projectId = '" + projectId + "' ";
            int hasImage = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(*) from UsersForProjects where projectId = '" + projectId + "' ";
            int countUsersForProject = Convert.ToInt32(cmd.ExecuteScalar());
            for (int i=1; i<= countUsersForProject; i++)
            {
                //Add the user IDs of the project users:
                cmd.CommandText = "select[userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY UsersForProjectsId ASC), * FROM [UsersForProjects] where projectId = '" + projectId + "' ) as t where rowNum = '" + i+"'";
                string temp_userId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select loginId from Users where userId = '" + temp_userId + "' ";
                int temp_loginId = Convert.ToInt32(cmd.ExecuteScalar());
                int int_loginId = Convert.ToInt32(loginId);
                //If the user in the list is not me, add him/her to the set:
                if (int_loginId != temp_loginId)
                {
                    usersToAdd.Add(temp_userId);
                }
            }
            //The second is to guarantee that the sorted set has been filled without duplicates, then we fill the below list:
            for (int i=0; i< usersToAdd.Count; i++)
            {
                string temp_userId = usersToAdd.ElementAt(i);
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + temp_userId + "'  ";
                string temp_user_name = cmd.ExecuteScalar().ToString();
                drpProjectUsers.Items.Add(temp_user_name);
            }
            if(hasImage == 1)
            {
                //Add the images to a listbox:
                //Allow the master to remove the images one by one, or by selecting them all at once then clicking remove:
            }
            connect.Close();
            txtTitle.Text = project_name;
            txtDescription.Text = project_description;
            calStartDate.SelectedDate = Convert.ToDateTime(project_startedDate);
            calStartDate.TodaysDate = Convert.ToDateTime(project_startedDate);
            hideOrShowCalendar();
        }
        protected void drpProjectUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        protected void btnRemoveProjectUser_Click(object sender, EventArgs e)
        {
            lblRemoveUserError.Visible = false;
            lblRemoveUserError.Text = "";
            try
            {
                for (int i = drpProjectUsers.Items.Count; i >= 0; i--)
                {
                    int indexToRemove = drpProjectUsers.SelectedIndex;
                    if (indexToRemove > -1)
                    {
                        //Check if the selected developer is involved in something in the project:
                        SqlCommand cmd = connect.CreateCommand();
                        connect.Open();
                        cmd.CommandText = "select count(*) from Projects " +
                            "inner join userstories on UserStories.projectId = Projects.projectId " +
                            "inner join UsersForUserStories on UserStories.userStoryId = UsersForUserStories.userStoryId " +
                            "where userId = '" + usersToAdd.ElementAt(indexToRemove) + "' and UserStories.projectId = '" + projectId + "' ";
                        int totalUserStories = Convert.ToInt32(cmd.ExecuteScalar());
                        cmd.CommandText = "select count(*) " +
                            "from UserStories " +
                            "inner join SprintTasks on SprintTasks.userStoryId = UserStories.userStoryId " +
                            "inner join UsersForSprintTasks on SprintTasks.sprintTaskId = UsersForSprintTasks.sprintTaskId " +
                            "where userId = '" + usersToAdd.ElementAt(indexToRemove) + "' and UserStories.projectId = '" + projectId + "' ";
                        int totalSprintTasks = Convert.ToInt32(cmd.ExecuteScalar());
                        cmd.CommandText = "select count(*) from SprintTasks " +
                            "inner join UserStories on SprintTasks.userStoryId = UserStories.userStoryId " +
                            "inner join TestCases on TestCases.sprintTaskId = SprintTasks.sprintTaskId " +
                            "inner join UsersForTestCases on TestCases.testCaseId = UsersForTestCases.testCaseId " +
                            "where userId = '" + usersToAdd.ElementAt(indexToRemove) + "' and UserStories.projectId = '" + projectId + "' ";
                        int totalTestCases = Convert.ToInt32(cmd.ExecuteScalar());
                        connect.Close();
                        //If he/she is not involved in anything, allow the deletion:
                        if (totalUserStories == 0 && totalSprintTasks == 0 && totalTestCases == 0)
                        {
                            drpProjectUsers.Items.RemoveAt(indexToRemove);
                            usersToAdd.Remove(usersToAdd.ElementAt(indexToRemove));
                        }
                        else
                        {
                            string name = drpProjectUsers.SelectedItem.ToString();
                            lblRemoveUserError.Visible = true;
                            lblRemoveUserError.Text = name + " is invloved in \"" + totalUserStories + "\" user stories, \"" + totalSprintTasks + "\" sprint tasks," +
                                " and \"" + totalTestCases + "\" test cases.<br/>";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

        }
        protected bool isAccessAllowed()
        {
            bool allowed = true;
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select userId from Users where loginId = '"+loginId+"' ";
            int userId = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select project_createdBy from Projects where projectId = '"+projectId+"' ";
            int creatorId = Convert.ToInt32(cmd.ExecuteScalar());
            int int_roleId = Convert.ToInt32(roleId);
            //If the user trying to edit is not the creator and not an admin:
            if (userId != creatorId && int_roleId != 1)
                allowed = false;
            connect.Close();
            return allowed;
        }
        protected void goBack()
        {
            addSession();
            //if (!string.IsNullOrWhiteSpace(previousPage)) Response.Redirect(previousPage);
            //else Response.Redirect("Home.aspx");
            Response.Redirect("ViewProject.aspx?id="+projectId);
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
            previousPage = previous_page;
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
            //Delete all the users assigned for this project:
            cmd.CommandText = "delete from UsersForProjects where projectId = '"+projectId+"' ";
            cmd.ExecuteScalar();
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
                    "('" + developerResponsible_userId + "', '" + projectId + "', '" + currentTime + "')";
                cmd.ExecuteScalar();
            }
            connect.Close();
        }
        protected void sendEmail()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select user_email from Users where userId like '" + userId + "' ";
            string emailTo = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId like '" + userId + "' ";
            string name = cmd.ExecuteScalar().ToString();
            connect.Close();
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your project (" + txtTitle.Text + ") has been successfully updated.\n" +
                "The below is the description you typed: \n\n\"" + txtDescription.Text + "\"\n\nBest regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
            Email email = new Email();
            email.sendEmail(emailTo, messageBody);
        }
        protected void addNewEntry()
        {
            int hasImage = 0;
            //if (files.Count > 0)
            //{
            //    storeImagesInServer();
            //    hasImage = 1;
            //}
            //Store new topic as neither approved nor denied and return its ID:
            string projectId = storeProject(hasImage);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessProject(projectId);
            //storeImagesInDB(projectId, hasImage, files);
            lblError.Visible = true;
            lblError.ForeColor = System.Drawing.Color.Green;
            lblError.Text = "The project has been successfully updated and an email notification has been sent to you. <br/>";
            wait.Visible = false;
            try
            {
                if (files != null)
                    files.Clear();
            }
            catch (Exception e)
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
            string projectId = this.projectId;
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
            string startDate = calStartDate.SelectedDate.ToString();
            cmd.CommandText = "update Projects set project_name = '" + project_name + "', project_description='" + description + "' ," +
                "project_startedDate = '" + startDate + "' where projectId = '" + projectId + "' ";
            cmd.ExecuteScalar();
            connect.Close();
            return projectId;
        }
        protected void hideOrShowCalendar()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select count(*) from UserStories where projectId = '" + projectId + "' ";
            int countUserStoriesForProject = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();
            calStartDate.Visible = (countUserStoriesForProject > 0) ? false : true;
            lblStartDate.Visible = (calStartDate.Visible) ? true : false;
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
            if (calStartDate.Visible)
            {
                //Check for the correct start date and ensure that it's in the future:
                int differenceInDays = (calStartDate.SelectedDate - DateTime.Now).Days;
                if (differenceInDays < 0)
                {
                    correct = false;
                    lblStartDateError.Visible = true;
                    lblStartDateError.Text = "Input Error: Please select a current or future start date.";
                }
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
            goBack();
            //Response.Redirect("Home");
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
            else if (file.ContentLength == 0 && file == null)
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
            //This is to clear the list of users, and then fill it later from a set to avoid duplicates:
            drpProjectUsers.Items.Clear();
            for (int i = 0; i < usersToAdd.Count; i++)
            {
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from users where userId = '" + usersToAdd.ElementAt(i) + "' ";
                string temp_name = cmd.ExecuteScalar().ToString();
                lblListOfUsers.Text += temp_name + "<br/>";
                //Fill the list
                drpProjectUsers.Items.Add(temp_name);
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