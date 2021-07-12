using Engie.PCC.Api.Models;
using Engie.PCC.Api.Notifiers;
using Engie.PCC.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Controllers
{
    [Route("")]
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status500InternalServerError)]
    public class ProductionPlanController : ControllerBase
    {
        private readonly IProductionPlanService _productionPlanService;
        private readonly ILoadResultNotifier _loadResultNotifier;
        private readonly ILogger<ProductionPlanController> _logger;

        public ProductionPlanController(
            IProductionPlanService productionPlanService,
            ILoadResultNotifier loadResultNotifier,
            ILogger<ProductionPlanController> logger)
        {
            _productionPlanService = productionPlanService;
            _loadResultNotifier= loadResultNotifier;
            _logger = logger;
        }

        /// <summary>
        /// Calculate the load for each powerplant.
        /// </summary>
        /// <param name="payload">The <see cref="PayLoad"/> object</param>
        /// <returns> A list of <see cref="PowerplantResult"/> with the load to apply on each powerplant</returns>
        [HttpPost("productionplan", Name="Production Plan Load Calculator")]
        [ProducesResponseType(typeof(List<PowerplantResult>), StatusCodes.Status200OK)]
        public async Task<List<PowerplantResult>> CalculateProductionPlanLoad([FromBody] Models.PayLoad payload)
        {
            _logger.LogInformation($"Received request to calculate load {payload.Load} at {DateTime.Now.ToShortTimeString() }.");
            _logger.LogDebug($"Paylod: \"{JsonConvert.SerializeObject(payload)}\".");

            var result = await _productionPlanService.Calculate(payload.Load, payload.Fuels, payload.Powerplants);

            _logger.LogDebug($"Notify result: \"{JsonConvert.SerializeObject(result)}\".");
            await _loadResultNotifier.NotifyLoadResultAsync(payload, result);

            return result;
        }
    }
}
