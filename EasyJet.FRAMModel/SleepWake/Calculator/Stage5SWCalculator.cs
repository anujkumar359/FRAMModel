using System;
using System.Collections.Generic;
using System.Linq;
using EasyJet.FRAMModel.SleepWake.Helpers;
using EasyJet.FRAMModel.SleepWake.Entities;

namespace EasyJet.FRAMModel.SleepWake.Calculator
{
    /// <summary>
    /// Represents a calculator for Stage 5 software processes.
    /// This class contains methods to process the last window of data, integrating various biological and circadian metrics.
    /// </summary>
    internal class Stage5SWCalculator
    {
        /// <summary>
        /// Utility functions instance providing various helper methods for calculations and operations.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// Processes the last window of data based on provided datetime range and biological metrics.
        /// </summary>
        /// <param name="datetimeRange">A list of DateTime objects representing the range of interest.</param>
        /// <param name="homeostaticsWoNan">An array of homeostatic values excluding NaN entries.</param>
        /// <param name="circadiansWoNan">An array of circadian values excluding NaN entries.</param>
        /// <param name="rsDuties">A list of tuples representing the duties, each containing phase and alertness metrics.</param>
        /// <param name="isNextDay">A boolean indicating whether the processing is for the next day.</param>
        /// <returns>A <see cref="Stage5SWResponse"/> object containing the results of the processing.</returns>
        public Stage5SWResponse ProcessLastWindow(
        List<DateTime> datetimeRange,
        double[] homeostaticsWoNan,
        double[] circadiansWoNan,
        List<(double StartPhase, double EndPhase, double StartCircadian, double StartHomeostatic, double StartAlertness, double EndCircadian, double EndHomeostatic, double EndAlertness)> rsDuties,
        bool isNextDay)
        {
            // 1. Convert datetime_range to array of double (OADate for interpolation)          
            double[] timeRange1 = datetimeRange
            .Select(dt => (dt - new DateTime(1970, 1, 1)).TotalSeconds)
            .ToArray();

            // 2. Create interpolation functions (linear interpolation)
            

            // 3. Create datetime array with INTERVAL_FREQUENCY_MINUTE intervals
            DateTime start = datetimeRange.First();
            DateTime end = datetimeRange.Last().AddMinutes(UtilityFunctions.INTERVAL_FREQUENCY_MINUTE);
            List<DateTime> timeRange2 = new List<DateTime>();
            for (DateTime dt = start; dt < end; dt = dt.AddMinutes(UtilityFunctions.INTERVAL_FREQUENCY_MINUTE))
            {
                timeRange2.Add(dt);
            }
            double[] timeRange2Double = timeRange2.Select(dt => (dt - new DateTime(1970, 1, 1)).TotalSeconds).ToArray();

            // 4. Apply interpolation functions           
            double[] interpolatedH = util.LinearInterpolate(timeRange1, homeostaticsWoNan, timeRange2Double);
            double[] interpolatedC = util.LinearInterpolate(timeRange1, circadiansWoNan, timeRange2Double);

            // 5. Store parameters
            double initialTime = util.DateTimeToDecimal(timeRange2.Last());
            double initialSw = interpolatedH.Last();
            double initialPhase = rsDuties.Last().Item2;

            // 6. Calculate the last part from the last commute end till the end of data
            DateTime beginDate = datetimeRange.Last().AddMinutes(UtilityFunctions.INTERVAL_FREQUENCY_MINUTE);
            DateTime endDate = beginDate.Date.AddDays(1); // Equivalent to adding one day

            // Calculate number of bins after CommuteEnd + AwakeOffset (30m)
            int nSize = (int)((endDate - beginDate).TotalMinutes / UtilityFunctions.INTERVAL_FREQUENCY_MINUTE);

            // 7. Generate time intervals (start from TIME_DELTA)
            List<double> timeIntervals = util.Linspace(util.TIME_DELTA, nSize * util.TIME_DELTA, nSize); 

            // 8. Calculate last circadian component
            double[] c = timeIntervals.Select(t => util.CircadianComponent(t % util.CIRCADIAN_LENGTH, initialPhase - initialTime)).ToArray();

            // 9. Append the last circadian values
            interpolatedC = interpolatedC.Concat(c).ToArray();

            // 10. Calculate last awake component
            List<double> lowers = util.Linspace(6, 9, 4); // 6,7,8,9
            List<double> uppers = util.Linspace(11, 14, 4); // 11,12,13,14
            var paramsList = lowers
                                .SelectMany(x => uppers.Select(y => Tuple.Create(Math.Round(x, 2), Math.Round(y, 2))))
                                .ToList();

            // 11. Find sleep episodes
            var sleeps = paramsList.Select(p => (p.Item1, p.Item2, util.FindASleepEpisodeNumba(0, nSize, p.Item1, p.Item2, initialSw, util.TIME_DELTA, c))).ToList();

            // 12. Convert tuples to an array
            // Assuming FindASleepEpisodeNumba returns a tuple (start, end)
            List<double[]> arr = sleeps.Select(s => new double[] { s.Item1, s.Item2, s.Item3.Item1 ? 1 : 0, s.Item3.Item2, s.Item3.Item3, s.Item3.Item4 - 1 }).ToList();

            // 13. Handle is_next_day condition
            if (isNextDay)
            {
                while (true)
                {
                    // Find indices where the third column is 1
                    var indexes = Enumerable.Range(0, arr.Count)
                                            .Where(i => arr[i][2] == 1)
                                            .ToList();

                    if (indexes.Count == 0)
                    {
                        // Choose the ones with the smallest second last column value
                        double minSecondLast = arr.Min(row => row[row.Length - 2]);
                        var filtered = arr.Where(row => row[row.Length - 2] == minSecondLast).ToArray();

                        // Choose the one with the highest 'sw' (assuming last column is sw)
                        var selected = filtered.OrderByDescending(row => row[3]).First();

                        int n = (int)selected[2];
                        double[] newTimeIntervals = util.Linspace(0, (n - 1) * util.TIME_DELTA, n).ToArray();
                        
                        double[] newH = util.HomeostaticAwakeComponent(newTimeIntervals, initialSw);
                        interpolatedH = interpolatedH.Concat(newH).ToArray();

                        n = (int)selected[3] - n;
                        newTimeIntervals = util.Linspace(0, (n - 1) * util.TIME_DELTA, n).ToArray();
                        var newSleepComponents = newTimeIntervals.Select(t => util.HomeostaticSleepComponent(t, interpolatedH.Last())).ToArray();
                        interpolatedH = interpolatedH.Concat(newSleepComponents).ToArray();

                        n = nSize - (int)selected[3];
                        newTimeIntervals = util.Linspace(0, (n - 1) * util.TIME_DELTA, n).ToArray();
                        newH = util.HomeostaticAwakeComponent(newTimeIntervals, interpolatedH.Last());
                        interpolatedH = interpolatedH.Concat(newH).ToArray();

                        break;
                    }
                    else
                    {
                        // Filter arr where third column is 1
                        var filteredArr = arr.Where(row => row[2] == 1).ToArray();

                        // Choose rows with the maximum threshold (second column)
                        double maxThreshold = filteredArr.Max(row => row[1]);
                        filteredArr = filteredArr.Where(row => row[1] == maxThreshold).ToArray();

                        // Choose the row with the highest sw (fourth column)
                        var selected = filteredArr.OrderByDescending(row => row[3]).First();

                        int n = (int)selected[2];
                        double[] newTimeIntervals = util.Linspace(0, (n - 1) * util.TIME_DELTA, n).ToArray();
                        double[] newH = util.HomeostaticAwakeComponent(newTimeIntervals, initialSw);
                        interpolatedH = interpolatedH.Concat(newH).ToArray();

                        n = (int)selected[3] - n;
                        newTimeIntervals = util.Linspace(0, (n - 1) * util.TIME_DELTA, n).ToArray();
                        var newSleepComponents = newTimeIntervals.Select(t => util.HomeostaticSleepComponent(t, interpolatedH.Last())).ToArray();
                        interpolatedH = interpolatedH.Concat(newSleepComponents).ToArray();

                        n = nSize - (int)selected[3];
                        newTimeIntervals = util.Linspace(0, (n - 1) * util.TIME_DELTA, n).ToArray();
                        newH = util.HomeostaticAwakeComponent(newTimeIntervals, interpolatedH.Last());
                        interpolatedH = interpolatedH.Concat(newH).ToArray();

                        break;
                    }
                }
            }
            else
            {
                // If the commute end is the same as commute begin, no sleep/wake opportunity
                if (arr.Any(row => row[2] == 0))
                {
                    // Filter the rows where the third column (index 2) equals 0
                    arr = arr.Where(row => row[2] == 0).ToList();
                }

                // Choose the highest threshold when waking occurs
                double maxThresholdWake = arr.Max(row => row[1]);
                var filteredWake = arr.Where(row => row[1] == maxThresholdWake).ToArray();

                // Choose the lowest threshold when sleeping occurs
                double minThresholdSleep = filteredWake.Min(row => row[0]);
                var filteredSleep = filteredWake.Where(row => row[0] == minThresholdSleep).ToArray();

                // Choose the row with the highest sw
                var selected = filteredSleep.OrderByDescending(row => row[3]).First();

                int n = (int)selected[4];
                double[] timeIntervalsAwake = util.Linspace(0, (n - 1) * util.TIME_DELTA, n).ToArray();
                double[] h = util.HomeostaticAwakeComponent(timeIntervalsAwake, initialSw);
                interpolatedH = interpolatedH.Concat(h).ToArray();

                n = nSize - (int)selected[4];
                double[] timeIntervalsSleep = util.Linspace(0, (n - 1) * util.TIME_DELTA, n).ToArray();
                var sleepComponents = timeIntervalsSleep.Select(t => util.HomeostaticSleepComponent(t, interpolatedH.Last())).ToArray();
                interpolatedH = interpolatedH.Concat(sleepComponents).ToArray();
            }

            // 14. Generate final time_range with INTERVAL_FREQUENCY_MINUTE intervals up to end date
            List<DateTime> finalTimeRange = new List<DateTime>();
            DateTime finalStart = datetimeRange.First();
            DateTime finalEnd = datetimeRange.Last().AddMinutes(UtilityFunctions.INTERVAL_FREQUENCY_MINUTE).Date.AddDays(1);
            for (DateTime dt = finalStart; dt < finalEnd; dt = dt.AddMinutes(UtilityFunctions.INTERVAL_FREQUENCY_MINUTE))
            {
                finalTimeRange.Add(dt);
            }

            // 15. Return the results
            return new Stage5SWResponse() { TimeRange = finalTimeRange, InterpolatedH = interpolatedH, InterpolatedC = interpolatedC };
        }
    }
}
