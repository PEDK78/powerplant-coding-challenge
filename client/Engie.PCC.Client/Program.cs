using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Engie.PCC.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ClientWebSocket webSocket = null;

            try
            {
                webSocket = new ClientWebSocket();
                await webSocket.ConnectAsync(new Uri("ws://localhost:8888/notifications"), CancellationToken.None);
                await Task.WhenAll(Receive(webSocket));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
            }
            finally
            {
                if (webSocket != null) { webSocket.Dispose(); }
            }
        }

        private static async Task Receive(ClientWebSocket webSocket)
        {
            try
            {
                byte[] buffer = new byte[8192];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                    else
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(buffer));
                    }
                }
            }
            catch (WebSocketException wse)
            {
                Console.WriteLine($"WebSocket error occured: {wse}");
                webSocket.Dispose();
            }
        }
    }
}
