using System;

namespace Muffin.StateManagement
{
    public class AppStateManagerOptions<TAppState>
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(15);
    }

    public class AppStateManagerOptionsBuilder<TAppState>
    {
        private AppStateManagerOptions<TAppState> Options = new AppStateManagerOptions<TAppState>();

        internal AppStateManagerOptions<TAppState> Build()
        {
            return Options;
        }

        public AppStateManagerOptionsBuilder<TAppState> UseTimeout(TimeSpan timeSpan)
        {
            Options.Timeout = timeSpan;
            return this;
        }
    }
}
