using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Scrum
{
    public class Layouts
    {
        public static string projectHeader(string projectId, string roleId, string loginId, string userId, string project_name, string project_description,
            string createdByUserId, string createdBy, string createdDate, string startDate, int isTerminated, int isDeleted, int hasImage, string imagesHTML)
        {
            string terminateCommand = "";
            string deleteCommand = "";
            string profileLink = "Created by " + createdBy + " ";
            string editLink = "";
            //Check if the user viewing the project is the creator, or if the current user viewing is an admin:
            int int_roleId = Convert.ToInt32(roleId);
            if (createdByUserId.Equals(userId) || int_roleId == 1)
            {
                deleteCommand = "&nbsp;<button id='remove_button' type='button' onclick=\"removeProject('" + projectId + "', '" + createdByUserId + "')\">Remove Project </button>";
                terminateCommand = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button id='terminate_button' type='button' onclick=\"terminateProject('" + projectId + "', '" + createdByUserId + "')\">Terminate Project </button>";
                editLink = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button id=\"edit_button\" type=\"button\" onclick=\"editProject('" + projectId + "')\" >Edit Project </button>";
                
            }
            profileLink = "Created by <a href=\"Profile.aspx?id=" + createdByUserId + "\">" + createdBy + " </a>";
            if (isTerminated == 1)
            {
                terminateCommand = "";
                editLink = "";
            }
            if (isDeleted == 1)
            {
                deleteCommand = "";
                terminateCommand = "";
                editLink = "";
            }
            string projectMembers = getProjectMembers(projectId);
            string header = 
                "<div id=\"header\" >" +
                "<div id=\"messageHead\" >" +
                "&nbsp;\"" + project_name + "\" " + profileLink + " on (" + getTimeFormat(createdDate) + "), start date is on (" + getTimeFormat(startDate) + ")</div>" +
                "<div id=\"messageDescription\"><br/>" + project_description + "<br /><br/>" + projectMembers +
                imagesHTML + "<br/></div>" +
                deleteCommand +
                terminateCommand +
                editLink +
                "</div>";
            return header;
        }
        protected static string getProjectMembers(string projectId)
        {
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            string members = "<hr>Current members of this project are:<br/>";
            string scrumMaster = "";
            List<string> names = new List<string>();
            cmd.CommandText = "select count(*) from UsersForProjects where projectId = '"+projectId+"' ";
            int totalUsersForProject = Convert.ToInt32(cmd.ExecuteScalar());
            for (int i = 1; i<= totalUsersForProject; i++)
            {
                cmd.CommandText = "select[userId] from(SELECT rowNum = ROW_NUMBER() OVER(ORDER BY UsersForProjectsId ASC), * FROM [UsersForProjects] where projectId = '" + projectId + "' ) as t where rowNum = '" + i + "'";
                string temp_userId = cmd.ExecuteScalar().ToString();
                cmd.CommandText = "select loginId from Users where userId = '" + temp_userId + "' ";
                int temp_loginId = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select roleId from Logins where loginId = '" + temp_loginId + "' ";
                int temp_roleId = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.CommandText = "select (user_firstname + ' ' + user_lastname) from Users where userId = '" + temp_userId + "' ";
                string temp_user_name = cmd.ExecuteScalar().ToString();
                if (temp_roleId == 2)//This user is a master:
                    scrumMaster = "<a href=\"Profile.aspx?id=" + temp_userId + "\">" + temp_user_name + " </a>";
                else
                    names.Add("<a href=\"Profile.aspx?id=" + temp_userId + "\">" + temp_user_name + " </a>");
            }
            connect.Close();
            members += "<table><tr><td>Scrum Master:</td><td>(" + scrumMaster + ")</td></tr>";
            foreach(string n in names)
            {
                members += "<tr><td>Developer:</td><td>("+n+")</td></tr>";
            }
            members +="</table><hr>";
            return members;
        }

        public static string userStoryHeader(string userStoryId, string roleId, string loginId, string userId, string userStory_uniqueId, string userStory_asARole,
            string createdByUserId, string createdBy, string userStory_iWant, string userStory_soThat, string userStory_dateIntroduced, string userStory_dateConsideredForImplementation,
            int userStory_hasImage, string imagesHTML, int userStory_isDeleted, string userStory_currentStatus, string[] userStoryMembers)
        {
            string deleteCommand = "";
            string profileLink = "Created by " + createdBy + " ";
            string editLink = "";
            //Check if the user viewing the project is the creator, or if the current user viewing is an admin:
            int int_roleId = Convert.ToInt32(roleId);
            if (createdByUserId.Equals(userId) || int_roleId == 1 || int_roleId == 2)
            {
                deleteCommand = "&nbsp;<button id='remove_button' type='button' onclick=\"removeUserStory('" + userStoryId + "', '" + createdByUserId + "')\">Remove User Story </button>";
                editLink = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button id=\"edit_button\" type=\"button\" onclick=\"editUserStory('" + userStoryId + "')\" >Edit User Story </button>";

            }
            profileLink = " Created by <a href=\"Profile.aspx?id=" + createdByUserId + "\">" + createdBy + " </a>";
            if (userStory_isDeleted == 1)
            {
                deleteCommand = "";
                editLink = "";
            }
            string usersForUserStory = getUserStoryMembers(userStoryId, userStoryMembers);
            
            string messageContent = "<div class=\"userStoryHeaderTable\"><table style=\"border: 1px solid black;\" >" +
                "<tr><th>User story ID</th><th>As a (type of user)</th> <th>I want to (some goal)</th> <th>So that (reason)</th> <th>Date introduced</th> <th>Date considered for implementation</th> <th>Developers responsible for</th> <th>Current status</th> </tr>" +
                "<tr> <td>"+ userStory_uniqueId + "</td> <td>"+userStory_asARole+"</td> <td>"+userStory_iWant+"</td> <td>"+userStory_soThat+"</td> <td>"+getTimeFormat(userStory_dateIntroduced) +"</td> <td>"+getTimeFormat(userStory_dateConsideredForImplementation) +"</td> <td>"+ usersForUserStory + "</td> <td>"+userStory_currentStatus+"</td> </tr>" +
                "</table></div>";
            string header =
                "<div id=\"header\" >" +
                "<div id=\"messageHead\" >" +
                " User Story ID:\""+ userStory_uniqueId + "\""+
                profileLink + " on (" + getTimeFormat(userStory_dateIntroduced) + "), start date is on (" + getTimeFormat(userStory_dateConsideredForImplementation) + ")</div>" +
                "<div id=\"messageDescription\"><br/>" + messageContent + "<br /><hr>" +
                imagesHTML + "<br/></div>" +
                deleteCommand +
                editLink +
                "</div>";
            return header;
        }
        protected static string getUserStoryMembers(string userStoryId, string[] userStoryMembers)
        {
            string members = "";
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            for (int i = 0; i < userStoryMembers.Length; i++)
            {
                cmd.CommandText = "select (user_firstname +' '+ user_lastname) from Users where userId = '" + userStoryMembers[i] + "' ";
                string temp_name = cmd.ExecuteScalar().ToString();
                members += "<a href=\"Profile.aspx?id=" + userStoryMembers[i] + "\">" + temp_name + " </a><br/>";
            }
            connect.Close();
            return members;
        }
        public static string sprintTaskHeader(string sprintTaskId, string roleId, string loginId, string userId, string userStoryId, string sprintTask_createdBy,
            string sprintTask_createdDate, string createdBy, string sprintTask_uniqueId, string sprintTask_taskDescription, string sprintTask_dateIntroduced, string sprintTask_dateConsideredForImplementation,
            int sprintTask_isDeleted, string imagesHTML, int sprintTask_hasImage, string sprintTask_currentStatus, string[] sprintTaskMembers, string userStoryUId,
            string sprintTask_dateCompleted, string sprintTask_editedBy, string sprintTask_editedDate, string sprintTask_previousVersion)
        {
            sprintTask_dateCompleted = (string.IsNullOrWhiteSpace(sprintTask_dateCompleted)) ? "Not completed" : getTimeFormat(sprintTask_dateCompleted);
            sprintTask_editedBy = (string.IsNullOrWhiteSpace(sprintTask_editedBy)) ? "Not edited" : sprintTask_editedBy;
            sprintTask_editedDate = (string.IsNullOrWhiteSpace(sprintTask_editedDate)) ? "Not edited" : getTimeFormat(sprintTask_editedDate);
            sprintTask_previousVersion = (string.IsNullOrWhiteSpace(sprintTask_previousVersion)) ? "No previous version" : sprintTask_previousVersion;
            string deleteCommand = "";
            string profileLink = "Created by " + createdBy + " ";
            string editLink = "";
            //Check if the user viewing the sprint task is the creator, or if the current user viewing is an admin:
            int int_roleId = Convert.ToInt32(roleId);
            if (sprintTask_createdBy.Equals(userId) || int_roleId == 1 || int_roleId == 2)
            {
                deleteCommand = "&nbsp;<button id='remove_button' type='button' onclick=\"removeSprintTask('" + sprintTaskId + "', '" + sprintTask_createdBy + "')\">Remove Sprint Task </button>";
                editLink = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button id=\"edit_button\" type=\"button\" onclick=\"editSprintTask('" + sprintTaskId + "')\" >Edit Sprint Task </button>";

            }
            profileLink = " Created by <a href=\"Profile.aspx?id=" + sprintTask_createdBy + "\">" + createdBy + " </a>";
            if (sprintTask_isDeleted == 1)
            {
                deleteCommand = "";
                editLink = "";
            }
            string usersForSprintTask = getSprintTaskMembers(sprintTaskId, sprintTaskMembers);

            string messageContent = "<div class=\"userStoryHeaderTable\"><table style=\"border: 1px solid black;\" >" +
                "<tr><th>Sprint Task ID</th><th>User story ID</th> <th>Task description</th> <th>Date introduced</th> <th>Date considered for implementation</th> <th>Date completed</th> <th>Developers responsible for</th> <th>Current status</th> </tr>" +
                "<tr> <td>" + sprintTask_uniqueId + "</td> <td>" + userStoryUId + "</td> <td>" + sprintTask_taskDescription + "</td> <td>" + getTimeFormat(sprintTask_dateIntroduced) + "</td> <td>" + getTimeFormat(sprintTask_dateConsideredForImplementation) + "</td> <td>" + sprintTask_dateCompleted + "</td> <td>" + usersForSprintTask  + "</td> <td>" + sprintTask_currentStatus + "</td> </tr>" +
                "</table></div>";
            string header =
                "<div id=\"header\" >" +
                "<div id=\"messageHead\" >" +
                " Sprint Task ID:\"" + sprintTask_uniqueId + "\"" +
                profileLink + " on (" + getTimeFormat(sprintTask_dateIntroduced) + "), start date is on (" + getTimeFormat(sprintTask_dateConsideredForImplementation) + ")</div>" +
                "<div id=\"messageDescription\"><br/>" + messageContent + "<br /><hr>" +
                imagesHTML + "<br/></div>" +
                deleteCommand +
                editLink +
                "</div>";
            return header;
        }
        protected static string getSprintTaskMembers(string sprintTaskId, string[] sprintTaskMembers)
        {
            string members = "";
            Configuration config = new Configuration();
            SqlConnection connect = new SqlConnection(config.getConnectionString());
            SqlCommand cmd = connect.CreateCommand();
            connect.Open();
            for (int i = 0; i < sprintTaskMembers.Length; i++)
            {
                cmd.CommandText = "select (user_firstname +' '+ user_lastname) from Users where userId = '" + sprintTaskMembers[i] + "' ";
                string temp_name = cmd.ExecuteScalar().ToString();
                members += "<a href=\"Profile.aspx?id=" + sprintTaskMembers[i] + "\">" + temp_name + " </a><br/>";
            }
            connect.Close();
            return members;
        }
        public static string getTimeFormat(string originalTime)
        {
            string format = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(originalTime))
                {
                    DateTime dateTime = Convert.ToDateTime(originalTime);
                    //DateTime dateTime = DateTime.ParseExact(originalTime, "mmddyyyyHH:mm:ss", CultureInfo.CurrentCulture);
                    string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
                    string day = dateTime.Day.ToString("00");
                    string year = dateTime.Year.ToString("0000");
                    string hours = dateTime.Hour.ToString("00");
                    //string minutes = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Minute);
                    string minutes = dateTime.Minute.ToString("00");
                    string seconds = dateTime.Second.ToString("00");
                    //string milliseconds = dateTime.Millisecond.ToString();
                    //Get AM/PM:
                    string dayOrNight = "AM";
                    int int_hours = Convert.ToInt32(hours);
                    if (int_hours > 12)
                        dayOrNight = "PM";
                    //Change the hour format to 12-hour-format:
                    if (int_hours == 0)
                        hours = "12";
                    else if (int_hours > 12)
                        hours = int_hours - 12 + "";
                    format = month + " " + day + ", " + year + " " + hours + ":" + minutes + ":" + seconds + " " + dayOrNight;
                }
            }catch(Exception e)
            {
                Console.WriteLine("Error: "+ e);
            }
            return format;
        }
        public static string getBirthdateFormat(string originalTime)
        {
            string format = "";
            if (!string.IsNullOrWhiteSpace(originalTime))
            {
                DateTime dateTime = Convert.ToDateTime(originalTime);
                //DateTime dateTime = DateTime.ParseExact(originalTime, "mmddyyyyHH:mm:ss", CultureInfo.CurrentCulture);
                string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
                string day = dateTime.Day.ToString("00");
                string year = dateTime.Year.ToString("0000");
                format = month + " " + day + ", " + year;
            }
            return format;
        }
        public static string phoneFormat(string phone_input)
        {
            //The input will arrive as XXXXXXXXXX and
            //the format output must be (XXX) XXX-XXXX with the space!
            string phone_output = "";
            string area_code = phone_input.Substring(0, 3);
            string three_digits = phone_input.Substring(3, 3);
            string four_digits = phone_input.Substring(6, 4);
            phone_output = "(" + area_code + ") " + three_digits + "-" + four_digits;
            return phone_output;
        }
        public static string getOriginalTimeFormat(string oldFormat)
        {
            string newFormat = "";
            if (!string.IsNullOrWhiteSpace(oldFormat))
            {
                DateTime dateTime = Convert.ToDateTime(oldFormat);
                newFormat = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dateTime);
            }
            return newFormat;
        }
    }
}