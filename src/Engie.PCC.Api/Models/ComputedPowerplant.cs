using Engie.PCC.Api.Services.Calculators;

namespace Engie.PCC.Api.Models
{
    /// <summary>
    /// Internal object to store result of computation.
    /// </summary>
    internal class ComputedPowerplant
    {
        private ComputedPowerplant(Fuels fuels, Powerplant powerplant, PowerplantCalculatorFactory calculatorFactory)
        {
            Powerplant = powerplant;

            var calculator = calculatorFactory.GetCalculator(powerplant.Type);
            CostPerMWH = calculator.ComputeCostPerMWH(fuels, powerplant);
            MinAvailablePower = calculator.ComputeMinimumDeliverablePower(fuels, powerplant);
            MaxAvailablePower = calculator.ComputeMaximumDeliverablePower(fuels, powerplant);
        }

        /// <summary>
        /// The <see cref="Powerplant"/>.
        /// </summary>
        public Powerplant Powerplant { get; private set; }


        /// <summary>
        /// The computed cost per MWH
        /// </summary>
        public decimal CostPerMWH { get; private set; }

        /// <summary>
        /// Gets the minimum power in MW the plant can generate.
        /// </summary>
        public decimal MinAvailablePower { get; private set; }

        /// <summary>
        /// Gets the maximum power in MW the plant can generate.
        /// </summary>
        public decimal MaxAvailablePower { get; private set; }


        /// <summary>
        /// The power in MW the plant should generate.
        /// </summary>
        public decimal ToProduce { get; set; }


        public static ComputedPowerplant Create(Fuels fuels, Powerplant powerplant, PowerplantCalculatorFactory calculatorFactory)
        {
            return new ComputedPowerplant(fuels, powerplant, calculatorFactory);
        }
    }
}
