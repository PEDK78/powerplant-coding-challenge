using Engie.PCC.Api.Models;

namespace Engie.PCC.Api.Services.Calculators
{
    /// <summary>
    /// Dedicated calculator for Turbojet powerplant
    /// </summary>
    public class PowerplantCalculatorTurbojet : IPowerplantTypeCalculator
    {
        private void CheckPowerplantType(PowerPlantType type)
        {
            if (type != PowerPlantType.Turbojet)
            {
                throw new PowerplantCalculatorException($"Expect a \"{PowerPlantType.Turbojet.ToString()}\" type.");
            }
        }

        /// <inheritdoc/>
        public decimal ComputeCostPerMWH(Fuels fuels, Powerplant powerplant)
        {
            CheckPowerplantType(powerplant.Type);

            decimal CO2Cost = 0;
            decimal MWHCost = fuels.KerosineEuroMWh * (1 / powerplant.Efficiency);

            return CO2Cost + MWHCost;
        }

        /// <inheritdoc/>
        public decimal ComputeMinimumDeliverablePower(Fuels fuels, Powerplant powerplant)
        {
            CheckPowerplantType(powerplant.Type);

            return powerplant.MinPower;
        }

        /// <inheritdoc/>
        public decimal ComputeMaximumDeliverablePower(Fuels fuels, Powerplant powerplant)
        {
            CheckPowerplantType(powerplant.Type);

            return powerplant.MaxPower;
        }
    }
}
