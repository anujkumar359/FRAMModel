using EasyJet.FRAMModel.SleepWake.Entities;
using EasyJet.FRAMModel.SleepWake.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Calculator
{
    /// <summary>
    /// Represents a calculator for Stage 4 software processes.
    /// This class provides methods to process homeostatic values related to non-duty periods.
    /// </summary>
    internal class Stage4SWCalculator
    {
        /// <summary>
        /// Utility functions instance providing various helper methods for calculations and operations.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// Provides an instance of the Stage4SWHelper class, which contains methods for processing 
        /// sleep-wake cycle data and other utilities related to Stage 4 software processes.
        /// </summary>
        private Stage4SWHelper stage4SWHelper = new Stage4SWHelper();

        /// <summary>
        /// Processes homeostatic values during non-duty periods based on provided data.
        /// </summary>
        /// <param name="datetimeRange">A list of DateTime objects representing the range of interest.</param>
        /// <param name="nonDutySleeps">A list of arrays, where each array contains values for non-duty sleep periods.</param>
        /// <param name="nonDutyIndexes">A list of tuples, each representing the start and end indices of non-duty periods.</param>
        /// <param name="lastSwVals">A list of last switch values affecting homeostasis calculations.</param>
        /// <param name="homeostatics">A list of homeostatic values to be processed.</param>
        /// <returns>A <see cref="Stage4SWResponse"/> object containing the results of the processing.</returns>
        public Stage4SWResponse ProcessNonDutyHomeostatics(List<DateTime> datetimeRange, List<double[][]> nonDutySleeps, List<Tuple<int, int>> nonDutyIndexes, List<double> lastSwVals, List<double> homeostatics)
        {
            // Homeos without NaN (in C#, NaN can be represented using double.NaN)
            double[] homeostaticsWoNan = homeostatics.ToArray(); // Avoid modifying original array
            int nCols = 5;
            double tDelta = UtilityFunctions.N_DELTA * util.TIME_DELTA;  // N_DELTA and TIME_DELTA need to be defined

            for (int idx = 0; idx < nonDutyIndexes.Count; idx++)
            {
                var (start, end) = nonDutyIndexes[idx];
                // Initialize variables
                double[][] sleeps = nonDutySleeps[idx];
                int beginIdx = start;
                int endIdx = end;
                int offsetIdx = start;
                double lastSw = lastSwVals[idx];

                // If applicable, select the values less than 14 (from the third-to-last column)
                List<int> indexes = Enumerable.Range(0, sleeps.Length)
                                 .Where(i => sleeps[i][sleeps[i].Length - 3] <= 13.9).ToList();

                // Filter sleeps array using the indexes found
                if (indexes.Count > 0)
                {
                    sleeps = indexes.Select(i => sleeps[i]).ToArray();
                }

                // Get the index of the row with the highest value in the third-to-last column
                int maxIndex = sleeps
                    .Select((row, index) => new { Value = row[row.Length - 3], Index = index })
                    .OrderByDescending(x => x.Value)
                    .First()
                    .Index;

                // Get the row with the highest value in the third-to-last column
                sleeps = new double[][] { sleeps[maxIndex] };

               
                // Calculate the number of sleeps (number of rows divided by n_cols)
                int nSleeps = sleeps[0].Length / nCols;
                double[][] reshaped = new double[nSleeps][];
                for (int i = 0; i < nSleeps; i++)
                {
                    reshaped[i] = sleeps[0].Skip(i * nCols).Take(nCols).ToArray();
                }

                sleeps = reshaped;

                sleeps[0][2] = lastSw; // Replace last sw with its initial value

                // Call a separate function to calculate homeos
                List<double> h = stage4SWHelper.CalculateNonDutyHomeostatics(sleeps, nSleeps, datetimeRange, offsetIdx, tDelta);

                
                // Calculate ndiff
                int ndiff = (end - start) - (int)sleeps.Last().Last();

                int length_to_update = endIdx + 1 - ndiff - beginIdx;

                // Fill NaN values with the calculated ones
                for (int i = 0; i < length_to_update; i++)
                {
                    homeostaticsWoNan[beginIdx + i] = h[i];
                }
            }

            return new Stage4SWResponse() { HomeostaticsWoNan = homeostaticsWoNan };
        }
        
    }
}
