using System.Collections.Generic;

namespace EasyJet.FRAMModel.SleepWake.Entities
{
    /// <summary>
    /// Represents the response for Stage 2 Sleep-Wake calculation.
    /// </summary>
    internal class Stage2SWResponse
    {
        /// <summary>
        /// Gets or sets the Circadians without NaN values.
        /// </summary>
        public double[] CircadiansWoNan { get; set; }

        /// <summary>
        /// Gets or sets the list of CVals.
        /// </summary>
        public List<double[]> CVals { get; set; }

        /// <summary>
        /// Gets or sets the list of LastSwVals.
        /// </summary>
        public List<double> LastSwVals { get; set; }
    }
}
