using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Tests
{
    public class ApiClient : IDisposable
    {
        private ClientWebSocket _webSocket = null;
        public ApiClient(Uri uri)
        {
            Connect(uri);
        }

        public void Dispose()
        {
            if (_webSocket != null) { _webSocket.Dispose(); }
        }

        private void Connect(Uri uri)
        {
            _webSocket = new ClientWebSocket();
            _webSocket.ConnectAsync(uri, CancellationToken.None).Wait();
        }

        public async Task<string> Receive(CancellationToken cancellationToken)
        {
            try
            {
                byte[] buffer = new byte[8192];
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                    }
                    else
                    {
                        return Encoding.UTF8.GetString(buffer).Trim();
                    }
                }
            }
            catch (WebSocketException)
            {
                _webSocket.Dispose();
                throw;
            }
            return "";
        }
    }
}
