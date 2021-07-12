using Newtonsoft.Json;
using System.Collections.Generic;

namespace Engie.PCC.Api.Models
{
    public class WebSocketMessage
    {
        [JsonProperty("input")]
        public PayLoad Input { get; set; }

        [JsonProperty("output")]
        public List<PowerplantResult> Output { get; set; }

        public static WebSocketMessage Create(PayLoad payload, List<PowerplantResult> result)
        {
            return new WebSocketMessage
            {
                Input = payload,
                Output = result
            };
        }
    }
}
