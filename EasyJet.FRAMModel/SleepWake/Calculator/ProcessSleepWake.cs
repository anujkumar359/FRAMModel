using EasyJet.FRAMModel.Engine.ExternalContract;
using EasyJet.FRAMModel.SleepWake.Entities;
using EasyJet.FRAMModel.SleepWake.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyJet.FRAMModel.SleepWake.Calculator
{
    /// <summary>
    /// Represents a class that processes sleep and wake data.
    /// </summary>
    internal class ProcessSleepWake
    {
        /// <summary>
        /// Utility functions instance.
        /// </summary>
        private UtilityFunctions util = new UtilityFunctions();

        /// <summary>
        /// Calculates the sleep and wake data based on the provided request and workload score.
        /// </summary>
        /// <param name="request">The FRAM model request.</param>
        /// <param name="workLoadScore">The workload score.</param>
        /// <returns>An array of strings representing the calculated sleep and wake data.</returns>
        public string[] Calculate(IFRMModelRequest request, string[] workLoadScore)
        {
            DataTable dt = new DataTable(); // This line now works
            dt.Columns.Add("RowIndex", typeof(int));
            dt.Columns.Add("StartDate", typeof(DateTime));
            dt.Columns.Add("EndDate", typeof(DateTime));
            dt.Columns.Add("StartTime", typeof(DateTime));
            dt.Columns.Add("EndTime", typeof(DateTime));
            dt.Columns.Add("CommuteTime", typeof(TimeSpan));
            dt.Columns.Add("CrewRoute", typeof(string));
            dt.Columns.Add("SbyCallout", typeof(string));
            dt.Columns.Add("SectorCount", typeof(int));
            dt.Columns.Add("NightStopFlag", typeof(string));
            dt.Columns.Add("DutyID", typeof(int));
            dt.Columns.Add("DutyEnd", typeof(DateTime));
            dt.Columns.Add("DutyBegin", typeof(DateTime));
            dt.Columns.Add("RestTime", typeof(TimeSpan));
            dt.Columns.Add("BlockIDSW", typeof(int));
            dt.Columns.Add("DayInBlock", typeof(int));
            dt.Columns.Add("DayInBlockSW", typeof(int));
            dt.Columns.Add("BlockLength", typeof(int));
            dt.Columns.Add("BlockLengthSW", typeof(int));
            dt.Columns.Add("CommuteEst", typeof(TimeSpan));
            dt.Columns.Add("CommuteBegin", typeof(DateTime));
            dt.Columns.Add("CommuteEnd", typeof(DateTime));
            dt.Columns.Add("MinRestTime", typeof(TimeSpan));
            dt.Columns.Add("DutyBeginDate", typeof(DateTime));
            dt.Columns.Add("DutyEndDate", typeof(DateTime));
            dt.Columns.Add("DutyBeginTime", typeof(TimeSpan));
            dt.Columns.Add("DutyEndTime", typeof(TimeSpan));
            dt.Columns.Add("MorningFinish", typeof(DateTime));
            dt.Columns.Add("EveningFinish", typeof(DateTime));
            dt.Columns.Add("DualFinish", typeof(DateTime));
            dt.Columns.Add("NeutralFinish", typeof(DateTime));
            dt.Columns.Add("DutyType", typeof(string));
            dt.Columns.Add("ejDisruptive", typeof(string));
            dt.Columns.Add("IsContactable", typeof(bool));
            dt.Columns.Add("IsStandby", typeof(bool));
            dt.Columns.Add("FRAMWorkloadScore", typeof(double));

            for (int i = 0; i < request.IdxInBlock.Length; i++)
            {
                dt.Rows.Add();
                DataRow dataRow = dt.Rows[dt.Rows.Count - 1];
                dataRow["RowIndex"] = i + 1;
                dataRow["StartDate"] = Convert.ToDateTime(DateTime.Parse(request.StartDateLocalTime[i], (IFormatProvider)new CultureInfo("en-GB")));
                dataRow["EndDate"] = Convert.ToDateTime(DateTime.Parse(request.EndDateLocalTime[i], (IFormatProvider)new CultureInfo("en-GB")));
                dataRow["StartTime"] = Convert.ToDateTime(DateTime.Parse(request.StartTimeLocalTime[i], (IFormatProvider)new CultureInfo("en-GB"))).ToString("HH:mm");
                dataRow["EndTime"] = Convert.ToDateTime(DateTime.Parse(request.EndTimeLocalTime[i], (IFormatProvider)new CultureInfo("en-GB"))).ToString("HH:mm");
                dataRow["SectorCount"] = request.OperationalSectorCount[i];
                dataRow["SbyCallout"] = request.SbyCallout[i];
                dataRow["CommuteTime"] = request.CommuteTime[i];
                dataRow["CrewRoute"] = request.CrewRoute[i];
                dataRow["NightStopFlag"] = request.NightStopFlag[i];
                dt.Rows[i]["FRAMWorkloadScore"] = workLoadScore[i];
            }

            dt = PreprocessData(dt);

            return ProcessSleepwake(dt);
        }

        /// <summary>
        /// Processes the sleep and wake data based on the provided DataTable and workload score.
        /// </summary>
        /// <param name="dt">The DataTable containing the sleep and wake data.</param>
        /// <param name="workLoadScore">The workload score.</param>
        /// <returns>An array of strings representing the processed sleep and wake data.</returns>
        public string[] ProcessSleepwake(DataTable dt)
        {
            // Group by 'DutyID' and create a list of begin/end datetimes
            var beginEndDates = dt.AsEnumerable()
                .GroupBy(row => row.Field<int>("DutyID"))
                .Select(group => util.GetBeginEndDates(group.CopyToDataTable()))
                .ToList();

            var dutyIds = dt.AsEnumerable()
                .Select(row => row.Field<int>("DutyID"))
                .Distinct()
                .ToList();

            //Stage 1
            Stage1SWCalculator stage1SWCalculator = new Stage1SWCalculator();
            Stage1SWResponse stage1SWResponse = stage1SWCalculator.ProcessConsecutiveDuties(dt, beginEndDates, dutyIds);

            //Stage 2
            Stage2SWHelper stage2Helper = new Stage2SWHelper();
            List<Tuple<int, int>> nonDutyIndexes = new List<Tuple<int, int>>();

            nonDutyIndexes = stage2Helper.GetNanIndexes(stage1SWResponse.Alertnesses);

            Stage2SWCalculator stage2SWCalculator = new Stage2SWCalculator();

            Stage2SWResponse stage2SWResponse = stage2SWCalculator.ProcessNonDutyCircadians(stage1SWResponse, nonDutyIndexes, beginEndDates);

            //Stage 3
            Stage3SWCalculator stage3SWCalculator = new Stage3SWCalculator();
            Stage3SWResponse stage3SWResponse = stage3SWCalculator.ProcessNonDuties(stage1SWResponse.TotalDatetimeRange, beginEndDates, nonDutyIndexes, stage2SWResponse.LastSwVals, stage2SWResponse.CVals);

            //Stage 4
            Stage4SWCalculator stage4SWCalculator = new Stage4SWCalculator();
            Stage4SWResponse stage4SWResponse = stage4SWCalculator.ProcessNonDutyHomeostatics(stage1SWResponse.TotalDatetimeRange, stage3SWResponse.NonDutySleeps, nonDutyIndexes, stage2SWResponse.LastSwVals, stage1SWResponse.Homeostatics);

            if (beginEndDates.Count == 1)
            {
                List<DateTime> t = stage1SWResponse.TotalDatetimeRange;
                List<double> h = stage1SWResponse.Homeostatics;
                List<double> c = stage1SWResponse.Circadians;
            }
            else
            {
                if (stage2SWResponse.CircadiansWoNan.Any(x => double.IsNaN(x)))
                {
                    stage2SWResponse.CircadiansWoNan = util.InterpolateData(stage2SWResponse.CircadiansWoNan);
                }
                if (stage4SWResponse.HomeostaticsWoNan.Any(x => double.IsNaN(x)))
                {
                    stage4SWResponse.HomeostaticsWoNan = util.InterpolateData(stage4SWResponse.HomeostaticsWoNan);
                }
            }

            bool isNextDay = ((DateTime)dt.Rows[dt.Rows.Count - 1]["CommuteBegin"]).Day !=
                                (stage1SWResponse.TotalDatetimeRange.Last().AddMinutes(UtilityFunctions.INTERVAL_FREQUENCY_MINUTE)).Day;

            //Stage 5
            Stage5SWCalculator stage5SWCalculator = new Stage5SWCalculator();
            Stage5SWResponse stage5SWResponse = stage5SWCalculator.ProcessLastWindow(stage1SWResponse.TotalDatetimeRange, stage4SWResponse.HomeostaticsWoNan,
                stage2SWResponse.CircadiansWoNan, stage1SWResponse.RsDuties, isNextDay);

            //Stage 6
            Stage6SWCalculator stage6SWCalculator = new Stage6SWCalculator();
            dt = stage6SWCalculator.Calculate(dt, stage5SWResponse);

            Scoring scoring = new Scoring();
            return scoring.CalculateScore(dt); 
        }

        /// <summary>
        /// Preprocesses the provided DataTable by removing empty rows, sorting, and calculating additional columns.
        /// </summary>
        /// <param name="dt">The DataTable to preprocess.</param>
        /// <returns>The preprocessed DataTable.</returns>
        public DataTable PreprocessData(DataTable dt)
        {
            // Inside the PreprocessData method
            var rowsToRemove = dt.AsEnumerable().Where(row => row.ItemArray.All(field => field == DBNull.Value || field.ToString() == string.Empty)).ToList();
            foreach (var row in rowsToRemove)
            {
                dt.Rows.Remove(row);
            }

            // Drop rows where any of the specified columns are NaN/null
            string[] requiredColumns = { "StartDate", "StartTime", "EndDate", "EndTime", "CommuteTime" };

            var sortedRows = dt.AsEnumerable()
            .OrderBy(row => row.Field<DateTime>("StartDate"))
                           .ThenBy(row => row.Field<DateTime>("StartTime").TimeOfDay)
            .CopyToDataTable();

            // Replacing the original table with the sorted one
            dt = sortedRows;

            foreach (DataRow row in dt.Rows)
            {
                DateTime dutyBegin = DateTime.Parse(row["StartDate"].ToString())
                                   .Add(DateTime.Parse(row["StartTime"].ToString()).TimeOfDay);
                row["DutyBegin"] = dutyBegin;

                DateTime dutyEnd = DateTime.Parse(row["EndDate"].ToString())
                                   .Add(DateTime.Parse(row["EndTime"].ToString()).TimeOfDay);
                row["DutyEnd"] = dutyEnd;
            }

            // Calculate RestTime as the difference between the next DutyBegin and current DutyEnd           
            for (int i = 0; i < dt.Rows.Count - 1; i++)
            {
                DateTime currentDutyEnd = DateTime.Parse(dt.Rows[i]["DutyEnd"].ToString());
                DateTime nextDutyBegin = DateTime.Parse(dt.Rows[i + 1]["DutyBegin"].ToString());
                dt.Rows[i]["RestTime"] = nextDutyBegin - currentDutyEnd;
            }

            dt.Rows[dt.Rows.Count - 1]["RestTime"] = DBNull.Value;

            // Check if RestTime is less than MIN_REST_TIME
            List<int> idxMask = dt.AsEnumerable().
                Where(row => row["RestTime"] != DBNull.Value && !string.IsNullOrEmpty(row["RestTime"].ToString()) &&
                row.Field<TimeSpan>("RestTime") < util.MIN_REST_TIME)
                      .Select(row => dt.Rows.IndexOf(row))
                      .ToList();

            idxMask.Sort((a, b) => b.CompareTo(a));
            // Process rows with insufficient rest time
            foreach (var idx in idxMask)
            {
                dt = util.MergeRows(dt, idx);
            }

            // Recalculate RestTime after merging
            for (int i = 0; i < dt.Rows.Count - 1; i++)
            {
                DateTime currentDutyEnd = DateTime.Parse(dt.Rows[i]["DutyEnd"].ToString());
                DateTime nextDutyBegin = DateTime.Parse(dt.Rows[i + 1]["DutyBegin"].ToString());
                dt.Rows[i]["RestTime"] = nextDutyBegin - currentDutyEnd;
            }

            // Set RestTime for the last row
            dt.Rows[dt.Rows.Count - 1]["RestTime"] = util.MIN_ONE_DAY_OFF;

            int blockIDSW = 1;
            dt.Rows[0]["BlockIDSW"] = blockIDSW;
            for (int i = 1; i < dt.Rows.Count; i++)
            {
                DateTime currentStartDate = DateTime.Parse(dt.Rows[i]["StartDate"].ToString());
                DateTime previousStartDate = DateTime.Parse(dt.Rows[i - 1]["StartDate"].ToString());

                if ((currentStartDate - previousStartDate).TotalDays > 1)
                {
                    blockIDSW++;
                }
                dt.Rows[i]["BlockIDSW"] = blockIDSW;
            }

            // Calculate DayInBlockSW
            foreach (var group in dt.AsEnumerable().GroupBy(row => row.Field<int>("BlockIDSW")))
            {
                int dayInBlock = 1;
                foreach (var row in group)
                {
                    row.SetField("DayInBlockSW", dayInBlock++);
                }
            }

            // Calculate BlockLengthSW
            foreach (var group in dt.AsEnumerable().GroupBy(row => row.Field<int>("BlockIDSW")))
            {
                int blockLength = group.Count();
                foreach (var row in group)
                {
                    row.SetField("BlockLengthSW", blockLength);
                }
            }

            // Set DutyID, DayInBlock, BlockLength
            foreach (DataRow row in dt.Rows)
            {
                row["DutyID"] = row["BlockIDSW"].ToString();
                row["DayInBlock"] = row["DayInBlockSW"];
                row["BlockLength"] = row["BlockLengthSW"];
            }

            // Calculate estimated commute times
            foreach (DataRow row in dt.Rows)
            {
                row["CommuteEst"] = util.EstimatedCommuteTime(TimeSpan.Parse(row["CommuteTime"].ToString())); // Implement EstimatedCommuteTime method as needed
            }

            // Calculate CommuteBegin and CommuteEnd
            foreach (DataRow row in dt.Rows)
            {
                row["CommuteBegin"] = DateTime.Parse(row["DutyBegin"].ToString()) - (TimeSpan)row["CommuteEst"];
                row["CommuteEnd"] = DateTime.Parse(row["DutyEnd"].ToString()) + (TimeSpan)row["CommuteEst"];
            }

            // Round CommuteBegin and CommuteEnd if INTERVAL_FREQUENCY_MINUTE is not 1
            if (UtilityFunctions.INTERVAL_FREQUENCY_MINUTE != 1)
            {
                foreach (DataRow row in dt.Rows)
                {
                    row["CommuteBegin"] = util.RoundToNearestMinute((DateTime)row["CommuteBegin"], UtilityFunctions.INTERVAL_FREQUENCY_MINUTE);
                    row["CommuteEnd"] = util.RoundToNearestMinute((DateTime)row["CommuteEnd"], UtilityFunctions.INTERVAL_FREQUENCY_MINUTE);
                }
            }

            var validCommuteTimes = dt.AsEnumerable()
            .Where(row => row.Field<TimeSpan>("CommuteEst") > TimeSpan.Zero)
            .Select(row => row.Field<TimeSpan>("CommuteEst"))
            .ToList();

            int avgCommuteMinutes = validCommuteTimes.Count > 0
                ? (int)validCommuteTimes.Average(t => t.TotalMinutes)
                : 0;

            // Apply timedelta_to_time (just keeping the TimeSpan as is in this case)
            foreach (DataRow row in dt.Rows)
            {
                row["CommuteEst"] = util.TimedeltaToTime((TimeSpan)row["CommuteEst"]);
            }

            // Calculate MinRestTime (difference between CommuteBegin of next row and CommuteEnd of current row)
            for (int i = 0; i < dt.Rows.Count - 1; i++)
            {
                DateTime nextCommuteBegin = (DateTime)dt.Rows[i + 1]["CommuteBegin"];
                DateTime currentCommuteEnd = (DateTime)dt.Rows[i]["CommuteEnd"];
                dt.Rows[i]["MinRestTime"] = nextCommuteBegin - currentCommuteEnd;
            }

            // Set MinRestTime for the last row
            dt.Rows[dt.Rows.Count - 1]["MinRestTime"] = util.MIN_REST_TIME - TimeSpan.FromMinutes(avgCommuteMinutes * 2);

            // Check for rows where MinRestTime is less than MIN_SLEEP_TIME
            idxMask = dt.AsEnumerable()
               .Where(row => row.Field<TimeSpan>("MinRestTime") < util.MIN_SLEEP_TIME)
               .Select(row => dt.Rows.IndexOf(row))
               .ToList();

            // If any rows violate the minimum sleep time, print an error
            if (idxMask.Count > 0)
            {
                Console.WriteLine($"ERROR: Not enough time for MIN_SLEEP_TIME={util.MIN_SLEEP_TIME}!");
                var cols = new[] { "CrewID", "DutyID", "CommuteBegin", "DutyBegin", "DutyEnd", "CommuteEnd",
                                "RestTime", "StartFDP", "EndFDP", "SbyCallout", "CrewRoute","HSBY"};
            }

            foreach (DataRow row in dt.Rows)
            {
                DateTime dutyBegin = DateTime.Parse(row["DutyBegin"].ToString());
                DateTime dutyEnd = DateTime.Parse(row["DutyEnd"].ToString());
                row["DutyBeginDate"] = dutyBegin.Date;
                row["DutyEndDate"] = dutyEnd.Date;
                row["DutyBeginTime"] = dutyBegin.TimeOfDay;
                row["DutyEndTime"] = dutyEnd.TimeOfDay;
            }

            DateTime morningFinish = new DateTime();
            DateTime eveningFinish = new DateTime();

            foreach (DataRow row in dt.Rows)
            {
                DateTime dutyBeginDate = (DateTime.Parse(row["DutyBegin"].ToString())).Date;

                morningFinish = dutyBeginDate.Add(new TimeSpan(17, 59, 0));  // 17:59
                eveningFinish = dutyBeginDate.Add(new TimeSpan(18, 0, 0));   // 18:00

                row["MorningFinish"] = morningFinish;
                row["EveningFinish"] = eveningFinish;
                row["DualFinish"] = eveningFinish;
                row["NeutralFinish"] = morningFinish;

                TimeSpan dutyBeginTime = (TimeSpan)row["DutyBeginTime"];
                DateTime dutyEnd = DateTime.Parse(row["DutyEnd"].ToString());

                if (dutyBeginTime <= new TimeSpan(9, 29, 0) && dutyEnd <= morningFinish)
                {
                    row["DutyType"] = "Morning";
                }
                else if (dutyBeginTime >= new TimeSpan(9, 30, 0) && dutyEnd >= eveningFinish)
                {
                    row["DutyType"] = "Evening";
                }
                else if (dutyBeginTime <= new TimeSpan(9, 29, 0) && dutyEnd >= eveningFinish)
                {
                    row["DutyType"] = "Dual";
                }
                else if (dutyBeginTime >= new TimeSpan(9, 30, 0) && dutyEnd <= morningFinish)
                {
                    row["DutyType"] = "Neutral";
                }
                else
                {
                    row["DutyType"] = "Undefined";
                }
            }

            // easyJet Disruptive Duty Classification
            foreach (DataRow row in dt.Rows)
            {
                TimeSpan dutyEndTime = (TimeSpan)row["DutyEndTime"];
                TimeSpan dutyBeginTime = (TimeSpan)row["DutyBeginTime"];

                if (dutyEndTime >= new TimeSpan(1, 0, 0) && dutyEndTime <= new TimeSpan(1, 59, 0))
                {
                    row["ejDisruptive"] = "Late";
                }
                else if (dutyEndTime >= new TimeSpan(2, 0, 0) && dutyEndTime <= new TimeSpan(4, 59, 0))
                {
                    row["ejDisruptive"] = "Night";
                }
                else if (dutyBeginTime >= new TimeSpan(2, 0, 0) && dutyBeginTime <= new TimeSpan(6, 59, 0))
                {
                    row["ejDisruptive"] = "Early";
                }
                else
                {
                    row["ejDisruptive"] = "Non-Disruptive";
                }
            }

            foreach (DataRow row in dt.Rows)
            {
                string crewRoute = row["CrewRoute"].ToString();
                row["IsContactable"] = util.CONTACTABLES.Contains(crewRoute);
                row["IsStandby"] = util.STANDBYS.Contains(crewRoute);
            }

            // Filter the times before 9 AM for CommuteTime/CommuteEnd
            var contactableRows = dt.AsEnumerable()
                .Where(row => (bool)row["IsContactable"] &&
                              ((DateTime)row["CommuteBegin"]).Hour < 9 &&
                              ((DateTime)row["CommuteEnd"]).Hour < 9)
                .ToList();

            foreach (DataRow row in contactableRows)
            {
                DateTime commuteBegin = (DateTime)row["CommuteBegin"];
                DateTime commuteEnd = (DateTime)row["CommuteEnd"];

                // Shift the time values to start from 9 AM
                int shiftHours = 9 - commuteBegin.Hour;
                row["CommuteBegin"] = commuteBegin.AddHours(shiftHours);
                row["CommuteEnd"] = commuteEnd.AddHours(shiftHours);
            }

            var contactableMask = dt.AsEnumerable()
            .Where(row => (bool)row["IsContactable"] &&
                          ((DateTime)row["CommuteBegin"]).Hour < 9 &&
                          ((DateTime)row["CommuteEnd"]).Hour < 9)
            .ToList();

            foreach (DataRow row in contactableMask)
            {
                DateTime commuteBegin = (DateTime)row["CommuteBegin"];
                DateTime commuteEnd = (DateTime)row["CommuteEnd"];
                int shiftHours = 9 - commuteBegin.Hour;
                row["CommuteBegin"] = commuteBegin.AddHours(shiftHours);
                row["CommuteEnd"] = commuteEnd.AddHours(shiftHours);
            }

            // Filter and shift commute times for 'Standby' with 'HOME'
            var standbyMask = dt.AsEnumerable()
                .Where(row => row["SbyCallout"].ToString() == "HOME" &&
                              (bool)row["IsStandby"] &&
                              ((DateTime)row["CommuteBegin"]).Hour < 9 &&
                              ((DateTime)row["CommuteEnd"]).Hour < 9)
                .ToList();

            foreach (DataRow row in standbyMask)
            {
                DateTime commuteBegin = (DateTime)row["CommuteBegin"];
                DateTime commuteEnd = (DateTime)row["CommuteEnd"];
                int shiftHours = 9 - commuteBegin.Hour;
                row["CommuteBegin"] = commuteBegin.AddHours(shiftHours);
                row["CommuteEnd"] = commuteEnd.AddHours(shiftHours);
            }

            // Remove temporary columns from the DataTable
            var columnsToDelete = new[] {
            "DutyBeginDate", "DutyBeginTime", "DutyEndDate", "DutyEndTime",
            "MorningFinish", "EveningFinish", "DualFinish", "NeutralFinish"
            };

            foreach (string col in columnsToDelete)
            {
                if (dt.Columns.Contains(col))
                {
                    dt.Columns.Remove(col);
                }
            }
            return dt;
        }
    }
}
