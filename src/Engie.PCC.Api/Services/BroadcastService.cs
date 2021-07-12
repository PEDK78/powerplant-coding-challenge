using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace Engie.PCC.Api.Services
{
    /// <summary>
    /// Broadcast of messages to registered <see cref="WebSocket">clients</see>.
    /// </summary>
    public class BroadcastService : IBroadcastService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        /// <summary>
        /// Add the <see cref="WebSocket">client</see> the subscribers list.
        /// </summary>
        public void Subscribe(WebSocket webSocket)
        {
            _sockets.TryAdd(Guid.NewGuid().ToString(), webSocket);
        }

        /// <summary>
        /// Remove the <see cref="WebSocket">client</see> to subscribers list.
        /// </summary>
        public void Unsubscribe(WebSocket webSocket)
        {
            var key = _sockets.FirstOrDefault(p => p.Value == webSocket).Key;

            if (key != null)
            {
                _sockets.TryRemove(key, out WebSocket socket);
            }
        }

        /// <summary>
        /// Broadcasts a message to all subscribers.
        /// </summary>
        public async Task BroadcastMessageAsync(string message)
        {
            var msg = Encoding.UTF8.GetBytes(message);

            foreach (var webSocket in _sockets.Values)
            {
                try
                {
                    if (webSocket.State == WebSocketState.Open)
                    {            
                        await webSocket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), 
                            WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
                    }
                }
                catch (WebSocketException e)
                {
                    switch (e.WebSocketErrorCode)
                    {
                        case WebSocketError.UnsupportedVersion:
                        case WebSocketError.UnsupportedProtocol:
                        case WebSocketError.NotAWebSocket:
                        case WebSocketError.ConnectionClosedPrematurely:
                            Unsubscribe(webSocket);
                            break;
                    }
                }
            }
        }
    }
}
