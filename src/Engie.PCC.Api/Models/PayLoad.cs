using Newtonsoft.Json;
using System.Collections.Generic;

namespace Engie.PCC.Api.Models
{
    public class PayLoad
    {
        /// <summary>
        /// The load is the amount of energy (MWh) that need to be generated during one hour
        /// </summary>
        [JsonProperty("load")]
        public decimal Load { get; set; }

        /// <summary>
        /// The cost of the fuels of each powerplant
        /// </summary>
        [JsonProperty("fuels")]
        public Fuels Fuels { get; set; }

        /// <summary>
        /// The powerplants at disposal to generate the demanded load
        /// </summary>
        [JsonProperty("powerplants")]
        public IEnumerable<Powerplant> Powerplants { get; set; }

        public static PayLoad Create (decimal load, Fuels fuels, IEnumerable<Powerplant> powerPlants)
        {
            return new PayLoad
            {
                Load = load,
                Fuels = fuels,
                Powerplants = powerPlants
            };
        }
    }
}
