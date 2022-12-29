using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Muffin.Common.Util
{
    public static class DateTimeExtensions
    {
        public static DateTime Min(DateTime d1, DateTime d2)
        {
            if (d1 < d2)
                return d1;
            return d2;
        }

        public static DateTime Max(DateTime d1, DateTime d2)
        {
            if (d1 > d2)
                return d1;
            return d2;
        }

        public static Dictionary<DateTimeRange, T> GetTimeSpanCoherentRanges<T>(this IEnumerable<T> objs, TimeSpan maxTimeGap, Func<T, DateTime> selector)
        {
            return objs.GetTimeSpanCoherentRanges((x, y) => maxTimeGap, selector);
        }

        public static Dictionary<DateTimeRange, T> GetTimeSpanCoherentRanges<T>(this IEnumerable<T> objs, Func<T, T, TimeSpan> maxTimeGapFunc, Func<T, DateTime> selector)
        {
            var ordered = objs
                .OrderBy(selector)
                .ToArray();

            var result = new Dictionary<DateTimeRange, T>();
            for (var i = 0; i < ordered.Length; i++)
            {
                T last = ordered[i];
                DateTime a = selector(ordered[i]);
                DateTime start = a;

                for (var j = i + 1; j < ordered.Length; j++)
                {
                    DateTime b = selector(ordered[j]);

                    if (TimeSpanHelper.Abs(b - a) <= maxTimeGapFunc(last, ordered[j]))
                    {
                        a = b;
                        last = ordered[j];
                    }
                    else
                    {
                        i = j - 1;
                        var range = new DateTimeRange(start, b);
                        result.Add(range, last);
                        break;
                    }
                }
            }

            if (ordered.Length > 0 && result.Count == 0)
            {
                var min = ordered.Min(selector);
                var max = ordered.Max(selector);
                result.Add(new DateTimeRange(min, max), ordered.FirstOrDefault());
            }

            return result;
        }

        public static T[][] GetTimeSpanCoherentObjects<T>(this IEnumerable<T> objs, TimeSpan maxTimeGap, Func<T, DateTime> selector)
        {
            return objs.GetTimeSpanCoherentObjects((x, y) => maxTimeGap, selector);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <param name="maxTimeGapFunc">Start und aktuelles sind hier die Parameter</param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static T[][] GetTimeSpanCoherentObjects<T>(this IEnumerable<T> objs, Func<T, T, TimeSpan> maxTimeGapFunc, Func<T, DateTime> selector)
        {
            var ordered = objs
                .OrderBy(selector)
                .ToArray();

            List<List<T>> result = new List<List<T>>();
            for (var i = 0; i < ordered.Length; i++)
            {
                T last = ordered[i];
                DateTime a = selector(ordered[i]);
                var tmp = new List<T>();
                tmp.Add(ordered[i]);

                for (var j = i + 1; j < ordered.Length; j++)
                {
                    DateTime b = selector(ordered[j]);

                    if (TimeSpanHelper.Abs(b - a) <= maxTimeGapFunc(last, ordered[j]))
                    {
                        tmp.Add(ordered[j]);
                        a = b;
                        last = ordered[j];
                    }
                    else
                    {
                        i = j - 1;
                        break;
                    }
                }
                result.Add(tmp);
            }

            return result
                .Select(x => x.ToArray())
                .ToArray();
        }

        public static bool IsOnSameDay(DateTime d1, DateTime d2)
        {
            return d1.SameDay(d2);
        }

        public static bool SameDay(this DateTime d1, DateTime d2)
        {
            return d1.Date.Day == d2.Date.Day && d1.Date.Month == d2.Date.Month && d1.Date.Year == d2.Date.Year;
        }

        public static double TimeSice1970InMilliseconds(this DateTime date)
        {
            return date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public static double TimeSice1970InSeconds(this DateTime date)
        {
            return date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        public static DateTime FromMillisSince1970(long unixDate)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddMilliseconds(unixDate).ToLocalTime();
        }

        public static DateTime FromSecondsSince1970(long unixDate)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddSeconds(unixDate).ToLocalTime();
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTime(DateTime date)
        {
            return (long)TimeSice1970InSeconds(date);
        }

        public static DateTime CombineDateAndTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }

        public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static bool IsLastDayOfWeek(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsLastDayOfMonth(this DateTime date)
        {
            return date.Day == date.LastDayOfMonth().Day;
        }

        public static bool IsLastDayOfYear(this DateTime date)
        {
            return date.Day == 31 && date.Month == 12;
        }

        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return date.AddDays(1 - (date.Day)).AddMonths(1).AddDays(-1);
        }

        public static DateTime LastDayOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 12, 31);
        }

        public static int MonthDiff(DateTime date1, DateTime date2)
        {
            if (date1.Month < date2.Month)
            {
                return (date2.Year - date1.Year) * 12 + date2.Month - date1.Month;
            }
            else
            {
                return (date2.Year - date1.Year - 1) * 12 + date2.Month - date1.Month + 12;
            }
        }

        public static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        public static IEnumerable<DateTime> EachWeek(DateTime from, DateTime thru)
        {
            var _from = from;
            while (_from.DayOfWeek != DayOfWeek.Monday)
                _from = _from.AddDays(1);

            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(7))
                yield return day;
        }

        public static IEnumerable<DateTime> EachMonth(DateTime from, DateTime thru)
        {
            for (var month = from.Date; month.Date <= thru.Date; month = month.AddMonths(1))
                yield return month;
        }

        public static IEnumerable<DateTime> EachYear(DateTime from, DateTime thru)
        {
            for (var year = from.Date; year.Date <= thru.Date; year = year.AddYears(1))
                yield return year;
        }

        public static DateTime NextDayOfWeek(this DateTime date, DayOfWeek dayOfWeek)
        {
            return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
        }

        public static DateTime GetLastWeekdayOfMonth(this DateTime date, DayOfWeek day)
        {
            DateTime lastDayOfMonth = new DateTime(date.Year, date.Month, 1)
                .AddMonths(1).AddDays(-1);
            int wantedDay = (int)day;
            int lastDay = (int)lastDayOfMonth.DayOfWeek;
            return lastDayOfMonth.AddDays(
                lastDay >= wantedDay ? wantedDay - lastDay : wantedDay - lastDay - 7);
        }

        public static string ToTString(this DateTime date)
        {
            return date.ToString("dd.MM.yyyyTHH:mm");
        }

        public static DateTime ParseTString(string str)
        {
            return DateTime.ParseExact(str.Replace('T', ' '), "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        public static DateTime FullDays(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }

        public static string ToDayName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dateTime.DayOfWeek);
        }

        public static string ToMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }

        public static string ToShortMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        public static string ToGermanDayName(this DateTime dateTime)
        {
            return new CultureInfo("de-DE").DateTimeFormat.GetDayName(dateTime.DayOfWeek);
        }

        public static string ToGermanMonthName(this DateTime dateTime)
        {
            return new CultureInfo("de-DE").DateTimeFormat.GetMonthName(dateTime.Month);
        }

        public static string ToGermanShortDayName(this DateTime dateTime)
        {
            return new CultureInfo("de-DE").DateTimeFormat.GetAbbreviatedDayName(dateTime.DayOfWeek);
        }

        public static string ToGermanShortMonthName(this DateTime dateTime)
        {
            return new CultureInfo("de-DE").DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        #region Truncate

        public static DateTime TruncateMillis(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        }

        public static DateTime Truncate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }

        public static DateTime TruncateWeekly(this DateTime date, DayOfWeek dayOfWeek = DayOfWeek.Monday)
        {
            if (date.DayOfWeek == dayOfWeek)
                return date.Truncate();
            var yesterday = date.AddDays(-1);
            while (yesterday.DayOfWeek != dayOfWeek)
                yesterday = yesterday.AddDays(-1);
            return yesterday.Truncate();
        }

        public static DateTime TruncateMonthly(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime TruncateYearly(this DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }

        public static DateTime TruncateSeconds(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
        }

        public static DateTime TruncateMinutes(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, date.Second);
        }

        public static DateTime TruncateHours(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, date.Minute, date.Second);
        }

        #endregion

        #region Extend

        public static DateTime Extend(this DateTime date)
        {
            return date.Truncate().AddDays(1).AddMilliseconds(-1);
        }

        public static DateTime ExtendWeekly(this DateTime date, DayOfWeek dayOfWeek = DayOfWeek.Sunday)
        {
            if (date.DayOfWeek == dayOfWeek)
                return date.Extend();
            var tomorrow = date.AddDays(1);
            while (tomorrow.DayOfWeek != dayOfWeek)
                tomorrow = tomorrow.AddDays(1);
            return tomorrow.Extend();
        }

        public static DateTime ExtendMonthly(this DateTime date)
        {
            return date.TruncateMonthly().AddMonths(1).AddMilliseconds(-1);
        }

        public static DateTime ExtendYearly(this DateTime date)
        {
            return date.TruncateYearly().AddYears(1).AddMilliseconds(-1);
        }

        #endregion

        #region Feiertage

        /* Neujahr */
        public static DateTime GetNewYear(int year)
        {
            return new DateTime(year, 1, 1);
        }

        /* Heilige drei Könige */
        public static DateTime GetEpiphany(int year)
        {
            return new DateTime(year, 1, 6);
        }

        /* Tag der Arbeit */
        public static DateTime GetDayOfWork(int year)
        {
            return new DateTime(year, 5, 1);
        }

        /* Ostersonntag */
        public static DateTime GetEasterSunday(int year)
        {
            int c;
            int i;
            int j;
            int k;
            int l;
            int n;
            int OsterTag;
            int OsterMonat;

            c = year / 100;
            n = year - 19 * ((int)(year / 19));
            k = (c - 17) / 25;
            i = c - c / 4 - ((int)(c - k) / 3) + 19 * n + 15;
            i = i - 30 * ((int)(i / 30));
            i = i - (i / 28) * ((int)(1 - (i / 28)) * ((int)(29 / (i + 1))) * ((int)(21 - n) / 11));
            j = year + ((int)year / 4) + i + 2 - c + ((int)c / 4);
            j = j - 7 * ((int)(j / 7));
            l = i - j;

            OsterMonat = 3 + ((int)(l + 40) / 44);
            OsterTag = l + 28 - 31 * ((int)OsterMonat / 4);

            return new DateTime(year, OsterMonat, OsterTag);// Convert.ToDateTime( OsterTag.ToString() + "." + OsterMonat + "." + year );
        }

        /* Osermontag */
        public static DateTime GetEasterMonday(int year)
        {
            return GetEasterSunday(year).AddDays(1);
        }

        /* Karfreitag */
        public static DateTime GetGoodFriday(int year)
        {
            return GetEasterSunday(year).AddDays(-2);
        }

        /* Christi Himmelfahrt */
        public static DateTime GetAscensionDay(int year)
        {
            return GetEasterSunday(year).AddDays(39);
        }

        /* Pfingstsonntag */
        public static DateTime GetWhitsunday(int year)
        {
            return GetEasterSunday(year).AddDays(49);
        }

        /* Pfingstmontag */
        public static DateTime GetWhitmonday(int year)
        {
            return GetEasterSunday(year).AddDays(50);
        }

        /* Fronleichnam */
        public static DateTime GetCorpusChristi(int year)
        {
            return GetEasterSunday(year).AddDays(60);
        }

        /* Tag der deutschen Einheit */
        public static DateTime GetGermanUnificationDay(int year)
        {
            return new DateTime(year, 10, 3);
        }

        /* Reformationstag */
        public static DateTime GetReformationDay(int year)
        {
            return new DateTime(year, 10, 31);
        }

        /* Allerheiligen */
        public static DateTime GetAllHallows(int year)
        {
            return new DateTime(year, 11, 1);
        }

        /* 1. Weihnachtsfeiertag */
        public static DateTime GetChristmasDay(int year)
        {
            return new DateTime(year, 12, 25);
        }

        /* 2. Weihnachtsfeiertag */
        public static DateTime GetDayAfterChristmas(int year)
        {
            return new DateTime(year, 12, 26);
        }

        public static DateTime[] GetHolidays(DateTime date)
        {
            return GetHolidays(date.Year);
        }

        public static DateTime[] GetHolidays(int year)
        {
            return new DateTime[] {
                GetNewYear( year ),
                GetEpiphany(year),
                GetDayOfWork(year),
                GetEasterSunday(year),
                GetEasterMonday(year),
                GetGoodFriday(year),
                GetAscensionDay(year),
                GetWhitsunday(year),
                GetWhitmonday(year),
                GetCorpusChristi(year),
                GetGermanUnificationDay(year),
                GetReformationDay(year),
                GetAllHallows(year),
                GetChristmasDay(year),
                GetDayAfterChristmas(year)
            };
        }

        public static bool IsHoliday(DateTime date)
        {
            return GetHolidays(date).Any(x => x.SameDay(date));
        }

        #endregion

        public static bool IsDateInRange(this DateTime date, DateTime from, DateTime to)
        {
            return date >= from && date <= to;
        }

        public static bool IsDateInRange(this DateTime date, DateTimeRange range)
        {
            return range.IsInRange(date);
        }

        public static int CountWorkDays(DateTime from, DateTime to)
        {
            var _from = from.FullDays();
            var _to = to.FullDays();
            var range = (int)(_to - _from).TotalDays + 1;
            range -= CountDays(DayOfWeek.Saturday, _from, _to);
            range -= CountDays(DayOfWeek.Sunday, _from, _to);
            return range;
        }

        public static int CountDays(DayOfWeek day, DateTime start, DateTime end)
        {
            TimeSpan ts = end - start;                       // Total duration
            int count = (int)Math.Floor(ts.TotalDays / 7);   // Number of whole weeks
            int remainder = (int)(ts.TotalDays % 7);         // Number of remaining days
            int sinceLastDay = (int)(end.DayOfWeek - day);   // Number of days since last [day]
            if (sinceLastDay < 0) sinceLastDay += 7;         // Adjust for negative days since last [day]
            if (remainder >= sinceLastDay) count++;

            return count;
        }

        public static bool IsRangeOnlyWeekend(DateTime start, DateTime end, out int daysCount)
        {
            var saturdayCount = CountDays(DayOfWeek.Saturday, start, end);
            var sundayCount = CountDays(DayOfWeek.Sunday, start, end);
            daysCount = saturdayCount + sundayCount;
            if (saturdayCount == 1 || sundayCount == 1)
                return true;
            return false;
        }

        #region Datetime Ranges

        #region Single Day

        public static DateTimeRange GetDayRange(this DateTime date)
        {
            var from = date.Truncate();
            var to = from.AddDays(1).AddMilliseconds(-1);
            return new DateTimeRange()
            {
                From = from,
                To = to
            };
        }

        public static DateTimeRange GetWeekRange(this DateTime date, DayOfWeek dow = DayOfWeek.Monday)
        {
            var from = date.TruncateWeekly(dow);
            var to = from.AddDays(7).AddMilliseconds(-1);
            return new DateTimeRange()
            {
                From = from,
                To = to
            };
        }

        public static DateTimeRange GetMonthRange(this DateTime date)
        {
            var from = date.TruncateMonthly();
            var to = from.AddMonths(1).AddMilliseconds(-1);
            return new DateTimeRange()
            {
                From = from,
                To = to
            };
        }

        public static DateTimeRange GetYearRange(this DateTime date)
        {
            var from = date.TruncateYearly();
            var to = from.AddYears(1).AddMilliseconds(-1);
            return new DateTimeRange()
            {
                From = from,
                To = to
            };
        }

        public static DateTimeRange GetMonthlyTranscend(this DateTime date)
        {
            var from = date.ExtendMonthly();
            var to = from.AddDays(1).TruncateMonthly();
            return new DateTimeRange()
            {
                From = from,
                To = to
            };
        }

        #endregion

        #region Ranges

        public static IEnumerable<DateTimeRange> EachDayRange(this DateTimeRange range)
        {
            return EachDayRange(range.From, range.To);
        }

        public static IEnumerable<DateTimeRange> EachDayRange(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return DateTimeRange.DayRange(day);
        }

        public static IEnumerable<DateTimeRange> EachWeekRange(this DateTimeRange range)
        {
            return EachWeekRange(range.From, range.To);
        }

        public static IEnumerable<DateTimeRange> EachWeekRange(DateTime from, DateTime thru)
        {
            var _from = from;
            while (_from.DayOfWeek != DayOfWeek.Monday)
                _from = _from.AddDays(1);

            for (var week = from.Date; week.Date <= thru.Date; week = week.AddDays(7))
                yield return DateTimeRange.WeekRange(week, DayOfWeek.Monday);
        }

        public static IEnumerable<DateTimeRange> EachMonthRange(this DateTimeRange range)
        {
            return EachMonthRange(range.From, range.To);
        }

        public static IEnumerable<DateTimeRange> EachMonthRange(DateTime from, DateTime thru)
        {
            for (var month = from.Date; month.Date <= thru.Date; month = month.AddMonths(1))
                yield return DateTimeRange.MonthRange(month);
        }

        public static IEnumerable<DateTimeRange> EachYearRange(this DateTimeRange range)
        {
            return EachYearRange(range.From, range.To);
        }

        public static IEnumerable<DateTimeRange> EachYearRange(DateTime from, DateTime thru)
        {
            for (var year = from.Date; year.Date <= thru.Date; year = year.AddYears(1))
                yield return DateTimeRange.YearRange(year);
        }

        #endregion

        #endregion

        private static readonly Random rnd = new Random();
        public static DateTime GetRandomDate(DateTime from, DateTime to)
        {
            var range = to - from;
            var randTimeSpan = new TimeSpan((long)(rnd.NextDouble() * range.Ticks));
            return from + randTimeSpan;
        }

        public static DateTime GetRandomDate(this DateTimeRange range)
        {
            return GetRandomDate(range.From, range.To);
        }
    }

    public class DateTimeRange : IEnumerable<DateTime>
    {
        public DateTimeRange() { }
        public DateTimeRange(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }
        public DateTimeRange(DateTime referenceDate, TimeSpan past, TimeSpan future)
        {
            From = referenceDate.Add(-past);
            To = referenceDate.Add(future);
        }

        public DateTimeRange(DateTime[] dates)
        {
            if (dates == null || dates.Length == 0)
            {
                return;
            }

            From = dates.Min();
            To = dates.Max();
        }

        public bool IsInRange(DateTime date)
        {
            return date >= From && date <= To;
        }

        public bool Intersects(DateTimeRange range)
        {
            return this.To > range.From || range.To > this.From;
        }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public static DateTimeRange Now
        {
            get
            {
                return new DateTimeRange(DateTime.Now, DateTime.Now);
            }
        }

        public static DateTimeRange UtcNow
        {
            get
            {
                return new DateTimeRange(DateTime.UtcNow, DateTime.UtcNow);
            }
        }

        public static DateTimeRange DayRange(DateTime date)
        {
            DateTime _date = date.Truncate();
            return new DateTimeRange()
            {
                From = _date,
                To = _date.AddDays(1).AddMilliseconds(-1)
            };
        }

        public static DateTimeRange WeekRange(DateTime date, DayOfWeek dayOfWeek = DayOfWeek.Monday)
        {
            DateTime _date = date.TruncateWeekly(dayOfWeek);
            return new DateTimeRange()
            {
                From = _date,
                To = _date.AddDays(7).AddMilliseconds(-1)
            };
        }

        public static DateTimeRange MonthRange(DateTime date)
        {
            DateTime _date = date.TruncateMonthly();
            return new DateTimeRange()
            {
                From = _date,
                To = _date.AddMonths(1).AddMilliseconds(-1)
            };
        }

        public static DateTimeRange YearRange(DateTime date)
        {
            DateTime _date = date.TruncateYearly();
            return new DateTimeRange()
            {
                From = _date,
                To = _date.AddYears(1).AddMilliseconds(-1)
            };
        }

        #region IEnumerable<DateTime> Member

        private IEnumerable<DateTime> _range { get { return DateTimeExtensions.EachDay(From, To); } }
        IEnumerator<DateTime> IEnumerable<DateTime>.GetEnumerator()
        {
            return _range.GetEnumerator();
        }

        #endregion

        #region IEnumerable Member

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _range.GetEnumerator();
        }

        #endregion

        #region DateTime Functions

        public DateTimeRange AddDays(double value)
        {
            return new DateTimeRange(From.AddDays(value), To.AddDays(value));
        }

        #endregion

        #region Inner Range helper

        public IEnumerable<DateTime> EachDay()
        {
            return DateTimeExtensions.EachDay(From, To);
        }

        public IEnumerable<DateTimeRange> EachDayRange()
        {
            return DateTimeExtensions.EachDayRange(From, To);
        }

        #endregion

        #region Date Functions

        public string ToString(string format)
        {
            return string.Format("{0} - {1}", From.ToString(format), To.ToString(format)); ;
        }

        #endregion

        #region Implict

        public static implicit operator DateTimeRange(string s)
        {
            var parts = s.Split('-');
            var s0 = parts[0].Trim();
            var s1 = parts[1].Trim();
            DateTime from;
            DateTime.TryParse(s0, CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out from);
            DateTime to;
            DateTime.TryParse(s1, CultureInfo.GetCultureInfo("de-DE"), DateTimeStyles.None, out to);
            return new DateTimeRange(from, to);
        }

        #endregion
    }

    public static class TimeSpanExtensions
    {
        public static string ToPrettyFormat(this TimeSpan span)
        {

            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new StringBuilder();
            if (span.Days > 0)
                sb.AppendFormat("{0} Tag{1} ", span.Days, span.Days > 1 ? "e" : String.Empty);
            if (span.Hours > 0)
                sb.AppendFormat("{0} Stunde{1} ", span.Hours, span.Hours > 1 ? "n" : String.Empty);
            if (span.Minutes > 0)
                sb.AppendFormat("{0} Minute{1} ", span.Minutes, span.Minutes > 1 ? "n" : String.Empty);
            return sb.ToString();
        }

        public static string ToShortFormat(this TimeSpan span)
        {

            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new StringBuilder();
            if (span.Days > 0)
                return string.Format("{0} Tag{1} ", span.Days, span.Days > 1 ? "e" : String.Empty);
            if (span.Hours > 0)
                return string.Format("{0} Stunde{1} ", span.Hours, span.Hours > 1 ? "n" : String.Empty);
            if (span.Minutes > 0)
                return string.Format("{0} Minute{1} ", span.Minutes, span.Minutes > 1 ? "n" : String.Empty);
            return "Jetzt";
        }
    }

    public enum SpecialDayOfYear
    {
        /// <summary>
        /// Unbekannt
        /// </summary>
        [Display(Name = "Unbekannt")]
        Unknown = 0,

        /// <summary>
        /// Neujahr
        /// </summary>
        [Display(Name = "Neujahr")]
        NewYear = 1,

        /// <summary>
        /// Heilige drei Könige ( Baden-Württemberg, Bayern, Sachsen-Anhalt )
        /// </summary>
        [Display(Name = "Heilige drei Könige")]
        Epiphany = 2,

        /// <summary>
        /// Internationaler Frauentag ( Berlin )
        /// </summary>
        [Display(Name = "Internationaler Frauentag")]
        InternationalWomensDay = 3,

        /// <summary>
        /// Gründonnerstag
        /// </summary>
        [Display(Name = "Gründonnerstag")]
        HolyThursday = 4,

        /// <summary>
        /// Karfreitag
        /// </summary>
        [Display(Name = "Karfreitag")]
        GoodDay = 5,

        /// <summary>
        /// Ostersonntag (Brandenburg)
        /// </summary>
        [Display(Name = "Ostersonntag")]
        EasterSunday = 6,

        /// <summary>
        /// Ostermontag
        /// </summary>
        [Display(Name = "Ostermontag")]
        EasterMonday = 7,

        /// <summary>
        /// Tag der Arbeit
        /// </summary>
        [Display(Name = "Tag der Arbeit")]
        LabourDay = 8,

        /// <summary>
        /// Christi Himmelfahrt
        /// </summary>
        [Display(Name = "Christi Himmelfahrt")]
        AscensionDay = 9,

        /// <summary>
        /// Pfingstsonntag (Brandenburg)
        /// </summary>
        [Display(Name = "Pfingstsonntag")]
        PentecostSunday = 10,

        /// <summary>
        /// Pfingstmontag
        /// </summary>
        [Display(Name = "Pfingstmontag")]
        WhitMonday = 11,

        /// <summary>
        /// Fronleichnam (Baden-Württemberg, Bayern, Hessen, Nordrhein-Westfalen, Rheinland-Pfalz, Saarland)
        /// </summary>
        [Display(Name = "Fronleichnam")]
        CorpusChristi = 12,

        /// <summary>
        /// Mariä Himmelfahrt (Saarland)
        /// </summary>
        [Display(Name = "Mariä Himmelfahrt")]
        AssumptionDay = 13,

        /// <summary>
        /// Weltkindertag (Thüringen)
        /// </summary>
        [Display(Name = "Weltkindertag")]
        WorldChildrensDay = 14,

        /// <summary>
        /// Tag der deutschen Einheit
        /// </summary>
        [Display(Name = "Tag der deutschen Einheit")]
        AnniversaryOfGermanUnification = 15,

        /// <summary>
        /// Reformationstag (Brandenburg, Bremen, Hamburg, Mecklenburg-Vorpommern, Niedersachsen, Sachsen, Sachsen-Anhalt, Schleswig-Holstein,Thüringen)
        /// </summary>
        [Display(Name = "Reformationstag")]
        ReformationDay = 16,

        /// <summary>
        /// Allerheiligen (Baden-Württemberg, Bayern, Nordrhein-Westfalen, Rheinland-Pfalz, Saarland)
        /// </summary>
        [Display(Name = "Allerheiligen")]
        AllSaintsDay = 17,

        /// <summary>
        /// Buß- und Bettag (Sachsen)
        /// </summary>
        [Display(Name = "Buß- und Bettag")]
        DayOfPrayerAndRepentance = 18,

        /// <summary>
        /// 1. Weihnachtsfeiertag
        /// </summary>
        [Display(Name = "1. Weihnachtsfeiertag")]
        ChristmasDay = 19,

        /// <summary>
        /// 2. Weihnachtsfeiertag
        /// </summary>
        [Display(Name = "2. Weihnachtsfeiertag")]
        BoxingDay = 20,

        /// <summary>
        /// Schmotzige Donnerstag / Weiberfastnacht (52 Tage vor Ostern)
        /// </summary>
        [Display(Name = "Weiberfastnacht")]
        FatThursday = 21,

        /// <summary>
        /// Fastnachtssamstag (50 Tage vor Ostern)
        /// </summary>
        [Display(Name = "Fastnachtssamstag")]
        CarnivalSaturday = 22,

        /// <summary>
        /// Fastnachtssonntag (49 Tage vor Ostern)
        /// </summary>
        [Display(Name = "Fastnachtssonntag")]
        CarnivalSunday = 23,

        /// <summary>
        /// Rosenmontag (48 Tage vor Ostern)
        /// </summary>
        [Display(Name = "Rosenmontag")]
        RoseMonday = 24,

        /// <summary>
        /// Fastnacht (47 Tage vor Ostern)
        /// </summary>
        [Display(Name = "Fastnacht")]
        Carnival = 25,

        /// <summary>
        /// Aschermittwoch (46 Tage vor Ostern)
        /// </summary>
        [Display(Name = "Aschermittwoch")]
        AshWednesday = 26,
    }

    public class SpecialDayOfYearDate
    {
        public SpecialDayOfYear SpecialDayOfYear { get; set; }
        public DateTime DateTime { get; set; }
    }

    public static class SpecialDayOfYearHelper
    {
        /// <summary>
        /// Errechnet den Ostersonntag für ein entsprechendes Jahr
        /// </summary>
        public static DateTime Computus(int year)
        {
            int day = 0;
            int month = 0;

            int g = year % 19;
            int c = year / 100;
            int h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c / 4)) % 7) + 28;
            month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Errechnet das Datum des Buß- und Bettags aus dem übergebenen Jahr
        /// </summary>
        /// <param name="year">Das Jahr YYYY als integer-Wert</param>
        /// <returns>Das errechnete Datum des Buß- und Bettags in dem angegebenen Jahr</returns>
        public static DateTime GetDayOfPrayerAndRepentanceDate(int year)
        {
            //	Der Buß- und Bettag ist immer ein Mittwoch, er liegt zwischen dem 16. und 22. November
            return GetLastWeekday(new DateTime(year, 11, 22), DayOfWeek.Wednesday);
        }

        /// <summary>
        /// Bestimmt innerhalb der letzten 7 Tage das Datum mit dem gewünschten Wochentag.  
        /// </summary>
        /// <param name="startDate">Ausgangsdatum, und zwar das letztmögliche Datum innerhalb 
        /// des Bereichs von 7 Tagen (Beispiel: in einem Bereich Do/Fr/Sa/So/Mo/Di/Mi der Mittwoch)</param>
        /// <param name="targetDayOfWeek">Der gewünschte Wochentag (im Beispiel der Mittwoch)</param>
        /// <returns>Das Datum mit dem gewünschten Wochentag innerhalb der letzten 7 Tage.</returns>
        private static DateTime GetLastWeekday(DateTime startDate, DayOfWeek targetDayOfWeek)
        {
            DayOfWeek startDayOfWeek = startDate.DayOfWeek;
            if (startDayOfWeek == targetDayOfWeek)
            {
                return startDate;
            }
            int diff = 0;
            if (startDayOfWeek < targetDayOfWeek)
            {
                diff = targetDayOfWeek - startDayOfWeek - 7;
            }
            else if (startDayOfWeek > targetDayOfWeek)
            {
                diff = targetDayOfWeek - startDayOfWeek;
            }
            return startDate.AddDays(diff);
        }

        private static Dictionary<int, Map<SpecialDayOfYear, DateTime>> _cache = new Dictionary<int, Map<SpecialDayOfYear, DateTime>>();
        private static object _lock = new object();
        public static Map<SpecialDayOfYear, DateTime> GetSpecialDays(int year)
        {
            lock (_lock)
            {
                if (!_cache.TryGetValue(year, out Map<SpecialDayOfYear, DateTime> map))
                {
                    map = new Map<SpecialDayOfYear, DateTime>();

                    // Statische Feiertage basierend auf fixem Datum
                    map.Add(SpecialDayOfYear.NewYear, new DateTime(year, 1, 1));
                    map.Add(SpecialDayOfYear.Epiphany, new DateTime(year, 1, 6));
                    map.Add(SpecialDayOfYear.InternationalWomensDay, new DateTime(year, 3, 8));
                    map.Add(SpecialDayOfYear.LabourDay, new DateTime(year, 5, 1));
                    map.Add(SpecialDayOfYear.AssumptionDay, new DateTime(year, 8, 15));
                    map.Add(SpecialDayOfYear.WorldChildrensDay, new DateTime(year, 9, 20));
                    map.Add(SpecialDayOfYear.AnniversaryOfGermanUnification, new DateTime(year, 10, 3));
                    map.Add(SpecialDayOfYear.ReformationDay, new DateTime(year, 10, 31));
                    map.Add(SpecialDayOfYear.AllSaintsDay, new DateTime(year, 11, 1));
                    map.Add(SpecialDayOfYear.ChristmasDay, new DateTime(year, 12, 25));
                    map.Add(SpecialDayOfYear.BoxingDay, new DateTime(year, 12, 26));

                    // Dynamische Feiertage basieren auf Ostern
                    var easterSunday = Computus(year);
                    map.Add(SpecialDayOfYear.EasterSunday, easterSunday);
                    map.Add(SpecialDayOfYear.HolyThursday, easterSunday.AddDays(-3));
                    map.Add(SpecialDayOfYear.GoodDay, easterSunday.AddDays(-2));
                    map.Add(SpecialDayOfYear.EasterMonday, easterSunday.AddDays(1));
                    map.Add(SpecialDayOfYear.AscensionDay, easterSunday.AddDays(39));
                    map.Add(SpecialDayOfYear.PentecostSunday, easterSunday.AddDays(49));
                    map.Add(SpecialDayOfYear.WhitMonday, easterSunday.AddDays(50));
                    map.Add(SpecialDayOfYear.CorpusChristi, easterSunday.AddDays(60));

                    map.Add(SpecialDayOfYear.FatThursday, easterSunday.AddDays(-52));
                    map.Add(SpecialDayOfYear.CarnivalSaturday, easterSunday.AddDays(-50));
                    map.Add(SpecialDayOfYear.CarnivalSunday, easterSunday.AddDays(-49));
                    map.Add(SpecialDayOfYear.RoseMonday, easterSunday.AddDays(-48));
                    map.Add(SpecialDayOfYear.Carnival, easterSunday.AddDays(-47));
                    map.Add(SpecialDayOfYear.AshWednesday, easterSunday.AddDays(-46));

                    // Dynamische Feiertage basiered auf Buß und Bettag
                    var dayOfPrayerAndRepentance = GetDayOfPrayerAndRepentanceDate(year);
                    map.Add(SpecialDayOfYear.DayOfPrayerAndRepentance, dayOfPrayerAndRepentance);

                    _cache.Add(year, map);
                }

                return map;
            }
        }

        public static DateTime? GetDate(this SpecialDayOfYear specialDay, int year)
        {
            var specialDays = GetSpecialDays(year);
            if (specialDays.TryGetValue(specialDay, out DateTime result))
            {
                return result;
            }
            return null;
        }

        public static bool IsSpecialDayOfYear(this DateTime dateTime)
        {
            return IsSpecialDayOfYear(dateTime, out _);
        }

        public static bool IsSpecialDayOfYear(this DateTime dateTime, out SpecialDayOfYear specialDayOfYear)
        {
            var specialDays = GetSpecialDays(dateTime.Year);
            if (specialDays.TryGetValue(dateTime, out specialDayOfYear))
            {
                return true;
            }
            specialDayOfYear = SpecialDayOfYear.Unknown;
            return false;
        }
    }
}
