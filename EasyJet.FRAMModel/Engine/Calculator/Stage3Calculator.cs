using EasyJet.FRAMModel.Engine.Entities;
using System;
using System.Collections.Generic;


namespace EasyJet.FRAMModel.Engine.Calculator
{
  internal class Stage3Calculator
  {
    private Common common = new Common();

    public Stage3Response Calculate(Stage2Response stage2Response)
    {
      Stage3Response stage3Response = new Stage3Response();
      List<Decimal> dutiesContribution = this.GetLongDutiesContribution(stage2Response);
      List<Decimal> sectorContribution = this.GetSectorContribution(stage2Response);
      List<Decimal> contextualRestPartTwo = this.GetIneffectiveContextualRestPartTwo(stage2Response);
      List<Decimal> nightContribution = this.GetEarlyToNightContribution(stage2Response);
      stage3Response.LongDutiesContributionList = dutiesContribution;
      stage3Response.SectorContributionList = sectorContribution;
      stage3Response.IneffectiveContextualRestPartTwoList = contextualRestPartTwo;
      stage3Response.EarlyToNightContributionList = nightContribution;
      stage3Response.DutyBlock = stage2Response.DutyBlock;
      stage3Response.ConsecutiveElongatedDutyList = stage2Response.ConsecutiveElongatedDutyList;
      stage3Response.ConsecutiveHighSectorDutyList = stage2Response.ConsecutiveHighSectorDutyList;
      stage3Response.MorningToEveningTransitionContributionList = stage2Response.MorningToEveningTransitionContributionList;
      stage3Response.IneffectiveContextualRestPartOneList = stage2Response.IneffectiveContextualRestPartOneList;
      stage3Response.NightToEarlyContributionList = stage2Response.NightToEarlyContributionList;
      stage3Response.SuboptimalNightContributionList = stage2Response.SuboptimalNightContributionList;
      return stage3Response;
    }

    public List<Decimal> GetEarlyToNightContribution(Stage2Response stage2Response)
    {
      List<Decimal> earlyToNightContributionList = new List<Decimal>();
      for (int index = 0; index < stage2Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        Decimal num1 = 0M;
        if (index == 0)
          earlyToNightContributionList.Add(num1);
        else if (!stage2Response.DutyBlock.DutyPeriods[index].IsDutyNightFinish)
          earlyToNightContributionList.Add(num1);
        else if (this.EarlyToNightContribution_SumOverNightToEarlyContribution(earlyToNightContributionList, index) > 0M)
          earlyToNightContributionList.Add(num1);
        else if (stage2Response.MorningToEveningTransitionContributionList[index] > 0M)
          earlyToNightContributionList.Add(num1);
        else if (this.EarlyToNightContribution_SumOverFramMorningStartFlag(stage2Response, index) > 0M)
        {
          Decimal timeInDecimalFormat = this.common.GetTimeInDecimalFormat(stage2Response.DutyBlock.DutyPeriods[index].HoursBetweenMidnight);
          string empty = string.Empty;
          Decimal num2 = 0M;
          Decimal betweenMidnightFactor = this.EarlyToNightContribution_GetMinDutyHoursBetweenMidnightFactor(timeInDecimalFormat);
          string morningStartFlagGb = this.EarlyToNightContribution_CountFramMorningStartFlagGb(stage2Response, index);
          if (morningStartFlagGb != "NA")
            num2 = this.EarlyToNightContribution_GetMinMorningStartFactor(index - int.Parse(morningStartFlagGb));
          Decimal num3 = num2;
          Decimal num4 = betweenMidnightFactor * num3;
          earlyToNightContributionList.Add(num4);
        }
        else
          earlyToNightContributionList.Add(num1);
      }
      return earlyToNightContributionList;
    }

    public List<Decimal> GetIneffectiveContextualRestPartTwo(Stage2Response stage2Response)
    {
      List<Decimal> contextualRestPartTwo = new List<Decimal>();
      for (int index = 0; index < stage2Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        Decimal num1 = 0M;
        if (index == stage2Response.DutyBlock.DutyBlockDutyCount - 1)
        {
          contextualRestPartTwo.Add(num1);
        }
        else
        {
          Decimal contextualRestPartOne = stage2Response.IneffectiveContextualRestPartOneList[index];
          TimeSpan timeSpan = stage2Response.DutyBlock.DutyPeriods[index + 1].StartDateTimeZulu - stage2Response.DutyBlock.DutyPeriods[index].EndDateTimeZulu;
          string time = timeSpan.ToString();
          if (timeSpan.Days != 0)
            time = (timeSpan.Days * 24 + timeSpan.Hours).ToString() + ":" + (object) timeSpan.Minutes;
          Decimal timeInDecimalFormat = this.common.GetTimeInDecimalFormat(time);
          Decimal num2 = contextualRestPartOne - timeInDecimalFormat;
          contextualRestPartTwo.Add(num2);
        }
      }
      return contextualRestPartTwo;
    }

    public List<Decimal> GetSectorContribution(Stage2Response stage2Response)
    {
      List<Decimal> sectorContribution = new List<Decimal>();
      for (int index = 0; index < stage2Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        Decimal num1 = 0M;
        if (stage2Response.DutyBlock.DutyPeriods[index].OperationalSectorCount > 0)
        {
          int operationalSectorCount = stage2Response.DutyBlock.DutyPeriods[index].OperationalSectorCount;
          int consecutiveHighSectorDuty = stage2Response.ConsecutiveHighSectorDutyList[index];
          Decimal num2 = this.SectorContribution_GetMinFactorOperationalSectorCount(operationalSectorCount) * this.SectorContribution_GetMinFactorOperationalHighSectorDuty(consecutiveHighSectorDuty);
          sectorContribution.Add(num2);
        }
        else
          sectorContribution.Add(num1);
      }
      return sectorContribution;
    }

    public List<Decimal> GetLongDutiesContribution(Stage2Response stage2Response)
    {
      Decimal timeInDecimalFormat1 = this.common.GetTimeInDecimalFormat("09:00:00");
      List<Decimal> dutiesContribution = new List<Decimal>();
      for (int index = 0; index < stage2Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        Decimal timeInDecimalFormat2 = this.common.GetTimeInDecimalFormat(stage2Response.DutyBlock.DutyPeriods[index].DutyLength);
        Decimal num1 = 0M;
        if (stage2Response.DutyBlock.DutyPeriods[index].DutyPeriodOfDutyBlock == 1 && stage2Response.DutyBlock.DutyPeriods[index].IsDutyMorningStart)
        {
          if (stage2Response.DutyBlock.DutyBlockDutyCount == 1)
          {
            Decimal num2 = 0.5M * this.LongDutiesContribution_GetFirstMinTimeFactorDutyLength(timeInDecimalFormat2);
            dutiesContribution.Add(num2);
          }
          else
          {
            Decimal factorDutyLength = this.LongDutiesContribution_GetFirstMinTimeFactorDutyLength(timeInDecimalFormat2);
            dutiesContribution.Add(factorDutyLength);
          }
        }
        else if (stage2Response.DutyBlock.DutyPeriods[index].DutyPeriodOfDutyBlock > 4)
        {
          Decimal factorDutyLength = this.LongDutiesContribution_GetFirstMinTimeFactorDutyLength(timeInDecimalFormat2);
          dutiesContribution.Add(factorDutyLength);
        }
        else if (timeInDecimalFormat2 > timeInDecimalFormat1)
        {
          Decimal num3 = this.LongDutiesContribution_GetSecondMinTimeFactorDutyLength(timeInDecimalFormat2) * this.LongDutiesContribution_GetMinTimeFactorElongatedDuty(stage2Response.ConsecutiveElongatedDutyList[index]);
          dutiesContribution.Add(num3);
        }
        else
          dutiesContribution.Add(num1);
      }
      return dutiesContribution;
    }

    public Decimal EarlyToNightContribution_GetMinMorningStartFactor(int countMorningStart)
    {
      Decimal morningStartFactor = 0M;
      morningStartFactor = !((Decimal) countMorningStart >= 7M) ? -(0.0083M * (Decimal) (countMorningStart * countMorningStart * countMorningStart)) + 0.07M * (Decimal) (countMorningStart * countMorningStart) - 0.2267M * (Decimal) countMorningStart + 1.1229M : 0.10M;
      return morningStartFactor;
    }

    public string EarlyToNightContribution_CountFramMorningStartFlagGb(
      Stage2Response stage2Response,
      int index)
    {
      string morningStartFlagGb = "NA";
      for (int index1 = 0; index1 < index; ++index1)
      {
        if (stage2Response.DutyBlock.DutyPeriods[index1].IsDutyMorningStart)
          morningStartFlagGb = index1.ToString();
      }
      return morningStartFlagGb;
    }

    public Decimal EarlyToNightContribution_GetMinDutyHoursBetweenMidnightFactor(
      Decimal dutyLengthInDecimal)
    {
      Decimal betweenMidnightFactor = 0M;
      if (dutyLengthInDecimal >= 1.01M)
        betweenMidnightFactor = this.common.GetMinTimeFactorWithSquarePower(374M, dutyLengthInDecimal, -56.685M, 1108.8M, -1082.6M);
      return betweenMidnightFactor;
    }

    public Decimal EarlyToNightContribution_SumOverNightToEarlyContribution(
      List<Decimal> earlyToNightContributionList,
      int index)
    {
      Decimal earlyContribution = 0M;
      for (int index1 = 0; index1 < index; ++index1)
        earlyContribution += earlyToNightContributionList[index1];
      return earlyContribution;
    }

    public Decimal EarlyToNightContribution_SumOverFramMorningStartFlag(
      Stage2Response stage2Response,
      int index)
    {
      Decimal morningStartFlag = 0M;
      for (int index1 = 0; index1 < index; ++index1)
        morningStartFlag += (Decimal) (stage2Response.DutyBlock.DutyPeriods[index1].IsDutyMorningStart ? 1 : 0);
      return morningStartFlag;
    }

    public Decimal LongDutiesContribution_GetMinTimeFactorElongatedDuty(int consecutiveElongatedDuty)
    {
      Decimal factorElongatedDuty = 0M;
      if (consecutiveElongatedDuty > 0)
        factorElongatedDuty = 0.1589M * (Decimal) (consecutiveElongatedDuty * consecutiveElongatedDuty) - 0.3182M * (Decimal) consecutiveElongatedDuty + 1.17M;
      return factorElongatedDuty;
    }

    public Decimal LongDutiesContribution_GetFirstMinTimeFactorDutyLength(
      Decimal dutyLengthInDecimal)
    {
      Decimal timeInDecimalFormat = this.common.GetTimeInDecimalFormat("09:10:00");
      Decimal factorDutyLength = 0M;
      if (dutyLengthInDecimal > timeInDecimalFormat)
        factorDutyLength = this.common.GetMinTimeFactorWithCubicPower(91M, dutyLengthInDecimal, -2431.8M, 5224.1M, -2880.4M, 473.59M);
      return factorDutyLength;
    }

    public Decimal LongDutiesContribution_GetSecondMinTimeFactorDutyLength(
      Decimal dutyLengthInDecimal)
    {
      Decimal timeInDecimalFormat = this.common.GetTimeInDecimalFormat("09:10:00");
      Decimal factorDutyLength = 0M;
      if (dutyLengthInDecimal > timeInDecimalFormat)
        factorDutyLength = this.common.GetMinTimeFactorWithCubicPower(220M, dutyLengthInDecimal, -1013.3M, 2176.7M, -1200.2M, 197.33M);
      return factorDutyLength;
    }

    public Decimal SectorContribution_GetMinFactorOperationalSectorCount(int operationalSectorCount)
    {
      Decimal operationalSectorCount1 = 0M;
      if (operationalSectorCount >= 2)
        operationalSectorCount1 = 0.1566M * (Decimal) (operationalSectorCount * operationalSectorCount) + 4.8516M * (Decimal) operationalSectorCount - 10.468M;
      return operationalSectorCount1;
    }

    public Decimal SectorContribution_GetMinFactorOperationalHighSectorDuty(
      int consecutiveHighSectorDuty)
    {
      Decimal operationalHighSectorDuty = 1M;
      if (consecutiveHighSectorDuty > 0)
        operationalHighSectorDuty = 0.1589M * (Decimal) (consecutiveHighSectorDuty * consecutiveHighSectorDuty) - 0.3182M * (Decimal) consecutiveHighSectorDuty + 1.17M;
      return operationalHighSectorDuty;
    }
  }
}
