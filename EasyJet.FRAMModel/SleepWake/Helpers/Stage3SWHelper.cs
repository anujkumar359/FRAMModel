using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Helpers
{
    /// <summary>
    /// Provides helper methods for Stage 3 software processes.
    /// This class contains methods for processing sleep-related data and calculations specific to Stage 3.
    /// </summary>
    internal class Stage3SWHelper
    {
        /// <summary>
        /// Utility functions instance providing various helper methods for calculations and operations.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// Processes fixed sleep data and returns a list of tuples containing calculated values related to sleep.
        /// </summary>
        /// <param name="data">A list of double arrays, where each array contains sleep-related data to be processed.</param>
        /// <param name="datetimeRange">A list of DateTime objects representing the range of interest for the processing.</param>
        /// <param name="offsetIdx">An integer representing the index offset used in the calculations.</param>
        /// <param name="tDelta">A double representing the time delta used in the calculations.</param>
        /// <returns>A list of tuples, where each tuple contains various calculated metrics related to fixed sleep periods.</returns>        
        public double[][] ProcessFixedSleeps(List<double[]> data, List<DateTime> datetimeRange, int offsetIdx, double tDelta)
        {
            List<double[]> results = new List<double[]>();

            foreach (var x in data)
            {
                double swVal = x[x.Length - 3];
                int startIdx = (int)x[x.Length - 1];
                DateTime startDt = datetimeRange[offsetIdx + startIdx];

                // Create datetime array for 1 day, starting from the first finding
                var timeDt = new List<DateTime>();
                DateTime endDt = startDt.AddDays(1).AddMinutes(UtilityFunctions.N_DELTA * UtilityFunctions.INTERVAL_FREQUENCY_MINUTE);

                for (DateTime dt = startDt; dt < endDt; dt = dt.AddMinutes(UtilityFunctions.N_DELTA * UtilityFunctions.INTERVAL_FREQUENCY_MINUTE))
                {
                    timeDt.Add(dt);
                }

                // Create fixed sleep/wake times
                DateTime sbDt = new DateTime(timeDt[0].Date.Year, timeDt[0].Date.Month, timeDt[0].Date.Day, 23, 0, 0);
                DateTime seDt = new DateTime(timeDt[0].AddDays(1).Year, timeDt[0].AddDays(1).Month, timeDt[0].AddDays(1).Day, 7, 0, 0);

                // Find awake indexes and calculate sleep begin index
                var awakeIndexes = timeDt.Select((dt, i) => new { dt, i }).
                    Where(z => z.dt <= sbDt).Select(z => z.i).ToArray();
                
                int sbIdx = startIdx + awakeIndexes.LastOrDefault();

                // Find last homeostatic awake component value
                double timeDec = awakeIndexes.Length * tDelta - tDelta;
                double h = util.HomeostaticAwakeComponent(timeDec, swVal);

                // Find sleep indexes and calculate sleep end index
                var sleepIndexes = timeDt.Select((dt, i) => new { dt, i })
                    .Where(y => y.dt > sbDt && y.dt <= seDt).Select(y => y.i).ToArray();

                int seIdx = startIdx + sleepIndexes.LastOrDefault();

                // Find last homeostatic sleep component value
                timeDec = sleepIndexes.Length * tDelta - tDelta;
                h = util.HomeostaticSleepComponent(timeDec, h);

                // Store the indexes found with previous findings
                var result = x.Concat(new double[] { 7, 13, h, sbIdx, seIdx }).ToArray();
                results.Add(result);
            }

            return results.ToArray();
        }

        /// <summary>
        /// Calculates the number of nights between two specified DateTime values.
        /// </summary>
        /// <param name="datetime1">The first DateTime value representing the start date.</param>
        /// <param name="datetime2">The second DateTime value representing the end date.</param>
        /// <returns>The number of full nights between the two DateTime values. 
        /// If the second date is earlier than the first, returns zero.</returns>
        public int NumberOfNights(DateTime datetime1, DateTime datetime2)
        {
            // Ensure datetime2 is after datetime1
            if (datetime2 <= datetime1)
            {
                throw new ArgumentException($"Second datetime {datetime2} cannot be earlier than first datetime {datetime1}");
            }

            // Calculate the number of days between two dates
            int nOfNights = (datetime2 - datetime1).Days;

            // If the time part of datetime1 is greater than the time part of datetime2, increment the night count
            if (datetime1.TimeOfDay > datetime2.TimeOfDay)
            {
                nOfNights++;
            }

            // If the time part of datetime1 is less than the AWAKE_TIME, increment the night count
            if (datetime1.TimeOfDay < util.AWAKE_TIME)
            {
                nOfNights++;
            }

            return nOfNights;
        }

        /// <summary>
        /// Processes sleep and wake data for non-duty periods and returns a list of double arrays representing the results.
        /// </summary>
        /// <param name="data">A list of double arrays containing sleep and wake-related data to be processed.</param>
        /// <param name="endIdx">An integer representing the index at which to end processing the data.</param>
        /// <param name="tDelta">A double representing the time delta used in the calculations.</param>
        /// <param name="c">An array of doubles representing constants or coefficients relevant to the processing.</param>
        /// <param name="paramsList">A list of tuples, each containing two double values that represent parameters for the calculations.</param>
        /// <returns>A list of double arrays, where each array contains processed data related to sleep and wake patterns during non-duty periods.</returns>
        public List<double[]> TwoSleepWakeOnNonDuty(List<double[]> data, int endIdx, double tDelta, double[] c, List<Tuple<double, double>> paramsList)
        {
            while (true)
            {
                var sleepEpisodes = util.FindNextSleepEpisodeNumba(data, endIdx, tDelta, c, paramsList).ToList();

                if (sleepEpisodes.Count > 0)
                {
                    // Convert sleepEpisodes to a 2D array
                    var dataArray = sleepEpisodes.Select(row => row.ToArray()).ToArray();
                    data = dataArray.ToList();

                    // Find indexes where the last column equals (endIdx - 1)
                    var indexes = dataArray.Select((value, index) => value.Last() == (endIdx - 1) ? index : -1)
                                           .Where(index => index != -1)
                                           .ToArray();

                    if (indexes.Length > 0)
                    {
                        // Filter data based on found indexes
                        data = indexes.Select(i => dataArray[i]).ToList();
                        break;
                    }
                }
                else
                {
                    var sleepWindow = FindSleepWithinWindow(data, c, endIdx, tDelta);
                    if (sleepWindow.Count > 0)
                    {
                        data = sleepWindow;
                        break;
                    }
                    else
                    {
                        Environment.Exit(1);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Finds and extracts sleep data within a specified window from the provided data.
        /// </summary>
        /// <param name="data">A list of double arrays containing sleep-related data to be processed.</param>
        /// <param name="c">An array of doubles representing constants or coefficients used in the calculations.</param>
        /// <param name="endIdx">An integer representing the index at which to stop processing the data.</param>
        /// <param name="tDelta">A double representing the time delta to be used in the calculations.</param>
        /// <returns>A list of double arrays, each containing sleep data that falls within the specified window.</returns>
        public List<double[]> FindSleepWithinWindow(List<double[]> data, double[] c, int endIdx, double tDelta)
        {
            double endSw = 13.9; // For the next sw, which is assumed to be 14, hence lower than this
            var results = new List<double[]>();

            foreach (var x in data)
            {
                double sw = x[x.Length - 3]; // Third-to-last element
                int se = (int)x[x.Length - 1]; // Last element
                double tGap = (endIdx - se) * tDelta;
                double[] timeIntervalGap = util.Linspace(tDelta, tGap, (int)(tGap / tDelta)).ToArray();
                int nGap = timeIntervalGap.Length;

                // Calculate homeostatic awake component
                double[] hw = util.HomeostaticAwakeComponent(timeIntervalGap, sw);
                int nSlice = (int)(8 / tDelta); // At least 8h awake
                int nSize = nGap - nSlice;
                double tMax = nSize * tDelta;

                for (int idx = 0; idx < hw.Length - nSlice; idx++)
                {
                    tMax -= tDelta;
                    double[] timeInterval = util.Linspace(0, tMax, nSize).ToArray();
                    nSize--;

                    // Calculate homeostatic sleep component
                    double[] hs = timeInterval.Select(t => util.HomeostaticSleepComponent(t, hw[nSlice + idx])).ToArray();
                    int beginIdx = Array.IndexOf(hs, hs.FirstOrDefault(h => h >= endSw));

                    if ((nSlice + idx + 1 + beginIdx) == nGap)
                    {
                        double[] h = hw.Take(nSlice + idx + 1).Concat(hs.Skip(1).Take(beginIdx)).ToArray();
                        int isb = se + nSlice + idx;
                        int ise = se + nGap - 1;

                        if (isb == ise) continue;

                        double low = Math.Round(hs[0] + c[isb], 1);
                        double hig = Math.Round(h.Last() + c[ise], 1);

                        if (low == hig) continue;

                        var appendedResult = x.Concat(new double[] { low, hig, h.Last(), isb + 1, ise }).ToArray();
                        results.Add(appendedResult);
                        break;
                    }
                }
            }
            return results;
        }
    }
}
