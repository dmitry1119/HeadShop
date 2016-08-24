using System;
using System.Globalization;

namespace RH.HeadShop.Helpers
{
    /// <summary> Helper class for working with string </summary>
    public static class StringConverter
    {
        public static float ToFloat(string str)
        {
            return (float)ToDouble(str, float.NaN);
        }
        public static float ToFloat(string str, float nanValue)
        {
            return (float)ToDouble(str, nanValue);
        }

        public static double ToDouble(object value)
        {
            return value == null ? double.NaN : ToDouble(value.ToString());
        }
        public static double ToDouble(string str)
        {
            return ToDouble(str, Double.NaN);
        }
        public static double ToDouble(string str, double nanValue)
        {
            if (string.IsNullOrEmpty(str) || str.Length == 0) return nanValue;
            var newSeparator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            var oldSeparator = newSeparator == "," ? "." : ",";
            var s = str.Replace(oldSeparator, newSeparator).Trim(new[] { ' ', '\t' });

            double result;
            if (double.TryParse(s, NumberStyles.Any, NumberFormatInfo.CurrentInfo, out result))
                return result;

            return nanValue;
        }

        public static DateTime ToDateTime(object value)
        {
            return ToDateTime(value, DateTime.MinValue);
        }
        public static DateTime ToDateTime(object value, DateTime nanValue)
        {
            return value == null ? nanValue : ToDateTime(value.ToString(), nanValue);
        }
        public static DateTime ToDateTime(string str)
        {
            return ToDateTime(str, DateTime.MinValue);
        }
        public static DateTime ToDateTime(string str, DateTime nanValue)
        {
            if (string.IsNullOrEmpty(str) || str.Length == 0)
                return nanValue;
            var newSeparator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            var oldSeparator = newSeparator == "," ? "." : ",";
            var s = str.Replace(oldSeparator, newSeparator).Trim(new[] { ' ', '\t' });

            DateTime result;
            return DateTime.TryParse(s, NumberFormatInfo.CurrentInfo, DateTimeStyles.None, out result) ? result : nanValue;
        }

        public static int ToInt(object value, int nanValue = int.MinValue)
        {
            return value == null ? nanValue : ToInt(value.ToString(), nanValue);
        }
        public static int ToInt(string str, int nanValue = int.MinValue)
        {
            var value = ToDouble(str);
            return double.IsNaN(value) ? nanValue : (int)value;
        }

        public static string DaysToStr(int days)
        {
            var mod = days % 10;
            if (mod == 0 || days > 10 && days < 20)
                return days + " days ";
            if (mod == 1)
                return days + " day ";
            if (mod < 5)
                return days + " days";
            return days + " days ";
        }
        public static string HoursToStr(int hours)
        {
            var mod = hours % 10;
            if (mod == 0 || hours > 10 && hours < 20)
                return hours + " hours ";
            if (mod == 1)
                return hours + " hour ";
            if (mod < 5)
                return hours + " hours ";
            return hours + " hours ";
        }
        public static string MinutesToStr(int minutes)
        {
            var mod = minutes % 10;
            if (mod == 0 || minutes > 10 && minutes < 20)
                return minutes + " minutes ";
            if (mod == 1)
                return minutes + " minute ";
            if (mod < 5)
                return minutes + " minutes ";
            return minutes + " minutes ";
        }
        public static string SecondsToStr(this int seconds)
        {
            var mod = seconds % 10;
            if (mod == 0 || seconds > 10 && seconds < 20)
                return seconds + " seconds ";
            if (mod == 1)
                return seconds + " second ";
            if (mod < 5)
                return seconds + " seconds ";
            return seconds + " seconds ";
        }
    }
}
