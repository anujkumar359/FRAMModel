using EasyJet.FRAMModel.SleepWake.Entities;
using EasyJet.FRAMModel.SleepWake.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Calculator
{
    /// <summary>
    /// Represents a calculator for Stage 2 software processes.
    /// This class handles calculations related to circadian rhythms during non-duty periods.
    /// </summary>
    internal class Stage2SWCalculator
    {
        /// <summary>
        /// Utility functions instance providing various helper methods for calculations and operations.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// Calculates the Stage 2 software response based on the provided consecutive duties and non-duty indexes.
        /// </summary>
        /// <param name="consecutiveDuties">The Stage 1 software response containing duty information and circadian values.</param>
        /// <param name="nonDutyIndexes">A list of tuples representing the start and end indices of non-duty periods.</param>
        /// <param name="beginEndDates">A list of tuples containing the beginning and ending dates and their associated time spans.</param>
        /// <returns>A <see cref="Stage2SWResponse"/> object containing the results of the calculations.</returns>
        public Stage2SWResponse ProcessNonDutyCircadians(Stage1SWResponse consecutiveDuties, List<Tuple<int, int>> nonDutyIndexes, List<(int, DateTime, DateTime, TimeSpan, TimeSpan)> beginEndDates)
        {
            Stage2SWResponse stage2SWResponse = ProcessNonDutyCircadians(beginEndDates, consecutiveDuties.RsDuties, nonDutyIndexes, consecutiveDuties.Circadians.ToArray());
            return stage2SWResponse;
        }

        /// <summary>
        /// Processes circadian values during non-duty periods based on the provided beginning and ending dates.
        /// </summary>
        /// <param name="beginEndDates">A list of tuples containing the beginning and ending dates along with their time spans.</param>
        /// <param name="rsDuties">A list of tuples representing duties, including phase and alertness metrics.</param>
        /// <param name="nonDutyIndexes">A list of tuples representing the start and end indices of non-duty periods.</param>
        /// <param name="circadians">An array of circadian values to be processed.</param>
        /// <returns>A <see cref="Stage2SWResponse"/> object containing the processed circadian values.</returns>
        public Stage2SWResponse
        ProcessNonDutyCircadians(List<(int, DateTime, DateTime, TimeSpan, TimeSpan)> beginEndDates,
                                 List<(double StartPhase, double EndPhase, double StartCircadian, double StartHomeostatic, double StartAlertness, double EndCircadian, double EndHomeostatic, double EndAlertness)> rsDuties,
                                 List<Tuple<int, int>> nonDutyIndexes,
                                 double[] circadians)
        {
            double[] circadiansWoNan = (double[])circadians.Clone();
            List<double[]> cVals = new List<double[]>();
            List<double> lastSwVals = new List<double>();

            for (int idx = 0; idx < nonDutyIndexes.Count; idx++)
            {               
                var (start, end) = nonDutyIndexes[idx];

                int nSize = (end - start) + 1;
                double lastPhase = rsDuties[idx].EndPhase;
                double nextPhase = rsDuties[idx + 1].StartPhase;
                double lastSw = rsDuties[idx].EndHomeostatic;
                double nextSw = rsDuties[idx + 1].StartHomeostatic;

                // Determine t_shift
                double tShift;
                double[] tShifts;
                if (idx == 0)
                {
                    tShift = util.DateTimeTimeToDecimal(beginEndDates[idx].Item3);
                }
                else
                {
                    tShift = util.DateTimeTimeToDecimal(beginEndDates[idx].Item3.Add(util.AWAKE_OFFSET));
                }

                tShifts = util.Linspace(tShift, tShift - (nextPhase - lastPhase), nSize).ToArray();
                    
                   
                lastSwVals.Add(lastSw);

                // Generate time intervals and calculate circadian component
                double[] timeIntervals = util.Linspace(0, (nSize - 1) * (UtilityFunctions.N_DELTA * util.TIME_DELTA), nSize).ToArray();                                                
                double[] c = new double[timeIntervals.Length];
                for (int i = 0; i < timeIntervals.Length; i++)
                {
                    double a = util.CircadianComponent(timeIntervals[i] % util.CIRCADIAN_LENGTH, lastPhase - tShifts[i]);
                    c[i] = a;

                }
                cVals.Add(c);
                Array.Copy(c, 0, circadiansWoNan, start, c.Length);
            }

            return (new Stage2SWResponse() { CircadiansWoNan = circadiansWoNan, CVals = cVals, LastSwVals = lastSwVals});
        }
    }
}
