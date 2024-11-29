using System.Runtime.InteropServices;

//#nullable disable
namespace EasyJet.FRAMModel.Engine.ExternalContract
{
  [ComVisible(true)]
  [Guid("819FC1B3-12E7-4A92-90F2-42FA05CC2972")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("EasyJet.FRAMModel.FRMModelResponse")]
  public class FRMModelResponse : IFRMModelResponse
  {
    [DispId(1)]
    public string[] FRMScore { get; set; }

    [DispId(2)]
    public int ErrorNumber { get; set; }

    [DispId(3)]
    public string ErrorDescription { get; set; }
  }
}
