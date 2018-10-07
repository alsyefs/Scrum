using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Scrum.Accounts
{
    public class Process
    {
        static List<HttpPostedFile> files;
        static Configuration config = new Configuration();
        SqlConnection connect = new SqlConnection(config.getConnectionString());
        public void allowUserAccessProject(string projectId, string loginId)
        {
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            //Note: by adding the user to this table, he/she is given access to the newly-created table.
            //Add the creator to UsersForProjects table: 
            cmd.CommandText = "insert into UsersForProjects (userId, projectId, usersForProjects_joinedTime) values " +
                "('" + userId + "', '" + projectId + "', '" + DateTime.Now + "')";
            cmd.ExecuteScalar();
            connect.Close();
        }
        public void sendEmail(string loginId, string title, string description)
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
            string messageBody = "Hello " + name + ",\nThis email is to notify you that your project (" + title + ") has been successfully submitted and will be reviewed.\n" +
                "You will be notified by email once the review is complete. The below is the description you typed: \n\n\"" + description + "\"\n\nBest regards,\nScrum Support\nScrum.UWL@gmail.com";
            Email email = new Email();
            email.sendEmail(emailTo, messageBody);
        }
        public bool addNewEntry(string loginId, string description, string project_name, DateTime startDate)
        {
            bool completed = true;
            int hasImage = 0;
            if (files.Count > 0)
            {
                storeImagesInServer();
                hasImage = 1;
            }
            //Store new topic as neither approved nor denied and return its ID:
            string projectId = storeProject(hasImage, description, project_name, loginId, startDate);
            //Allow the creator of topic to access it when it's approved and add the new tags to the topic:
            allowUserAccessProject(projectId, loginId);
            storeImagesInDB(projectId, hasImage, files);
            return completed;
            //lblError.Visible = true;
            //lblError.ForeColor = System.Drawing.Color.Green;
            //lblError.Text = "The project has been successfully submitted and an email notification has been sent to you. <br/>" +
            //    "Your project will be reviewed and you will be notified by email once the review is complete.";
        }
        public void storeImagesInDB(string projectId, int hasImage, List<HttpPostedFile> files)
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
        public void storeImagesInServer()
        {
            //Loop through images and store each one of them:
            for (int i = 0; i < files.Count; i++)
            {
                string path = System.Web.HttpContext.Current.Server.MapPath("~/images/" + files[i].FileName);
                //string path = Server.MapPath("~/images/" + files[i].FileName);
                string fileExtension = System.IO.Path.GetExtension(files[i].FileName);
                if (fileExtension.ToLower() == ".jpg" || fileExtension.ToLower() == ".tiff" || fileExtension.ToLower() == ".jpeg" ||
                    fileExtension.ToLower() == ".png" || fileExtension.ToLower() == ".gif" || fileExtension.ToLower() == ".bmp" ||
                    fileExtension.ToLower() == ".tif")
                {
                    try
                    {
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
        public string storeProject(int hasImage, string description, string project_name,
            string loginId, DateTime startDate)
        {
            string projectId = "";
            DateTime entryTime = DateTime.Now;
            connect.Open();
            SqlCommand cmd = connect.CreateCommand();
            description = description.Replace("'", "''");
            description = description.Replace("\n", "<br />");
            description = description.Replace("\r", "&nbsp;&nbsp;&nbsp;&nbsp;");
            project_name = project_name.Replace("'", "''");
            project_name = project_name.Replace("'", "''");
            project_name = project_name.Replace("\n", "");
            project_name = project_name.Replace("\r", "&nbsp;&nbsp;&nbsp;&nbsp;");
            //Get the current user's ID:
            cmd.CommandText = "select userId from Users where loginId = '" + loginId + "' ";
            string userId = cmd.ExecuteScalar().ToString();
            cmd.CommandText = "insert into Projects (project_name, project_description, project_createdBy, project_createdDate, project_isTerminated, project_isDeleted, project_startedDate, project_hasImage) values " +
                "('" + project_name + "', '" + description + "', '" + userId + "', '" + entryTime + "', 0, 0, '" + startDate + "', '" + hasImage + "') ";
            cmd.ExecuteScalar();
            cmd.CommandText = "select [projectId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY projectId ASC), * FROM [Projects] " +
                "where project_createdBy = '" + userId + "' and project_name like '" + project_name + "' and project_createdDate = '" + Layouts.getOriginalTimeFormat(entryTime.ToString()) + "' "
                + " and project_startedDate = '" + Layouts.getOriginalTimeFormat(startDate.ToString()) + "' "
                + " and project_hasImage = '" + hasImage +
                "' and project_isDeleted = '0' and project_isTerminated = '0' " +
                " ) as t where rowNum = '1'";
            projectId = cmd.ExecuteScalar().ToString();
            connect.Close();
            return projectId;
        }
    }
}