using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engie.PCC.Api.Models
{
    public class ApiError
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Optional, list of related errors
        /// </summary>
        public IEnumerable<string> Errors { get; set; }

        /// <summary>
        /// The full exception trace
        /// </summary>
        public string FullException { get; set; }
    }
}
