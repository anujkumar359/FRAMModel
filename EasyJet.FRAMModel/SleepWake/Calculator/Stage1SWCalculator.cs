
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Globalization;
using EasyJet.FRAMModel.SleepWake.Helpers;
using EasyJet.FRAMModel.SleepWake.Entities;
using EasyJet.FRAMModel.Engine.ExternalContract;

namespace EasyJet.FRAMModel.SleepWake.Calculator
{
    /// <summary>
    /// Represents a Stage 1 Sleep-Wake Calculator.
    /// </summary>
    internal class Stage1SWCalculator
    {

        /// <summary>
        /// A utility functions instance providing various helper methods for calculations and operations.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// A helper instance for Stage 1, assisting with specific logic and data management.
        /// </summary>
        private Stage1SWHelper stage1Helper = new Stage1SWHelper();

        List<double> lowers;

        List<double> uppers;

        List<double> phases;

        public Stage1SWCalculator()
        {
            List<double> lowers = util.Linspace(7, 9, 11);
            List<double> uppers = util.Linspace(12, 14, 11);
            List<double> phases = util.Linspace(15, 17, 11);

            //stage1Helper.THRESHOLDS 
            util.THRESHOLDS = (from x in lowers
                               from y in uppers
                               from z in phases
                               select (Math.Round(x, 2), Math.Round(y, 2), Math.Round(z, 2))).ToList();
        }

        /// <summary>
        /// Processes consecutive duties and returns the Stage 1 Sleep-Wake response.
        /// </summary>
        /// <param name="dt">The DataTable containing duty records.</param>
        /// <param name="beginEndDates">The list of duty begin and end dates.</param>
        /// <param name="dutyIds">The list of duty IDs.</param>
        /// <returns>The Stage 1 Sleep-Wake response.</returns>
        public Stage1SWResponse ProcessConsecutiveDuties(DataTable dt, List<(int, DateTime, DateTime, TimeSpan, TimeSpan)> beginEndDates, List<int> dutyIds)
        {
            var cols = new[] { "DutyID", "CommuteBegin", "CommuteEnd", "MinRestTime", "DutyType", "ejDisruptive" };
            var rsDuties = new List<(double, double, double, double, double, double, double, double)>();
            var circadians = new List<double>();
            var homeostatics = new List<double>();
            var totalDatetimeRange = new List<DateTime>();

            // Calculate initial components before the first duty
            DateTime initialTime = util.SetInitialDateTime(beginEndDates[0].Item2);
            DateTime beginDate = initialTime.Date;
            DateTime endDate = initialTime;
            List<DateTime> datetimeRange = stage1Helper.GetDatetimeRange(beginDate, endDate);
            totalDatetimeRange.AddRange(datetimeRange);
            var timeIntervals = Enumerable.Range(0, datetimeRange.Count).Select(t => t * util.TIME_DELTA).ToArray();

            // Determine initial homeostatic component
            List<double> h = new List<double>();
            if (initialTime.Date == beginEndDates[0].Item2.Date)
            {
                // When on the same day, consider sleep                
                foreach (var t in timeIntervals)
                {
                    double adjustedT = t + (UtilityFunctions.INITIAL_SLEEP - util.DateTimeToDecimal(initialTime));
                    h.Add(util.HomeostaticSleepComponent(adjustedT, UtilityFunctions.INITIAL_SLEEP));
                }
            }
            else
            {
                // Handling overnight or previous day commutes
                for (int t = 0; t < timeIntervals.Length; t++)
                {
                    if (timeIntervals[t] < 7)
                    {
                        double val = util.HomeostaticSleepComponent(timeIntervals[t], 10);
                        h.Add(val);
                    }
                    else if (timeIntervals[t] < 20)
                    {
                        h.Add(util.HomeostaticAwakeComponent(timeIntervals[t] - 7, h.Last()));
                    }
                    else
                    {
                        h.Add(util.HomeostaticSleepComponent(timeIntervals[t] - 20, 10));
                    }
                }
            }

            // Initial circadian component
            double[] c = stage1Helper.CircadianComponent(timeIntervals.Select(t => t % util.CIRCADIAN_LENGTH).ToArray(), util.INITIAL_PHASE);
            homeostatics.AddRange(h);
            circadians.AddRange(c);

            var phases = new List<double> { util.INITIAL_PHASE };

            // Loop over each duty record
            for (int idx = 0; idx < beginEndDates.Count; idx++)
            {               
                var beginEndDate = beginEndDates[idx];
                var dutyId = dutyIds[idx];
                var dfTemp = dt.AsEnumerable()
                .Where(row => row.Field<int>("DutyID") == dutyId).
                    CopyToDataTable().DefaultView.ToTable(false, cols);
                int nDuty = beginEndDate.Item1;
                var dutyBeginDate = stage1Helper.SetInitialDatetimeWithNap(beginEndDate.Item2);
                beginDate = dutyBeginDate.Item1;
                var hnap = dutyBeginDate.Item2;
                endDate = beginEndDate.Item3.AddMinutes(util.AWAKE_OFFSET.TotalMinutes);
                datetimeRange = stage1Helper.GetDatetimeRange(beginDate, endDate);
                totalDatetimeRange.AddRange(datetimeRange);

                // Process single-day or multi-day duties
                if (nDuty == 1)
                {
                    double phase = phases.Last();
                    timeIntervals = Enumerable.Range(0, datetimeRange.Count).Select(t => t * util.TIME_DELTA).ToArray();
                    double sw = hnap.Length > 0 ? hnap[hnap.Length - 1] : 14.0;
                    h = util.HomeostaticAwakeComponent(timeIntervals, sw).ToList();
                    double initialTimeDecimal = util.DateTimeToDecimal(beginDate);
                    c = stage1Helper.CircadianComponent(timeIntervals.Select(t => t % util.CIRCADIAN_LENGTH).ToArray(), phase - initialTimeDecimal);

                    if (hnap.Any())
                    {
                        h = hnap.Concat(h.Take(h.Count - hnap.Length)).ToList();
                    }
                    var a = c.Zip(h, (ci, hi) => ci + hi).ToArray();
                    circadians.AddRange(c);
                    homeostatics.AddRange(h);
                    rsDuties.Add((phase, phase, c[0], h[0], a[0], c[c.Length - 1], h[h.Count - 1], a[a.Length - 1]));
                }
                else
                {
                    //Process multiple-day duties
                    var (circadian, homeostatic, resultPhases) = ProcessConsecutiveDuty(dfTemp);
                    var a = circadian.Zip(homeostatic, (ci, hi) => ci + hi).ToArray();
                    circadians.AddRange(circadian);
                    homeostatics.AddRange(homeostatic);
                    phases = resultPhases.Select(p => p.Phase).ToList();
                    rsDuties.Add((phases.First(), phases.Last(), circadian[0], homeostatic[0], a[0], circadian[circadian.Length - 1], homeostatic[homeostatic.Length - 1], a[a.Length - 1]));
                }

                // Handle non-duty days
                if (idx < beginEndDates.Count - 1)
                {
                    beginDate = endDate;
                    endDate = util.SetInitialDateTime(beginEndDates[idx + 1].Item2);
                    datetimeRange = stage1Helper.GenerateDateTimeRange(beginDate, endDate);
                    totalDatetimeRange.AddRange(datetimeRange);
                    int nSize = datetimeRange.Count;
                    circadians.AddRange(Enumerable.Repeat(double.NaN, nSize));
                    homeostatics.AddRange(Enumerable.Repeat(double.NaN, nSize));
                }
            }

            // Finalize processing if only one duty record is present
            if (beginEndDates.Count == 1)
            {
                datetimeRange = stage1Helper.GetDatetimeRange(endDate, endDate.Date.AddDays(1));
                totalDatetimeRange.AddRange(datetimeRange);
                timeIntervals = Enumerable.Range(0, datetimeRange.Count).Select(t => t * util.TIME_DELTA).ToArray();
                double lastTime = util.DateTimeToDecimal(endDate);
                c = stage1Helper.CircadianComponent(timeIntervals.Select(t => t % util.CIRCADIAN_LENGTH).ToArray(), phases.Last() - lastTime);
                circadians.AddRange(c);

                // Handle different scenarios based on end time
                if (endDate.TimeOfDay < new TimeSpan(9, 0, 0))
                {
                    // Assume sleep (min 2h) after arrival until 9 am, awake till 22, then sleep
                    double sleepLen = Math.Max(2, 9 - lastTime);
                    timeIntervals = util.ArangeValues(0,sleepLen, util.TIME_DELTA);
                    
                    h = timeIntervals.Select(t => util.HomeostaticSleepComponent(t, homeostatics.Last())).ToList();
                    homeostatics.AddRange(h);

                    // Awake period until 22
                    double awake = timeIntervals.Last() + lastTime;
                    timeIntervals = util.ArangeValues(awake, 22, util.TIME_DELTA);                    
                    if (timeIntervals.Length > 0 && timeIntervals[0] == awake) timeIntervals = timeIntervals.Skip(1).ToArray();
                    if (timeIntervals.Last() + util.TIME_DELTA >= 22) timeIntervals = timeIntervals.Take(timeIntervals.Length - 1).ToArray();
                    h = util.HomeostaticAwakeComponent(timeIntervals.Select(t => t - awake).ToArray(), h.Last()).ToList();
                    homeostatics.AddRange(h);

                    // Sleep after 22 until the end
                    timeIntervals = util.ArangeValues(22, 24, util.TIME_DELTA);
                    h = timeIntervals.Select(t => util.HomeostaticSleepComponent(t, h.Last())).ToList();
                    homeostatics.AddRange(h);
                }
                else
                {
                    // Awake until the end of the day                   
                    timeIntervals = util.ArangeValues(lastTime, 24, util.TIME_DELTA);
                    if (Math.Abs(timeIntervals.Last() - 24) < double.Epsilon) timeIntervals = timeIntervals.Take(timeIntervals.Length - 1).ToArray();
                    h = util.HomeostaticAwakeComponent(timeIntervals.Select(t => t - lastTime).ToArray(), homeostatics.Last()).ToList();
                    homeostatics.AddRange(h);
                }
            }

            // Calculate alertness
            var alertnesses =circadians.Zip(homeostatics, (circadian, homeostatic) => circadian + homeostatic).ToList();
            
            return (new Stage1SWResponse { Circadians = circadians, Homeostatics = homeostatics, Alertnesses = alertnesses, TotalDatetimeRange = totalDatetimeRange, RsDuties = rsDuties });
        }

        /// <summary>
        /// Processes consecutive duty records and returns the circadian and homeostatic components.
        /// </summary>
        /// <param name="df">The DataTable containing duty records.</param>
        /// <returns>A tuple containing the circadian and homeostatic components, and a list of sleep episodes.</returns>
        public (double[], double[], List<SleepEpisode>) ProcessConsecutiveDuty(DataTable df)
        {
            // Assuming 'df' is a DataTable with the required columns
            DateTime beginDate = df.AsEnumerable().Min(row => row.Field<DateTime>("CommuteBegin")).Date;
            DateTime endDate = df.AsEnumerable().Max(row => row.Field<DateTime>("CommuteEnd")).Add(util.AWAKE_OFFSET).Date.AddDays(1);

            List<DateTime> datetimeRange = stage1Helper.GetDatetimeRange(beginDate, endDate);
            int[] beginAwakeIndexes = util.GetTimeIndexes(datetimeRange.ToArray(), df.AsEnumerable().Select(row => row.Field<DateTime>("CommuteBegin")).ToArray(), -util.AWAKE_OFFSET);
            (DateTime beginDateInitial, double[] hnap, double[] cnap) = stage1Helper.SetInitialDatetimeWithNap(df.Rows[0].Field<DateTime>("CommuteBegin"));

            beginAwakeIndexes[0] = util.GetTimeIndexes(datetimeRange.ToArray(), new DateTime[] { beginDateInitial })[0] + hnap.Length;
            int[] endAwakeIndexes = util.GetTimeIndexes(datetimeRange.ToArray(), df.AsEnumerable().Select(row => row.Field<DateTime>("CommuteEnd")).ToArray(), util.AWAKE_OFFSET);

            double sw = hnap.Any() ? hnap.Last() : 14.0; // First homeostatic awake and its alertness level
            string[] dutyTypes = df.AsEnumerable().Select(row => row.Field<string>("DutyType")).ToArray();
            List<SleepEpisode> resultsFound = new List<SleepEpisode>();
            double? phase = null;
            int? sleepBeginIdx = null;

            int beginIdx = 0;

            for (int idx = 0; idx < beginAwakeIndexes.Length - 1; idx++)
            {
                if (idx == 0)
                    beginIdx = beginAwakeIndexes[idx];   // Current commute begin - awake offset
                int endIdx = endAwakeIndexes[idx];                       // Current commute end + awake offset
                int nextIdx = beginAwakeIndexes[idx + 1];                // Next commute begin - awake offset
                string currentDuty = dutyTypes[idx];                     // Current duty type
                string nextDuty = dutyTypes[idx + 1];                    // Next duty type
                bool transition = currentDuty != nextDuty;

                var results = util.THRESHOLDS.Select(threshold => stage1Helper.FindSleepEpisodesNumba(beginIdx, endIdx, nextIdx, threshold.Item1, threshold.Item2, threshold.Item3, sw))
                                                 .Where(r => r != null).ToList();

                if (results.Count == 0)
                {
                    int isParamsUpdated = 0;

                    while (true)
                    {  
                        if (isParamsUpdated == 0)
                        {
                            lowers = util.Linspace(6, 10, 17);
                            uppers = util.Linspace(10, 14, 17);
                            phases = util.Linspace(15, 17, 11);
                        }
                        else if (isParamsUpdated == 1)
                        {
                            lowers = util.Linspace(6, 9, 13);
                            uppers = util.Linspace(9, 13, 17);
                            phases = util.Linspace(15, 17, 11);
                        }
                        else if (isParamsUpdated == 2)
                        {
                            lowers = util.Linspace(8, 11, 13);
                            uppers = util.Linspace(11, 14, 13);
                            phases = util.Linspace(15, 17, 11);
                        }
                        else if (isParamsUpdated == 3)
                        {
                            lowers = util.Linspace(6, 9.5, 15);
                            uppers = util.Linspace(9.5, 14, 19);
                            phases = util.Linspace(15, 17, 11);
                        }

                        isParamsUpdated++;

                        // Generate combinations of (l, u, p)
                        var paramsList = lowers.SelectMany(l =>
                                            uppers.SelectMany(u =>
                                                phases.Select(p => (l: Math.Round(l, 2), u: Math.Round(u, 2), p: Math.Round(p, 2)))))
                                        .ToList();

                        results = paramsList.Select(param =>
                            stage1Helper.FindSleepEpisodesNumba(beginIdx, endIdx, nextIdx, param.l, param.u, param.p, sw))
                            .Where(r => r != null)
                            .ToList();

                        if (results.Count > 0)
                        {
                            break;
                        }

                        if (isParamsUpdated > 3 && results.Count == 0)
                        {
                            break;
                        }
                    }
                }

                var resultArray = results.ToArray();
                var result = stage1Helper.SelectParams(resultArray.ToList(), phase ?? 0, sleepBeginIdx, transition);
                resultsFound.Add(result);
                phase = result.Phase;
                sleepBeginIdx = result.SleepStart;
                beginIdx = result.SleepEnd; // Assuming beginIdx is derived from SleepEnd
                sw = result.Homeostatic;
            }

            double[] circadians1 = resultsFound.SelectMany(x => x.Circadians).ToArray();
            double[] homeostatics1 = resultsFound.SelectMany(x => x.Homeostatics).ToArray();

            int finalIdx = endAwakeIndexes.Last();                      // Awake end index after the last duty
            int sleepEndIdx = resultsFound.Last().SleepEnd;          // Sleep end index before the last duty
            sw = homeostatics1.Last();                                  // Homoestatic component value before the last duty
            double initialTime = sleepEndIdx * util.TIME_DELTA;              // Subtract for circadian phase initial position            
            double[] timeIntervals = Enumerable.Range(0, finalIdx - sleepEndIdx)
                                   .Select(i => i * util.TIME_DELTA)
                                   .ToArray();
            double[] homeostatics2 = util.HomeostaticAwakeComponent(timeIntervals, sw);
            double[] circadians2 = stage1Helper.CircadianComponent(timeIntervals.Select(t => (t % util.CIRCADIAN_LENGTH)).ToArray(), phase.GetValueOrDefault() - initialTime);

            double[] circadians = circadians1.Concat(circadians2).ToArray();
            double[] homeostatics = homeostatics1.Concat(homeostatics2).ToArray();

            if (hnap.Any())
            {
                homeostatics = hnap.Concat(homeostatics).ToArray();
                circadians = cnap.Concat(circadians).ToArray();
            }

            return (circadians, homeostatics, resultsFound);
        }              
    }
}
