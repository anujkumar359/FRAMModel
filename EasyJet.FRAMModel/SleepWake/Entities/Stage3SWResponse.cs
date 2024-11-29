using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Entities
{
    /// <summary>
    /// Represents the response for Stage 3 Sleep-Wake.
    /// </summary>
    internal class Stage3SWResponse
    {
        /// <summary>
        /// Gets or sets the list of non-duty sleep values.
        /// </summary>
        public List<double[][]> NonDutySleeps { get; set; }
    }
}
