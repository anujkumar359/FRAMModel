using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyJet.FRAMModel.SleepWake.Entities
{
    /// <summary>
    /// Represents the response for Stage 5 Sleep-Wake calculation.
    /// </summary>
    internal class Stage5SWResponse
    {
        /// <summary>
        /// Gets or sets the time range for the response.
        /// </summary>
        public List<DateTime> TimeRange { get; set; }

        /// <summary>
        /// Gets or sets the interpolated H values for the response.
        /// </summary>
        public double[] InterpolatedH { get; set; }

        /// <summary>
        /// Gets or sets the interpolated C values for the response.
        /// </summary>
        public double[] InterpolatedC { get; set; }
    }
}
