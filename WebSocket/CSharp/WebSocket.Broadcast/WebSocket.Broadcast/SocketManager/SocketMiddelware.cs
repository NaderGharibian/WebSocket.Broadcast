using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocket.Broadcast.SocketManager
{
    public class SocketMiddelware
    {
        private readonly RequestDelegate _next;
        private SocketHandler Handler { get; set; }
        private IHostApplicationLifetime applicationLifetime;
        public SocketMiddelware(RequestDelegate next, SocketHandler handler, IHostApplicationLifetime lifeTime)
        {
            _next = next;
            Handler = handler;
            applicationLifetime = lifeTime;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var party = context.Request.Headers["party"].FirstOrDefault() ?? "";
            if (!context.WebSockets.IsWebSocketRequest && party != null)
                context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var socket = await context.WebSockets.AcceptWebSocketAsync();

            await Handler.OnConnected(socket, party);
            await Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await Handler.Receive(party, socket, result, buffer);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await Handler.OnDisconnected(socket, applicationLifetime);
                }
            });
        }

        private async Task Receive(System.Net.WebSockets.WebSocket webSocket, Action<WebSocketReceiveResult, byte[]> messageHandler)
        {
            var buf = new byte[6 * 1024];

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buf), applicationLifetime.ApplicationStopping);

                    messageHandler(result, buf);
                }
                catch (Exception ex)
                {
                    break; // disconnected most likely
                }

            }
            await Handler.OnDisconnected(webSocket, applicationLifetime);
        }
    }
}