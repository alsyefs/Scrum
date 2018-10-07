using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

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
            //Check if the user viewing the project is the creator, or if the current user viewing is an admin:
            int int_roleId = Convert.ToInt32(roleId);
            if (createdByUserId.Equals(userId) || int_roleId == 1)
            {
                deleteCommand = "&nbsp;<button id='remove_button' type='button' onclick=\"removeProject('" + projectId + "', '" + createdByUserId + "')\">Remove Project </button>";
            }
            //if (int_roleId == 1)
            //{
                profileLink = "Created by <a href=\"Profile.aspx?id=" + createdByUserId + "\">" + createdBy + " </a>";
                terminateCommand = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button id='terminate_button' type='button' onclick=\"terminateProject('" + projectId + "', '" + createdByUserId + "')\">Terminate Project </button>";
            //}
            if (isTerminated == 1)
                terminateCommand = "";
            if (isDeleted == 1)
            {
                deleteCommand = "";
                terminateCommand = "";
            }
            string header = 
                "<div id=\"header\">" +
                "<div id=\"messageHead\">" +
                "&nbsp;\"" + project_name + "\" " + profileLink + " on (" + getTimeFormat(createdDate) + "), start date is on (" + getTimeFormat(startDate) + ")</div>" +
                "<div id=\"messageDescription\"><br/>" + project_description + "<br /><br/>" +
                imagesHTML + "<br/></div>" +
                deleteCommand +
                terminateCommand +
                "</div>";
            return header;
        }
        public static string getTimeFormat(string originalTime)
        {
            string format = "";
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