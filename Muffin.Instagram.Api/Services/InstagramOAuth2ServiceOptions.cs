using System;
using System.Collections.Generic;

namespace CreateIf.Instagram.Services
{
    public class InstagramOAuth2ServiceOptions
    {
        public string ApiVersion { get; internal set; } = "v16.0";
        public string AppId { get; internal set; }
        public string AppSecet { get; internal set; }
        public string RedirectUri { get; internal set; }
        public List<string> Scopes { get; internal set; } = new List<string>();
        public bool UseRefreshService { get; internal set; }
        public Type RefreshServiceType { get; internal set; }
        public Type RefreshServiceImplementationType { get; internal set; }

        /// <summary>
        /// The service interval when access tokens get refreshed
        /// </summary>
        public TimeSpan RefreshServiceInterval { get; internal set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// The minimum remaining age of the access token until it get refreshed.
        /// </summary>
        public TimeSpan MinAccessTokenLifeSpan { get; internal set; } = TimeSpan.FromDays(7);

        public InstagramOAuth2Events Events { get; private set; } = new InstagramOAuth2Events();
    }
}