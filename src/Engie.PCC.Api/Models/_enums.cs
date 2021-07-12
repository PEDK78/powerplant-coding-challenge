using System.Runtime.Serialization;

namespace Engie.PCC.Api.Models
{
    public enum PowerPlantType
    {
        [EnumMember(Value = "windturbine")]
        WindTurbine,

        [EnumMember(Value = "gasfired")]
        GasFired,

        [EnumMember(Value = "turbojet")]
        Turbojet
    }
}
