using Engie.PCC.Api.Models;

namespace Engie.PCC.Api.Services.Calculators
{
    /// <summary>
    /// Dedicated calculator for Gas Fired powerplant
    /// </summary>
    public class PowerplantCalculatorGasfired : IPowerplantTypeCalculator
    {
        private const decimal CO2TonPerMWH = 0.3m;

        private void CheckPowerplantType(PowerPlantType type)
        {
            if(type != PowerPlantType.GasFired)
            {
                throw new PowerplantCalculatorException($"Expect a \"{PowerPlantType.GasFired.ToString()}\" type.");
            }
        }

        /// <inheritdoc/>
        public decimal ComputeCostPerMWH(Fuels fuels, Powerplant powerplant)
        {
            CheckPowerplantType(powerplant.Type);

            decimal CO2Cost = fuels.Co2EuroTon * CO2TonPerMWH;
            decimal MWHCost = fuels.GasEuroMWh * (1 / powerplant.Efficiency);

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
