using EasyJet.FRAMModel.Engine.Entities;
using System;
using System.Collections.Generic;


namespace EasyJet.FRAMModel.Engine.Calculator
{
  internal class Stage2Calculator
  {
    private Common common = new Common();

    public Stage2Response Calculate(Stage1Response stage1Response)
    {
      Stage2Response stage2Response = new Stage2Response();
      List<int> consecutiveElongatedDuty = this.IntermediateCalculations_GetConsecutiveElongatedDuty(stage1Response);
      List<int> consecutiveHighSectorDuty = this.IntermediateCalculations_GetConsecutiveHighSectorDuty(stage1Response);
      List<Decimal> transitionContribution = this.GetMorningToEveningTransitionContribution(stage1Response);
      List<Decimal> contextualRestPartOne = this.GetIneffectiveContextualRestPartOne(stage1Response);
      List<Decimal> earlyContribution = this.GetNightToEarlyContribution(stage1Response);
      List<Decimal> nightContribution = this.GetSuboptimalNightContribution(stage1Response);
      stage2Response.ConsecutiveElongatedDutyList = consecutiveElongatedDuty;
      stage2Response.ConsecutiveHighSectorDutyList = consecutiveHighSectorDuty;
      stage2Response.MorningToEveningTransitionContributionList = transitionContribution;
      stage2Response.IneffectiveContextualRestPartOneList = contextualRestPartOne;
      stage2Response.NightToEarlyContributionList = earlyContribution;
      stage2Response.SuboptimalNightContributionList = nightContribution;
      stage2Response.DutyBlock = stage1Response.DutyBlock;
      return stage2Response;
    }

    public List<int> IntermediateCalculations_GetConsecutiveElongatedDuty(
      Stage1Response stage1Response)
    {
      List<int> consecutiveElongatedDuty = new List<int>();
      for (int index = 0; index < stage1Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        if (stage1Response.DutyBlock.DutyPeriods[index].IsDutyElongated)
        {
          if (stage1Response.DutyBlock.DutyPeriods[index].DutyPeriodOfDutyBlock == 1)
          {
            consecutiveElongatedDuty.Add(1);
          }
          else
          {
            int num = consecutiveElongatedDuty[index - 1] + 1;
            consecutiveElongatedDuty.Add(num);
          }
        }
        else
          consecutiveElongatedDuty.Add(0);
      }
      return consecutiveElongatedDuty;
    }

    public List<int> IntermediateCalculations_GetConsecutiveHighSectorDuty(
      Stage1Response stage1Response)
    {
      List<int> consecutiveHighSectorDuty = new List<int>();
      for (int index = 0; index < stage1Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        if (stage1Response.DutyBlock.DutyPeriods[index].IsDutyHighSector)
        {
          if (stage1Response.DutyBlock.DutyPeriods[index].DutyPeriodOfDutyBlock == 1)
          {
            consecutiveHighSectorDuty.Add(1);
          }
          else
          {
            int num = consecutiveHighSectorDuty[index - 1] + 1;
            consecutiveHighSectorDuty.Add(num);
          }
        }
        else
          consecutiveHighSectorDuty.Add(0);
      }
      return consecutiveHighSectorDuty;
    }

    public List<Decimal> GetMorningToEveningTransitionContribution(Stage1Response stage1Response)
    {
      Decimal timeInDecimalFormat1 = this.common.GetTimeInDecimalFormat("22:00:00");
      List<Decimal> transitionContribution = new List<Decimal>();
      for (int index = 0; index < stage1Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        if (index == 0)
          transitionContribution.Add(0M);
        else if (stage1Response.DutyBlock.DutyPeriods[index].IsDutyEveningFinish && stage1Response.DutyBlock.DutyPeriods[index - 1].IsDutyMorningStart)
        {
          Decimal timeInDecimalFormat2 = this.common.GetTimeInDecimalFormat(stage1Response.DutyBlock.DutyPeriods[index].HoursBetweenMidnight);
          Decimal num = 0M;
          if (timeInDecimalFormat2 > timeInDecimalFormat1)
            num = this.common.GetMinTimeFactorWithCubicPower(149M, timeInDecimalFormat2, -1367.7M, 6404.0M, -8288.0M, 3269.6M);
          transitionContribution.Add(num);
        }
        else
          transitionContribution.Add(0M);
      }
      return transitionContribution;
    }

    public List<Decimal> GetIneffectiveContextualRestPartOne(Stage1Response stage1Response)
    {
      List<Decimal> contextualRestPartOne = new List<Decimal>();
      foreach (DutyPeriod dutyPeriod in (IEnumerable<DutyPeriod>) stage1Response.DutyBlock.DutyPeriods)
      {
        if (dutyPeriod.IsaHomeStandbyFlag)
          contextualRestPartOne.Add(this.common.GetTimeInDecimalFormat("12:00:00"));
        else if (dutyPeriod.OperationalSectorCount == 0)
          contextualRestPartOne.Add(this.common.GetTimeInDecimalFormat("13:00:00"));
        else if (dutyPeriod.DutyPeriodOfDutyBlock < stage1Response.DutyBlock.DutyBlockDutyCount)
        {
          Decimal timeInDecimalFormat1 = this.common.GetTimeInDecimalFormat(dutyPeriod.DutyLength);
          Decimal timeInDecimalFormat2 = this.common.GetTimeInDecimalFormat(dutyPeriod.EndTimeCrewReferenceTime);
          Decimal operationalSectorCount = (Decimal) dutyPeriod.OperationalSectorCount;
          Decimal lengthTimeFactor = this.IneffectiveContextualRestPartOne_GetMinDutyLengthTimeFactor(timeInDecimalFormat1, operationalSectorCount);
          Decimal crewRefTimeFactor = this.IneffectiveContextualRestPartOne_GetMinCrewRefTimeFactor(timeInDecimalFormat2, operationalSectorCount);
          contextualRestPartOne.Add(lengthTimeFactor + crewRefTimeFactor);
        }
        else
          contextualRestPartOne.Add(0M);
      }
      return contextualRestPartOne;
    }

    public List<Decimal> GetNightToEarlyContribution(Stage1Response stage1Response)
    {
      List<Decimal> nightToEarlyContributionList = new List<Decimal>();
      for (int index = 0; index < stage1Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        Decimal num1 = 0M;
        if (index == 0)
          nightToEarlyContributionList.Add(num1);
        else if (!stage1Response.DutyBlock.DutyPeriods[index].IsDutyMorningStart)
          nightToEarlyContributionList.Add(num1);
        else if (this.NightToEarlyContribution_SumOverNightToEarlyContribution(nightToEarlyContributionList, index) > 0M)
          nightToEarlyContributionList.Add(num1);
        else if (this.NightToEarlyContribution_SumOverFramNightFinishFlag(stage1Response, index) > 0M)
        {
          Decimal timeInDecimalFormat = this.common.GetTimeInDecimalFormat(stage1Response.DutyBlock.DutyPeriods[index].StartTimeLocalTime);
          string empty = string.Empty;
          Decimal num2 = 0M;
          Decimal dutyStartTimeFactor = this.NightToEarlyContribution_GetMinDutyStartTimeFactor(timeInDecimalFormat);
          string nightFinishFlagGb = this.NightToEarlyContribution_CountFramNightFinishFlagGb(stage1Response, index);
          if (nightFinishFlagGb != "NA")
            num2 = this.NightToEarlyContribution_GetMinNightFinishFactor(index - int.Parse(nightFinishFlagGb));
          Decimal num3 = num2;
          Decimal num4 = dutyStartTimeFactor * num3;
          nightToEarlyContributionList.Add(num4);
        }
        else
          nightToEarlyContributionList.Add(num1);
      }
      return nightToEarlyContributionList;
    }

    public List<Decimal> GetSuboptimalNightContribution(Stage1Response stage1Response)
    {
      Decimal timeInDecimalFormat1 = this.common.GetTimeInDecimalFormat("24:10:00");
      List<Decimal> nightContribution = new List<Decimal>();
      for (int index = 0; index < stage1Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        if (index == 0 && stage1Response.DutyBlock.DutyPeriods[index].IsDutyNightFinish)
        {
          Decimal timeInDecimalFormat2 = this.common.GetTimeInDecimalFormat(stage1Response.DutyBlock.DutyPeriods[index].HoursBetweenMidnight);
          Decimal num = 0M;
          if (timeInDecimalFormat2 >= timeInDecimalFormat1)
            num = this.common.GetMinTimeFactorWithSquarePower(224M, timeInDecimalFormat2, -34.011M, 665.27M, -649.58M);
          nightContribution.Add(num);
        }
        else
          nightContribution.Add(0M);
      }
      return nightContribution;
    }

    private Decimal NightToEarlyContribution_SumOverNightToEarlyContribution(List<Decimal> nightToEarlyContributionList, int index)
    {
      Decimal earlyContribution = 0M;
      for (int index1 = 0; index1 < index; ++index1)
        earlyContribution += nightToEarlyContributionList[index1];
      return earlyContribution;
    }

    private Decimal NightToEarlyContribution_SumOverFramNightFinishFlag(
      Stage1Response stage1Response,
      int index)
    {
      Decimal framNightFinishFlag = 0M;
      for (int index1 = 0; index1 < index; ++index1)
        framNightFinishFlag += (Decimal) (stage1Response.DutyBlock.DutyPeriods[index1].IsDutyNightFinish ? 1 : 0);
      return framNightFinishFlag;
    }

    private string NightToEarlyContribution_CountFramNightFinishFlagGb(
      Stage1Response stage1Response,
      int index)
    {
      string nightFinishFlagGb = "NA";
      for (int index1 = 0; index1 < index; ++index1)
      {
        if (stage1Response.DutyBlock.DutyPeriods[index1].IsDutyNightFinish)
          nightFinishFlagGb = index1.ToString();
      }
      return nightFinishFlagGb;
    }

    private Decimal NightToEarlyContribution_GetMinNightFinishFactor(int countNightFinishFlag)
    {
      Decimal nightFinishFactor = 0M;
      nightFinishFactor = !((Decimal) countNightFinishFlag >= 7M) ? -(0.0083M * (Decimal) (countNightFinishFlag * countNightFinishFlag * countNightFinishFlag)) + 0.07M * (Decimal) (countNightFinishFlag * countNightFinishFlag) - 0.2267M * (Decimal) countNightFinishFlag + 1.1229M : 0.10M;
      return nightFinishFactor;
    }

    private Decimal NightToEarlyContribution_GetMinDutyStartTimeFactor(Decimal dutyLengthInDecimal)
    {
      Decimal timeInDecimalFormat = this.common.GetTimeInDecimalFormat("09:20:00");
      Decimal dutyStartTimeFactor = 0M;
      if (dutyLengthInDecimal < timeInDecimalFormat)
        dutyStartTimeFactor = this.common.GetMinTimeFactorWithCubicPower(688M, dutyLengthInDecimal, -33196M, 34036M, -12042M, 1480M);
      return dutyStartTimeFactor;
    }

    private Decimal IneffectiveContextualRestPartOne_GetMinDutyLengthTimeFactor(
      Decimal dutyLengthInDecimal,
      Decimal sectorCount)
    {
      Decimal timeInDecimalFormat1 = this.common.GetTimeInDecimalFormat("13:00:00");
      Decimal timeInDecimalFormat2 = this.common.GetTimeInDecimalFormat("10:00:00");
      Decimal timeInDecimalFormat3 = this.common.GetTimeInDecimalFormat("09:00:00");
      Decimal timeInDecimalFormat4 = this.common.GetTimeInDecimalFormat("07:00:00");
      Decimal timeInDecimalFormat5 = this.common.GetTimeInDecimalFormat("06:00:00");
      Decimal timeInDecimalFormat6 = this.common.GetTimeInDecimalFormat("05:00:00");
      return !(sectorCount == 1M) || !(dutyLengthInDecimal > timeInDecimalFormat2) ? (!(sectorCount == 2M) || !(dutyLengthInDecimal > timeInDecimalFormat2) ? (!(sectorCount == 3M) || !(dutyLengthInDecimal > timeInDecimalFormat3) ? (!(sectorCount == 4M) || !(dutyLengthInDecimal > timeInDecimalFormat4) ? (!(sectorCount == 5M) || !(dutyLengthInDecimal > timeInDecimalFormat4) ? (!(sectorCount == 6M) || !(dutyLengthInDecimal > timeInDecimalFormat4) ? (!(sectorCount == 7M) || !(dutyLengthInDecimal > timeInDecimalFormat5) ? (!(sectorCount == 8M) || !(dutyLengthInDecimal <= timeInDecimalFormat6) ? (!(sectorCount == 8M) || !(dutyLengthInDecimal > timeInDecimalFormat6) ? timeInDecimalFormat1 : this.common.GetMinTimeFactorWithCubicPower(1.42M, dutyLengthInDecimal, 1.8492M, -2.4548M, 1.6057M, 0.2602M)) : this.common.GetTimeInDecimalFormat("13:30:00")) : this.common.GetMinTimeFactorWithCubicPower(1.38M, dutyLengthInDecimal, 1.5975M, -1.9603M, 1.3104M, 0.2881M)) : this.common.GetMinTimeFactorWithCubicPower(1.38M, dutyLengthInDecimal, 1.8396M, -2.4031M, 1.5648M, 0.2215M)) : this.common.GetMinTimeFactorWithCubicPower(1.33M, dutyLengthInDecimal, 1.8899M, -2.3873M, 1.4646M, 0.2485M)) : this.common.GetMinTimeFactorWithCubicPower(1.33M, dutyLengthInDecimal, 1.1815M, -1.0476M, 0.6637M, 0.3916M)) : this.common.GetMinTimeFactorWithCubicPower(1.33M, dutyLengthInDecimal, 2.3158M, -3.6912M, 2.6364M, -0.0834M)) : this.common.GetMinTimeFactorWithCubicPower(1.33M, dutyLengthInDecimal, 2.569M, -4.3899M, 3.2715M, -0.2819M)) : this.common.GetMinTimeFactorWithCubicPower(1.29M, dutyLengthInDecimal, 2.157M, -3.4093M, 2.4771M, -0.0809M);
    }

    private Decimal IneffectiveContextualRestPartOne_GetMinCrewRefTimeFactor(
      Decimal dutyCrewRefTimeInDecimal,
      Decimal sectorCount)
    {
      Decimal timeInDecimalFormat1 = this.common.GetTimeInDecimalFormat("20:00:00");
      Decimal timeInDecimalFormat2 = this.common.GetTimeInDecimalFormat("19:00:00");
      return !(sectorCount == 1M) || !(dutyCrewRefTimeInDecimal > timeInDecimalFormat1) ? (!(sectorCount == 2M) || !(dutyCrewRefTimeInDecimal > timeInDecimalFormat1) ? (!(sectorCount == 3M) || !(dutyCrewRefTimeInDecimal > timeInDecimalFormat1) ? (!(sectorCount == 4M) || !(dutyCrewRefTimeInDecimal > timeInDecimalFormat1) ? (!(sectorCount == 5M) || !(dutyCrewRefTimeInDecimal > timeInDecimalFormat2) ? (!(sectorCount == 6M) || !(dutyCrewRefTimeInDecimal > timeInDecimalFormat2) ? (!(sectorCount == 7M) || !(dutyCrewRefTimeInDecimal > timeInDecimalFormat2) ? (!(sectorCount == 8M) || !(dutyCrewRefTimeInDecimal > timeInDecimalFormat2) ? 0M : this.common.GetMinTimeFactorWithCubicPower(0.42M, dutyCrewRefTimeInDecimal, -0.6076M, 2.6023M, -3.0838M, 1.1202M)) : this.common.GetMinTimeFactorWithCubicPower(0.41M, dutyCrewRefTimeInDecimal, -0.5537M, 2.4808M, -3.0344M, 1.1322M)) : this.common.GetMinTimeFactorWithCubicPower(0.41M, dutyCrewRefTimeInDecimal, -0.4560M, 2.1415M, -2.6687M, 1.0070M)) : this.common.GetMinTimeFactorWithCubicPower(0.40M, dutyCrewRefTimeInDecimal, -0.3067M, 1.6423M, -2.1258M, 0.8124M)) : this.common.GetMinTimeFactorWithCubicPower(0.40M, dutyCrewRefTimeInDecimal, -0.066M, 0.7852M, -1.1484M, 0.4515M)) : this.common.GetMinTimeFactorWithCubicPower(0.40M, dutyCrewRefTimeInDecimal, -0.1304M, 1.0585M, -1.5252M, 0.6148M)) : this.common.GetMinTimeFactorWithCubicPower(0.40M, dutyCrewRefTimeInDecimal, -0.1499M, 1.1515M, -1.6767M, 0.6902M)) : this.common.GetMinTimeFactorWithCubicPower(0.38M, dutyCrewRefTimeInDecimal, 0.0323M, 0.5163M, -0.9803M, 0.4447M);
    }
  }
}
