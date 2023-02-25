using System;
using CreateIf.Instagram.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CreateIF.Instagram.Api.Controllers
{

    [Route("instagram-auth")]
    public class AuthController : Controller
    {

        private readonly InstagramOAuth2Events _events;

        public AuthController(IServiceProvider serviceProvider)
        {
            _events = serviceProvider.GetRequiredService<InstagramOAuth2Events>();
        }

        public virtual IActionResult DoAuth(AuthResponse model)
        {
            if (model != null)
            {
                if (model.Success)
                {
                    _events.InvokeOnAuthSuccess(model.Code, HttpContext);
                    return _events.InvokeOnAuthSuccessRedirection(model.Code, HttpContext) ?? Ok();
                }
                else
                {
                    var exception = new Exception($"{model.Error} {model.ErrorReason} {model.ErrorDescription}");
                    _events.InvokeOnAuthError(exception, HttpContext);
                    return _events.InvokeOnAuthErrorRedirection(exception, HttpContext) ?? BadRequest(model.ErrorReason);
                }
            }
            return BadRequest();
        }
    }

    public class AuthResponse
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_reason")]
        public string ErrorReason { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        [JsonIgnore]
        public bool Success => string.IsNullOrWhiteSpace(Error);
    }
}