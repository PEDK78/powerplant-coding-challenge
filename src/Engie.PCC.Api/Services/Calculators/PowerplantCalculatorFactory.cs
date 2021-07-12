using Engie.PCC.Api.Models;
using System.Collections.Generic;

namespace Engie.PCC.Api.Services.Calculators
{
    /// <summary>
    /// Factory using the flyweight pattern to provide the powerplant type related calcultor
    /// </summary>
    public class PowerplantCalculatorFactory
    {
        private Dictionary<string, IPowerplantTypeCalculator> calculators { get; set; } = new Dictionary<string, IPowerplantTypeCalculator>();

        public PowerplantCalculatorFactory()
        {
            calculators.Add(PowerPlantType.GasFired.ToString(), new PowerplantCalculatorGasfired());
            calculators.Add(PowerPlantType.Turbojet.ToString(), new PowerplantCalculatorTurbojet());
            calculators.Add(PowerPlantType.WindTurbine.ToString(), new PowerplantCalculatorWindTurbine());
        }

        /// <summary>
        /// Get the powerplant type related calcultor
        /// </summary>
        /// <param name="powerplantType">The type of the powerplant</param>
        /// <returns>A object implementing the <see cref="IPowerplantTypeCalculator"/> interface.</returns>
        public IPowerplantTypeCalculator GetCalculator(PowerPlantType powerplantType)
        {
            return calculators[powerplantType.ToString()];
        }
    }
}
