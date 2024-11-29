using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Helpers
{

    /// <summary>
    /// Provides helper methods for Stage 2 software processes.
    /// This class contains methods for data manipulation and analysis specific to Stage 2 calculations.
    /// </summary>
    internal class Stage2SWHelper
    {
        /// <summary>
        /// Retrieves the indexes of NaN (Not a Number) values in the provided list.
        /// </summary>
        /// <param name="lst">A list of double values to be checked for NaN entries.</param>
        /// <returns>A list of tuples, where each tuple contains the start and end indexes of NaN segments in the list.</returns>
        public List<Tuple<int, int>> GetNanIndexes(List<double> lst)
        {
            List<int> nanIndices = lst.Select((x, i) => new { Value = x, Index = i })
                                      .Where(x => double.IsNaN(x.Value))
                                      .Select(x => x.Index)
                                      .ToList();

            List<Tuple<int, int>> nonDutyIndexes = new List<Tuple<int, int>>();

            if (nanIndices.Count == 0)
                return nonDutyIndexes;

            int startIdx = nanIndices[0];

            for (int i = 1; i < nanIndices.Count; i++)
            {
                if (nanIndices[i] != nanIndices[i - 1] + 1)
                {
                    int endIdx = nanIndices[i - 1];
                    nonDutyIndexes.Add(Tuple.Create(startIdx, endIdx));
                    startIdx = nanIndices[i];
                }
            }

            // Add the last block if necessary
            nonDutyIndexes.Add(Tuple.Create(startIdx, nanIndices.Last()));

            return nonDutyIndexes;
        }
    }
}
