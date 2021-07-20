using Newtonsoft.Json;

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {

            StartWebSockets().GetAwaiter().GetResult();


        }

        public static async Task StartWebSockets()
        {
            var serviceUri = new Uri("ws://localhost:48192/ws");
            var client = new ClientWebSocket();
            Console.WriteLine("party name?");
            client.Options.SetRequestHeader("party", Console.ReadLine());

            await client.ConnectAsync(serviceUri, CancellationToken.None);

            Console.WriteLine($"web socket connection established @ {DateTime.Now:F}");
            var send = Task.Run(async () =>
            {


                string msg;
                while ((msg = Console.ReadLine()) != null && msg != string.Empty)
                {
                    //var obj = JsonConvert.SerializeObject(new { lat = msg });
                    var bytes = Encoding.UTF8.GetBytes(msg);
                    await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true
                        , CancellationToken.None);
                }
                await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            });
            var receive = ReceiveAsync(client);
            await Task.WhenAll(send, receive);
        }

        private static async Task ReceiveAsync(ClientWebSocket client)
        {
            var buf = new byte[1024 * 6];
            while (true)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);
                Console.WriteLine(Encoding.UTF8.GetString(buf, 0, result.Count));
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
                }
            }
        }
    }
}
