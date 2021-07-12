using Engie.PCC.Api.Services;
using Engie.PCC.Api.Services.Calculators;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Tests
{
    public abstract class TestBase
    {
        private ProductionPlanService _productionPlanService;
        public ProductionPlanService GetProductionPlanService()
        {
            if(_productionPlanService != null) { return _productionPlanService; }

            var logger = NullLogger<ProductionPlanService>.Instance;

            _productionPlanService = new ProductionPlanService(
                new PowerplantCalculatorFactory(),
                logger);

            return _productionPlanService;
        }
    }
}
