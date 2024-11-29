using EasyJet.FRAMModel.SleepWake.Entities;
using EasyJet.FRAMModel.SleepWake.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Calculator
{

    /// <summary>
    /// Represents a calculator for Stage 6 Sleep-Wake calculations.
    /// </summary>
    internal class Stage6SWCalculator
    {
        /// <summary>
        /// Utility functions instance.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// Stage 6 Sleep-Wake helper instance.
        /// </summary>
        private Stage6SWHelper stage6Helper = new Stage6SWHelper();

        /// <summary>
        /// Calculates the Stage 6 Sleep-Wake values based on the provided data table and Stage 5 Sleep-Wake response.
        /// </summary>
        /// <param name="dt">The data table containing the sleep-wake data.</param>
        /// <param name="stage5SWResponse">The Stage 5 Sleep-Wake response.</param>
        /// <returns>The calculated data table with Stage 6 Sleep-Wake values.</returns>
        public DataTable Calculate(DataTable dt, Stage5SWResponse stage5SWResponse)
        {
            double[] alertness = stage5SWResponse.InterpolatedH.Zip(stage5SWResponse.InterpolatedC, (x, y) => x + y).ToArray();

            var (awakeIndexes, sleepIndexes) = stage6Helper.GetDerivativeChangeIndexes(stage5SWResponse.InterpolatedH);

            if (awakeIndexes.Count != sleepIndexes.Count)
            {
                // Append the last index of t (len(t) - 1) to sleep_indexes
                sleepIndexes.Add(stage5SWResponse.TimeRange.Count - 1);
            }

            List<int> beginDutyIndexes = util.GetTimeIndexes(stage5SWResponse.TimeRange.ToArray(), dt.AsEnumerable().Select(row => row.Field<DateTime>("CommuteBegin")).ToArray()).ToList();
            List<int> endDutyIndexes = util.GetTimeIndexes(stage5SWResponse.TimeRange.ToArray(), dt.AsEnumerable().Select(row => row.Field<DateTime>("CommuteEnd")).ToArray()).ToList();
            List<Tuple<int, int>> onDutyIndexes = beginDutyIndexes
                                .Zip(endDutyIndexes, (begin, end) => Tuple.Create(begin, end))
                                .ToList();

            HashSet<int> indexes = new HashSet<int>(awakeIndexes);
            indexes.UnionWith(sleepIndexes); // This performs the union operation

            // Convert to a sorted list if needed
            List<int> sortedIndexes = indexes.OrderBy(i => i).ToList();

            var (cols, results) = stage6Helper.GetSleepWakeWhenOperating(stage5SWResponse.TimeRange, stage5SWResponse.InterpolatedH.ToList(), sortedIndexes, onDutyIndexes);

            // Add columns to the DataTable
            dt.Columns.Add(cols[0], typeof(DateTime)); // AwakeTime            
            dt.Columns.Add(cols[1], typeof(DateTime)); // SleepTime
            dt.Columns.Add(cols[2], typeof(double));   // AlertnessWhenAwake
            dt.Columns.Add(cols[3], typeof(double));   // AlertnessWhenSleep

            // Add rows to the DataTable
            for (int i = 0; i < results.Count; i++)
            {
                dt.Rows[i][cols[0]] = results[i].Item1;
                dt.Rows[i][cols[1]] = results[i].Item2;
                dt.Rows[i][cols[2]] = results[i].Item3;
                dt.Rows[i][cols[3]] = results[i].Item4;
            }

            var (cols1, results1) = stage6Helper.GetLastSleepWakeBeforeOperating(stage5SWResponse.TimeRange, sortedIndexes, beginDutyIndexes);

            foreach (var col in cols1)
            {
                if (col == "SleepLengthBeforeOperating")
                    dt.Columns.Add(col, typeof(double)); // Sleep length as double
                else
                    dt.Columns.Add(col, typeof(DateTime)); // Sleep begin/end as DateTime
            }

            for (int i = 0; i < results1.Count; i++)
            {
                dt.Rows[i][cols1[0]] = results1[i].Item1;
                dt.Rows[i][cols1[1]] = results1[i].Item2;
                dt.Rows[i][cols1[2]] = results1[i].Item3;
            }
            dt.Columns.Add("AlertnessBeforeDuty", typeof(double));
            dt.Columns.Add("AlertnessAfterDuty", typeof(double));


            for (int i = 0; i < onDutyIndexes.Count; i++)
            {
                var (start_idx, end_idx) = onDutyIndexes[i];

                // Add the alertness slice as a comma-separated string
                dt.Rows[i]["AlertnessBeforeDuty"] = alertness[start_idx];
                dt.Rows[i]["AlertnessAfterDuty"] = alertness[end_idx];
            }

            double[] adiff = new double[alertness.Length - 1];
            for (int i = 0; i < alertness.Length - 1; i++)
            {
                adiff[i] = alertness[i + 1] - alertness[i];
            }

            dt.Columns.Add("SumOfDiffs", typeof(double));

            for (int i = 0; i < onDutyIndexes.Count; i++)
            {
                var (start_idx, end_idx) = onDutyIndexes[i];
                double sumOfDiffs = 0;
                for (int j = start_idx; j < end_idx; j++)
                {
                    sumOfDiffs += adiff[j];
                }

                // Add the sum of differences to the DataTable
                dt.Rows[i]["SumOfDiffs"] = sumOfDiffs;
            }

            dt.Columns.Add("SleepLenghtBeforeOperating_cum_0.9", typeof(double));

            dt = dt.AsEnumerable().GroupBy(d => new { BlockIDSW = d.Field<int>("BlockIDSW") })
                              .SelectMany(group =>
                              {
                                  var sleepLengths = group.Select(g => g.Field<double>("SleepLengthBeforeOperating")).ToArray();
                                  var cumulativeSleepLengths = stage6Helper.CumulativeSleepLengthEffect(sleepLengths, 0.9);

                                  // Assign the cumulative values back to the data
                                  return group.Zip(cumulativeSleepLengths, (g, cumSleep) =>
                                  {
                                      g.SetField("SleepLenghtBeforeOperating_cum_0.9", cumSleep);
                                      return g;
                                  });
                              }).CopyToDataTable();

            dt.Columns.Add("SumOfDiffs_cum_0.5", typeof(double));
            dt = dt.AsEnumerable().GroupBy(d => new { BlockIDSW = d.Field<int>("BlockIDSW") })
                              .SelectMany(group =>
                              {
                                  var alertLengths = group.Select(g => g.Field<double>("SumOfDiffs")).ToArray();
                                  var CumulativeAlertnessLengths = stage6Helper.CumulativeAlertnessEffect(alertLengths, 0.5);

                                  // Assign the cumulative values back to the data
                                  return group.Zip(CumulativeAlertnessLengths, (g, cumAlert) =>
                                  {
                                      g.SetField("SumOfDiffs_cum_0.5", cumAlert);
                                      return g;
                                  });
                              }).CopyToDataTable();

            return dt;
        }
    }
}
