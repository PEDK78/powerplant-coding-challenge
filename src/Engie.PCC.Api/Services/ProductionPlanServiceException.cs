using System;
using System.Runtime.Serialization;

namespace Engie.PCC.Api.Services
{
    /// <summary>
    /// Exception thrown by the <see cref="ProductionPlanService"/> when something goes wrong
    /// </summary>
    [Serializable]
    public class ProductionPlanServiceException : Exception
    {
        public ProductionPlanServiceException()
        {
        }

        public ProductionPlanServiceException(string message) : base(message)
        {
        }

        public ProductionPlanServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ProductionPlanServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}