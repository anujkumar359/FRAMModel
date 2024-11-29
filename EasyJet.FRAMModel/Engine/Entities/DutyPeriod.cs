using System;


namespace EasyJet.FRAMModel.Engine.Entities
{
  internal class DutyPeriod
  {
    public int DutyPeriodOfDutyBlock { get; set; }

    public int OperationalSectorCount { get; set; }

    public bool IsaHomeStandbyFlag { get; set; }

    public DateTime StartDateLocalTime { get; set; }

    public string StartTimeLocalTime { get; set; }

    public DateTime EndDateLocalTime { get; set; }

    public string EndTimeLocalTime { get; set; }

    public DateTime EndDateCrewReferenceTime { get; set; }

    public string EndTimeCrewReferenceTime { get; set; }

    public DateTime StartDateTimeZulu { get; set; }

    public DateTime EndDateTimeZulu { get; set; }

    public string DutyLength { get; set; }

    public bool IsDutyMorningStart { get; set; }

    public bool IsDutyEveningFinish { get; set; }

    public bool IsDutyNightFinish { get; set; }

    public bool IsDutyElongated { get; set; }

    public bool IsDutyHighSector { get; set; }

    public string HoursBetweenMidnight { get; set; }
  }
}
