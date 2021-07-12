using Engie.PCC.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Services
{
    public interface IProductionPlanService
    {
        /// <summary>
        /// Compute the load to apply on each powerplant
        /// </summary>
        /// <param name="load">The requested load</param>
        /// <param name="fuels">Information about price</param>
        /// <param name="powerPlants">A list of <see cref="Powerplant"/> to use for dispatching the requested load.</param>
        /// <returns>A list of <see cref="PowerplantResult"/> containing the load to apply on each powerplant</returns>
        Task<List<PowerplantResult>> Calculate(decimal load, Fuels fuels, IEnumerable<Powerplant> powerPlants);
    }
}
