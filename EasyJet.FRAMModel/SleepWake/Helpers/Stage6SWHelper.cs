using System.Linq;
using System;
using System.Collections.Generic;

namespace EasyJet.FRAMModel.SleepWake.Helpers
{
    /// <summary>
    /// Helper class for Stage 6 of Sleep-Wake calculations.
    /// </summary>
    internal class Stage6SWHelper
    {
        /// <summary>
        /// Utility functions instance.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// The optimal sleep time.
        /// </summary>
        private int OPTIMAL_SLEEP_TIME = 8;

        /// <summary>
        /// Calculates the cumulative sleep length effect based on daily scores and alpha.
        /// </summary>
        /// <param name="dailyScores">The array of daily scores.</param>
        /// <param name="alpha">The alpha value.</param>
        /// <returns>The array of cumulative sleep length effect.</returns>
        public double[] CumulativeSleepLengthEffect(double[] dailyScores, double alpha)
        {
            // Calculate the difference from optimal sleep
            double[] sleepDiff = new double[dailyScores.Length];
            for (int i = 0; i < dailyScores.Length; i++)
            {
                sleepDiff[i] = dailyScores[i] - OPTIMAL_SLEEP_TIME;
            }

            // Initialize the cumulative effective sleep value array
            double[] result = new double[dailyScores.Length];

            // Calculate the cumulative effective sleep values with carry-over effect
            result[0] = sleepDiff[0];
            for (int i = 1; i < dailyScores.Length; i++)
            {
                result[i] = alpha * result[i - 1] + sleepDiff[i];
            }

            return result;
        }

        /// <summary>
        /// Calculates the cumulative alertness effect based on daily scores and alpha.
        /// </summary>
        /// <param name="dailyScores">The array of daily scores.</param>
        /// <param name="alpha">The alpha value.</param>
        /// <returns>The array of cumulative alertness effect.</returns>
        public double[] CumulativeAlertnessEffect(double[] dailyScores, double alpha)
        {
            int n = dailyScores.Length;
            double[] days = Enumerable.Range(0, n).Select(i => (double)i).ToArray();

            // Calculate the weights based on alpha and the length of daily scores
            double[] weights = new double[n];
            for (int i = 0; i < n; i++)
            {
                weights[i] = Math.Pow(alpha, n - i);
            }

            // Calculate cumulative weighted sum
            double[] result = new double[n];
            result[0] = weights[0] * dailyScores[0];

            for (int i = 1; i < n; i++)
            {
                result[i] = result[i - 1] + weights[i] * dailyScores[i];
            }

            return result;
        }

        /// <summary>
        /// Gets the sleep-wake periods when operating based on the provided data.
        /// </summary>
        /// <param name="t">The list of DateTime values.</param>
        /// <param name="arr">The list of double values.</param>
        /// <param name="awakeSleepIndexes">The list of awake-sleep indexes.</param>
        /// <param name="onDutyIndexes">The list of on-duty indexes.</param>
        /// <returns>A tuple containing the column names and the sleep-wake results.</returns>
        public (List<string> cols, List<Tuple<DateTime, DateTime, double, double>> results) GetSleepWakeWhenOperating(
            List<DateTime> t, List<double> arr, List<int> awakeSleepIndexes, List<Tuple<int, int>> onDutyIndexes)
        {
            var results = new List<Tuple<DateTime, DateTime, double, double>>();

            foreach (var onDuty in onDutyIndexes)
            {
                int startIdx = onDuty.Item1;
                int endIdx = onDuty.Item2;

                // Find the last awake index before startIdx
                int awakeIndex = awakeSleepIndexes.Where(idx => startIdx - idx >= 0).Last();

                // Find the first sleep index after endIdx
                int sleepIndex = awakeSleepIndexes.Where(idx => endIdx - idx <= 0).First();

                results.Add(new Tuple<DateTime, DateTime, double, double>(
                    t[awakeIndex],
                    t[sleepIndex],
                    arr[startIdx],
                    arr[endIdx]
                ));
            }

            var cols = new List<string> { "AwakeTime", "SleepTime", "AlertnessWhenAwake", "AlertnessWhenSleep" };
            return (cols, results);
        }

        /// <summary>
        /// Gets the last sleep-wake period before operating based on the provided data.
        /// </summary>
        /// <param name="t">The list of DateTime values.</param>
        /// <param name="indexes">The list of indexes.</param>
        /// <param name="beginDutyIndexes">The list of begin-duty indexes.</param>
        /// <returns>A tuple containing the column names and the sleep-wake results.</returns>
        public (List<string> cols, List<Tuple<DateTime, DateTime, double>> results) GetLastSleepWakeBeforeOperating(
            List<DateTime> t, List<int> indexes, List<int> beginDutyIndexes)
        {
            var results = new List<Tuple<DateTime, DateTime, double>>();

            foreach (int start in beginDutyIndexes)
            {
                // Find the indexes before the start time
                var idxs = indexes.Where(idx => start - idx >= 0).Reverse().Take(2).Reverse().ToArray();

                int awakeIndex = idxs.Length > 0 ? idxs.Last() : 0;
                int sleepIndex = idxs.Length > 1 ? idxs.First() : 0;

                // Calculate sleep duration
                double sleepDuration = (awakeIndex - sleepIndex) * util.TIME_DELTA;

                results.Add(new Tuple<DateTime, DateTime, double>(
                    t[sleepIndex],
                    t[awakeIndex],
                    sleepDuration
                ));
            }

            var cols = new List<string>
                {
                    "SleepBeginBeforeOperating",
                    "SleepEndBeforeOperating",
                    "SleepLengthBeforeOperating"
                };
            return (cols, results);
        }

        /// <summary>
        /// Gets the indexes of derivative changes in an array.
        /// </summary>
        /// <param name="arr">The array to analyze.</param>
        /// <returns>A tuple containing the indexes of local maxima and local minima.</returns>
        public (List<int> localMax, List<int> localMin) GetDerivativeChangeIndexes(double[] arr)
        {
            // Compute the difference between consecutive elements
            var derivativeChanges = arr.Skip(1).Zip(arr, (next, prev) => next - prev > 0).ToList();

            // Find local maxima (change from increasing to decreasing)
            var localMax = new List<int>();
            var localMin = new List<int>();
            for (int i = 1; i < derivativeChanges.Count; i++)
            {
                if (derivativeChanges[i - 1] && !derivativeChanges[i])
                {
                    localMax.Add(i);
                }
                if (!derivativeChanges[i - 1] && derivativeChanges[i])
                {
                    localMin.Add(i);
                }
            }
            return (localMax, localMin);
        }
    }
}
