using System;
using System.Runtime.Serialization;

namespace Engie.PCC.Api.Services.Calculators
{

    /// <summary>
    /// Exception thrown by object implementing <see cref="IPowerplantTypeCalculator"/> when powerplant type is invalid.
    /// </summary>
    [Serializable]
    public class PowerplantCalculatorException : Exception
    {
        public PowerplantCalculatorException()
        {
        }

        public PowerplantCalculatorException(string message) : base(message)
        {
        }

        public PowerplantCalculatorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PowerplantCalculatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}