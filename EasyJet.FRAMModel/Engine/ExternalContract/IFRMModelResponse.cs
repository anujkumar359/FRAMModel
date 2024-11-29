using System.Runtime.InteropServices;

//#nullable disable
namespace EasyJet.FRAMModel.Engine.ExternalContract
{
  [ComVisible(true)]
  [Guid("293045D7-9BFC-46C3-95A7-5046994CD131")]
  [InterfaceType(ComInterfaceType.InterfaceIsDual)]
  public interface IFRMModelResponse
  {
    [DispId(1)]
    string[] FRMScore { get; set; }

    [DispId(2)]
    int ErrorNumber { get; set; }

    [DispId(3)]
    string ErrorDescription { get; set; }
  }
}
