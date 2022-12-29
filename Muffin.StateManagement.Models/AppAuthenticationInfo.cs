using Newtonsoft.Json;
using System.Collections.Generic;

namespace Muffin.StateManagement.Models
{
    public class AppAuthenticationInfo
    {
        [JsonProperty(PropertyName = "signInOptions")]
        public Dictionary<string, AppSignInOption> SignInOptions { get; set; } = new Dictionary<string, AppSignInOption>();

        [JsonProperty(PropertyName = "signOutPath")]
        public string SignOutPath { get; set; } = "/Auth/Logout";
    }


}
