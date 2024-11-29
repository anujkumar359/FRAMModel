using System;
using System.Collections.Generic;


namespace EasyJet.FRAMModel.Engine.Entities
{
  internal class Stage4Response
  {
    public DutyBlock DutyBlock { get; set; }

    public List<int> ConsecutiveElongatedDutyList { get; set; }

    public List<int> ConsecutiveHighSectorDutyList { get; set; }

    public List<Decimal> MorningToEveningTransitionContributionList { get; set; }

    public List<Decimal> IneffectiveContextualRestPartOneList { get; set; }

    public List<Decimal> NightToEarlyContributionList { get; set; }

    public List<Decimal> SuboptimalNightContributionList { get; set; }

    public List<Decimal> LongDutiesContributionList { get; set; }

    public List<Decimal> SectorContributionList { get; set; }

    public List<Decimal> IneffectiveContextualRestPartTwoList { get; set; }

    public List<Decimal> EarlyToNightContributionList { get; set; }

    public List<Decimal> IneffectiveContextualRestPartList { get; set; }
  }
}
