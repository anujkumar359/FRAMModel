using System;
using System.Collections.Generic;


namespace EasyJet.FRAMModel.Engine.Entities
{
  internal class Stage2Response
  {
    public DutyBlock DutyBlock { get; set; }

    public List<int> ConsecutiveElongatedDutyList { get; set; }

    public List<int> ConsecutiveHighSectorDutyList { get; set; }

    public List<Decimal> MorningToEveningTransitionContributionList { get; set; }

    public List<Decimal> IneffectiveContextualRestPartOneList { get; set; }

    public List<Decimal> NightToEarlyContributionList { get; set; }

    public List<Decimal> SuboptimalNightContributionList { get; set; }
  }
}
