using Engie.PCC.Api.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Middlewares
{
    /// <summary>
    /// middleware for handling incoming <see cref="WebSocket"/> connections.
    /// </summary>
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IBroadcastService _broadcastService;

        public WebSocketMiddleware(RequestDelegate next, IBroadcastService broadcastService)
        {
            _next = next;
            _broadcastService = broadcastService;
        }

        public async Task Invoke(HttpContext context)
        {

            if (!context.WebSockets.IsWebSocketRequest || !string.Equals(context.Request.Path, "/Notifications", StringComparison.OrdinalIgnoreCase))
            {
                await _next.Invoke(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
            _broadcastService.Subscribe(socket);

            // Loop listening in on the socket.
            while (socket.State == WebSocketState.Open)
            {
                try
                {
                    //not specified if the client should provide notification ...
                    //so I assume it is a one way communication only
                }
                catch (WebSocketException e)
                {
                    if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely){socket.Abort();}
                }
            }

            _broadcastService.Unsubscribe(socket);
        }
    }
}
