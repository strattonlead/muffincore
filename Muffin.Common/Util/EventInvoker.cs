using System;

namespace Muffin.Common.Util
{
    public class EventInvoker
    {
        private Func<EventHandler> GetEventHandler;
        private object sender;

        public EventInvoker(object sender, Func<EventHandler> GetEventHandler)
        {
            this.sender = sender;
            this.GetEventHandler = GetEventHandler;
        }

        public void Raise()
        {
            GetEventHandler()?.Invoke(sender, new EventArgs());
        }
    }
}
