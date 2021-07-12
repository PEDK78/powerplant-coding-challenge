using Engie.PCC.Api.Models;

namespace Engie.PCC.Api.Services.Calculators
{
    /// <summary>
    /// Dedicated calculator for Wind Turbine powerplant
    /// </summary>
    public class PowerplantCalculatorWindTurbine : IPowerplantTypeCalculator
    {
        private void CheckPowerplantType(PowerPlantType type)
        {
            if (type != PowerPlantType.WindTurbine)
            {
                throw new PowerplantCalculatorException($"Expect a \"{PowerPlantType.WindTurbine.ToString()}\" type.");
            }
        }

        /// <inheritdoc/>
        public decimal ComputeCostPerMWH(Fuels fuels, Powerplant powerplant)
        {
            CheckPowerplantType(powerplant.Type);

            decimal CO2Cost = 0;
            decimal MWHCost = 0;

            return CO2Cost + MWHCost;
        }

        /// <inheritdoc/>
        public decimal ComputeMinimumDeliverablePower(Fuels fuels, Powerplant powerplant)
        {
            CheckPowerplantType(powerplant.Type);

            return (powerplant.MinPower * fuels.Wind) / 100;
        }

        /// <inheritdoc/>
        public decimal ComputeMaximumDeliverablePower(Fuels fuels, Powerplant powerplant)
        {
            CheckPowerplantType(powerplant.Type);

            return (powerplant.MaxPower * fuels.Wind) / 100;
        }
    }
}
