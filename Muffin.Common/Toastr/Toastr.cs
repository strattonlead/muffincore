using Newtonsoft.Json;
using System.Collections.Generic;

namespace Muffin.Common.Toastr
{
    public class Toastr
    {
        [JsonProperty(PropertyName = "toastrMessages")]
        public List<ToastrMessage> ToastrMessages { get; set; } = new List<ToastrMessage>();
        public void AddToastr(string message, string title, string type)
        {
            ToastrMessages.Add(new ToastrMessage()
            {
                Message = message,
                Type = type,
                Title = title
            });
        }

        public void AddPrimary(string message, string title = null)
        {
            AddToastr(message, title, "primary");
        }

        public void AddSuccess(string message, string title = null)
        {
            AddToastr(message, title, "success");
        }

        public void AddInfo(string message, string title = null)
        {
            AddToastr(message, title, "info");
        }

        public void AddWarning(string message, string title = null)
        {
            AddToastr(message, title, "warning");
        }

        public void AddDanger(string message, string title = null)
        {
            AddToastr(message, title, "danger");
        }
    }

    public class ToastrMessage
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
    }
}
