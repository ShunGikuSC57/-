using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace EntityMaker
{
    /// <summary>
    /// Class to describe the extension method
    /// </summary>
    public static class Extensions
    {
        #region Generic
        /// <summary>
        /// Determines whether the object is null or not.
        /// </summary>
        /// <param name="obj">target object</param>
        /// <returns>null:true not null:false</returns>
        public static bool IsNull<T>(this T target) => target == null;

        /// <summary>
        /// Determines if the object is non-null.
        /// </summary>
        /// <param name="obj">target object</param>
        /// <returns>Not null: true null: false</returns>
        public static bool IsNotNull<T>(this T target) => target != null;
        #endregion

        #region String
        /// <summary>
        /// Verifies the existence of a value in a string and returns the result.
        /// </summary>
        /// <param name="s">target string</param>
        /// <returns>With value: true Null, empty, space only: false</returns>
        public static bool HasValue(this string s) => !string.IsNullOrWhiteSpace(s);

        /// <summary>
        /// Verifies if a string is a number and returns the result.
        /// </summary>
        /// <param name="s">target string</param>
        /// <returns>数値:true 数値以外:false</returns>
        public static bool IsNumeric(this string s) => s.ToDecimal().HasValue;

        /// <summary>
        /// Converts a string to a 16-bit signed integer (null-allowed type).
        /// </summary>
        /// <param name="s">target string</param>
        /// <returns>Nullable short</returns>
        public static short? ToShort(this string s)
        {
            short? result = null;
            if (s.HasValue())
                if (short.TryParse(s.Replace(",", string.Empty), out short n)) result = n;
            return result;
        }

        /// <summary>
        /// Converts a string to a 32-bit signed integer (null-allowed type).
        /// </summary>
        /// <param name="s">target string</param>
        /// <returns>Nullable int</returns>
        public static int? ToInt(this string s)
        {
            int? result = null;
            if (s.HasValue())
                if (int.TryParse(s.Replace(",", string.Empty), out int n)) result = n;
            return result;
        }

        /// <summary>
        /// Converts a string to a 64-bit signed integer (null-allowed type).
        /// </summary>
        /// <param name="s">target string</param>
        /// <returns>Nullable long</returns>
        public static long? ToLong(this string s)
        {
            long? result = null;
            if (s.HasValue())
                if (long.TryParse(s.Replace(",", string.Empty), out long n)) result = n;
            return result;
        }

        /// <summary>
        /// Converts a string to a double-precision floating-point number (null-allowed type).
        /// </summary>
        /// <param name="s">target string</param>
        /// <returns>Nullable double</returns>
        public static double? ToDouble(this string s)
        {
            double? result = null;
            if (s.HasValue())
                if (double.TryParse(s.Replace(",", string.Empty), out double n)) result = n;
            return result;
        }

        /// <summary>
        /// Converts a string to an equivalent decimal number (null-allowed type).
        /// </summary>
        /// <param name="s">target string</param>
        /// <returns>Nullable decimal</returns>
        public static decimal? ToDecimal(this string s)
        {
            decimal? result = null;
            if (s.HasValue())
                if (decimal.TryParse(s.Replace(",", string.Empty), out decimal n)) result = n;
            return result;
        }

        /// <summary>
        /// Converts a string to a DateTime object (null-allowed type).
        /// See method remarks for supported formats.
        /// </summary>
        /// <param name="s">target string</param>
        /// <returns>Nullable DateTime</returns>
        /// <remarks>
        /// 対応フォーマット
        /// ・yyyyMMdd
        /// ・yyyyMMddHHmmss
        /// ・y/m/d
        /// ・yyyy/MM/dd
        /// ・yyyy/MM/dd HH:mm
        /// ・yyyy/MM/dd HH:mm:ss
        /// </remarks>
        public static DateTime? ToDateTime(this string s)
        {
            DateTime? result = null;
            if (s.HasValue())
            {
                try
                {
                    var length = s.Split('/').Length;
                    // Determines if there is a format (with /:).
                    if (length == 1)
                    {
                        var format = string.Empty;
                        if (s.Length == 8)
                            format = "yyyyMMdd";
                        else if (s.Length == 14)
                            format = "yyyyMMddHHmmss";
                        if (format.HasValue() && DateTime.TryParseExact(s, format, null, DateTimeStyles.None, out DateTime d))
                            result = d;
                    }
                    else if (length == 3)
                    {
                        // If it is timed, the value should be 3, so the decision is fixed at 3.
                        // Don't use TryParse with formatting because "1/7/1" will become "2001/7/1".
                        var dt = s.Split(' ');
                        if (dt.Length == 1)
                        {
                            var date = s.Split('/');
                            result = new DateTime(date[0].ToInt() ?? 1, date[1].ToInt() ?? 1, date[2].ToInt() ?? 1);
                        }
                        else if (dt.Length == 2)
                        {
                            var date = dt[0].Split('/');
                            var time = dt[1].Split(':');
                            if (time.Length == 2)
                                // hour, minute
                                result = new DateTime(date[0].ToInt() ?? 1, date[1].ToInt() ?? 1, date[2].ToInt() ?? 1, time[0].ToInt() ?? 0, time[1].ToInt() ?? 0, 0);
                            else if (time.Length == 3)
                                // hour, minute, second
                                result = new DateTime(date[0].ToInt() ?? 1, date[1].ToInt() ?? 1, date[2].ToInt() ?? 1, time[0].ToInt() ?? 0, time[1].ToInt() ?? 0, time[2].ToInt() ?? 0);
                        }
                    }
                }
                catch { }
            }

            return result;
        }
        #endregion

        #region DateTime
        /// <summary>
        /// Converts a DateTime to a string in yyyy/MM/dd format.
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static string ToStringDate(this DateTime d) => d.ToString("yyyy/MM/dd");

        /// <summary>
        /// Converts a DateTime to a string in yyyyMMdd format.
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static string ToStringDateNoFormat(this DateTime d) => d.ToString("yyyyMMdd");

        /// <summary>
        /// Converts a DateTime to a string in the format "yyyy年MM月dd日".
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static string ToStringDateJapanese(this DateTime d) => d.ToString("yyyy年MM月dd日");

        /// <summary>
        /// Converts a DateTime to a string in the format "gy年M月d日".
        /// Before the first year of the Meiji era (1868) is an exception and will return an empty string.
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static string ToStringDateJapaneseYear(this DateTime d)
        {
            var result = string.Empty;
            try
            {
                var ci = new CultureInfo("ja-JP", false);
                ci.DateTimeFormat.Calendar = new JapaneseCalendar();
                result = d.ToString("gy年M月d日", ci);
            }
            catch { }
            return result;
        }

        /// <summary>
        /// Converts a DateTime to a string in HH:mm:ss format.
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static string ToStringTime(this DateTime d) => d.ToString("HH:mm:ss");

        /// <summary>
        /// Converts a DateTime to a string in yyyy/MM/dd HH:mm format.
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static string ToStringDateTimeHM(this DateTime d) => d.ToString("yyyy/MM/dd HH:mm");

        /// <summary>
        /// Converts a DateTime to a string in yyyy/MM/dd HH:mm:ss format.
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static string ToStringDateTime(this DateTime d) => d.ToString("yyyy/MM/dd HH:mm:ss");

        /// <summary>
        /// Converts a DateTime to a string in yyyyMMddHHmmss format.
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static string ToStringDateTimeNoFormat(this DateTime d) => d.ToString("yyyyMMddHHmmss");

        /// <summary>
        /// Returns the last day of the month (the last day of the month) including the date.
        /// </summary>
        /// <param name="d">Target DateTime object</param>
        /// <returns>Date string</returns>
        public static DateTime GetEndOfMonth(this DateTime d) => new DateTime(d.Year, d.Month, 1).AddMonths(1).AddDays(-1);
        #endregion

        #region Decimal
        /// <summary>
        /// Get the smallest integer value rounded up to the nearest decimal place of the target decimal number.
        /// </summary>
        /// <param name="d">Target value</param>
        /// <returns>The smallest integer value rounded up.</returns>
        public static decimal Ceiling(this decimal d) => Math.Ceiling(d);

        /// <summary>
        /// Gets the largest integer value of the target decimal number, rounded down to the nearest whole number.
        /// </summary>
        /// <param name="d">Target value</param>
        /// <returns>The largest integer value that can be truncated</returns>
        public static decimal Floor(this decimal d) => Math.Floor(d);

        /// <summary>
        /// Rounds the target decimal number to the nearest integer value.
        /// </summary>
        /// <param name="d">Target value</param>
        /// <returns>Integer value of the result of rounding</returns>
        public static decimal Round(this decimal d) => Math.Round(d);

        /// <summary>
        /// Rounds the target decimal number to the specified decimal fraction.
        /// </summary>
        /// <param name="d">Target value</param>
        /// <param name="digits">Number of decimal places of the return value</param>
        /// <returns>Integer value of the result of rounding</returns>
        public static decimal Round(this decimal d, int digits) => Math.Round(d, digits);
        #endregion

        #region Boolean
        /// <summary>
        /// Get the value (false for null) from a Nullable Boolean.
        /// </summary>
        /// <param name="b">Target Nullable Boolean</param>
        /// <returns>Boolean value</returns>
        public static bool GetValue(this bool? b) => b ?? false;
        #endregion

        #region Json
        /// <summary>
        /// Serialize the object to Json.
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="src">target object</param>
        /// <returns>Json</returns>
        public static string ToJson<T>(this T target)
        {
            var result = string.Empty;
            try
            {
                using (var ms = new MemoryStream())
                using (var sr = new StreamReader(ms))
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    serializer.WriteObject(ms, target);
                    ms.Position = 0;
                    var json = sr.ReadToEnd();
                    result = json;
                }
            }
            catch { }
            return result;
        }

        /// <summary>
        /// Deserializes a string to the specified type.
        /// </summary>
        /// <typeparam name="T">Type to convert</typeparam>
        /// <param name="json">Json</param>
        /// <returns>The specified type object</returns>
        public static T? ToEntity<T>(this string json)
        {
            var result = default(T);
            try
            {
                if (json.HasValue())
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(T));
                        result = (T?)serializer.ReadObject(ms);    
                    }
                }
            }
            catch { }
            return result;
        }
        #endregion
    }
}
