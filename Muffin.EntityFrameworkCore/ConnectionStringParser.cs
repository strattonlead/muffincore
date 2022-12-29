using System;
using System.Collections.Generic;

namespace Muffin.EntityFrameworkCore
{
    public static class ConnectionStringParser
    {
        public static ConnectionString Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Parameter connection string is null or whitespace");
            }

            var result = new Dictionary<string, string>();
            var pairs = s.Split(";");
            foreach (var pair in pairs)
            {
                var entry = pair.Split('=');
                if (entry.Length == 2)
                {
                    result.Add(entry[0].ToLower(), entry[1]);
                }
            }

            return new ConnectionString(result);
        }
    }

    public class ConnectionString
    {
        // password=tide1905;Trusted_Connection=False;MultipleActiveResultSets=true
        public string Server { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool TrustedConnection { get; set; }
        public bool MultipleActiveResultSets { get; set; }

        public ConnectionString(Dictionary<string, string> pairs)
        {
            if (pairs.TryGetValue("server", out var server))
            {
                Server = server.ToString();
            }

            if (pairs.TryGetValue("database", out var database))
            {
                Database = database.ToString();
            }

            if (pairs.TryGetValue("user id", out var username))
            {
                Username = username.ToString();
            }

            if (pairs.TryGetValue("password", out var password))
            {
                Password = password.ToString();
            }

            if (pairs.TryGetValue("trusted_connection", out var tcs) && bool.TryParse(tcs, out var trustedConnection))
            {
                TrustedConnection = trustedConnection;
            }

            if (pairs.TryGetValue("multipleactiveresultsets", out var marss) && bool.TryParse(marss, out var multipleActiveResultSets))
            {
                MultipleActiveResultSets = multipleActiveResultSets;
            }
        }
    }
}
