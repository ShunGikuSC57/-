using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace EntityMaker
{
    /// <summary>
    /// 拡張メソッドを記述するクラス
    /// </summary>
    public static class Extensions
    {
        #region Generic
        /// <summary>
        /// そのオブジェクトがnullかどうかを判定します。
        /// </summary>
        /// <param name="obj">対象オブジェクト</param>
        /// <returns>null:true nullじゃない:false</returns>
        public static bool IsNull<T>(this T target) => target == null;

        /// <summary>
        /// そのオブジェクトがnull以外かどうかを判定します。
        /// </summary>
        /// <param name="obj">対象オブジェクト</param>
        /// <returns>nullじゃない:true null:false</returns>
        public static bool IsNotNull<T>(this T target) => target != null;
        #endregion

        #region String
        /// <summary>
        /// 文字列に値が存在するか検証し結果を返却します。
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>値あり:true Null、空文字、空白のみ:false</returns>
        public static bool HasValue(this string str) => !string.IsNullOrWhiteSpace(str);

        /// <summary>
        /// 文字列が数値か検証し結果を返却します。
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>数値:true 数値以外:false</returns>
        public static bool IsNumeric(this string str) => str.ToDecimal().HasValue;

        /// <summary>
        /// 文字列を16ビット符号付整数(null許容型)に変換します。
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>16ビット符号付整数(null許容型)</returns>
        public static int? ToShort(this string str)
        {
            short? result = null;
            if (str.HasValue())
                if (short.TryParse(str.Replace(",", string.Empty), out short n)) result = n;
            return result;
        }

        /// <summary>
        /// 文字列を32ビット符号付整数(null許容型)に変換します。
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>32ビット符号付整数(null許容型)</returns>
        public static int? ToInt(this string str)
        {
            int? result = null;
            if (str.HasValue())
                if (int.TryParse(str.Replace(",", string.Empty), out int n)) result = n;
            return result;
        }

        /// <summary>
        /// 文字列を64ビット符号付整数(null許容型)に変換します。
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>64ビット符号付整数(null許容型)</returns>
        public static long? ToLong(this string str)
        {
            long? result = null;
            if (str.HasValue())
                if (long.TryParse(str.Replace(",", string.Empty), out long n)) result = n;
            return result;
        }

        /// <summary>
        /// 文字列を倍精度浮動小数点数(null許容型)に変換します。
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>64ビット符号付整数(null許容型)</returns>
        public static double? ToDouble(this string str)
        {
            double? result = null;
            if (str.HasValue())
                if (double.TryParse(str.Replace(",", string.Empty), out double n)) result = n;
            return result;
        }

        /// <summary>
        /// 文字列を等価の10進数(null許容型)に変換します。
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>64ビット符号付整数(null許容型)</returns>
        public static decimal? ToDecimal(this string str)
        {
            decimal? result = null;
            if (str.HasValue())
                if (decimal.TryParse(str.Replace(",", string.Empty), out decimal n)) result = n;
            return result;
        }

        /// <summary>
        /// 文字列をDateTimeオブジェクト(null許容型)に変換します。
        /// 対応フォーマットはメソッドの備考を参照してください。
        /// </summary>
        /// <param name="str">対象文字列</param>
        /// <returns>DateTimeオブジェクト(null許容型)</returns>
        /// <remarks>
        /// 対応フォーマット
        /// ・yyyyMMdd
        /// ・yyyyMMddHHmmss
        /// ・y/m/d
        /// ・yyyy/MM/dd
        /// ・yyyy/MM/dd HH:mm
        /// ・yyyy/MM/dd HH:mm:ss
        /// </remarks>
        public static DateTime? ToDateTime(this string str)
        {
            DateTime? result = null;
            if (str.HasValue())
            {
                try
                {
                    var length = str.Split('/').Length;
                    // フォーマットあり（/:が付いている）かを判定する。
                    if (length == 1)
                    {
                        var format = string.Empty;
                        if (str.Length == 8)
                            format = "yyyyMMdd";
                        else if (str.Length == 14)
                            format = "yyyyMMddHHmmss";
                        if (format.HasValue() && DateTime.TryParseExact(str, format, null, DateTimeStyles.None, out DateTime d))
                            result = d;
                    }
                    else if (length == 3)
                    {
                        // 時刻付きであった場合も3になるはずのため3固定で判定する。
                        // 「1/7/1」などが「2001/7/1」になってしまうためフォーマット付きの場合TryParseは使わない
                        var dt = str.Split(' ');
                        if (dt.Length == 1)
                        {
                            var date = str.Split('/');
                            result = new DateTime(date[0].ToInt() ?? 1, date[1].ToInt() ?? 1, date[2].ToInt() ?? 1);
                        }
                        else if (dt.Length == 2)
                        {
                            var date = dt[0].Split('/');
                            var time = dt[1].Split(':');
                            if (time.Length == 2)
                                // 時分
                                result = new DateTime(date[0].ToInt() ?? 1, date[1].ToInt() ?? 1, date[2].ToInt() ?? 1, time[0].ToInt() ?? 0, time[1].ToInt() ?? 0, 0);
                            else if (time.Length == 3)
                                // 時分秒
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
        /// DateTimeオブジェクトをyyyy/MM/dd形式の文字列に変換します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>日付文字列</returns>
        public static string ToStringDate(this DateTime d) => d.ToString("yyyy/MM/dd");

        /// <summary>
        /// DateTimeオブジェクトをyyyyMMdd形式の文字列に変換します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>日付文字列</returns>
        public static string ToStringDateNoFormat(this DateTime d) => d.ToString("yyyyMMdd");

        /// <summary>
        /// DateTimeオブジェクトをyyyy年MM月dd日形式の文字列に変換します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>日付文字列</returns>
        public static string ToStringDateJapanese(this DateTime d) => d.ToString("yyyy年MM月dd日");

        /// <summary>
        /// DateTimeオブジェクトをgy年M月d日形式の文字列に変換します。
        /// 明治元年以前は例外になるため空文字を返却します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>日付文字列</returns>
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
        /// DateTimeオブジェクトをHH:mm:ss形式の文字列に変換します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>時刻文字列</returns>
        public static string ToStringTime(this DateTime d) => d.ToString("HH:mm:ss");

        /// <summary>
        /// DateTimeオブジェクトをyyyy/MM/dd HH:mm形式の文字列に変換します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>日時文字列</returns>
        public static string ToStringDateTimeHM(this DateTime d) => d.ToString("yyyy/MM/dd HH:mm");

        /// <summary>
        /// DateTimeオブジェクトをyyyy/MM/dd HH:mm:ss形式の文字列に変換します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>日時文字列</returns>
        public static string ToStringDateTime(this DateTime d) => d.ToString("yyyy/MM/dd HH:mm:ss");

        /// <summary>
        /// DateTimeオブジェクトをyyyyMMddHHmmss形式の文字列に変換します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>日時文字列</returns>
        public static string ToStringDateTimeNoFormat(this DateTime d) => d.ToString("yyyyMMddHHmmss");

        /// <summary>
        /// 日付を含む月の最終日 (末日) を返却します。
        /// </summary>
        /// <param name="d">対象DateTimeオブジェクト</param>
        /// <returns>日付の属する月の最終日を返します。</returns>
        public static DateTime GetEndOfMonth(this DateTime d) => new DateTime(d.Year, d.Month, 1).AddMonths(1).AddDays(-1);
        #endregion

        #region Decimal
        /// <summary>
        /// 対象の10進数の小数以下を切り上げた最小の整数値を取得します。
        /// </summary>
        /// <param name="d">対象の値</param>
        /// <returns>切り上げた最小の整数値</returns>
        public static decimal Ceiling(this decimal d) => Math.Ceiling(d);

        /// <summary>
        /// 対象の10進数の小数以下を切り捨てた最大の整数値を取得します。
        /// </summary>
        /// <param name="d">対象の値</param>
        /// <returns>切り捨てた最大の整数値</returns>
        public static decimal Floor(this decimal d) => Math.Floor(d);

        /// <summary>
        /// 対象の10進数を最も近い整数値に丸めます。
        /// </summary>
        /// <param name="d">対象の値</param>
        /// <returns>丸めた結果の整数値</returns>
        public static decimal Round(this decimal d) => Math.Round(d);

        /// <summary>
        /// 対象の10進数を指定した少数部で丸めます。
        /// </summary>
        /// <param name="d">対象の値</param>
        /// <param name="digits">戻り値の小数部の桁数</param>
        /// <returns>丸めた結果の整数値</returns>
        public static decimal Round(this decimal d, int digits) => Math.Round(d, digits);
        #endregion

        #region Boolean
        /// <summary>
        /// Nullable Boolean から値(nullの場合はfalse)を取得します。
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool GetValue(this bool? b) => b ?? false;
        #endregion

        #region Json
        /// <summary>
        /// オブジェクトをJsonにシリアライズします。
        /// </summary>
        /// <typeparam name="T">対象の型</typeparam>
        /// <param name="src">対象オブジェクト</param>
        /// <returns>Json文字列</returns>
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
        /// 文字列を指定した型へデシリアライズします。
        /// </summary>
        /// <typeparam name="T">変換する型</typeparam>
        /// <param name="json">Json文字列</param>
        /// <returns>指定した型オブジェクト</returns>
        public static T ToEntity<T>(this string json)
        {
            var result = default(T);
            try
            {
                if (json.HasValue())
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    {
                        var serializer = new DataContractJsonSerializer(typeof(T));
                        result = (T)serializer.ReadObject(ms);
                    }
                }
            }
            catch { }
            return result;
        }
        #endregion
    }
}
