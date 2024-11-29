using EasyJet.FRAMModel.Engine.Entities;
using System;
using System.Collections.Generic;


namespace EasyJet.FRAMModel.Engine.Calculator
{
  internal class Stage4Calculator
  {
    private Common common = new Common();

    public Stage4Response Calculate(Stage3Response stage3Response)
    {
      return new Stage4Response()
      {
        IneffectiveContextualRestPartList = this.GetIneffectiveContextualRestPart(stage3Response),
        DutyBlock = stage3Response.DutyBlock,
        ConsecutiveElongatedDutyList = stage3Response.ConsecutiveElongatedDutyList,
        ConsecutiveHighSectorDutyList = stage3Response.ConsecutiveHighSectorDutyList,
        MorningToEveningTransitionContributionList = stage3Response.MorningToEveningTransitionContributionList,
        IneffectiveContextualRestPartOneList = stage3Response.IneffectiveContextualRestPartOneList,
        NightToEarlyContributionList = stage3Response.NightToEarlyContributionList,
        SuboptimalNightContributionList = stage3Response.SuboptimalNightContributionList,
        LongDutiesContributionList = stage3Response.LongDutiesContributionList,
        SectorContributionList = stage3Response.SectorContributionList,
        IneffectiveContextualRestPartTwoList = stage3Response.IneffectiveContextualRestPartTwoList,
        EarlyToNightContributionList = stage3Response.EarlyToNightContributionList
      };
    }

    public List<Decimal> GetIneffectiveContextualRestPart(Stage3Response stage3Response)
    {
      List<Decimal> contextualRestPart = new List<Decimal>();
      for (int index = 0; index < stage3Response.DutyBlock.DutyBlockDutyCount; ++index)
      {
        Decimal num = 0M;
        if (stage3Response.IneffectiveContextualRestPartTwoList[index] == 0M)
          contextualRestPart.Add(num);
        else if (stage3Response.DutyBlock.DutyPeriods[index].DutyPeriodOfDutyBlock != 1)
        {
          Decimal minTimeFactor = this.IneffectiveContextualRestPart_GetMinTimeFactor(stage3Response.IneffectiveContextualRestPartTwoList[index]);
          contextualRestPart.Add(minTimeFactor);
        }
        else
          contextualRestPart.Add(num);
      }
      return contextualRestPart;
    }

    public Decimal IneffectiveContextualRestPart_GetMinTimeFactor(
      Decimal ineffectiveContextualRestPartTwo)
    {
      Decimal minTimeFactor = 0M;
      Decimal timeInDecimalFormat1 = this.common.GetTimeInDecimalFormat("07:45:00");
      Decimal timeInDecimalFormat2 = this.common.GetTimeInDecimalFormat("19:00:00");
      minTimeFactor = !(ineffectiveContextualRestPartTwo < -timeInDecimalFormat1) ? (!(-timeInDecimalFormat1 <= ineffectiveContextualRestPartTwo) || !(ineffectiveContextualRestPartTwo < timeInDecimalFormat2) ? 500M : this.IneffectiveContextualRestPart_GetMinTimeFactorWithFourPower(ineffectiveContextualRestPartTwo)) : -6M;
      return minTimeFactor;
    }

    private Decimal IneffectiveContextualRestPart_GetMinTimeFactorWithFourPower(
      Decimal ineffectiveContextualRestPartTwo)
    {
      return 1126.9M * (ineffectiveContextualRestPartTwo * ineffectiveContextualRestPartTwo * ineffectiveContextualRestPartTwo * ineffectiveContextualRestPartTwo) + 14.145M * (ineffectiveContextualRestPartTwo * ineffectiveContextualRestPartTwo * ineffectiveContextualRestPartTwo) + 42.146M * (ineffectiveContextualRestPartTwo * ineffectiveContextualRestPartTwo) + 44.061M * ineffectiveContextualRestPartTwo - 0.3455M;
    }
  }
}
