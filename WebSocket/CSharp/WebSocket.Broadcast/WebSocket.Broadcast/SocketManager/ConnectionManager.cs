using WebSocket.Broadcast.Domain;

using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocket.Broadcast.SocketManager
{
    public class ConnectionManager
    {
        private ConcurrentDictionary<string, SocketTrack> _connections = new ConcurrentDictionary<string, SocketTrack>();

        public SocketTrack GetSocketByID(string id)
        {
            return _connections.FirstOrDefault(x => x.Key == id).Value;
        }
        public string GetID(System.Net.WebSockets.WebSocket socket)
        {
            return _connections.FirstOrDefault(x => x.Value.Socket == socket).Key;
        }
        //public string GetByOrder(WebSocket socket)
        //{
        //    return _connections.FirstOrDefault(x => x.Value. == socket).Key;
        //}
        public  List<SocketTrack> BroadcastParty(string party)
        {
            return _connections.Where(i=> i.Value.Party == party).Select(i=> i.Value).ToList();
        }
        public async Task RemoveSocketAsync(string Id, IHostApplicationLifetime lifeTime)
        {
            _connections.TryRemove(Id, out var socket);
            await socket.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socket connection closed!", lifeTime.ApplicationStopping);

        }
        public void AddSocket(System.Net.WebSockets.WebSocket socket, string party)
        {
            var _socket = new SocketTrack { Socket = socket, Party = party };
            _connections.TryAdd(Guid.NewGuid().ToString("N"), _socket);
        }
    }

}
