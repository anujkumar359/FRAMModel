using System;
using System.Collections.Generic;

namespace EasyJet.FRAMModel.SleepWake.Entities
{
    /// <summary>
    /// Represents the response for Stage 1 Sleep-Wake calculation.
    /// </summary>
    internal class Stage1SWResponse
    {
        /// <summary>
        /// Gets or sets the list of circadian values.
        /// </summary>
        public List<double> Circadians { get; set; }

        /// <summary>
        /// Gets or sets the list of homeostatic values.
        /// </summary>
        public List<double> Homeostatics { get; set; }

        /// <summary>
        /// Gets or sets the list of alertness values.
        /// </summary>
        public List<double> Alertnesses { get; set; }

        /// <summary>
        /// Gets or sets the list of total datetime range values.
        /// </summary>
        public List<DateTime> TotalDatetimeRange { get; set; }

        /// <summary>
        /// Gets or sets the list of duty ranges.
        /// </summary>
        public List<(double StartPhase, double EndPhase, double StartCircadian, double StartHomeostatic, double StartAlertness, double EndCircadian, double EndHomeostatic, double EndAlertness)> RsDuties { get; set; }
    }
}
