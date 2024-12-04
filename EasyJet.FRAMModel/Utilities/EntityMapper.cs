using EasyJet.FRAMModel.Engine.Entities;
using EasyJet.FRAMModel.Engine.Exceptions;
using EasyJet.FRAMModel.Engine.ExternalContract;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

//#nullable disable
namespace EasyJet.FRAMModel.Utilities
{
    internal class EntityMapper
    {
        public List<DutyBlock> GetDutyBlockList(IFRMModelRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("Null request");
            if (((IEnumerable<object>)new object[21]
            {
        (object) request.IdxInBlock,
        (object) request.OperationalSectorCount,
        (object) request.IsaHomeStandbyFlag,
        (object) request.StartDateLocalTime,
        (object) request.StartTimeLocalTime,
        (object) request.EndDateLocalTime,
        (object) request.EndTimeLocalTime,
        (object) request.EndDateCrewReferenceTime,
        (object) request.EndTimeCrewReferenceTime,
        (object) request.StartDateTimeZulu,
        (object) request.EndDateTimeZulu,
        (object) request.DutyLength,
        (object) request.IsDutyMorningStart,
        (object) request.IsDutyEveningFinish,
        (object) request.IsDutyNightFinish,
        (object) request.IsDutyElongated,
        (object) request.IsDutyHighSector,
        (object) request.HoursBetweenMidnight,
        (object) request.IsStandby,
        (object) request.IsContactable,
        (object) request.CommuteTime
            }).Any<object>((Func<object, bool>)(x => x == null)))
                throw new ArgumentNullException("Null request parameter value");
            this.ValidateRequestFormat(request);
            this.ValidateRequestValues(request);
            return this.MapRequestValuesToDutyBlock(request);
        }

        public List<DutyBlock> MapRequestValuesToDutyBlock(IFRMModelRequest request)
        {
            List<DutyBlock> dutyBlock1 = new List<DutyBlock>();
            int num1 = 0;
            ArrayList filteredDutyBlockList = this.GetFilteredDutyBlockList(request);
            for (int index1 = 0; index1 < filteredDutyBlockList.Count; ++index1)
            {
                DutyBlock dutyBlock2 = new DutyBlock();
                List<DutyPeriod> dutyPeriodList = new List<DutyPeriod>();
                dutyBlock2.DutyBlockDutyCount = int.Parse(filteredDutyBlockList[index1].ToString());
                if (num1 != 0)
                    ++num1;
                int num2 = dutyBlock2.DutyBlockDutyCount + num1;
                for (int index2 = num1; index2 < num2; ++index2)
                {
                    dutyPeriodList.Add(new DutyPeriod()
                    {
                        DutyPeriodOfDutyBlock = request.IdxInBlock[index2],
                        OperationalSectorCount = request.OperationalSectorCount[index2],
                        IsaHomeStandbyFlag = Convert.ToBoolean(request.IsaHomeStandbyFlag[index2]),
                        StartDateLocalTime = Convert.ToDateTime(DateTime.Parse(request.StartDateLocalTime[index2], (IFormatProvider)new CultureInfo("en-GB"))),
                        StartTimeLocalTime = Convert.ToDateTime(DateTime.Parse(request.StartTimeLocalTime[index2], (IFormatProvider)new CultureInfo("en-GB"))).ToString("HH:mm"),
                        EndDateLocalTime = Convert.ToDateTime(DateTime.Parse(request.EndDateLocalTime[index2], (IFormatProvider)new CultureInfo("en-GB"))),
                        EndTimeLocalTime = Convert.ToDateTime(DateTime.Parse(request.EndTimeLocalTime[index2], (IFormatProvider)new CultureInfo("en-GB"))).ToString("HH:mm"),
                        EndDateCrewReferenceTime = Convert.ToDateTime(DateTime.Parse(request.EndDateCrewReferenceTime[index2], (IFormatProvider)new CultureInfo("en-GB"))),
                        EndTimeCrewReferenceTime = Convert.ToDateTime(DateTime.Parse(request.EndTimeCrewReferenceTime[index2], (IFormatProvider)new CultureInfo("en-GB"))).ToString("HH:mm"),
                        StartDateTimeZulu = Convert.ToDateTime(DateTime.Parse(request.StartDateTimeZulu[index2], (IFormatProvider)new CultureInfo("en-GB"))),
                        EndDateTimeZulu = Convert.ToDateTime(DateTime.Parse(request.EndDateTimeZulu[index2], (IFormatProvider)new CultureInfo("en-GB"))),
                        DutyLength = request.DutyLength[index2],
                        IsDutyMorningStart = Convert.ToBoolean(request.IsDutyMorningStart[index2]),
                        IsDutyEveningFinish = Convert.ToBoolean(request.IsDutyEveningFinish[index2]),
                        IsDutyNightFinish = Convert.ToBoolean(request.IsDutyNightFinish[index2]),
                        IsDutyElongated = Convert.ToBoolean(request.IsDutyElongated[index2]),
                        IsDutyHighSector = Convert.ToBoolean(request.IsDutyHighSector[index2]),
                        HoursBetweenMidnight = request.HoursBetweenMidnight[index2],                        
                    });
                    num1 = index2;
                }
                dutyBlock2.DutyPeriods = (IList<DutyPeriod>)dutyPeriodList;
                dutyBlock1.Add(dutyBlock2);
            }
            return dutyBlock1;
        }

        public ArrayList GetFilteredDutyBlockList(IFRMModelRequest request)
        {
            ArrayList filteredDutyBlockList = new ArrayList();
            int length = request.IdxInBlock.Length;
            for (int index = 0; index < length; ++index)
            {
                int num = request.IdxInBlock[index];
                if (index == length - 1)
                    filteredDutyBlockList.Add((object)num);
                else if (request.IdxInBlock[index + 1] <= num)
                    filteredDutyBlockList.Add((object)num);
            }
            return filteredDutyBlockList;
        }

        private void ValidateRequestValues(IFRMModelRequest request)
        {
            string[] formats = new string[1] { "d/M/yyyy" };
            for (int index = 0; index < request.IdxInBlock.Length; ++index)
            {
                if (request.IdxInBlock[index] <= 0)
                    throw new InvalidDataValueException("IdxInBlock", index, request.IdxInBlock[index].ToString());
                if (request.OperationalSectorCount[index] < 0)
                    throw new InvalidDataValueException("OperationalSectorCount", index, request.OperationalSectorCount[index].ToString());
                if (request.IsaHomeStandbyFlag[index] != 0 && request.IsaHomeStandbyFlag[index] != 1)
                    throw new InvalidDataValueException("IsaHomeStandbyFlag", index, request.IsaHomeStandbyFlag[index].ToString());
                DateTime result;
                if (!DateTime.TryParseExact(request.StartDateLocalTime[index], formats, (IFormatProvider)new CultureInfo("en-GB"), DateTimeStyles.None, out result))
                    throw new InvalidDataValueException("StartDateLocalTime", index, request.StartDateLocalTime[index].ToString());
                if (!DateTime.TryParseExact(request.EndDateLocalTime[index], formats, (IFormatProvider)new CultureInfo("en-GB"), DateTimeStyles.None, out result))
                    throw new InvalidDataValueException("EndDateLocalTime", index, request.EndDateLocalTime[index].ToString());
                if (!DateTime.TryParseExact(request.EndDateCrewReferenceTime[index], formats, (IFormatProvider)new CultureInfo("en-GB"), DateTimeStyles.None, out result))
                    throw new InvalidDataValueException("EndDateCrewReferenceTime", index, request.EndDateCrewReferenceTime[index].ToString());
                if (request.IsDutyMorningStart[index] != 0 && request.IsDutyMorningStart[index] != 1)
                    throw new InvalidDataValueException("IsDutyMorningStart", index, request.IsDutyMorningStart[index].ToString());
                if (request.IsDutyEveningFinish[index] != 0 && request.IsDutyEveningFinish[index] != 1)
                    throw new InvalidDataValueException("IsDutyEveningFinish", index, request.IsDutyEveningFinish[index].ToString());
                if (request.IsDutyNightFinish[index] != 0 && request.IsDutyNightFinish[index] != 1)
                    throw new InvalidDataValueException("IsDutyNightFinish", index, request.IsDutyNightFinish[index].ToString());
                if (request.IsDutyElongated[index] != 0 && request.IsDutyElongated[index] != 1)
                    throw new InvalidDataValueException("IsDutyElongated", index, request.IsDutyElongated[index].ToString());
                if (request.IsDutyHighSector[index] != 0 && request.IsDutyHighSector[index] != 1)
                    throw new InvalidDataValueException("IsDutyHighSector", index, request.IsDutyHighSector[index].ToString());
            }
        }

        private void ValidateRequestFormat(IFRMModelRequest request)
        {
            if (!((IEnumerable<int>)new int[21]
            {
        request.IdxInBlock.Length,
        request.OperationalSectorCount.Length,
        request.IsaHomeStandbyFlag.Length,
        request.StartDateLocalTime.Length,
        request.StartTimeLocalTime.Length,
        request.EndDateLocalTime.Length,
        request.EndTimeLocalTime.Length,
        request.EndDateCrewReferenceTime.Length,
        request.EndTimeCrewReferenceTime.Length,
        request.StartDateTimeZulu.Length,
        request.EndDateTimeZulu.Length,
        request.DutyLength.Length,
        request.IsDutyMorningStart.Length,
        request.IsDutyEveningFinish.Length,
        request.IsDutyNightFinish.Length,
        request.IsDutyElongated.Length,
        request.IsDutyHighSector.Length,
        request.HoursBetweenMidnight.Length,
        request.IsContactable.Length,
        request.IsStandby.Length,
        request.CommuteTime.Length
            }).All<int>((Func<int, bool>)(x => x == request.IdxInBlock.Length)))
                throw new InvalidDataFormatException("Mismatch of duty periods count");
        }

        public IFRMModelResponse GetScoreArray(ScoreList scoreList)
        {
            if (scoreList == null)
                throw new ArgumentNullException("FRAM Score is null");
            List<string> stringList = new List<string>();
            foreach (DutyBlockScore dutyBlockScore in scoreList.DutyBlockScoreList)
            {
                foreach (DutyPeriodScore dutyPeriodScore in dutyBlockScore.DutyPeriodScoreList)
                    stringList.Add(dutyPeriodScore.Score.ToString());
            }
            FRMModelResponse scoreArray = new FRMModelResponse();
            scoreArray.FRMScore = stringList.ToArray();
            return (IFRMModelResponse)scoreArray;
        }
    }
}
