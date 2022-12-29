//using System;
//using System.Collections.Generic;

//namespace Muffin.Common.Util.WebSockets
//{
//    public class SubscriptionChannelManager
//    {
//        public Dictionary<string, SubscriptionChannel> Channels { get; private set; } = new Dictionary<string, SubscriptionChannel>();

//        public SubscriptionChannel CreateSubscriptionChannel(string name)
//        {
//            if (Channels.ContainsKey(name))
//            {
//                throw new ArgumentException($"Channel {name} already exists!");
//            }

//            var channel = new SubscriptionChannel(name);
//            Channels.Add(name, channel);
//            return channel;
//        }
//    }

//    public class SubscriptionChannel
//    {
//        public string Name { get; set; }

//        public SubscriptionChannel(string name)
//        {
//            Name = name;
//        }
//    }
//}
