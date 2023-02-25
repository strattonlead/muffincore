using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Muffin.Common.Api.WebSockets;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Models
{
    public class WebSocketActionResult : IActionResult
    {
        public WebSocketResponse WebSocketResponse { get; protected set; }
        public ApiRequest ApiRequest { get; protected set; }
        private readonly ILogger Logger;

        public WebSocketActionResult(WebSocketResponse webSocketResponse, ILogger logger)
        {
            WebSocketResponse = webSocketResponse;
            Logger = logger;
        }

        public WebSocketActionResult(WebSocketResponse webSocketResponse, ApiRequest apiRequest, ILogger logger)
            : this(webSocketResponse, logger)
        {
            ApiRequest = apiRequest;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            Logger?.LogInformation($"WebSocketActionResult ExecuteResultAsync Controller: {WebSocketResponse?.WebSocketRequest?.Controller} Action: {WebSocketResponse?.WebSocketRequest?.Action} RID: {WebSocketResponse?.WebSocketRequest?.RequestId} Principal: {WebSocketResponse?.WebSocketRequest?.Principal?.Identity?.Name} SocketId: {WebSocketResponse?.WebSocketRequest?.SocketId}");
            await WebSocketResponse.SendObjectAsync(ApiRequest);
            Logger?.LogInformation($"WebSocketActionResult ExecuteResultAsync sent");
        }
    }

    public class WebSocketActionResult<T> : WebSocketActionResult
    {
        public Expression<Action<T>> Expression { get; private set; }

        public WebSocketActionResult(WebSocketResponse WebSocketsResponse, Expression<Action<T>> expression, ILogger logger)
            : base(WebSocketsResponse, logger)
        {
            Expression = expression;
            ApiRequest = ApiRequestHelper.FromExpression(expression, WebSocketsResponse.WebSocketRequest?.RequestId);
        }
    }
}
