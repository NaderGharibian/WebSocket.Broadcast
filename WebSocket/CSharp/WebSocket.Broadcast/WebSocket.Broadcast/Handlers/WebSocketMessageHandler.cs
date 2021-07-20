using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

using WebSocket.Broadcast.SocketManager;

namespace WebSocket.Broadcast.Handlers
{
    public class WebSocketMessageHandler : SocketHandler
    {
        public WebSocketMessageHandler(ConnectionManager connections) : base(connections)
        {

        }
        public override async Task OnConnected(System.Net.WebSockets.WebSocket socket, string party)
        {
            await base.OnConnected(socket,party);
            var socketID = Connections.GetID(socket);
            await BroadcastParty(party, $"{socketID} just joined the party " + party);
        }
        public override  async Task Receive( string party, System.Net.WebSockets.WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketID = Connections.GetID(socket);
            var message = $"{socketID} said: {Encoding.UTF8.GetString(buffer, 0, result.Count)}";
            await BroadcastParty(party, message);// SendByIDMessage(socketID, message)
                 

        }
    }
}
