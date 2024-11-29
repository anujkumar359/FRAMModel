using EasyJet.FRAMModel.SleepWake.Entities;
using EasyJet.FRAMModel.SleepWake.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Calculator
{
    /// <summary>
    /// Represents a calculator for processing non-duty data to calculate sleep episodes.
    /// </summary>
    internal class Stage3SWCalculator
    {
        /// <summary>
        /// Represents the utility functions for sleep-wake related operations.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// Represents the helper class for stage 3 sleep-wake operations.
        /// </summary>
        private Stage3SWHelper stage3Helper = new Stage3SWHelper();

        /// <summary>
        /// Processes non-duty data to calculate sleep episodes.
        /// </summary>
        /// <param name="datetimeRange">The list of DateTime values representing the range of dates.</param>
        /// <param name="beginEndDates">The list of tuples containing the start and end dates for each non-duty period.</param>
        /// <param name="nonDutyIndexes">The list of tuples containing the indexes for each non-duty period.</param>
        /// <param name="lastSwVals">The list of last sleep-wake values for each non-duty period.</param>
        /// <param name="cVals">The list of C values for each non-duty period.</param>
        /// <returns>The list of sleep episodes for each non-duty period.</returns>
        public Stage3SWResponse ProcessNonDuties(List<DateTime> datetimeRange,
            List<(int, DateTime, DateTime, TimeSpan, TimeSpan)> beginEndDates,
            List<Tuple<int, int>> nonDutyIndexes, List<double> lastSwVals, List<double[]> cVals)
        {
            List<double[][]> nonDutySleeps = new List<double[][]>();

            // Calculate the time delta
            double tDelta = UtilityFunctions.N_DELTA * util.TIME_DELTA;

            // If datetimeRange is a List, convert it to an array (C# arrays are a bit different than NumPy arrays, 
            // but this ensures it behaves similarly for performance optimization).
            DateTime[] datetimeRangeArray = datetimeRange.ToArray();

            for (int idx = 0; idx < nonDutyIndexes.Count; idx++)
            {
                // Calculate start_idx and end_idx
                int startIdx = 0;
                int endIdx = (nonDutyIndexes[idx].Item2 - nonDutyIndexes[idx].Item1) + 1;

                // Get offset_idx
                int offsetIdx = nonDutyIndexes[idx].Item1;

                // Calculate last_min_awake
                TimeSpan lastMinAwake = beginEndDates[idx]
                    .Item5.Add(new TimeSpan(0, 2 * util.AWAKE_OFFSET.Minutes, 0)); // Assuming AWAKE_OFFSET is in minutes

                // Retrieve commute_end and commute_begin from the respective indexes
                DateTime commuteEnd = beginEndDates[idx].Item3;
                DateTime commuteBegin = beginEndDates[idx + 1].Item2;

                // Set awake_time using a custom method (C# equivalent of set_initial_datetime)
                DateTime awakeTime = util.SetInitialDateTime(commuteBegin);

                // Calculate the number of nights (Assuming number_of_nights is a method that takes two DateTime arguments)
                int numberOfNights = stage3Helper.NumberOfNights(commuteEnd, commuteBegin);
                List<double> lowers = util.Linspace(6, 9, 7);
                List<double> uppers = util.Linspace(11, 14, 7);

                // Condition checking if last_min_awake is less than 6 hours and commute_end is before 8:00 AM
                if (lastMinAwake < TimeSpan.FromHours(6) && commuteEnd.TimeOfDay < new TimeSpan(8, 0, 0))
                {
                    lowers = util.Linspace(8, 11, 7);
                    uppers = util.Linspace(11, 14, 7);
                }

                var paramsList = lowers
                                .SelectMany(x => uppers.Select(y => Tuple.Create(Math.Round(x, 2), Math.Round(y, 2))))
                                .ToList();

                var lastSw = lastSwVals[idx];
                var cVal = cVals[idx];

                int nSleeps = 0;
                var sleeps = paramsList
                            .Select(param => (param.Item1, param.Item2, util.FindASleepEpisodeNumba(startIdx, endIdx, param.Item1, param.Item2, lastSw, tDelta, cVal)))
                            .Select(item => new double[] { item.Item1, item.Item2 }.Concat(new double[] { item.Item3.Item2, item.Item3.Item3, item.Item3.Item4 }).ToArray())
                            .ToArray();

                nSleeps += 1;
                int[] indexes;
                if (numberOfNights == 1)
                {
                    indexes = Enumerable.Range(0, sleeps.GetLength(0)) // Loop over rows
                        .Where(i => sleeps[i][sleeps[i].Length - 1] == (endIdx - 1)) // Check if last column equals endIdx - 1
                        .ToArray();
                    if (indexes.Length == 0)
                    {
                        lowers = util.Linspace(6, 10, 17);
                        uppers = util.Linspace(10, 14, 17);
                        paramsList = lowers
                                .SelectMany(x => uppers.Select(y => Tuple.Create(Math.Round(x, 2), Math.Round(y, 2))))
                                .ToList();
                        sleeps = paramsList
                                .Select(param => (param.Item1, param.Item2, util.FindASleepEpisodeNumba(startIdx, endIdx, param.Item1, param.Item2, lastSw, tDelta, cVal)))
                                .Select(item => new double[] { item.Item1, item.Item2 }.Concat(new double[] { item.Item3.Item2, item.Item3.Item3, item.Item3.Item4 }).ToArray())
                                .ToArray();
                        double remainingWindow = (endIdx - sleeps.Select(row => row[row.Length - 1])
                                                .Average()) * tDelta;

                        // Find indexes where the last column equals (endIdx - 1)
                        indexes = Enumerable.Range(0, sleeps.GetLength(0))
                           .Where(i => sleeps[i][sleeps[i].Length - 1] == (endIdx - 1))
                           .ToArray();
                        if (remainingWindow > 12)
                        {
                            sleeps = stage3Helper.TwoSleepWakeOnNonDuty(sleeps.ToList(), endIdx, tDelta, cVal, paramsList).Cast<double[]>().ToArray();
                            nonDutySleeps.Add(sleeps);
                        }
                        if (indexes.Length == 0)
                        {

                            // Find the maximum value in the last column of sleeps
                            double maxLastIndex = sleeps.Max(x => x[x.Length - 1]);

                            // Get the indexes where the last column matches the maximum value
                            indexes = sleeps
                                .Select((value, index) => new { Value = value, Index = index })
                                .Where(item => item.Value[item.Value.Length - 1] == maxLastIndex)
                                .Select(item => item.Index).ToArray();

                            // Append the corresponding sleeps to nonDutySleeps
                            nonDutySleeps.Add(indexes.Select(i => sleeps[i]).ToArray());
                        }
                    }
                    else
                    {
                        // Append the relevant sleeps to nonDutySleeps
                        nonDutySleeps.Add(indexes.Select(i => sleeps[i]).ToArray());
                    }
                    continue;
                }

                indexes = sleeps
                    .Select((value, index) => new { Value = value, Index = index })
                    .Where(item => (item.Value[item.Value.Length - 2] * tDelta) <= (util.CIRCADIAN_LENGTH - (lastMinAwake.TotalSeconds / 3600)))
                    .Select(item => item.Index)
                    .ToArray();

                // If there are any indexes found, filter the sleeps list
                if (indexes.Length > 0)
                {
                    sleeps = indexes.Select(i => sleeps[i]).ToArray();
                }
                if (numberOfNights == 2)
                {
                    sleeps = stage3Helper.TwoSleepWakeOnNonDuty(sleeps.ToList(), endIdx, tDelta, cVal, paramsList).Cast<double[]>().ToArray();
                    nonDutySleeps.Add(sleeps); // Assuming sleeps is a List<double[]>
                    continue; // Continue to the next iteration
                }

                lowers = util.Linspace(7, 9, 3);
                uppers = util.Linspace(12, 14, 3);
                paramsList = lowers
                        .SelectMany(x => uppers.Select(y => Tuple.Create(Math.Round(x, 2), Math.Round(y, 2))))
                        .ToList();

                TimeSpan sleepBeginMin = new TimeSpan(22, 0, 0);  // 10:00 PM
                TimeSpan sleepBeginMax = new TimeSpan(23, 59, 0); // 11:59 PM

                for (int numSleeps = 1; numSleeps <= numberOfNights; numSleeps++)
                {
                    if (numSleeps >= (numberOfNights - 1))
                    {
                        // More granularity, 0.5 step size for thresholds
                        lowers = util.Linspace(6, 9, 7); // Equivalent of np.linspace(6, 9, 7, dtype=float)
                        uppers = util.Linspace(10, 14, 9); // Equivalent of np.linspace(10, 14, 9, dtype=float)

                        // Assuming sleeps is a 2D array, calculate the mean
                        double meanDifference = sleeps.Select(row => endIdx - row[row.Length - 1])
                                                       .Average();

                        if (meanDifference <= 45)
                        {
                            lowers = util.Linspace(6, 10, 9); // Equivalent of np.linspace(6, 10, 9, dtype=float)
                            uppers = util.Linspace(10, 14, 9); // Equivalent of np.linspace(10, 14, 9, dtype=float)
                        }
                        if (commuteBegin.TimeOfDay <= new TimeSpan(4, 0, 0))
                        {
                            lowers = util.Linspace(7, 11, 9); // Equivalent of np.linspace(6, 10, 9, dtype=float)
                            uppers = util.Linspace(10, 14, 9); // Equivalent of np.linspace(10, 14, 9, dtype=float)
                        }
                        paramsList = lowers
                        .SelectMany(x => uppers.Select(y => Tuple.Create(Math.Round(x, 2), Math.Round(y, 2))))
                        .ToList();
                    }
                    else if (numSleeps >= numberOfNights - 2)
                    {
                        lowers = util.Linspace(6, 9, 7); // Equivalent of np.linspace(6, 10, 9, dtype=float)
                        uppers = util.Linspace(11, 14, 7); // Equivalent of np.linspace(10, 14, 9, dtype=float)

                        paramsList = lowers
                        .SelectMany(x => uppers.Select(y => Tuple.Create(Math.Round(x, 2), Math.Round(y, 2))))
                        .ToList();
                    }

                    if (numSleeps >= 2 && numSleeps <= (numberOfNights - 3))
                    {

                        // Assuming process_fixed_sleeps is a method that returns an array of sleep data
                        sleeps = stage3Helper.ProcessFixedSleeps(sleeps.ToList(), datetimeRange, offsetIdx, tDelta);

                        // Check for no results found
                        if (sleeps.Length == 0)
                        {
                            Environment.Exit(1); // Equivalent to quit() in Python
                        }

                        sleeps = sleeps.ToArray(); // Assuming sleepResults is already an array or convert it if necessary
                    }
                    else
                    {
                        bool isThresholdChanged = false;

                        while (true)
                        {
                            sleeps = util.FindNextSleepEpisodeNumba(sleeps.ToList(), endIdx, tDelta, cVal, paramsList);

                            if (sleeps.Length == 0 && !isThresholdChanged)
                            {
                                lowers = util.Linspace(7, 10.5, 15); // Equivalent of np.linspace(6, 10, 9, dtype=float)
                                uppers = util.Linspace(10.5, 14, 15); // Equivalent of np.linspace(10, 14, 9, dtype=float)

                                paramsList = lowers
                                .SelectMany(x => uppers.Select(y => Tuple.Create(Math.Round(x, 2), Math.Round(y, 2))))
                                .ToList();

                                isThresholdChanged = true;
                                continue;
                            }

                            var selectedColumns = sleeps.Select(row => new double[] { row[row.Length - 6], row[row.Length - 2] }).ToArray();

                            // Step 2: Compute the difference between consecutive elements in the selected columns
                            double[] diffs = selectedColumns.Select(cols => cols[1] - cols[0]).ToArray();

                            // Step 3: Multiply by t_delta and find where the result is greater than 5
                            indexes = diffs.Select((diff, id) => new { diff, id })
                                                 .Where(x => x.diff * tDelta > 5)
                                                 .Select(x => x.id)
                                                 .ToArray();

                            if (indexes.Length > 0)
                            {
                                sleeps = indexes.Select(i => sleeps[i]).ToArray(); // Assign the selected sleeps
                                break;
                            }
                            else
                            {
                                if ((numberOfNights - numSleeps) == 1)
                                {
                                    break;
                                }
                                else
                                {
                                    Environment.Exit(1); // Equivalent to quit() in Python
                                }
                            }
                        }
                    }

                    if (numSleeps == numberOfNights - 2)
                    {
                        var _sleeps = sleeps.ToArray(); // Assuming sleeps is a List or Array
                    }

                    if (numSleeps >= 2 && numSleeps <= (numberOfNights - 3))
                    {
                        var lastTwoColumns = sleeps.Select(row => new double[] { row[row.Length - 2], row[row.Length - 1] })
                                   .ToArray();

                        // Compute the differences between consecutive rows for both second-to-last and last columns
                        var diffs = Enumerable.Range(0, lastTwoColumns.Length)
                                              .Select(i => new double[] {
                                  (lastTwoColumns[i][1] - lastTwoColumns[i][0]) * tDelta
                                              })
                                              .ToArray();

                        // Find the indexes where the condition holds true (difference between 6 and 9)
                        indexes = diffs.Select((diff, index) => new { diff, index })
                                           .Where(x => (x.diff[0] >= 6 && x.diff[0] <= 9))
                                           .Select(x => x.index)  // +1 to shift the index since diffs are calculated from the second element
                                           .ToArray();

                        if (indexes.Length > 0)
                            sleeps = sleeps.Where((s, i) => indexes.Contains(i)).ToArray();
                        // Filter the last sleep begin times from previous sleep begin min/max
                        var sleepBegins = sleeps.Select(s => (int)s[s.Length - 2]).ToArray();

                        // Step 2: Calculate indexes
                        indexes = sleepBegins.Select(sb => offsetIdx + sb).ToArray();

                        Func<TimeSpan, TimeSpan, bool> operatorFunc;
                        // Step 3: Get corresponding DateTime values and convert them to time
                        var arr = indexes.Select(i => datetimeRange[i].TimeOfDay).ToArray();

                        // Step 4: Apply filter using conditional operator
                        if (sleepBeginMin <= sleepBeginMax)
                        {
                            operatorFunc = (x, y) => x >= sleepBeginMin && x <= sleepBeginMax;
                        }
                        else
                        {
                            operatorFunc = (x, y) => x >= sleepBeginMin || x <= sleepBeginMax;
                        }

                        // Step 5: Find the matching indexes based on the operator function
                        indexes = arr
                            .Select((time, index) => new { time, index })
                            .Where(x => operatorFunc(x.time, sleepBeginMax))
                            .Select(x => x.index)
                            .ToArray();

                        // Step 6: Filter sleeps array if matching indexes are found
                        if (indexes.Length > 0)
                        {
                            sleeps = sleeps.Where((s, i) => indexes.Contains(i)).ToArray();
                        }
                    }
                    // Applying the first filter if the conditions are met
                    else if (numSleeps >= 1 && numSleeps <= (numberOfNights - 2))
                    {
                        var lastTwoColumns = sleeps.Select(row => new double[] { row[row.Length - 2], row[row.Length - 1] })
                                   .ToArray();

                        // Compute the differences between consecutive rows for both second-to-last and last columns
                        var diffs = Enumerable.Range(0, lastTwoColumns.Length - 1)
                                              .Select(i => new double[] {
                                  (lastTwoColumns[i][1] - lastTwoColumns[i][0]) * tDelta
                                              })
                                              .ToArray();

                        // Find the indexes where the condition holds true (difference between 6 and 9)
                        indexes = diffs.Select((diff, index) => new { diff, index })
                                           .Where(x => (x.diff[0] >= 6 && x.diff[0] <= 9))
                                           .Select(x => x.index)  // +1 to shift the index since diffs are calculated from the second element
                                           .ToArray();

                        // Filter the sleeps array by the valid indexes
                        if (indexes.Length > 0)
                        {
                            sleeps = indexes.Select(i => sleeps[i]).ToArray();
                        }

                        // Filter the last sleep begin times from 22:00 to 23:59
                        sleepBeginMin = new TimeSpan(22, 0, 0); // 22:00:00
                        sleepBeginMax = new TimeSpan(23, 59, 0); // 23:59:00

                        var sleepBegins = sleeps.Select(s => s[s.Length - 2]).ToArray();
                        var sleepBeginIndexes = sleepBegins.Select(b => offsetIdx + b).ToArray();
                        var arr = sleepBeginIndexes.Select(i => datetimeRange[(int)i].TimeOfDay).ToArray();

                        indexes = arr
                            .Select((time, index) => new { time, index })
                            .Where(x => (x.time >= sleepBeginMin) && (x.time <= sleepBeginMax))
                            .Select(x => x.index)
                            .ToArray();

                        // Select the unique times, to be used in the next condition
                        if (indexes.Length > 0)
                        {
                            sleeps = sleeps.Where((s, i) => indexes.Contains(i)).ToArray();

                            // Get unique times
                            arr = arr.Distinct().ToArray();
                            arr = arr.Where(x => (x >= sleepBeginMin) && (x <= sleepBeginMax)).ToArray();

                            sleepBeginMin = arr.Min();
                            sleepBeginMax = arr.Max();

                            // Adjust max if min equals max
                            if (sleepBeginMin == sleepBeginMax)
                            {
                                sleepBeginMax = sleepBeginMin.Add(TimeSpan.FromMinutes(UtilityFunctions.N_DELTA * UtilityFunctions.INTERVAL_FREQUENCY_MINUTE));
                            }
                        }
                    }

                    if (numSleeps == numberOfNights - 1)
                    {
                        // Find indexes where the last column matches (end_idx - 1)
                        indexes = sleeps.Select((row, id) => new { row, id })
                            .Where(x => x.row.Last() == (endIdx - 1))
                            .Select(x => x.id)
                            .ToArray();

                        if (indexes.Length > 0)
                        {
                            // Filter the sleeps array by the found indexes
                            sleeps = sleeps.Where((s, i) => indexes.Contains(i)).ToArray();
                            break;
                        }

                        double remaining_window = (endIdx - sleeps.Cast<double>().Average()) * tDelta;
                        if (remaining_window > 12)
                        {
                            // Filter last sleep in the 6-9 hour range
                            var diff = Enumerable.Range(0, sleeps.GetLength(0))
                                .Select(i => (sleeps[i][sleeps[i].Length - 1] - sleeps[i][sleeps[i].Length - 2]) * tDelta)
                                .ToArray();

                            indexes = Enumerable.Range(0, diff.Length)
                                .Where(i => diff[i] >= 6 && diff[i] <= 9)
                                .ToArray();

                            if (indexes.Length > 0)
                                sleeps = sleeps.Where((s, i) => indexes.Contains(i)).ToArray();

                            // Filter by sleep begin times (22:30 - 23:59)
                            sleepBeginMin = new TimeSpan(22, 0, 0);
                            sleepBeginMax = new TimeSpan(23, 59, 0);

                            // Replace sleep begin logic using DateTime
                            var sleepBegins1 = Enumerable.Range(0, sleeps.GetLength(0))
                                .Select(i => Convert.ToInt32(sleeps[i][sleeps[i].Length - 2]))
                                .ToArray();

                            indexes = sleepBegins1
                                .Select((begin, id) => new { begin, id })
                                .Where(x => datetimeRange[x.begin].TimeOfDay >= sleepBeginMin && datetimeRange[x.begin].TimeOfDay <= sleepBeginMax)
                                .Select(x => x.id)
                                .ToArray();

                            if (indexes.Length > 0)
                                sleeps = sleeps = sleeps.Where((s, i) => indexes.Contains(i)).ToArray();

                            // Call the next sleep episode method (custom logic based on your function)
                            bool notFound = false;
                            while (true)
                            {
                                var nextSleep = util.FindNextSleepEpisodeNumba(sleeps.ToList(), endIdx, tDelta, cVal, paramsList); // Placeholder method for custom logic
                                if (nextSleep.Length > 0 && !notFound)
                                {
                                    indexes = Enumerable.Range(0, nextSleep.Length)
                                        .Where(i => nextSleep[i][nextSleep[i].Length - 1] == (endIdx - 1))
                                        .ToArray();

                                    if (indexes.Length > 0)
                                    {
                                        sleeps = sleeps = sleeps.Where((s, i) => indexes.Contains(i)).ToArray();
                                        break;
                                    }
                                    else
                                    {
                                        notFound = true;
                                    }
                                }
                                else
                                {
                                    var foundWindow = stage3Helper.FindSleepWithinWindow(sleeps.ToList(), cVal, endIdx, tDelta); // Placeholder method for logic
                                    if (foundWindow.Count > 0)
                                    {
                                        sleeps = foundWindow.ToArray();
                                        break;
                                    }
                                    else
                                    {
                                        Environment.Exit(0);
                                    }
                                }
                            }
                        }
                        else if (remaining_window > 2)
                        {
                            var foundSleep = stage3Helper.FindSleepWithinWindow(sleeps.ToList(), cVal, endIdx, tDelta); // Custom method
                            if (foundSleep.Count > 0)
                            {
                                sleeps = foundSleep.ToArray();
                                break;
                            }
                            else
                            {
                                Environment.Exit(0);
                            }
                        }
                        else if (remaining_window <= 2)
                        {
                            break;
                        }
                        else
                        {
                            Environment.Exit(0);
                        }
                    }
                }
                nonDutySleeps.Add(sleeps);
            }

            return new Stage3SWResponse() { NonDutySleeps = nonDutySleeps };
        }
    }
}
