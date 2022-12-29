using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.Common.Api.WebSockets;
using Muffin.Common.Util;
using Muffin.WebSockets.Server.Handler;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Queue
{
    public class MessageHandlerQueue<TWebSocketHandler, TClientInterface>
        where TWebSocketHandler : WebSocketHandler
    {
        #region Properties

        public readonly IServiceProvider ServiceProvider;
        private readonly TWebSocketHandler MessageHandler;
        private readonly ILogger Logger;
        private readonly ConcurrentQueue<QueueItem<TClientInterface>> QueueItems;
        private readonly ExpirableDictionary<string, string> LastSendHashes;
        private readonly MessageHandlerQueueEvents<TClientInterface> Events;
        private readonly WebSocketHandlerEvents WebSocketHandlerEvents;

        #endregion

        #region Constructor

        public MessageHandlerQueue(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            MessageHandler = serviceProvider.GetRequiredService<TWebSocketHandler>();
            Logger = serviceProvider.GetRequiredService<ILogger<MessageHandlerQueue<TWebSocketHandler, TClientInterface>>>();
            Events = serviceProvider.GetRequiredService<MessageHandlerQueueEvents<TClientInterface>>();
            WebSocketHandlerEvents = serviceProvider.GetService<WebSocketHandlerEvents>();
            QueueItems = new ConcurrentQueue<QueueItem<TClientInterface>>();
            LastSendHashes = new ExpirableDictionary<string, string>();
        }

        #endregion

        #region Actions

        public async Task EnqueueBroadcastForSend(Expression<Action<TClientInterface>> action)
        {
            var queueItem = new QueueItem<TClientInterface>(action, null);

            Events?.InvokeOnBeforeEnqueue(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));
            QueueItems.Enqueue(queueItem);
            Events?.InvokeOnEnqueue(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));
            await _runAsync();
        }

        public async Task EnqueueBroadcastForSend(ApiRequest apiRequest)
        {
            var queueItem = new QueueItem<TClientInterface>(null, apiRequest);

            Events?.InvokeOnBeforeEnqueue(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));
            QueueItems.Enqueue(queueItem);
            Events?.InvokeOnEnqueue(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));
            await _runAsync();
        }

        public async Task EnqueueForSendUsingChannel(string channelId, Expression<Action<TClientInterface>> action)
        {
            await EnqueueForSend(null, channelId, action, null, null, null, null);
        }

        public async Task EnqueueForSendUsingChannel(string channelId, Expression<Action<TClientInterface>> action, IEnumerable<string> ignoreSocketIds)
        {
            await EnqueueForSend(null, channelId, action, null, null, null, ignoreSocketIds);
        }

        public async Task<WebSocketPromise<T>> EnqueueForSend<T>(string identity, Expression<Func<TClientInterface, T>> func)
        {
            if (WebSocketHandlerEvents == null)
            {
                throw new Exception("No WebSocketHandlerEvents configured");
            }

            var requestId = Guid.NewGuid().ToString();
            var promise = new WebSocketPromise<T>(requestId, WebSocketHandlerEvents);
            var action = Expression.Lambda<Action<TClientInterface>>(func.Body, func.Parameters);
            await EnqueueForSend(identity, null, action, null, requestId, null, null);

            return promise;
        }

        public async Task EnqueueForSend(string identity, Expression<Action<TClientInterface>> action)
        {
            await EnqueueForSend(identity, action, null);
        }

        public async Task EnqueueForSend(string identity, ApiRequest apiRequest)
        {
            await EnqueueForSend(identity, null, apiRequest, null);
        }

        public async Task EnqueueForSend(string identity, Expression<Action<TClientInterface>> action, string actionHash)
        {
            await EnqueueForSend(identity, action, null, actionHash);
        }

        public async Task EnqueueForSendButIgnore(string identity, Expression<Action<TClientInterface>> action, IEnumerable<string> ignoreSocketIds)
        {
            await EnqueueForSend(identity, null, action, null, null, null, ignoreSocketIds);
        }

        public async Task EnqueueForSend(string identity, Expression<Action<TClientInterface>> action, ApiRequest apiRequest, string actionHash)
        {
            await EnqueueForSend(identity, null, action, apiRequest, null, actionHash, null);
        }

        public async Task EnqueueForSend(string identity, string channelId, Expression<Action<TClientInterface>> action, ApiRequest apiRequest, string requestId, string actionHash, IEnumerable<string> ignoreSocketIds)
        {
            if (!string.IsNullOrWhiteSpace(identity) || !string.IsNullOrWhiteSpace(channelId))
            {
                var queueItem = new QueueItem<TClientInterface>(identity, action, apiRequest, actionHash, ignoreSocketIds);
                if (!string.IsNullOrWhiteSpace(requestId))
                {
                    queueItem.RequestId = requestId;
                }
                queueItem.ChannelId = channelId;

                Events?.InvokeOnBeforeEnqueue(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));
                QueueItems.Enqueue(queueItem);
                Events?.InvokeOnEnqueue(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));
                await _runAsync();
            }
        }

        public bool IsRunning { get; private set; }
        private static object _lock = new object();
        private bool IsDoRecheck;
        private async Task _runAsync()
        {
            try
            {
                lock (_lock)
                {
                    if (IsRunning)
                    {
                        Logger?.LogInformation($"MessageHandlerQueue set recheck");
                        IsDoRecheck = true;
                        return;
                    }
                    IsRunning = true;
                    IsDoRecheck = true;
                }

                Events?.InvokeOnStartRunning(this, new MessageHandlerQueueEventArgs<TClientInterface>());
                while (IsDoRecheck)
                {
                    Logger?.LogInformation($"MessageHandlerQueue run");
                    lock (_lock)
                    {
                        IsDoRecheck = false;
                    }

                    while (QueueItems.TryDequeue(out QueueItem<TClientInterface> queueItem))
                    {
                        try
                        {
                            Events?.InvokeOnDequeue(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));

                            if (string.IsNullOrWhiteSpace(queueItem.Identity) && !queueItem.IsBroadcast && string.IsNullOrWhiteSpace(queueItem.ChannelId))
                            {
                                Logger?.LogInformation($"MessageHandlerQueue skip due to no identity or broadcast or channelid");
                                Events?.InvokeOnSkip(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));
                                continue;
                            }

                            List<string> receivers = new List<string>();
                            var sent = false;
                            if (!string.IsNullOrWhiteSpace(queueItem.ChannelId))
                            {
                                if (queueItem.Action != null)
                                {
                                    Logger?.LogInformation($"Pre InvokeSubscriptionAsync {queueItem.ChannelId} with action {queueItem.Action}");
                                    Events?.InvokeOnBeforeSend(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));

                                    Logger?.LogInformation($"InvokeSubscriptionAsync {queueItem.Identity} with action {queueItem.Action}");
                                    var temp = await MessageHandler.InvokeSubscriptionAsync(queueItem.ChannelId, queueItem.Action, queueItem.IgnoreSocketIds);
                                    Logger?.LogInformation($"InvokeSubscriptionAsync {queueItem.Identity} with action {queueItem.Action}");

                                    Events?.InvokeOnSent(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem, temp));
                                    sent = true;

                                    if (temp != null)
                                    {
                                        receivers.AddRange(temp);
                                    }
                                }

                                if (queueItem.ApiRequest != null)
                                {
                                    Logger?.LogInformation($"Pre InvokeSubscriptionAsync {queueItem.Identity} with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");
                                    Events?.InvokeOnBeforeSend(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));

                                    Logger?.LogInformation($"InvokeSubscriptionAsync {queueItem.Identity} with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");
                                    var temp = await MessageHandler.InvokeSubscriptionAsync(queueItem.ChannelId, queueItem.ApiRequest, queueItem.IgnoreSocketIds);
                                    Logger?.LogInformation($"InvokeSubscriptionAsync {queueItem.Identity} with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");

                                    Events?.InvokeOnSent(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem, temp));
                                    sent = true;

                                    if (temp != null)
                                    {
                                        receivers.AddRange(temp);
                                    }
                                }
                            }



                            if (!string.IsNullOrWhiteSpace(queueItem.ActionHash)
                                && LastSendHashes.TryGetValue(queueItem.Identity, out string hash)
                                && queueItem.ActionHash == hash)
                            {
                                Logger?.LogInformation($"MessageHandlerQueue skip model send {queueItem.Identity} {hash}");
                                Events?.InvokeOnSkip(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));
                                continue;
                            }

                            if (!sent && queueItem.Action != null)
                            {
                                if (queueItem.IsBroadcast)
                                {
                                    Logger?.LogInformation($"Pre InvokeAll with action {queueItem.Action}");
                                    Events?.InvokeOnBeforeSend(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));

                                    Logger?.LogInformation($"InvokeAll with action {queueItem.Action}");
                                    var temp = await MessageHandler.InvokeAll(queueItem.Action, queueItem.IgnoreSocketIds);
                                    Logger?.LogInformation($"InvokedAll with action {queueItem.Action}");

                                    Events?.InvokeOnSent(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem, temp));
                                    sent = true;

                                    if (temp != null)
                                    {
                                        receivers.AddRange(temp);
                                    }
                                }
                                else
                                {
                                    Logger?.LogInformation($"Pre InvokeWithIdentity {queueItem.Identity} with action {queueItem.Action}");
                                    Events?.InvokeOnBeforeSend(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));

                                    Logger?.LogInformation($"InvokeWithIdentity {queueItem.Identity} with action {queueItem.Action}");
                                    var temp = await MessageHandler.InvokeWithIdentity(queueItem.Identity, queueItem.RequestId, queueItem.Action, queueItem.IgnoreSocketIds);
                                    Logger?.LogInformation($"InvokedWithIdentity {queueItem.Identity} with action {queueItem.Action}");

                                    Events?.InvokeOnSent(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem, temp));
                                    sent = true;

                                    if (temp != null)
                                    {
                                        receivers.AddRange(temp);
                                    }
                                }
                            }

                            if (!sent && queueItem.ApiRequest != null)
                            {
                                if (queueItem.IsBroadcast)
                                {
                                    Logger?.LogInformation($"Pre InvokeAll with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");
                                    Events?.InvokeOnBeforeSend(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));

                                    Logger?.LogInformation($"InvokeAll with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");
                                    var temp = await MessageHandler.InvokeAll(queueItem.ApiRequest, queueItem.IgnoreSocketIds);
                                    Logger?.LogInformation($"InvokedAll with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");

                                    Events?.InvokeOnSent(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem, temp));
                                    sent = true;

                                    if (temp != null)
                                    {
                                        receivers.AddRange(temp);
                                    }
                                }
                                else
                                {
                                    Logger?.LogInformation($"Pre InvokeWithIdentity {queueItem.Identity} with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");
                                    Events?.InvokeOnBeforeSend(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem));

                                    Logger?.LogInformation($"InvokeWithIdentity {queueItem.Identity} with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");
                                    var temp = await MessageHandler.InvokeWithIdentity(queueItem.Identity, queueItem.ApiRequest, queueItem.IgnoreSocketIds);
                                    Logger?.LogInformation($"InvokedWithIdentity {queueItem.Identity} with apirequest {JsonConvert.SerializeObject(queueItem.ApiRequest)}");

                                    Events?.InvokeOnSent(this, new MessageHandlerQueueEventArgs<TClientInterface>(queueItem, temp));
                                    sent = true;

                                    if (temp != null)
                                    {
                                        receivers.AddRange(temp);
                                    }
                                }
                            }

                            if (receivers != null)
                            {
                                Logger.LogInformation($"Receivers: {string.Join(", ", receivers)}");
                            }

                            if (!string.IsNullOrWhiteSpace(queueItem.ActionHash))
                            {
                                Logger?.LogInformation($"MessageHandlerQueue add to last hashed {queueItem.ActionHash}");
                                LastSendHashes.Add(queueItem.Identity, queueItem.ActionHash);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger?.LogError("Send Failed!");
                            Logger?.LogError(ex.ToString());
                        }
                    }
                }

                IsRunning = false;
                Events?.InvokeOnEndRunning(this, new MessageHandlerQueueEventArgs<TClientInterface>());
            }
            catch (Exception e)
            {
                Events?.InvokeOnError(this, new MessageHandlerQueueEventArgs<TClientInterface>(e));
            }
        }

        #endregion
    }

    public class WebSocketPromise<T>
    {
        private readonly WebSocketHandlerEvents WebSocketHandlerEvents;
        public readonly string RequestId;
        private ApiRequest ApiRequest;
        //private Dictionary<string, object> ResponseParameters;
        private ManualResetEvent ResetEvent = new ManualResetEvent(false);
        public bool IsCompleted { get; private set; }
        public bool HasError => IsCompleted && Error != null;

        private T Result;
        public string Error { get; private set; }

        public WebSocketPromise(string requestId, WebSocketHandlerEvents webSocketHandlerEvents)
        {
            RequestId = requestId;
            WebSocketHandlerEvents = webSocketHandlerEvents;
            WebSocketHandlerEvents.OnApiReceive += WebSocketHandlerEvents_OnApiReceive;
        }

        ~WebSocketPromise()
        {
            WebSocketHandlerEvents.OnApiReceive -= WebSocketHandlerEvents_OnApiReceive;
        }

        private void WebSocketHandlerEvents_OnApiReceive(WebSocketHandler sender, ReceiveApiReventArgs args)
        {
            if (args?.ApiRequest?.RequestId != RequestId)
            {
                return;
            }

            //ResponseParameters = args.ApiRequest.Params;
            ApiRequest = args.ApiRequest;
            IsCompleted = true;
            ResetEvent.Set();
            GetResult();
        }

        public T GetResult(TimeSpan timeout)
        {
            return GetResult(timeout, CancellationToken.None);
        }

        public T GetResult()
        {
            return GetResult(null, CancellationToken.None);
        }

        public T GetResult(TimeSpan? timeout, CancellationToken cancellationToken = default)
        {
            if (Result == null)
            {
                Wait(timeout, cancellationToken);

                if (ApiRequest == null)
                {
                    Result = default;
                }
                else
                {
                    Result = ApiRequest.GetResult<T>();
                    Error = ApiRequest.Error;
                }
            }
            return Result;

            //if (ResponseParameters == null)
            //{
            //    return default;
            //}

            //if (ResponseParameters.TryGetValue("data", out var value))
            //{
            //    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value));
            //}

            //return default;
        }

        public WebSocketPromise<T> Wait(TimeSpan? timeout, CancellationToken cancellationToken = default)
        {
            if (timeout.HasValue)
            {
                WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, ResetEvent }, timeout.Value);
            }
            else
            {
                WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, ResetEvent });
            }
            return this;
        }
    }
}
