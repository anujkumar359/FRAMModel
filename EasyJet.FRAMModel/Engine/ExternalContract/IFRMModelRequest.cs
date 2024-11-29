using System.Runtime.InteropServices;

//#nullable disable
namespace EasyJet.FRAMModel.Engine.ExternalContract
{

    /// <summary>
    /// Represents the contract for the FRM Model request.
    /// </summary>
    [ComVisible(true)]
    [Guid("41257D86-84AB-47A4-B09F-110E110988F8")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IFRMModelRequest
    {
        /// <summary>
        /// Gets or sets the array of IdxInBlock values.
        /// </summary>
        [DispId(1)]
        int[] IdxInBlock { get; set; }

        /// <summary>
        /// Gets or sets the array of OperationalSectorCount values.
        /// </summary>
        [DispId(2)]
        int[] OperationalSectorCount { get; set; }

        /// <summary>
        /// Gets or sets the array of IsaHomeStandbyFlag values.
        /// </summary>
        [DispId(3)]
        int[] IsaHomeStandbyFlag { get; set; }

        /// <summary>
        /// Gets or sets the array of StartDateLocalTime values.
        /// </summary>
        [DispId(4)]
        string[] StartDateLocalTime { get; set; }

        /// <summary>
        /// Gets or sets the array of StartTimeLocalTime values.
        /// </summary>
        [DispId(5)]
        string[] StartTimeLocalTime { get; set; }

        /// <summary>
        /// Gets or sets the array of EndDateLocalTime values.
        /// </summary>
        [DispId(6)]
        string[] EndDateLocalTime { get; set; }

        /// <summary>
        /// Gets or sets the array of EndTimeLocalTime values.
        /// </summary>
        [DispId(7)]
        string[] EndTimeLocalTime { get; set; }

        /// <summary>
        /// Gets or sets the array of EndDateCrewReferenceTime values.
        /// </summary>
        [DispId(8)]
        string[] EndDateCrewReferenceTime { get; set; }

        /// <summary>
        /// Gets or sets the array of EndTimeCrewReferenceTime values.
        /// </summary>
        [DispId(9)]
        string[] EndTimeCrewReferenceTime { get; set; }

        /// <summary>
        /// Gets or sets the array of StartDateTimeZulu values.
        /// </summary>
        [DispId(10)]
        string[] StartDateTimeZulu { get; set; }

        /// <summary>
        /// Gets or sets the array of EndDateTimeZulu values.
        /// </summary>
        [DispId(11)]
        string[] EndDateTimeZulu { get; set; }

        /// <summary>
        /// Gets or sets the array of DutyLength values.
        /// </summary>
        [DispId(12)]
        string[] DutyLength { get; set; }

        /// <summary>
        /// Gets or sets the array of IsDutyMorningStart values.
        /// </summary>
        [DispId(13)]
        int[] IsDutyMorningStart { get; set; }

        /// <summary>
        /// Gets or sets the array of IsDutyEveningFinish values.
        /// </summary>
        [DispId(14)]
        int[] IsDutyEveningFinish { get; set; }

        /// <summary>
        /// Gets or sets the array of IsDutyNightFinish values.
        /// </summary>
        [DispId(15)]
        int[] IsDutyNightFinish { get; set; }

        /// <summary>
        /// Gets or sets the array of IsDutyElongated values.
        /// </summary>
        [DispId(16)]
        int[] IsDutyElongated { get; set; }

        /// <summary>
        /// Gets or sets the array of IsDutyHighSector values.
        /// </summary>
        [DispId(17)]
        int[] IsDutyHighSector { get; set; }

        /// <summary>
        /// Gets or sets the array of HoursBetweenMidnight values.
        /// </summary>
        [DispId(18)]
        string[] HoursBetweenMidnight { get; set; }

        /// <summary>
        /// Gets or sets the array of CrewRoute values.
        /// </summary>
        [DispId(19)]
        string[] CrewRoute { get; set; }

        /// <summary>
        /// Gets or sets the array of NightStopFlag values.
        /// </summary>
        [DispId(20)]
        string[] NightStopFlag { get; set; }

        /// <summary>
        /// Gets or sets the array of CommuteTime values.
        /// </summary>
        [DispId(21)]
        string[] CommuteTime { get; set; }

        /// <summary>
        /// Gets or sets the array of SbyCallout values.
        /// </summary>
        [DispId(22)]
        string[] SbyCallout { get; set; }
    }
}
