using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Scrum
{
    public class Layouts
    {
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
    }
}