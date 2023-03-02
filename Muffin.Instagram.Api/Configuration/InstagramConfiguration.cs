namespace CreateIF.Instagram.Api.Configuration
{
    public static class InstagramConfiguration
    {
        public static class Scopes
        {
            public const string USER_PROFILE = "user_profile";
            public const string USER_MEDIA = "user_media";
        }

        public const string AUTH_URL_TEMPLATE = "https://api.instagram.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}&response_type=code";
        public const string ACCESS_TOKEN_URL = "https://api.instagram.com/oauth/access_token";
        public const string LONG_LIVED_USER_ACCESS_TOKEN_URL = "https://graph.instagram.com/access_token?grant_type=ig_exchange_token&client_secret={0}&access_token={1}";
        public const string REFRESH_ACCESS_TOKEN_URL = "https://graph.instagram.com/refresh_access_token?grant_type=ig_refresh_token&access_token={0}";
        public const string ME_URL = "https://graph.instagram.com/v16.0/me?fields={0}&access_token={1}";
        public const string INSIGHTS_URL = "https://graph.facebook.com/{0}/{1}/insights";
        public const string BUSINESS_DISCOVERY_URL = "https://graph.facebook.com/{0}/{1}?fields=business_discovery.username({2}){followers_count}&access_token={3}";

        public static string GetAuthUrl(string clientId, string redirectUri, string scope)
        {
            return string.Format(AUTH_URL_TEMPLATE, clientId, redirectUri, scope);
        }

        public static string GetLongLivedUserAccessTokenUrl(string clientSecret, string accessToken)
        {
            return string.Format(LONG_LIVED_USER_ACCESS_TOKEN_URL, clientSecret, accessToken);
        }

        public static string GetRefreshAccessTokenUrl(string longLivedAccessToken)
        {
            return string.Format(REFRESH_ACCESS_TOKEN_URL, longLivedAccessToken);
        }

        public static string GetMeUrl(string accessToken, params string[] fields)
        {
            return string.Format(ME_URL, string.Join(",", fields), accessToken);
        }

        public static string GetBusinessDiscoveryUrl(string apiVersion, string myIgUserId, string queryUsername, string accessToken)
        {
            return $"https://graph.facebook.com/{apiVersion}/{myIgUserId}?fields=business_discovery.username({queryUsername}){{followers_count}}&access_token={accessToken}";
        }
    }
}