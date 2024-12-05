using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EasyJet.FRAMModel.SleepWake.Helpers
{
    internal class UtilityFunctions
    {
        // Thresholds from the 2014 Michael Ingre et al. paper
        /// <summary>
        /// The lower threshold for sleep.
        /// </summary>
        public const double SLEEP_LOWER_THRESHOLD = 7.0;

        /// <summary>
        /// The upper threshold for sleep.
        /// </summary>
        public const double SLEEP_UPPER_THRESHOLD = 8.0;

        /// <summary>
        /// The lower threshold for wake.
        /// </summary>
        public const double WAKE_LOWER_THRESHOLD = 13.0;

        /// <summary>
        /// The upper threshold for wake.
        /// </summary>
        public const double WAKE_UPPER_THRESHOLD = 14.0;

        /// <summary>
        /// The frequency of intervals in minutes.
        /// </summary>
        public const int INTERVAL_FREQUENCY_MINUTE = 10;

        /// <summary>
        /// The number of delta values.
        /// </summary>
        public const int N_DELTA = 3;

        /// <summary>
        /// The percent delta used to increment while applying similarity checks.
        /// </summary>
        public const double PERCENT_DELTA = 0.005;

        /// <summary>
        /// The number of time ticks for plotting purposes.
        /// </summary>
        public const int TIME_TICKS = 6;

        /// <summary>
        /// The initial phase value.
        /// </summary>
        public double INITIAL_PHASE = 16.2;

        /// <summary>
        /// The initial sleep value.
        /// </summary>
        public const int INITIAL_SLEEP = 9;

        /// <summary>
        /// The initial homeoawake value.
        /// </summary>
        public const double INITIAL_HOMEO = 14.0;

        /// <summary>
        /// Determines whether to plot consecutive values.
        /// </summary>
        public const bool IS_PLOT_CONSECUTIVE = true;

        /// <summary>
        /// Determines whether to plot components.
        /// </summary>
        public const bool IS_PLOT_COMPONENTS = true;

        /// <summary>
        /// Determines whether to plot actogram.
        /// </summary>
        public const bool IS_PLOT_ACTOGRAM = true;

        /// <summary>
        /// The long sleepers time span.
        /// </summary>
        public TimeSpan LONG_SLEEPERS;

        /// <summary>
        /// The average sleepers time span.
        /// </summary>
        public TimeSpan AVERAGE_SLEEPERS;

        /// <summary>
        /// The short sleepers time span.
        /// </summary>
        public TimeSpan SHORT_SLEEPERS;

        /// <summary>
        /// The one cycle sleep time span.
        /// </summary>
        public TimeSpan ONE_CYCLE_SLEEP;

        /// <summary>
        /// The minimum sleep time span.
        /// </summary>
        public TimeSpan MIN_SLEEP_TIME;

        /// <summary>
        /// The minimum one day off time span.
        /// </summary>
        public TimeSpan MIN_ONE_DAY_OFF;

        /// <summary>
        /// The minimum rest time span.
        /// </summary>
        public TimeSpan MIN_REST_TIME;

        /// <summary>
        /// The interval per minute value.
        /// </summary>
        public readonly double INTERVAL_PER_MINUTE;

        /// <summary>
        /// The interval per hour value.
        /// </summary>
        public readonly int INTERVAL_PER_HOUR;

        /// <summary>
        /// The interval per day value.
        /// </summary>
        public readonly int INTERVAL_PER_DAY;

        /// <summary>
        /// The circadian length value.
        /// </summary>
        public readonly double CIRCADIAN_LENGTH;

        /// <summary>
        /// The time delta value.
        /// </summary>
        public readonly double TIME_DELTA;

        /// <summary>
        /// The awake offset time span.
        /// </summary>
        public TimeSpan AWAKE_OFFSET;

        /// <summary>
        /// The awake time span.
        /// </summary>
        public readonly TimeSpan AWAKE_TIME;

        /// <summary>
        /// The list of contactables.
        /// </summary>
        public readonly List<string> CONTACTABLES;

        /// <summary>
        /// The list of standbys.
        /// </summary>
        public readonly List<string> STANDBYS;

        /// <summary>
        /// The list of thresholds.
        /// </summary>
        public List<(double, double, double)> THRESHOLDS;

        /// <summary>
        /// The crew ID.
        /// </summary>
        public readonly string CREW_ID;

        /// <summary>
        /// The list of date formats.
        /// </summary>
        public readonly List<string> DATE_FORMATS;

        /// <summary>
        /// Utility functions for sleep and wake calculations.
        /// </summary>
        public UtilityFunctions()
        {
            LONG_SLEEPERS = new TimeSpan(9, 0, 0);      // 9 hours, 0 minutes, 0 seconds
            AVERAGE_SLEEPERS = new TimeSpan(7, 30, 0);  // 7 hours, 30 minutes, 0 seconds
            SHORT_SLEEPERS = new TimeSpan(6, 0, 0);     // 6 hours, 0 minutes, 0 seconds
            ONE_CYCLE_SLEEP = new TimeSpan(1, 30, 0);   // 1 hour, 30 minutes, 0 seconds
            MIN_SLEEP_TIME = SHORT_SLEEPERS;
            MIN_ONE_DAY_OFF = new TimeSpan(36, 0, 0);    // 36 hours, 0 minutes, 0 seconds
            MIN_REST_TIME = new TimeSpan(8, 0, 0);      // 10 hours, 0 minutes, 0 seconds
            INTERVAL_PER_MINUTE = 1.0 / INTERVAL_FREQUENCY_MINUTE;
            INTERVAL_PER_HOUR = (int)(60 * INTERVAL_PER_MINUTE);
            INTERVAL_PER_DAY = 24 * INTERVAL_PER_HOUR;
            CIRCADIAN_LENGTH = 24.0;  // Placeholder value; replace with actual value if different
            TIME_DELTA = CIRCADIAN_LENGTH / INTERVAL_PER_DAY;
            AWAKE_OFFSET = new TimeSpan(0, 30, 0);  // 0 hours, 30 minutes, 0 seconds
            AWAKE_TIME = new TimeSpan(7, 0, 0);     // 7 hours, 0 minutes, 0 seconds
            THRESHOLDS = null;
            CREW_ID = null;
            CONTACTABLES = new List<string>
            {
                "CTB", "EXPT", "HCTB", "HCT", "HCTD", "HCTY", "IDCT",
                "TCBA", "NFID", "TCBP", "TCTB", "UNCT"
            };
            STANDBYS = new List<string>
            {
                "ADRS", "ADTY", "ASBY", "ASRS", "CSBE", "CSBL", "CSBY",
                "ESBY", "HSBY", "LSBY", "MSBY", "R21S", "SBE", "SBL",
                "SBM", "SBY", "TSBY"
            };
            DATE_FORMATS = new List<string> { "dd/MM/yyyy", "yyyy-MM-dd" };
        }

        /// <summary>
        /// Converts a DateTime object to a decimal representation of time.
        /// </summary>
        /// <param name="t">The DateTime object to convert.</param>
        /// <returns>The decimal representation of time.</returns>
        public double DateTimeToDecimal(DateTime t)
        {
            int h = t.Hour;//% 24; // Hours of the day
            int m = t.Minute;// % 60; // Minutes of the hour

            return h + m / 60.0;
        }

        /// <summary>
        /// Converts a DateTime object to a decimal representation of time.
        /// </summary>
        /// <param name="t">The DateTime object to convert.</param>
        /// <returns>The decimal representation of time.</returns>
        public double DateTimeTimeToDecimal(DateTime t)
        {
            int h = t.Hour;
            int m = t.Minute;
            return h + m / 60.0;
        }

        /// <summary>
        /// Calculates the homeostatic awake component.
        /// </summary>
        /// <param name="t">The time value.</param>
        /// <param name="sw">The sleep-wake value.</param>
        /// <param name="la">The lower asymptote value. Default is 2.4.</param>
        /// <param name="d">The decay constant value. Default is -0.0353.</param>
        /// <returns>The homeostatic awake component.</returns>
        public double HomeostaticAwakeComponent(double t, double sw, double la = 2.4, double d = -0.0353)
        {
            return la + (sw - la) * Math.Exp(d * t);
        }

        /// <summary>
        /// Calculates the homeostatic awake component for an array of time values.
        /// </summary>
        /// <param name="t">The array of time values.</param>
        /// <param name="sw">The sleep-wake value.</param>
        /// <param name="la">The lower asymptote value. Default is 2.4.</param>
        /// <param name="d">The decay constant value. Default is -0.0353.</param>
        /// <returns>An array of homeostatic awake components.</returns>
        public double[] HomeostaticAwakeComponent(double[] t, double sw, double la = 2.4, double d = -0.0353)
        {
            return t.Select(time => la + (sw - la) * Math.Exp(d * time)).ToArray();
        }

        /// <summary>
        /// Calculates the homeostatic sleep component.
        /// </summary>
        /// <param name="t">The time value.</param>
        /// <param name="ss">The sleep stability value.</param>
        /// <param name="ha">The higher asymptote value. Default is 14.3.</param>
        /// <param name="bl">The baseline value. Default is 12.2.</param>
        /// <param name="g">The gain value. Default is -0.3813.</param>
        /// <returns>The homeostatic sleep component.</returns>
        public double HomeostaticSleepComponent(double t, double ss, double ha = 14.3, double bl = 12.2, double g = -0.3813)
        {
            double result = ss + t * g * (bl - ha);
            if (result <= bl)
            {
                return result;
            }
            return ha - (ha - bl) * Math.Exp(g * (t - (bl - ss) / (g * (bl - ha))));
        }
        /// <summary>
        /// Sets the initial DateTime based on the given beginDate.
        /// </summary>
        /// <param name="beginDate">The begin date.</param>
        /// <returns>The initial DateTime.</returns>
        public DateTime SetInitialDateTime(DateTime beginDate)
        {
            TimeSpan beginTime = beginDate.TimeOfDay;
            TimeSpan threePM = new TimeSpan(15, 0, 0);

            if (beginTime >= threePM)
            {
                // Commute begins after 3pm, assume 8am due to the last non-duty end around 7am-8am
                DateTime initialDateTime = new DateTime(beginDate.Date.Year, beginDate.Date.Month, beginDate.Date.Day, 8, 0, 0);
                return initialDateTime;
            }
            else if (beginTime <= AWAKE_TIME)
            {
                // Commute begins before 7am, subtract offset from commute begin time
                return beginDate - AWAKE_OFFSET;
            }
            else
            {
                // Commute begins between 7am and 3pm, applying linear function for the initial wake time
                double slope = 0.2, constant = 5.5;
                double t = constant + slope * DateTimeTimeToDecimal(beginDate);
                int h = (int)t;
                int m = (int)Math.Floor((t - h) * 6) * 10; // 10 min steps

                DateTime initialDateTime = new DateTime(beginDate.Date.Year, beginDate.Date.Month, beginDate.Date.Day, h, m, 0);
                return initialDateTime;
            }
        }

        /// <summary>
        /// Calculates the Homeostatic Awake Component Numba.
        /// </summary>
        /// <param name="t">The time.</param>
        /// <param name="sw">The sleep/wake value.</param>
        /// <param name="la">The lower asymptote.</param>
        /// <param name="d">The decay constant.</param>
        /// <returns>The Homeostatic Awake Component Numba.</returns>
        public double HomeostaticAwakeComponentNumba(double t, double sw, double la = 2.4, double d = -0.0353)
        {
            return la + (sw - la) * Math.Exp(d * t);
        }

        /// <summary>
        /// Calculates the Homeostatic Sleep Component Numba.
        /// </summary>
        /// <param name="t">The time.</param>
        /// <param name="ss">The sleep/sleep value.</param>
        /// <param name="ha">The higher asymptote.</param>
        /// <param name="bl">The baseline.</param>
        /// <param name="g">The growth constant.</param>
        /// <returns>The Homeostatic Sleep Component Numba.</returns>
        public double HomeostaticSleepComponentNumba(double t, double ss, double ha = 14.3, double bl = 12.2, double g = -0.3813)
        {
            double result = ss + t * g * (bl - ha);
            if (result <= bl) return result;
            double bt = (bl - ss) / (g * (bl - ha));
            return ha - (ha - bl) * Math.Exp(g * (t - bt));
        }


        /// <summary>
        /// Calculates the circadian component.
        /// </summary>
        /// <param name="t">The time.</param>
        /// <param name="p">The phase.</param>
        /// <param name="cm">The constant component.</param>
        /// <param name="ca">The amplitude component.</param>
        /// <returns>The circadian component.</returns>
        public double CircadianComponent(double t, double p = 16.8, double cm = 0.0, double ca = 2.5)
        {
            return cm + ca * Math.Cos(2 * Math.PI / CIRCADIAN_LENGTH * (t - p));
        }

        /// <summary>
        /// Finds a sleep episode.
        /// </summary>
        /// <param name="beginIdx">The beginning index.</param>
        /// <param name="endIdx">The ending index.</param>
        /// <param name="lower">The lower threshold.</param>
        /// <param name="upper">The upper threshold.</param>
        /// <param name="sw">The sleep/wake value.</param>
        /// <param name="tDelta">The time delta.</param>
        /// <param name="c">The array of values.</param>
        /// <returns>A tuple containing the sleep episode information.</returns>
        public (bool, double, int, int) FindASleepEpisodeNumba(int beginIdx, int endIdx, double lower, double upper, double sw, double tDelta, double[] c)
        {
            double tw = 0, ts = 0;
            bool isAwake = true;
            int idxSleep = 0;
            double h = 0, ss = 0;  // Initial values for h and ss
            int idx = 0;
            for (idx = beginIdx; idx < endIdx; idx++)
            {
                if (isAwake)
                {
                    h = HomeostaticAwakeComponentNumba(tw, sw);  // Assuming this function exists
                    tw += tDelta;
                    ss = h;  // Last value used as the first for sleep
                }
                else
                {
                    h = HomeostaticSleepComponentNumba(ts, ss);  // Assuming this function exists
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
                    isAwake = true;
                    break;
                }
            }

            return (isAwake, h, idxSleep, idx);
        }

        /// <summary>
        /// Generates a list of evenly spaced values.
        /// </summary>
        /// <param name="start">The start value.</param>
        /// <param name="end">The end value.</param>
        /// <param name="num">The number of values.</param>
        /// <returns>A list of evenly spaced values.</returns>
        public List<double> Linspace(double start, double end, int num)
        {
            List<double> result = new List<double>(num);

            double step = (end - start) / (num - 1);
            step = double.NaN.Equals(step) ? 0 : step;
            for (int i = 0; i < num; i++)
            {
                result.Add(start + i * step);
            }
            return result;
        }

        /// <summary>
        /// Finds the next sleep episode.
        /// </summary>
        /// <param name="data">The list of data arrays.</param>
        /// <param name="end_idx">The ending index.</param>
        /// <param name="t_delta">The time delta.</param>
        /// <param name="c">The array of values.</param>
        /// <param name="paramsList">The list of parameter tuples.</param>
        /// <returns>A jagged array containing the sleep episode information.</returns>
        public double[][] FindNextSleepEpisodeNumba(List<double[]> data, int end_idx, double t_delta, double[] c, List<Tuple<double, double>> paramsList)
        {
            List<double[]> results = new List<double[]>();

            foreach (var param in paramsList)
            {
                double lower = param.Item1;
                double upper = param.Item2;

                foreach (var x in data)
                {
                    // Call the C# equivalent of find_a_sleep_episode_numba
                    var (isAwake, h, sleepIdx, wakeIdx) = FindASleepEpisodeNumba((int)x[x.Length - 1], end_idx, lower, upper, x[x.Length - 3], t_delta, c);

                    if (isAwake && h >= 13)
                    {
                        // Create a new array appending the extra values (lower, upper, h, sleepIdx, wakeIdx)
                        double[] newX = new double[x.Length + 5];
                        Array.Copy(x, newX, x.Length);  // Copy original array to new one

                        // Append the additional values
                        newX[x.Length] = lower;
                        newX[x.Length + 1] = upper;
                        newX[x.Length + 2] = h;
                        newX[x.Length + 3] = sleepIdx;
                        newX[x.Length + 4] = wakeIdx;

                        // Add the new array to the results list
                        results.Add(newX);
                    }
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// Interpolates missing (NaN) values in an array using linear interpolation.
        /// </summary>
        /// <param name="arr">The input array containing values and NaNs.</param>
        /// <returns>A new array with interpolated values replacing NaNs.</returns>
        public double[] InterpolateData(double[] arr)
        {
            int n = arr.Length;
            double[] indexes = Enumerable.Range(0, n).Select(i => (double)i).ToArray();

            // Extract valid indexes and values
            double[] validIndexes = indexes.Where(i => !double.IsNaN(arr[(int)i])).ToArray();
            double[] validValues = validIndexes.Select(i => arr[(int)i]).ToArray();

            // Interpolate the full array
            return LinearInterpolate(validIndexes, validValues, indexes);
        }

        /// <summary>
        /// Performs linear interpolation or extrapolation over a given set of points.
        /// </summary>
        /// <param name="x">The array of valid x-coordinates (e.g., indexes).</param>
        /// <param name="y">The array of valid y-coordinates (corresponding values).</param>
        /// <param name="newX">The array of x-coordinates for which to compute interpolated values.</param>
        /// <returns>An array of interpolated values corresponding to <paramref name="newX"/>.</returns>
        public double[] LinearInterpolate(double[] x, double[] y, double[] newX)
        {
            double[] interpolatedY = new double[newX.Length];
            for (int i = 0; i < newX.Length; i++)
            {
                double xVal = newX[i];

                // Check bounds for xVal
                if (xVal <= x[0])
                {
                    // Extrapolate using the first interval
                    interpolatedY[i] = y[0] + (y[1] - y[0]) * (xVal - x[0]) / (x[1] - x[0]);
                }
                else if (xVal >= x[x.Length - 1])
                {
                    // Extrapolate using the last interval
                    interpolatedY[i] = y[y.Length - 1] + (y[y.Length - 1] - y[y.Length - 2]) * (xVal - x[x.Length - 1]) / (x[x.Length - 1] - x[x.Length - 2]);
                }
                else
                {
                    // Find the interval [x[j], x[j+1]] where xVal lies
                    int j = Array.FindIndex(x, val => val > xVal) - 1;

                    double x0 = x[j], x1 = x[j + 1];

                    double y0;
                    double y1;

                    if (y.Length < x.Length && y.Length <= j + 1)
                    {
                        y0 = y[j]; y1 = y[y.Length - 1];
                    }
                    else
                    {
                        y0 = y[j]; y1 = y[j + 1];
                    }

                    // Perform linear interpolation
                    interpolatedY[i] = y0 + (y1 - y0) * (xVal - x0) / (x1 - x0);
                }
            }
            return interpolatedY;
        }

        /// <summary>
        /// Gets the time indexes.
        /// </summary>
        /// <param name="datetimeRange">The datetime range.</param>
        /// <param name="timeValues">The time values.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The time indexes.</returns>
        public int[] GetTimeIndexes(DateTime[] datetimeRange, DateTime[] timeValues, TimeSpan offset = default)
        {
            // Apply offset to timeValues
            var adjustedTimeValues = timeValues.Select(t => t + offset).ToArray();

            // Get indexes
            var indexes = new List<int>();
            foreach (var timeValue in adjustedTimeValues)
            {
                // Binary search to find the insertion index
                int index = Array.BinarySearch(datetimeRange, timeValue);
                if (index < 0)
                {
                    index = ~index; // Bitwise complement to get insertion index
                }
                indexes.Add(index);
            }

            return indexes.ToArray();
        }

        /// <summary>
        /// Gets the begin and end dates from the DataTable.
        /// </summary>
        /// <param name="dt">The DataTable.</param>
        /// <param name="cols">The column names.</param>
        /// <returns>The row count, begin date, end date, minimum rest time, and commute duration.</returns>
        public (int rowCount, DateTime beginDate, DateTime endDate, TimeSpan minRestTime, TimeSpan commuteDuration) GetBeginEndDates(DataTable dt, string[] cols = null)
        {
            if (cols == null) cols = new[] { "CommuteBegin", "CommuteEnd" };

            if (dt.Rows.Count == 0)
            {
                throw new ArgumentException("DataTable is empty.");
            }

            // Ensure the columns exist
            if (!dt.Columns.Contains(cols[0]) || !dt.Columns.Contains(cols[1]) || !dt.Columns.Contains("MinRestTime"))
            {
                throw new ArgumentException("Required columns are missing in the DataTable.");
            }

            DateTime beginDate = DateTime.Parse(dt.Rows[0][cols[0]].ToString());
            DateTime endDate = DateTime.Parse(dt.Rows[dt.Rows.Count - 1][cols[1]].ToString());
            string restTime = dt.Rows[dt.Rows.Count - 1]["MinRestTime"].ToString();
            TimeSpan minRestTime = TimeSpan.Parse(restTime);

            // Calculate the duration of the commute
            DateTime firstBeginDate = DateTime.Parse(dt.Rows[dt.Rows.Count - 1][cols[0]].ToString());
            DateTime lastEndDate = DateTime.Parse(dt.Rows[dt.Rows.Count - 1][cols[1]].ToString());
            TimeSpan commuteDuration = lastEndDate - firstBeginDate;

            // Return results
            return (dt.Rows.Count, beginDate, endDate, minRestTime, commuteDuration);
        }

        /// <summary>
        /// Merges rows in the DataTable.
        /// </summary>
        /// <param name="dt">The DataTable.</param>
        /// <param name="idx">The index.</param>
        /// <returns>The merged DataTable.</returns>
        public DataTable MergeRows(DataTable dt, int idx)
        {
            // Merging rows
            dt.Rows[idx]["EndDate"] = dt.Rows[idx + 1]["EndDate"];
            dt.Rows[idx]["EndTime"] = dt.Rows[idx + 1]["EndTime"];
            dt.Rows[idx]["DutyEnd"] = dt.Rows[idx + 1]["DutyEnd"];

            dt.Rows[idx]["SectorCount"] = Convert.ToInt32(dt.Rows[idx]["SectorCount"]) + Convert.ToInt32(dt.Rows[idx + 1]["SectorCount"]);

            // Compare and assign the maximum commute time
            TimeSpan commuteTime1 = DateTime.Parse(dt.Rows[idx]["CommuteTime"].ToString()).TimeOfDay;
            TimeSpan commuteTime2 = DateTime.Parse(dt.Rows[idx + 1]["CommuteTime"].ToString()).TimeOfDay;
            dt.Rows[idx]["CommuteTime"] = commuteTime1 > commuteTime2 ? commuteTime1 : commuteTime2;

            // Drop the row at idx + 1
            dt.Rows.RemoveAt(idx + 1);

            return dt;
        }

        /// <summary>
        /// Fills null values with "None" in the specified column of the DataTable.
        /// </summary>
        /// <param name="dt">The DataTable.</param>
        /// <param name="columnName">The column name.</param>
        private void FillNaWithNone(DataTable dt, string columnName)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row.IsNull(columnName))
                {
                    row[columnName] = "None";
                }
            }
        }

        /// <summary>
        /// Estimates the commute time based on the base time.
        /// </summary>
        /// <param name="t">The base time.</param>
        /// <returns>The estimated commute time.</returns>
        public TimeSpan EstimatedCommuteTime(TimeSpan t)
        {
            TimeSpan baseTime = t;
            double additionalMinutes = Math.Round(Math.Pow(0.03 * baseTime.TotalMinutes, 1.45));
            return baseTime.Add(TimeSpan.FromMinutes(additionalMinutes));
        }

        /// <summary>
        /// Rounds the DateTime to the nearest minute.
        /// </summary>
        /// <param name="dateTime">The DateTime.</param>
        /// <param name="minuteInterval">The minute interval.</param>
        /// <returns>The rounded DateTime.</returns>
        public DateTime RoundToNearestMinute(DateTime dateTime, int minuteInterval)
        {
            var totalMinutes = Math.Round(dateTime.TimeOfDay.TotalMinutes / minuteInterval) * minuteInterval;
            return dateTime.Date.AddMinutes(totalMinutes);
        }

        /// <summary>
        /// Converts the given TimeSpan to a TimeSpan with only hours, minutes, and seconds.
        /// </summary>
        /// <param name="t">The TimeSpan to convert.</param>
        /// <returns>A TimeSpan with only hours, minutes, and seconds.</returns>
        public TimeSpan TimedeltaToTime(TimeSpan t)
        {
            return new TimeSpan(t.Hours, t.Minutes, t.Seconds);
        }

        /// <summary>
        /// Generates a list of evenly spaced values within a specified range.
        /// </summary>
        /// <param name="start">The starting value of the sequence.</param>
        /// <param name="stop">The endpoint of the sequence (exclusive).</param>
        /// <param name="step">The increment between consecutive values.</param>
        /// <returns>A list of double values representing the range from start to stop (exclusive), incremented by step.</returns>        
        public double[] ArangeValues(double start, double stop, double step)
        {
            List<double> result = new List<double>();

            for (double value = start; value < stop; value += step)
            {
                result.Add(value);
            }

            return result.ToArray();
        }
    }
}




