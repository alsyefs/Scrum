using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scrum.Accounts.Master
{
    public partial class EditTestCase : System.Web.UI.Page
    {
        static string conn = "";
        SqlConnection connect = new SqlConnection(conn);
        string username, roleId, loginId, token;
        string previousPage = "";
        static string g_sprintTaskId = "";
        static string g_userStoryId = "";
        static string g_projectId = "";
        static string g_testCaseId = "";
        static List<HttpPostedFile> files;
        static SortedSet<string> parametersToAdd = new SortedSet<string>();
        protected void Page_Load(object sender, EventArgs e)
        {
            initialPageAccess();
            CheckErrors check = new CheckErrors();
            try
            {
                g_testCaseId = Request.QueryString["id"];
                if (!check.checkTestCaseAccess(g_testCaseId, loginId))
                    goBack();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                goBack();
            }
            if (!IsPostBack)
            {
                files = new List<HttpPostedFile>();
                fillInputs();
            }
            //The below to be used whenever needed in the other page:
            Session.Add("projectId", g_projectId);
            Session.Add("userStoryId", g_userStoryId);
            Session.Add("sprintTaskId", g_sprintTaskId);
            Session.Add("testCaseId", g_sprintTaskId);
            updateUniqueId();
            checkIfTestCaseDeleted();
        }
        protected void checkIfTestCaseDeleted()
        {
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            //Count the existance of the sprint task:
            cmd.CommandText = "select count(*) from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count > 0)//if count > 0, then the project ID exists in DB.
            {
                cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + g_testCaseId + "' ";
                string actual_creatorId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select testCase_isDeleted from TestCases where testCaseId = '" + g_testCaseId + "' ";
                int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
                if (isDeleted == 1)
                {
                    //Hide the submit buttons:
                    btnAddParameter.Visible = false;
                    btnRemoveParameter.Visible = false;
                    btnUpload.Visible = false;
                    btnSaveTestCase.Visible = false;
                }
            }
            connect.Close();
        }
        protected void updateUniqueId()
        {
            string newId = "";
            try
            {
                SqlCommand cmd = connect.CreateCommand();
                connect.Open();

                cmd.CommandText = "select top(1) testCase_uniqueId from TestCases where testCaseId = '" + g_testCaseId + "' order by testCaseId desc ";
                newId = cmd.ExecuteScalar().ToString();
                connect.Close();
            }
            catch (Exception e)
            {
                connect.Close();
                Console.WriteLine("Error: " + e);
            }
            try
            {
                if (newId.Contains("."))
                {
                    List<string> theId = newId.Split('.').ToList<string>();
                    int tempId = Convert.ToInt32(theId.ElementAt(1));
                    ++tempId;
                    string result = Convert.ToInt32(theId.ElementAt(0)) + "." + tempId;
                    newId = result.ToString();
                }
                else
                {
                    int tempId = Convert.ToInt32(newId.Replace(" ", ""));
                    newId = tempId.ToString() + ".1";
                    newId = newId.Replace(" ", "");
                }
            }
            catch (Exception e)
            {
                newId = "1";
                Console.WriteLine("Error: " + e);
            }
            txtUniqueTestCaseID.Text = newId;
        }
        protected void fillInputs()
        {
            parametersToAdd.Clear();
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            cmd.CommandText = "select testCase_createdBy from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string createdByUserId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_createdDate from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string createddate = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_uniqueId from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string testCaseUID = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_testCaseScenario from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string testCaseScenario = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_expectedOutput from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string expectedOutput = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select testCase_isDeleted from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int isDeleted = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select testCase_hasImage from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int hasImage = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select testCase_currentStatus from TestCases where testCaseId = '" + g_testCaseId + "' ";
            string currentStatus = cmd.ExecuteScalar().ToString();

            cmd.CommandText = "select count(testCase_editedBy) from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int countEditedBy = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(testCase_editedDate) from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int countEditedDate = Convert.ToInt32(cmd.ExecuteScalar());
            cmd.CommandText = "select count(testCase_previousVersion) from TestCases where testCaseId = '" + g_testCaseId + "' ";
            int countPreviousVersion = Convert.ToInt32(cmd.ExecuteScalar());
            string testCase_editedBy = "", testCase_editedDate = "", testCase_previousVersion = "";
            if (countEditedBy > 0)
            {
                cmd.CommandText = "select testCase_editedBy from TestCases where testCaseId = '" + g_testCaseId + "' ";
                testCase_editedBy = cmd.ExecuteScalar().ToString();
            }
            if (countEditedDate > 0)
            {
                cmd.CommandText = "select testCase_editedDate from TestCases where testCaseId = '" + g_testCaseId + "' ";
                testCase_editedDate = cmd.ExecuteScalar().ToString();
            }
            if (countPreviousVersion > 0)
            {
                cmd.CommandText = "select testCase_previousVersion from TestCases where testCaseId = '" + g_testCaseId + "' ";
                testCase_previousVersion = cmd.ExecuteScalar().ToString();
            }
            //Loop through the developers for the selected Test Case:
            cmd.CommandText = "select count(*) from Parameters where testcaseId = '" + g_testCaseId + "' ";
            int parametersForTestCase = Convert.ToInt32(cmd.ExecuteScalar());
            string inputParameters = "";
            for (int j = 1; j <= parametersForTestCase; j++)
            {
                cmd.CommandText = "select parameter_name from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY parameterId ASC), * FROM Parameters where testCaseId = '" + g_testCaseId + "' ) as t where rowNum = '" + j + "'";
                string parameter_name = cmd.ExecuteScalar().ToString();
                parametersToAdd.Add(parameter_name);
                drpInputParametersList.Items.Add(parameter_name);
                if (j == 1)
                    inputParameters = parameter_name;
                else
                    inputParameters = inputParameters + ",\n" + parameter_name;
            }
            //Convert the createdByUserId to a name:
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + createdByUserId + "' ";
            string createdByName = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            string imagesHTML = "";
            if (hasImage == 1)
            {
                cmd.CommandText = "select count(*) from ImagesForTestCases where testCaseId = '" + g_testCaseId + "' ";
                int totalImages = Convert.ToInt32(cmd.ExecuteScalar());
                for (int i = 1; i <= totalImages; i++)
                {
                    cmd.CommandText = "select [imageId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY imageId ASC), * FROM [ImagesForTestCases] where testCaseId = '" + g_testCaseId + "') as t where rowNum = '" + i + "'";
                    string imageId = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = "select image_name from Images where imageId = '" + imageId + "' ";
                    string image_name = cmd.ExecuteScalar().ToString();
                    imagesHTML = imagesHTML + "<a href='../../images/" + image_name + "' target=\"_blank\">" + image_name + "</a> <br />";
                }
            }
            cmd.CommandText = "select userStory_uniqueId from UserStories where userStoryId = '" + g_userStoryId + "' ";
            string userStoryUId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select sprintTask_uniqueId from SprintTasks where sprintTaskId = '" + g_sprintTaskId + "' ";
            string sprintTaskUId = cmd.ExecuteScalar().ToString();
            connect.Close();
            txtUniqueUserStoryID.Text = userStoryUId;
            txtUniqueSprintTaskID.Text = sprintTaskUId;
            txtTestCaseScenario.Text = testCaseScenario;
            txtExpectedOutput.Text = expectedOutput;
            drpCurrentStatus.SelectedValue = currentStatus;
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
            try
            {
                Session.Add("projectId", g_projectId);
                Session.Add("userStoryId", g_userStoryId);
                Session.Add("sprintTaskId", g_sprintTaskId);
                Session.Add("testCaseId", g_testCaseId);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }
        protected void getSession()
        {
            username = (string)(Session["username"]);
            roleId = (string)(Session["roleId"]);
            loginId = (string)(Session["loginId"]);
            token = (string)(Session["token"]);
            try
            {
                g_projectId = (string)(Session["projectId"]);
                g_userStoryId = (string)(Session["userStoryId"]);
                g_sprintTaskId = (string)(Session["sprintTaskId"]);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
            }
        }
        protected void goBack()
        {
            addSession();
            string backLink = "";
            try
            {
                g_testCaseId = (string)(Session["testCaseId"]);
                backLink = "ViewTestCase.aspx?id=" + g_testCaseId;
            }
            catch (Exception e)
            {
                backLink = "Home.aspx";
                Console.WriteLine("Error: " + e);
            }
            Response.Redirect(backLink);
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
        protected void btnAddParameter_Click(object sender, EventArgs e)
        {
            lblInputParametersError.Visible = false;
            //Check text for paramter:
            if (string.IsNullOrWhiteSpace(txtInputParameters.Text))
            {
                lblInputParametersError.Visible = true;
                lblInputParametersError.Text = "Invalid input: Please type something for the input paramters before adding it.";
            }
            else
            {
                bool noDuplicateParameter = true;
                string parameter = txtInputParameters.Text;
                parameter = parameter.TrimStart(' ').TrimEnd(' ');
                //check if there is another duplicate parameter:
                foreach (var p in drpInputParametersList.Items)
                {
                    if (p.ToString().Equals(parameter))
                    {
                        noDuplicateParameter = false;
                    }
                }
                if (noDuplicateParameter)
                {
                    //If passed the check, add the newly entered text:
                    parameter = parameter.Replace("'", "''");
                    drpInputParametersList.Items.Add(parameter);
                    parametersToAdd.Add(parameter);
                    //Clear the text entered for parameters and leave the second copy in the ListBox
                    txtInputParameters.Text = "";
                }
                else
                {
                    lblInputParametersError.Visible = true;
                    lblInputParametersError.Text = "Invalid input: The parameter you entered already exists, please type another parameter.";
                }
            }
        }
        protected void btnRemoveParameter_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = drpInputParametersList.Items.Count; i >= 0; i--)
                {
                    int indexToRemove = drpInputParametersList.SelectedIndex;
                    if (indexToRemove > -1)
                    {
                        parametersToAdd.Remove(parametersToAdd.ElementAt(indexToRemove));
                        drpInputParametersList.Items.RemoveAt(indexToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }
        }
        protected void hideErrorLabels()
        {
            lblUniqueSprintTaskIDError.Visible = false;
            lblUniqueUserStoryIDError.Visible = false;
            lblCurrentStatusError.Visible = false;
            fileNames.InnerHtml = "";
            lblTestCaseScenarioError.Visible = false;
            lblInputParametersError.Visible = false;
            lblInputParametersListError.Visible = false;
            lblUniqueTestCaseIDError.Visible = false;
            lblExpectedOutputError.Visible = false;
            lblAddTestCaseMessage.Visible = false;
        }
        protected void btnSaveTestCase_Click(object sender, EventArgs e)
        {
            hideErrorLabels();
            if (!string.IsNullOrWhiteSpace(lblInputParametersList.Text))
            {
                lblInputParametersList.Visible = true;
            }
            if (correctInput())
            {
                addNewEntry();
                sendEmail();
                clearNewTestCaseInputs();
            }
            updateUniqueId();
        }
        protected void clearNewTestCaseInputs()
        {
            try
            {
                txtExpectedOutput.Text = "";
                txtInputParameters.Text = "";
                txtTestCaseScenario.Text = "";
                drpCurrentStatus.SelectedIndex = 1;
                drpInputParametersList.Items.Clear();
                if (files != null)
                    files.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
        }
        protected void sendEmail()
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            Email email = new Email();
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select user_email from Users where userId like '" + userId + "' ";
            string emailTo = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId like '" + userId + "' ";
            string name = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "select project_name from Projects where projectId = '" + g_projectId + "' ";
            string project_name = cmd.ExecuteScalar().ToString();
            connect.Close();
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your test case#(" + txtUniqueTestCaseID.Text + ") for sprint task#(" + txtUniqueSprintTaskID.Text + ") has been successfully updated for the project (" + project_name + ") under the user story#(" + txtUniqueUserStoryID.Text + ").\n" +
                "\n\nBest regards,\nScrum Tool Support\nScrum.UWL@gmail.com";
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
            //Store new sprint task as neither approved nor denied and return its ID:
            string testCaseId = storeTestCase(hasImage);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessTestCase(testCaseId);
            storeImagesInDB(testCaseId, hasImage, files);
            lblAddTestCaseMessage.Visible = true;
            lblAddTestCaseMessage.ForeColor = System.Drawing.Color.Green;
            lblAddTestCaseMessage.Text = "The test case has been successfully updated and an email notification has been sent to you. <br/>";
            wait.Visible = false;
        }
        protected void allowUserAccessTestCase(string new_testCaseId)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Delete all the users assigned for this project:
            cmd.CommandText = "delete from UsersForTestCases where testCaseId = '" + new_testCaseId + "' ";
            cmd.ExecuteScalar();
            //Note: by adding the user to this table, he/she is given access to the newly-created table.
            //Add the creator to UsersForTestCase table.
            //Add the current user to the test case:
            cmd.CommandText = "insert into UsersForTestCases (userId, testCaseId, usersForTestCases_isNotified) values " +
                    "('" + userId + "', '" + new_testCaseId + "', '0')";
            cmd.ExecuteScalar();
            connect.Close();
        }
        protected string storeTestCase(int hasImage)
        {
            string new_testCaseId = "";
            DateTime createdDate = DateTime.Now;
            string testCaseUId = txtUniqueTestCaseID.Text.Replace(" ", "");
            testCaseUId = testCaseUId.Replace("'", "''");
            string sprintTaskUId = txtUniqueSprintTaskID.Text.Replace(" ", "");
            sprintTaskUId = sprintTaskUId.Replace("'", "''");
            string userStoryUId = txtUniqueUserStoryID.Text.Replace(" ", "");
            userStoryUId = userStoryUId.Replace("'", "''");
            string testScenario = txtTestCaseScenario.Text.Replace("'", "''");
            string expectedOutput = txtExpectedOutput.Text.Replace("'", "''");
            string currentStatus = drpCurrentStatus.SelectedValue;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string createdBy = cmd.ExecuteScalar().ToString();
            //Mark to original Test Case as deleted:
            cmd.CommandText = "update TestCases set testCase_currentStatus = 'Revised', testCase_editedBy = '" + createdBy + "', " +
                "testCase_editedDate = '" + createdDate + "', testCase_isDeleted = '1'  " +
                "where testCaseId = '" + g_testCaseId + "' ";
            cmd.ExecuteScalar();
            //Store the new user story in the database:
            cmd.CommandText = "insert into TestCases (sprintTaskId, testCase_createdBy, testCase_createdDate, testCase_uniqueId, testCase_testCaseScenario, testCase_expectedOutput, " +
                "testCase_hasImage, testCase_currentStatus, testCase_previousVersion) values " +
               "('" + g_sprintTaskId + "', '" + createdBy + "', '" + createdDate + "', '" + testCaseUId + "', '" + testScenario + "', '" + expectedOutput + "',  " +
               " '" + hasImage + "',  '" + currentStatus + "', '"+ g_sprintTaskId + "') ";
            cmd.ExecuteScalar();
            //Get the ID of the newly stored test case from the database:
            cmd.CommandText = "select testCaseId from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY testCaseId ASC), * FROM TestCases " +
                "where sprintTaskId = '" + g_sprintTaskId + "' and testCase_createdBy = '" + createdBy + "' and testCase_createdDate = '" + Layouts.getOriginalTimeFormat(createdDate.ToString()) + "' "
                + " and testCase_uniqueId like '" + testCaseUId + "'  "
                + " and testCase_hasImage = '" + hasImage + "' and testCase_currentStatus like '" + currentStatus + "' "
                + " and testCase_isDeleted = '0' "
                + " ) as t where rowNum = '1'";
            new_testCaseId = cmd.ExecuteScalar().ToString();
            //Store the parameters:
            foreach (var p in drpInputParametersList.Items)
            {
                cmd.CommandText = "insert into Parameters (testCaseId, parameter_name) values ('" + new_testCaseId + "', '" + p.ToString().Replace("'", "''") + "')";
                cmd.ExecuteScalar();
            }
            connect.Close();
            return new_testCaseId;
        }
        protected void storeImagesInDB(string testcaseId, int hasImage, List<HttpPostedFile> files)
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
                    //Add in ImagesForUserStories:
                    cmd.CommandText = "insert into ImagesForTestCases (imageId, testcaseId) values ('" + imageId + "', '" + testcaseId + "')";
                    cmd.ExecuteScalar();
                }
            }
            connect.Close();
        }
        protected void btnGoBackToListOfTestCases_Click(object sender, EventArgs e)
        {
            addSession();
            goBack();
        }
        protected bool correctInput()
        {
            bool correct = true;
            if (string.IsNullOrWhiteSpace(txtExpectedOutput.Text))
            {
                correct = false;
                lblExpectedOutputError.Text = "Invalid input: Please type something for the Expected output .";
                lblExpectedOutputError.Visible = true;
            }
            if (string.IsNullOrWhiteSpace(txtTestCaseScenario.Text))
            {
                correct = false;
                lblTestCaseScenarioError.Text = "Invalid input: Please type something for the test case scenario .";
                lblTestCaseScenarioError.Visible = true;
            }
            if (drpCurrentStatus.SelectedIndex == 0)
            {
                correct = false;
                lblCurrentStatusError.Visible = true;
                lblCurrentStatusError.Text = "Invalid input: Please select a status for this test case.";
            }
            if (drpInputParametersList.Items.Count == 0)
            {
                correct = false;
                lblInputParametersListError.Visible = true;
                lblInputParametersListError.Text = "Invalid input: Please add at least one parameter for this test case.";
            }
            return correct;
        }
    }
}