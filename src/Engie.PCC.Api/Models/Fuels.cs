using Newtonsoft.Json;

namespace Engie.PCC.Api.Models
{
    public class Fuels
    {
        /// <summary>
        /// The price of gas per MWh
        /// </summary>
        [JsonProperty("gas(euro/MWh)")]
        public decimal GasEuroMWh { get; set; }

        /// <summary>
        /// The price of kerosine per MWh
        /// </summary>
        [JsonProperty("kerosine(euro/MWh)")]
        public decimal KerosineEuroMWh { get; set; }

        /// <summary>
        /// The price of emission allowances
        /// </summary>
        [JsonProperty("co2(euro/ton)")]
        public decimal Co2EuroTon { get; set; }

        /// <summary>
        /// Percentage of wind
        /// </summary>
        [JsonProperty("wind(%)")]
        public decimal Wind { get; set; }

        public static Fuels Create(decimal gasPricePerMWh, decimal kerosenePricePerMWh, decimal windPercentage, decimal co2PricePerTon = 0)
        {
            return new Fuels
            {
                GasEuroMWh = gasPricePerMWh,
                KerosineEuroMWh = kerosenePricePerMWh,
                Co2EuroTon = co2PricePerTon,
                Wind = windPercentage
            };
        }
    }
}
