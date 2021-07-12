using Engie.PCC.Api.Models;
using Engie.PCC.Api.Services;
using Engie.PCC.Api.Services.Hosted;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Notifiers
{
    public class LoadResultNotifier : ILoadResultNotifier
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _taskQueue;

        public LoadResultNotifier(
            IServiceScopeFactory serviceScopeFactory,
            IBackgroundTaskQueue taskQueue)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _taskQueue = taskQueue;
        }

        /// <summary>
        /// Notification will be done using a background task ....
        /// Other options like message bus, event hub can be used.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        public async Task NotifyLoadResultAsync(PayLoad payload, List<PowerplantResult> results)
        {
            await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var broadcastService = scopedServices.GetRequiredService<IBroadcastService>();
                    await broadcastService.BroadcastMessageAsync(JsonConvert.SerializeObject(WebSocketMessage.Create(payload, results)));
                }
            });
        }
    }
}
