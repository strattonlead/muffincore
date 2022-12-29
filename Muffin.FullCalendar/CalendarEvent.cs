using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Muffin.FullCalendar
{
    public class CalendarEvent
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("start", ItemConverterType = typeof(CalendarEventDateTimeConverter))]
        public DateTime StartDate { get; set; } // 2010-01-09T12:30:00

        [JsonProperty("end", ItemConverterType = typeof(CalendarEventDateTimeConverter), NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? EndDate { get; set; } // 2010-01-09T12:30:00

        [JsonProperty("allDay")]
        public bool AllDay { get; set; }

        [JsonProperty("eventColor", NullValueHandling = NullValueHandling.Ignore)]
        public string Color { get; set; }

        [JsonProperty("eventBackgroundColor", NullValueHandling = NullValueHandling.Ignore)]
        public string BackgroundColor { get; set; }

        [JsonProperty("eventBorderColor", NullValueHandling = NullValueHandling.Ignore)]
        public string BorderColor { get; set; }

        [JsonProperty("eventTextColor", NullValueHandling = NullValueHandling.Ignore)]
        public string TextColor { get; set; }
    }

    public class CalendarEventDateTimeConverter : IsoDateTimeConverter
    {
        public CalendarEventDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        }
    }
}
