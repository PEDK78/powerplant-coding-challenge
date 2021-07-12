using Engie.PCC.Api.Models;

namespace Engie.PCC.Api.Services.Calculators
{
    public interface IPowerplantTypeCalculator
    {
        /// <summary>
        /// Compute cost per MWH
        /// </summary>
        /// <param name="fuels">a <see cref="Fuels"/> object containing price information</param>
        /// <param name="powerplant">The <see cref="Powerplant"/> for which computation must be done</param>
        /// <returns>The cost per MWH</returns>
        decimal ComputeCostPerMWH(Fuels fuels, Powerplant powerplant);

        /// <summary>
        /// Compute the minimum MWH deliverable by the powerplant
        /// </summary>
        /// <param name="fuels">a <see cref="Fuels"/> object containing price information</param>
        /// <param name="powerplant">The <see cref="Powerplant"/> for which computation must be done</param>
        /// <returns>The minimal MWH deliveralbe</returns>
        decimal ComputeMinimumDeliverablePower(Fuels fuels, Powerplant powerplant);

        /// <summary>
        /// Compute the maximum MWH deliverable by the powerplant
        /// </summary>
        /// <param name="fuels">a <see cref="Fuels"/> object containing price information</param>
        /// <param name="powerplant">The <see cref="Powerplant"/> for which computation must be done</param>
        /// <returns>The maxomal MWH deliveralbe</returns>
        decimal ComputeMaximumDeliverablePower(Fuels fuels, Powerplant powerplant);
    }
}
