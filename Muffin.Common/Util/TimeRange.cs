using Newtonsoft.Json;
using System;

namespace Muffin.Common.Util
{
    public struct TimeRange
    {
        #region Properties

        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }

        [JsonIgnore]
        public TimeSpan Duration => To - From;

        #endregion

        #region Constructor

        public TimeRange()
        {
            From = TimeSpan.Zero;
            To = TimeSpan.Zero;
        }
        public TimeRange(TimeSpan from, TimeSpan to)
        {
            From = from;
            To = to;
        }

        #endregion

        #region Helper

        public bool IsInRange(TimeSpan timeSpan)
        {
            return timeSpan >= From && timeSpan <= To;
        }

        public TimeRange? GetOverlappingRange(TimeRange other)
        {
            if (IsZero || other.IsZero)
            {
                return null;
            }

            if (To == other.To && From == other.From)
            {
                return new TimeRange(From, To);
            }

            if (From < other.From)
            {
                if (To <= other.From)
                {
                    return null;
                }

                return new TimeRange(other.From, To);
            }


            if (From >= other.To)
            {
                return null;
            }

            if (From >= other.From && To <= other.To)
            {
                return new TimeRange(From, To);
            }

            if (other.From > From && other.To >= To)
            {
                return new TimeRange(other.From, other.To);
            }

            return new TimeRange(From, other.To);
        }

        public bool IsZero => From == TimeSpan.Zero && To == TimeSpan.Zero;

        public static TimeRange Zero => new TimeRange();

        #endregion
    }
}
