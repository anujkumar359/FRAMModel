using System.Collections.Generic;


namespace EasyJet.FRAMModel.Engine.Entities
{
  internal class DutyBlock
  {
    public int DutyBlockDutyCount { get; set; }

    public IList<DutyPeriod> DutyPeriods { get; set; }
  }
}
