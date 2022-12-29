using Muffin.WebSockets.Server.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.WebSockets;

namespace Muffin.WebSockets.Server.Handler
{
    public class SocketInfo
    {
        public WebSocketConnectionId ConnectionId { get; set; }
        public string SocketId => ConnectionId?.ToString();
        public long? TenantId { get => ConnectionId?.TenantId; set => ConnectionId.TenantId = value; }
        public WebSocket WebSocket { get; set; }
        [Display(Name = "Erstellt am")]
        public DateTime CreatedDateUtc { get; set; } = DateTime.UtcNow;
        [Display(Name = "Zuletzt gesendet")]
        public DateTime LastSendDate { get; set; } = DateTime.UtcNow;
        [Display(Name = "Zuletzt empfangen")]
        public DateTime LastReceiveDate { get; set; } = DateTime.UtcNow;
        [Display(Name = "Letzte Aktion")]
        public DateTime LastActionDate
        {
            get
            {
                return new DateTime[] {
                    CreatedDateUtc,
                    LastSendDate,
                    LastReceiveDate
                }.Max();
            }
        }
        [Display(Name = "Identität")]
        public string Identity { get; set; }
        [Display(Name = "Rollen")]
        public IEnumerable<string> Roles { get; set; }
        [Display(Name = "Gesendet")]
        public long BytesSent { get; set; }
        [Display(Name = "Emfangen")]
        public long BytesReceived { get; set; }

        public List<string> Subscriptions { get; set; } = new List<string>();
        public void Subscribe(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (Subscriptions.Contains(name))
            {
                return;
            }

            Subscriptions.Add(name);
        }

        public void Unsubscribe(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (Subscriptions.Contains(name))
            {
                Subscriptions.Remove(name);
            }
        }

        public void UnsubscribeAll()
        {
            Subscriptions = new List<string>();
        }
    }
}
