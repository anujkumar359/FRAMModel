using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyJet.FRAMModel.SleepWake.Helpers
{
    internal class Stage1SWHelper
    {
        /// <summary>
        /// Represents the utility functions used by the Class1.
        /// </summary>
        private readonly UtilityFunctions utilityFunctions = new UtilityFunctions();        
        
        /// <summary>
        /// Selects the sleep episode based on the given parameters.
        /// </summary>
        /// <param name="results">The list of sleep episodes.</param>
        /// <param name="phase">The phase value.</param>
        /// <param name="prevIdx">The previous index.</param>
        /// <param name="transition">A flag indicating if it's a transition.</param>
        /// <returns>The selected sleep episode.</returns>
        public SleepEpisode SelectParams(List<SleepEpisode> results, double phase, int? prevIdx, bool transition)
        {
            if (results == null || results.Count == 0)
            {
                return null;
            }

            // Apply sleep length filter
            results = ApplySleepLengthFilter(results);

            if (results.Count == 0)
            {
                return null;
            }

            if (phase != 0)
            {
                // Apply phase similarity filter
                results = ApplyPhaseSimilarityFilter(results, phase, transition);

                // Apply sleep start similarity filter
                results = ApplySleepStartSimilarityFilter(results, prevIdx, transition);

                if (results.Count > 50)
                {
                    // Apply phase equality filter
                    results = ApplyPhaseEqualityFilter(results, phase);
                }
            }

            if (results.Count == 0)
            {
                return new SleepEpisode();
            }

            // Find and return the result with the maximum alertness
            return results.OrderByDescending(r => r.Alertness).FirstOrDefault();
        }

        /// <summary>
        /// Applies the phase similarity filter to the list of sleep episodes.
        /// </summary>
        /// <param name="results">The list of sleep episodes.</param>
        /// <param name="phase">The phase value.</param>
        /// <param name="transition">A flag indicating if it's a transition.</param>
        /// <param name="percent">The percentage delta.</param>
        /// <returns>The filtered list of sleep episodes.</returns>
        public List<SleepEpisode> ApplyPhaseSimilarityFilter(List<SleepEpisode> results, double? phase, bool transition, double percent = UtilityFunctions.PERCENT_DELTA)
        {
            List<SleepEpisode> filteredResults = new List<SleepEpisode>();

            while (true)
            {
                double min = 1.0 - percent;
                double max = 1.0 + percent;

                var mask = results
                    .Select((r, index) => new { Index = index, Value = r.Phase })
                    .Where(x => x.Value >= phase * min && x.Value <= phase * max)
                    .Select(x => x.Index)
                    .ToArray();

                filteredResults = mask.Select(i => results[i]).ToList();
                percent += UtilityFunctions.PERCENT_DELTA;

                if (transition && percent > 0.15)
                {
                    break;
                }

                if (!transition && percent > 0.02 && filteredResults.Count > 0)
                {
                    break;
                }
            }

            return filteredResults;
        }

        /// <summary>
        /// Applies the sleep start similarity filter to the list of sleep episodes.
        /// </summary>
        /// <param name="results">The list of sleep episodes.</param>
        /// <param name="prevIdx">The previous index.</param>
        /// <param name="transition">A flag indicating if it's a transition.</param>
        /// <param name="percent">The percentage delta.</param>
        /// <returns>The filtered list of sleep episodes.</returns>
        public List<SleepEpisode> ApplySleepStartSimilarityFilter(List<SleepEpisode> results, int? prevIdx, bool transition, double percent = UtilityFunctions.PERCENT_DELTA)
        {
            List<SleepEpisode> filteredResults = new List<SleepEpisode>();

            while (true)
            {
                double min = 1.0 - percent;
                double max = 1.0 + percent;

                int minIdx = (int)((prevIdx + utilityFunctions.CIRCADIAN_LENGTH / utilityFunctions.TIME_DELTA) * min);
                int maxIdx = (int)((prevIdx + utilityFunctions.CIRCADIAN_LENGTH / utilityFunctions.TIME_DELTA) * max);

                var mask = results
                    .Select((r, index) => new { Index = index, Start = r.SleepStart })
                    .Where(x => x.Start > minIdx && x.Start < maxIdx)
                    .Select(x => x.Index)
                    .ToArray();

                filteredResults = mask.Select(i => results[i]).ToList();
                percent += UtilityFunctions.PERCENT_DELTA;

                if (transition && percent > 0.20)
                {
                    break;
                }

                if (!transition && filteredResults.Count > 0)
                {
                    break;
                }
            }

            return filteredResults;
        }

        /// <summary>
        /// Applies a phase equality filter to the list of sleep episodes.
        /// </summary>
        /// <param name="results">The list of sleep episodes.</param>
        /// <param name="phase">The phase value to filter by.</param>
        /// <returns>The filtered list of sleep episodes.</returns>
        public static List<SleepEpisode> ApplyPhaseEqualityFilter(List<SleepEpisode> results, double phase)
        {
            List<SleepEpisode> filteredResults;

            // Get all unique phase values from results
            var phaseValues = results.Select(r => r.Phase).Distinct().ToList();

            if (phaseValues.Contains(phase))
            {
                // If the exact phase is in the list, use it
                filteredResults = results.Where(r => r.Phase == phase).ToList();
            }
            else
            {
                // Find the closest phase
                double closestPhase = phaseValues
                    .OrderBy(p => Math.Abs(p - phase))
                    .FirstOrDefault();

                filteredResults = results.Where(r => r.Phase == closestPhase).ToList();
            }

            return filteredResults;
        }

        /// <summary>
        /// Applies a sleep length filter to the list of sleep episodes.
        /// </summary>
        /// <param name="results">The list of sleep episodes.</param>
        /// <param name="percent">The percentage value to filter by.</param>
        /// <returns>The filtered list of sleep episodes.</returns>
        public List<SleepEpisode> ApplySleepLengthFilter(List<SleepEpisode> results, double percent = 0)
        {
            double minSleepHours = utilityFunctions.SHORT_SLEEPERS.TotalHours;
            double maxSleepHours = utilityFunctions.LONG_SLEEPERS.TotalHours;

            // Convert results to an array for LINQ operations
            var resultsArray = results.ToArray();
            var sleepLengths = resultsArray.Select(r => (r.SleepEnd - r.SleepStart) * utilityFunctions.TIME_DELTA).ToArray();

            int nSize = sleepLengths.Length > 11 ? 10 : 1;
            List<SleepEpisode> filteredResults = new List<SleepEpisode>();

            while (true)
            {
                var mask = sleepLengths
                    .Select((length, index) => new { length, index })
                    .Where(x => x.length > minSleepHours * (1.0 - percent) && x.length < maxSleepHours * (1.0 + percent))
                    .Select(x => x.index)
                    .ToArray();

                filteredResults = mask.Select(i => resultsArray[i]).ToList();
                percent += UtilityFunctions.PERCENT_DELTA;

                if (filteredResults.Count >= nSize)
                {
                    break;
                }
            }

            return filteredResults;
        }

        /// <summary>
        /// Calculates the circadian component for the given time values.
        /// </summary>
        /// <param name="t">The time values.</param>
        /// <param name="p">The phase value.</param>
        /// <param name="cm">The constant component.</param>
        /// <param name="ca">The amplitude component.</param>
        /// <returns>The circadian component.</returns>
        public double[] CircadianComponent(double[] t, double p = 16.8, double cm = 0.0, double ca = 2.5)
        {
            return t.Select(val => cm + ca * Math.Cos(2 * Math.PI / utilityFunctions.CIRCADIAN_LENGTH * (val - p))).ToArray();
        }

        /// <summary>
        /// Finds a nap within the specified time range.
        /// </summary>
        /// <param name="tBegin">The beginning time of the range.</param>
        /// <param name="tEnd">The ending time of the range.</param>
        /// <param name="tDelta">The time interval.</param>
        /// <param name="phase">The phase value.</param>
        /// <param name="g">The g value.</param>
        /// <param name="sw">The sw value.</param>
        /// <returns>A tuple containing the homeostatic and circadian components of the nap.</returns>
        public (double[] h, double[] circadians)? FindANap(
          double tBegin, double tEnd, double tDelta, double phase, double g, double sw)
        {
            int numPoints = (int)((tEnd - tBegin) / tDelta + 1);                                              
            double[] timeInterval = utilityFunctions.Linspace(tBegin, tEnd, numPoints).ToArray();
            double[] circadians = CircadianComponent(timeInterval.Select(t => t - tBegin).ToArray(), p: phase - tBegin);
            double[] invCircadians = CircadianComponent(timeInterval.Select(t => t - tBegin).ToArray(), p: phase - tBegin, cm: 0, ca: 0.5);
            invCircadians = invCircadians.Select(val => -val + sw).ToArray();
            double[] hw = timeInterval.Select(t => utilityFunctions.HomeostaticAwakeComponent(t - tBegin, sw)).ToArray();

            double slope = 0.25, constant = 6.25;
            double napBegin = slope * tEnd + constant;
            double ss = hw[Array.FindIndex(timeInterval, t => t >= napBegin)];

            double[] hs = timeInterval.Select(t => t < napBegin ? double.NaN : HomeostaticNapComponent(t - napBegin, ss)).ToArray();

            if (hs.All(double.IsNaN))
            {
                return null;
            }

            int index = -1;
            for (int i = 0; i < hs.Length && i < invCircadians.Length; i++)
            {
                if (hs[i] >= invCircadians[i])
                {
                    index = i;
                    break;
                }
            }
            double napEnd = timeInterval[index];
            int napBeginIdx = Array.FindIndex(timeInterval, t => t >= napBegin);
            int napEndIdx = Array.FindIndex(timeInterval, t => t >= napEnd);

            double[] h1 = hw.Take(napBeginIdx).Concat(hs.Skip(napBeginIdx).Take(napEndIdx - napBeginIdx)).ToArray();
            return (h1, circadians.Take(napEndIdx).ToArray());
        }

        /// <summary>
        /// Sets the initial datetime value with or without a nap based on the given begin date.
        /// </summary>
        /// <param name="beginDate">The begin date.</param>
        /// <returns>A tuple containing the initial datetime, homeostatic component, and circadian component.</returns>
        public Tuple<DateTime, double[], double[]> SetInitialDatetimeWithNap(DateTime beginDate)
        {
            double SLOPE = 0.2, CONSTANT = 5.5;
            // If the commute begins after 3 PM
            if (beginDate.TimeOfDay >= new TimeSpan(15, 0, 0)) // 3:00 PM
            {
                // Assume the initial time is 8 AM on the same date
                DateTime initialDatetime = beginDate.Date.Add(new TimeSpan(8, 0, 0));

                // Calculate homeostatic values with a nap
                double tEnd = utilityFunctions.DateTimeTimeToDecimal(beginDate);
                var nap = FindANap(8, tEnd, 10.0 / 60.0, utilityFunctions.INITIAL_PHASE, -0.3813, 13.9);

                return new Tuple<DateTime, double[], double[]>(initialDatetime, nap.Value.h, nap.Value.circadians);
            }
            // If the commute begins before 7 AM
            else if (beginDate.TimeOfDay <= utilityFunctions.AWAKE_TIME)
            {
                // Subtract offset from the commute begin time
                DateTime initialDatetime = beginDate - utilityFunctions.AWAKE_OFFSET;
                return new Tuple<DateTime, double[], double[]>(initialDatetime, new double[0], new double[0]);
            }
            // If the commute begins between 7 AM and 3 PM
            else
            {
                // Apply a linear function for the initial wake time
                double t = CONSTANT + SLOPE * utilityFunctions.DateTimeTimeToDecimal(beginDate);
                int hours = (int)t;
                int minutes = (int)Math.Floor((t - hours) * 6) * 10; // Round to the nearest 10 minutes

                DateTime initialDatetime = beginDate.Date.Add(new TimeSpan(hours, minutes, 0));
                return new Tuple<DateTime, double[], double[]>(initialDatetime, new double[0], new double[0]);
            }
        }

        /// <summary>
        /// Finds the sleep episodes using Numba.
        /// </summary>
        /// <param name="beginIdx">The beginning index.</param>
        /// <param name="endIdx">The ending index.</param>
        /// <param name="nextIdx">The next index.</param>
        /// <param name="lower">The lower threshold.</param>
        /// <param name="upper">The upper threshold.</param>
        /// <param name="phase">The phase.</param>
        /// <param name="sw">The sw value.</param>
        /// <returns>The sleep episode.</returns>
        public SleepEpisode FindSleepEpisodesNumba(int beginIdx, int endIdx, int nextIdx, double lower, double upper, double phase, double sw)
        {
            bool isAwake = true;
            double homeostatic = 0;
            double tw = 0, ts = 0;
            double initialTime = beginIdx * utilityFunctions.TIME_DELTA;
            double[] timeIntervals = GenerateTimeIntervals(beginIdx, nextIdx);
            // Compute circadian components
            double[] moddedTimeIntervals = timeIntervals.Select(t => t % utilityFunctions.CIRCADIAN_LENGTH).ToArray();
            double[] circadiansAll = CircadianComponentNumba(moddedTimeIntervals, phase - initialTime);
            List<double> circadians = new List<double>();
            List<double> homeostatics = new List<double>();

            int sleepStart = 0, sleepEnd = 0;
            double ss = 0, alertness = 0;

            for (int idx = beginIdx; idx < nextIdx; idx++)
            {
                double circadian = circadiansAll[idx - beginIdx];

                if (idx <= endIdx)
                {
                    homeostatic = utilityFunctions.HomeostaticAwakeComponentNumba(tw, sw);
                    alertness = homeostatic + circadian;
                    tw += utilityFunctions.TIME_DELTA;
                }
                else
                {
                    if (isAwake)
                    {
                        homeostatic = utilityFunctions.HomeostaticAwakeComponentNumba(tw, sw);
                        tw += utilityFunctions.TIME_DELTA;
                        ss = homeostatic;
                        ts = 0;
                    }
                    else
                    {
                        homeostatic = utilityFunctions.HomeostaticSleepComponentNumba(ts, ss);
                        ts += utilityFunctions.TIME_DELTA;
                        sw = homeostatic;
                        tw = 0;
                    }
                    alertness = homeostatic + circadian;

                    if (alertness <= lower && isAwake)
                    {
                        sleepStart = idx;
                        isAwake = false;
                    }

                    if (alertness >= upper && !isAwake)
                    {
                        sleepEnd = idx;
                        isAwake = true;
                    }
                }

                circadians.Add(circadian);
                homeostatics.Add(homeostatic);
            }

            if (sleepEnd != 0)
            {
                double[] circadiansArray = circadians.Take(sleepEnd - beginIdx).ToArray();
                double[] homeostaticsArray = homeostatics.Take(sleepEnd - beginIdx).ToArray();
                homeostatic = homeostaticsArray.Last();
                alertness = circadiansArray.Last() + homeostatic;

                return new SleepEpisode(lower, upper, phase, sleepStart, sleepEnd, homeostatic, alertness, circadiansArray, homeostaticsArray);
            }

            return null;
        }
        
        /// <summary>
        /// Finds the sleep episode.
        /// </summary>
        /// <param name="beginIdx">The beginning index.</param>
        /// <param name="endIdx">The ending index.</param>
        /// <param name="lower">The lower threshold.</param>
        /// <param name="upper">The upper threshold.</param>
        /// <param name="sw">The sw value.</param>
        /// <param name="tDelta">The tDelta value.</param>
        /// <param name="c">The c value.</param>
        /// <returns>The sleep episode.</returns>
        public (bool isAwake, double h, int idxSleep, int idx) FindSleepEpisode(
        int beginIdx, int endIdx, double lower, double upper, double sw, double tDelta, double[] c)
        {
            double tw = 0;
            double ts = 0;
            double ss = 0;
            bool isAwake = true;
            int idxSleep = 0;

            for (int idx = beginIdx; idx < endIdx; idx++)
            {
                double h;
                if (isAwake)
                {
                    h = utilityFunctions.HomeostaticAwakeComponent(tw, sw);
                    tw += tDelta;
                    ss = h; // last value used as the first for sleep
                }
                else
                {
                    h = utilityFunctions.HomeostaticSleepComponent(ts, ss);
                    ts += tDelta;
                }

                double a = h + c[idx];
                if (a <= lower && isAwake)
                {
                    idxSleep = idx;
                    isAwake = false;
                }
                if (a >= upper && !isAwake)
                {
                    return (true, h, idxSleep, idx);
                }
            }

            return (false, 0, -1, -1); // Return default values if no sleep episode found
        }

        /// <summary>
        /// Computes the circadian component using Numba.
        /// </summary>
        /// <param name="t">The time intervals.</param>
        /// <param name="p">The p value.</param>
        /// <param name="cm">The cm value.</param>
        /// <param name="ca">The ca value.</param>
        /// <returns>The circadian component.</returns>
        public double[] CircadianComponentNumba(double[] t, double p = 16.8, double cm = 0.0, double ca = 2.5)
        {
            double[] result = t.Select(time => cm + ca * Math.Cos(2 * Math.PI / utilityFunctions.CIRCADIAN_LENGTH * (time - p))).ToArray();
            return result;
        }

        /// <summary>
        /// Computes the homeostatic nap component.
        /// </summary>
        /// <param name="t">The t value.</param>
        /// <param name="ss">The ss value.</param>
        /// <param name="ha">The ha value.</param>
        /// <param name="g">The g value.</param>
        /// <returns>The homeostatic nap component.</returns>
        public double HomeostaticNapComponent(double t, double ss, double ha = 14.3, double g = -0.3813)
        {
            return ha - (ha - ss) * Math.Exp(g * t);
        }        

        /// <summary>
        /// Generates the time intervals.
        /// </summary>
        /// <param name="beginIdx">The beginning index.</param>
        /// <param name="nextIdx">The next index.</param>
        /// <returns>The time intervals.</returns>
        public double[] GenerateTimeIntervals(int beginIdx, int nextIdx)
        {
            return Enumerable.Range(0, nextIdx - beginIdx)
                                   .Select(i => i * utilityFunctions.TIME_DELTA)
                                   .ToArray();
        }

        /// <summary>
        /// Gets the DateTime range between the begin and end dates.
        /// </summary>
        /// <param name="beginDate">The begin date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The DateTime range.</returns>
        public List<DateTime> GetDatetimeRange(DateTime beginDate, DateTime endDate)
        {
            List<DateTime> datetimeRange = new List<DateTime>();
            for (DateTime dt = beginDate; dt < endDate; dt = dt.AddMinutes(UtilityFunctions.INTERVAL_FREQUENCY_MINUTE))
            {
                datetimeRange.Add(dt);
            }
            return datetimeRange;
        }

        /// <summary>
        /// Generates the DateTime range between the begin and end dates.
        /// </summary>
        /// <param name="beginDate">The begin date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The DateTime range.</returns>
        public List<DateTime> GenerateDateTimeRange(DateTime beginDate, DateTime endDate)
        {
            List<DateTime> datetimeRange = new List<DateTime>();

            // Calculate the time interval using TimeSpan
            TimeSpan timeInterval = TimeSpan.FromMinutes(UtilityFunctions.N_DELTA * UtilityFunctions.INTERVAL_FREQUENCY_MINUTE);

            // Add dates in the range
            for (DateTime current = beginDate; current < endDate; current = current.Add(timeInterval))
            {
                datetimeRange.Add(current);
            }

            return datetimeRange;           
        }
    }
}
