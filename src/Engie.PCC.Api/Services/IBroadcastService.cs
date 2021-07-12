using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Services
{
    public interface IBroadcastService
    {
        /// <summary>
        /// Add the <see cref="WebSocket">client</see> to the subscribers list.
        /// </summary>
        void Subscribe(WebSocket webSocket);

        /// <summary>
        /// Remove the <see cref="WebSocket">client</see> from the subscribers list.
        /// </summary>
        void Unsubscribe(WebSocket webSocket);

        /// <summary>
        /// Broadcasts a message to all subscribers.
        /// </summary>
        Task BroadcastMessageAsync(string message);
    }

}
