using Newtonsoft.Json;

namespace Engie.PCC.Api.Models
{
    public class PowerplantResult
    {
        /// <summary>
        /// The name of the powerplant
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// The load in MWH that the powerplant shoud supply
        /// </summary>
        [JsonProperty("p")]
        public decimal Power { get; set; }

        public static PowerplantResult Create(string name, decimal power)
        {
            return new PowerplantResult
            {
                Name = name,
                Power = power
            };            
        }
    }
}
