using EasyJet.FRAMModel.Engine.Entities;
using System;
using System.Collections.Generic;


namespace EasyJet.FRAMModel.Engine.Calculator
{
  internal class Stage5Calculator
  {
    public Stage5Response Calculate(Stage4Response stage4Response)
    {
      return new Stage5Response()
      {
        PreliminaryFRAMScoreList = this.GetPreliminaryFRAMScoreList(stage4Response),
        DutyBlock = stage4Response.DutyBlock,
        ConsecutiveElongatedDutyList = stage4Response.ConsecutiveElongatedDutyList,
        ConsecutiveHighSectorDutyList = stage4Response.ConsecutiveHighSectorDutyList,
        MorningToEveningTransitionContributionList = stage4Response.MorningToEveningTransitionContributionList,
        IneffectiveContextualRestPartOneList = stage4Response.IneffectiveContextualRestPartOneList,
        NightToEarlyContributionList = stage4Response.NightToEarlyContributionList,
        SuboptimalNightContributionList = stage4Response.SuboptimalNightContributionList,
        LongDutiesContributionList = stage4Response.LongDutiesContributionList,
        SectorContributionList = stage4Response.SectorContributionList,
        IneffectiveContextualRestPartTwoList = stage4Response.IneffectiveContextualRestPartTwoList,
        EarlyToNightContributionList = stage4Response.EarlyToNightContributionList,
        IneffectiveContextualRestPartList = stage4Response.IneffectiveContextualRestPartList
      };
    }

    public List<Decimal> GetPreliminaryFRAMScoreList(Stage4Response stage4Response)
    {
      List<Decimal> preliminaryFramScoreList = new List<Decimal>();
      for (int index = 0; index < stage4Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        Decimal num = 0M;
        if (index != 0)
          num = this.GetSumOfPreliminaryFRAMScore(index, stage4Response);
        preliminaryFramScoreList.Add(num);
      }
      return preliminaryFramScoreList;
    }

    private Decimal GetSumOfPreliminaryFRAMScore(int index, Stage4Response stage4Response)
    {
      Decimal preliminaryFramScore = 0M;
      for (int index1 = 0; index1 <= Math.Min(index - 1, 7); ++index1)
      {
        int secondValue = this.CalculateSecondValue(index1 + 1, stage4Response);
        Decimal framScoreValue = this.GetFRAMScoreValue(index1 + 1, secondValue);
        Decimal previousStageValues = this.CalculateSumOfPreviousStageValues(index, index1 + 1, stage4Response);
        preliminaryFramScore += framScoreValue * previousStageValues;
      }
      return preliminaryFramScore;
    }

    private Decimal CalculateSumOfPreviousStageValues(
      int index,
      int innerIndex,
      Stage4Response stage4Response)
    {
      return stage4Response.MorningToEveningTransitionContributionList[index - innerIndex] + stage4Response.LongDutiesContributionList[index - innerIndex] + stage4Response.SectorContributionList[index - innerIndex] + stage4Response.IneffectiveContextualRestPartList[index - innerIndex] + stage4Response.EarlyToNightContributionList[index - innerIndex] + stage4Response.NightToEarlyContributionList[index - innerIndex] + stage4Response.SuboptimalNightContributionList[index - innerIndex];
    }

    public int CalculateSecondValue(int innerIndex, Stage4Response stage4Response)
    {
      int val1 = 0;
      for (int index = 0; index <= innerIndex; ++index)
      {
        int int32 = Convert.ToInt32(stage4Response.DutyBlock.DutyPeriods[index].IsaHomeStandbyFlag);
        val1 = Math.Max(val1, int32);
      }
      return val1;
    }

    private Decimal GetFRAMScoreValue(int val1, int val2)
    {
      Decimal framScoreValue = 0M;
      if (val2 == 0)
      {
        switch (val1)
        {
          case 1:
            framScoreValue = 0.92M;
            break;
          case 2:
            framScoreValue = 0.81M;
            break;
          case 3:
            framScoreValue = 0.70M;
            break;
          case 4:
            framScoreValue = 0.55M;
            break;
          case 5:
            framScoreValue = 0.40M;
            break;
          case 6:
            framScoreValue = 0.20M;
            break;
          case 7:
            framScoreValue = 0.10M;
            break;
        }
      }
      else
      {
        switch (val1)
        {
          case 1:
            framScoreValue = 0.82M;
            break;
          case 2:
            framScoreValue = 0.71M;
            break;
          case 3:
            framScoreValue = 0.60M;
            break;
          case 4:
            framScoreValue = 0.45M;
            break;
          case 5:
            framScoreValue = 0.30M;
            break;
          case 6:
            framScoreValue = 0.20M;
            break;
          case 7:
            framScoreValue = 0.00M;
            break;
        }
      }
      return framScoreValue;
    }
  }
}
