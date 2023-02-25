using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CreateIF.Instagram.Api.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CreateIf.Instagram.Services
{
    public class InstagramOAuth2Events
    {
        public event InstagramOAuth2SuccessHandler OnAuthSuccess;
        public event InstagramOAuth2ErrorHandler OnAuthError;
        public event InstagramOAuth2RedirectionSuccessHandler OnAuthSuccessRedirection;
        public event InstagramOAuth2RedirectionErrorHandler OnAuthErrorRedirection;

        internal void InvokeOnAuthSuccess(string code, HttpContext httpContext)
        {
            OnAuthSuccess?.Invoke(code, httpContext);
        }

        internal void InvokeOnAuthError(Exception exception, HttpContext httpContext)
        {
            OnAuthError?.Invoke(exception, httpContext);
        }

        internal IActionResult InvokeOnAuthSuccessRedirection(string code, HttpContext httpContext)
        {
            return OnAuthSuccessRedirection?.Invoke(code, httpContext);
        }

        internal IActionResult InvokeOnAuthErrorRedirection(Exception exception, HttpContext httpContext)
        {
            return OnAuthErrorRedirection?.Invoke(exception, httpContext);
        }
    }

    public delegate void InstagramOAuth2SuccessHandler(string code, HttpContext httpContext);
    public delegate void InstagramOAuth2ErrorHandler(Exception exception, HttpContext httpContext);
    public delegate IActionResult InstagramOAuth2RedirectionSuccessHandler(string code, HttpContext httpContext);
    public delegate IActionResult InstagramOAuth2RedirectionErrorHandler(Exception exception, HttpContext httpContext);
}