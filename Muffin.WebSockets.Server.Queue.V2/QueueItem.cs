using Muffin.Common.Api.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Muffin.WebSockets.Server.Queue
{
    public class QueueItem<TClientInterface>
    {
        public bool IsBroadcast { get; set; }
        public string Identity { get; set; }
        public string ChannelId { get; set; }
        public Expression<Action<TClientInterface>> Action { get; set; }
        public ApiRequest ApiRequest { get; set; }
        public string ActionHash { get; set; }
        public IEnumerable<string> IgnoreSocketIds { get; set; }

        private string _requestId;
        public string RequestId
        {
            get
            {
                if (ApiRequest != null)
                {
                    return ApiRequest.RequestId;
                }
                return _requestId;
            }
            set
            {
                if (ApiRequest != null)
                {
                    ApiRequest.RequestId = value;
                }
                else
                {
                    _requestId = value;
                }
            }
        }

        public QueueItem(string identity, Expression<Action<TClientInterface>> action, ApiRequest apiRequest, string actionHash, IEnumerable<string> ignoreSocketIds)
        {
            Identity = identity;
            Action = action;
            ApiRequest = apiRequest;
            ActionHash = actionHash;
            IgnoreSocketIds = ignoreSocketIds;
        }

        public QueueItem(Expression<Action<TClientInterface>> action, ApiRequest apiRequest)
        {
            IsBroadcast = true;
            Action = action;
            ApiRequest = apiRequest;
        }
    }
}
