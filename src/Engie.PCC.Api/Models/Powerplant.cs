using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engie.PCC.Api.Models
{
    public class Powerplant
    {
        /// <summary>
        /// The name of the powerplant
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of the powerplant
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PowerPlantType Type { get; set; }

        /// <summary>
        /// The efficiency at which they convert a MWh of fuel into a MWh of electrical energy.
        /// </summary>
        [JsonProperty("efficiency")]
        public decimal Efficiency { get; set; }

        /// <summary>
        /// The maximum amount of power the powerplant can generate.
        /// </summary>
        [JsonProperty("pmin")]
        public decimal MinPower { get; set; }

        /// <summary>
        /// The minimum amount of power the powerplant generates when switched on.
        /// </summary>
        [JsonProperty("pmax")]
        public decimal MaxPower { get; set; }


        public static Powerplant Create(string name, PowerPlantType type, decimal efficiency, decimal pMin, decimal pMax)
        {
            return new Powerplant
            {
                Name = name,
                Type = type,
                Efficiency = efficiency,
                MinPower = pMin,
                MaxPower = pMax
            };
        }
    }
}
