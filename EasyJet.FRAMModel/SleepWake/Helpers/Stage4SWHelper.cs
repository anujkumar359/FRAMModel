using System.Collections.Generic;
using System;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Helpers
{

    /// <summary>
    /// Provides helper methods for Stage 4 software processes.
    /// This class contains utilities for handling and processing data related to sleep-wake cycles 
    /// and ProcessNonDutyHomeostatics as part of the overall analysis framework.
    /// </summary>
    internal class Stage4SWHelper
    {
        /// <summary>
        /// Utility functions instance providing various helper methods for calculations and operations.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// Calculates the homeostatic values during non-duty periods based on the provided data.
        /// </summary>
        /// <param name="data">A jagged array of doubles containing the data from which to calculate homeostatics.</param>
        /// <param name="n_sleeps">An integer representing the number of sleep periods to consider in the calculations.</param>
        /// <param name="datetime_range">A list of DateTime objects representing the range of interest for the calculations.</param>
        /// <param name="offset_idx">An integer index used to offset the calculations in the data array.</param>
        /// <param name="t_delta">A double representing the time delta used in the calculations.</param>
        /// <returns>A list of doubles containing the calculated homeostatic values for non-duty periods.</returns>
        public List<double> CalculateNonDutyHomeostatics(double[][] data, int n_sleeps, List<DateTime> datetime_range, int offset_idx, double t_delta)
        {
            List<double> h = new List<double>();
            int last_end = 0;

            for (int n_sleep = 0; n_sleep < data.Length; n_sleep++)
            {
                var x = data[n_sleep];
                int begin_idx = (int)x[x.Length - 2];
                int end_idx = (int)x[x.Length - 1];

                // Calculate homeostatic awake values
                List<double> awake_intervals = util.Linspace(0, (begin_idx - last_end - 1) * t_delta, begin_idx - last_end);
                h.AddRange(util.HomeostaticAwakeComponent(awake_intervals.ToArray(), x[2]));

                // Calculate homeostatic sleep values
                List<double> sleep_intervals = util.Linspace(0, (end_idx - begin_idx - 1) * t_delta, end_idx - begin_idx);
                if (n_sleep == (n_sleeps - 1))
                {
                    sleep_intervals = util.Linspace(0, (end_idx - begin_idx) * t_delta, end_idx - begin_idx + 1);
                }

                if (h.Count == 0)  // If directly sleeps, begin_idx==0
                {
                    h.AddRange(sleep_intervals.Select(t => util.HomeostaticSleepComponent(t, x[2])));
                }
                else
                {
                    double lastVal = h.Last();
                    h.AddRange(sleep_intervals.Select(t => util.HomeostaticSleepComponent(t, lastVal)));
                }

                last_end = end_idx;

                DateTime sb = datetime_range[begin_idx + offset_idx];
                DateTime se = datetime_range[end_idx + offset_idx];
                TimeSpan sl = se - sb;
            }

            return h;
        }
    }
}
