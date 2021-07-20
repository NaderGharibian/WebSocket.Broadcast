using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket.Broadcast.SocketManager
{
    public abstract class SocketHandler
    {
        public ConnectionManager Connections { get; set; }
        public SocketHandler(ConnectionManager connectionManager)
        {
            Connections = connectionManager;
        }
        public virtual async Task OnConnected(System.Net.WebSockets.WebSocket socket,string party)
        {
            await Task.Run(() =>
            {
                Connections.AddSocket(socket,party);
            });
        }
        public virtual async Task OnDisconnected(System.Net.WebSockets.WebSocket socket, IHostApplicationLifetime lifeTime)
        {
            await Connections.RemoveSocketAsync(Connections.GetID(socket), lifeTime);
        }
        public virtual async Task SendMessage(System.Net.WebSockets.WebSocket socket, string message)
        {
            if (socket.State == WebSocketState.None)
                return;

            await socket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(message), 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public virtual async Task BroadcastParty(string party, string message)
        {
            foreach (var item in Connections.BroadcastParty(party))
                await SendMessage(item.Socket, message);

        }
        //public async Task SendByIDMessage(string id,string message)
        //{
        //    await SendMessage(Connections.GetSocketByID(id), message);
        //}

        public abstract Task Receive(string party, System.Net.WebSockets.WebSocket socket, WebSocketReceiveResult result, byte[] buffer);
    }
    public static class SocketExtension
    {
        public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
        {
            services.AddTransient<ConnectionManager>();
            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(SocketHandler))
                    services.AddSingleton(type);
            }
            return services;
        }
        public static IApplicationBuilder MapSockets(this IApplicationBuilder app, PathString path, SocketHandler socket)
        {
            app.Map(path, (x) => x.UseMiddleware<SocketMiddelware>(socket));
            return app;
        }
    }

   
}
