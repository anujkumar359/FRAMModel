using System.Runtime.InteropServices;

//#nullable disable
namespace EasyJet.FRAMModel.Engine.ExternalContract
{

    /// <summary>
    /// Represents a request for FRM model.
    /// </summary>
    [ComVisible(true)]
    [Guid("5B2BFBBA-023B-4E38-A328-121919BDC500")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("EasyJet.FRAMModel.FRMModelRequest")]
    public class FRMModelRequest : IFRMModelRequest
    {
        /// <summary>
        /// Gets or sets the index in block.
        /// </summary>
        [DispId(1)]
        public int[] IdxInBlock { get; set; }

        /// <summary>
        /// Gets or sets the operational sector count.
        /// </summary>
        [DispId(2)]
        public int[] OperationalSectorCount { get; set; }

        /// <summary>
        /// Gets or sets the ISA home standby flag.
        /// </summary>
        [DispId(3)]
        public int[] IsaHomeStandbyFlag { get; set; }

        /// <summary>
        /// Gets or sets the start date local time.
        /// </summary>
        [DispId(4)]
        public string[] StartDateLocalTime { get; set; }

        /// <summary>
        /// Gets or sets the start time local time.
        /// </summary>
        [DispId(5)]
        public string[] StartTimeLocalTime { get; set; }

        /// <summary>
        /// Gets or sets the end date local time.
        /// </summary>
        [DispId(6)]
        public string[] EndDateLocalTime { get; set; }

        /// <summary>
        /// Gets or sets the end time local time.
        /// </summary>
        [DispId(7)]
        public string[] EndTimeLocalTime { get; set; }

        /// <summary>
        /// Gets or sets the end date crew reference time.
        /// </summary>
        [DispId(8)]
        public string[] EndDateCrewReferenceTime { get; set; }

        /// <summary>
        /// Gets or sets the end time crew reference time.
        /// </summary>
        [DispId(9)]
        public string[] EndTimeCrewReferenceTime { get; set; }

        /// <summary>
        /// Gets or sets the start date time zulu.
        /// </summary>
        [DispId(10)]
        public string[] StartDateTimeZulu { get; set; }

        /// <summary>
        /// Gets or sets the end date time zulu.
        /// </summary>
        [DispId(11)]
        public string[] EndDateTimeZulu { get; set; }

        /// <summary>
        /// Gets or sets the duty length.
        /// </summary>
        [DispId(12)]
        public string[] DutyLength { get; set; }

        /// <summary>
        /// Gets or sets the is duty morning start.
        /// </summary>
        [DispId(13)]
        public int[] IsDutyMorningStart { get; set; }

        /// <summary>
        /// Gets or sets the is duty evening finish.
        /// </summary>
        [DispId(14)]
        public int[] IsDutyEveningFinish { get; set; }

        /// <summary>
        /// Gets or sets the is duty night finish.
        /// </summary>
        [DispId(15)]
        public int[] IsDutyNightFinish { get; set; }

        /// <summary>
        /// Gets or sets the is duty elongated.
        /// </summary>
        [DispId(16)]
        public int[] IsDutyElongated { get; set; }

        /// <summary>
        /// Gets or sets the is duty high sector.
        /// </summary>
        [DispId(17)]
        public int[] IsDutyHighSector { get; set; }

        /// <summary>
        /// Gets or sets the hours between midnight.
        /// </summary>
        [DispId(18)]
        public string[] HoursBetweenMidnight { get; set; }

        /// <summary>
        /// Gets or sets the crew route.
        /// </summary>
        [DispId(19)]
        public string[] CrewRoute { get; set; }

        /// <summary>
        /// Gets or sets the night stop flag.
        /// </summary>
        [DispId(20)]
        public string[] NightStopFlag { get; set; }

        /// <summary>
        /// Gets or sets the commute time.
        /// </summary>
        [DispId(21)]
        public string[] CommuteTime { get; set; }

        /// <summary>
        /// Gets or sets the SBY callout.
        /// </summary>
        [DispId(22)]
        public string[] SbyCallout { get; set; }
    }
}
