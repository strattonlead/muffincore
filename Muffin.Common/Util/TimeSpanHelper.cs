using System;
using System.Collections.Generic;
using System.Linq;

namespace Muffin.Common.Util
{
    // https://stackoverflow.com/questions/16689468/how-to-produce-human-readable-strings-to-represent-a-timespan/21649465
    public static class TimeSpanHelper
    {
        public static string GetReadableTimespan(this TimeSpan ts)
        {
            // formats and its cutoffs based on totalseconds
            var cutoff = new SortedList<long, string> {
       {60, "{3:S}" },
       {60*60-1, "{2:M}, {3:S}"},
       {60*60, "{1:H}"},
       {24*60*60-1, "{1:H}, {2:M}"},
       {24*60*60, "{0:D}"},
       {Int64.MaxValue , "{0:D}, {1:H}"}
     };

            // find nearest best match
            var find = cutoff.Keys.ToList()
                          .BinarySearch((long)ts.TotalSeconds);
            // negative values indicate a nearest match
            var near = find < 0 ? Math.Abs(find) - 1 : find;
            // use custom formatter to get the string
            return String.Format(
                new HMSFormatter(),
                cutoff[cutoff.Keys[near]],
                ts.Days,
                ts.Hours,
                ts.Minutes,
                ts.Seconds);
        }

        public static TimeSpan Abs(TimeSpan ts)
        {
            return new TimeSpan(Math.Abs(ts.Ticks));
        }

        public static TimeSpan GetTimeSpan(DurationType type, int factor)
        {
            switch (type)
            {
                case DurationType.Always:
                    return TimeSpan.Zero;
                case DurationType.Second:
                    return TimeSpan.FromSeconds(factor);
                case DurationType.Minute:
                    return TimeSpan.FromMinutes(factor);
                case DurationType.Hour:
                    return TimeSpan.FromHours(factor);
                case DurationType.Day:
                    return TimeSpan.FromDays(factor);
                case DurationType.Week:
                    return TimeSpan.FromDays(factor * 7);
                case DurationType.Month:
                    return TimeSpan.FromDays(factor * 30);
                case DurationType.Year:
                    return TimeSpan.FromDays(factor * 365);
                default:
                    return TimeSpan.Zero;
            }
        }

        public static TimeSpan GetTimeSpan(DurationType type, int factor, DateTime referenceDate)
        {
            switch (type)
            {
                case DurationType.Always:
                    return TimeSpan.Zero;
                case DurationType.Second:
                    return TimeSpan.FromSeconds(factor);
                case DurationType.Minute:
                    return TimeSpan.FromMinutes(factor);
                case DurationType.Hour:
                    return TimeSpan.FromHours(factor);
                case DurationType.Day:
                    return TimeSpan.FromDays(factor);
                case DurationType.Week:
                    return referenceDate.AddDays(7 * factor) - referenceDate;
                case DurationType.Month:
                    return referenceDate.AddMonths(factor) - referenceDate;
                case DurationType.Year:
                    return referenceDate.AddYears(factor) - referenceDate;
                case DurationType.Single:
                    return TimeSpan.MaxValue;
                default:
                    return TimeSpan.Zero;
            }
        }

        public static DateTime GetNextDate(DurationType type, int factor, DateTime referenceDate)
        {
            var ts = GetTimeSpan(type, factor, referenceDate);
            return referenceDate.Add(ts);
        }

        public static TimeSpan HHmm(this TimeSpan timeSpan)
        {
            return TimeSpan.FromHours(timeSpan.Hours)
                .Add(TimeSpan.FromMinutes(timeSpan.Minutes));
        }
    }

    // formatter for forms of
    // seconds/hours/day
    public class HMSFormatter : ICustomFormatter, IFormatProvider
    {
        // list of Formats, with a P customformat for pluralization
        static Dictionary<string, string> timeformats = new Dictionary<string, string> {
        {"S", "{0:P:Seconds:Second}"},
        {"M", "{0:P:Minutes:Minute}"},
        {"H","{0:P:Hours:Hour}"},
        {"D", "{0:P:Days:Day}"}
    };

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return String.Format(new PluralFormatter(), timeformats[format], arg);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }

    // formats a numeric value based on a format P:Plural:Singular
    public class PluralFormatter : ICustomFormatter, IFormatProvider
    {

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg != null)
            {
                var parts = format.Split(':'); // ["P", "Plural", "Singular"]

                if (parts[0] == "P") // correct format?
                {
                    // which index postion to use
                    int partIndex = (arg.ToString() == "1") ? 2 : 1;
                    // pick string (safe guard for array bounds) and format
                    return String.Format("{0} {1}", arg, (parts.Length > partIndex ? parts[partIndex] : ""));
                }
            }
            return String.Format(format, arg);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }

    public enum DurationType
    {
        Always = 0,
        Second = 1,
        Minute = 2,
        Hour = 3,
        Day = 4,
        Week = 5,
        Month = 6,
        Year = 7,
        Single = 8
    }
}
