using System.Runtime.InteropServices;

//#nullable disable
namespace EasyJet.FRAMModel.Engine.ExternalContract
{
  [ComVisible(true)]
  [Guid("1493B48F-35F0-42A0-AE9A-C49EAFB49854")]
  [InterfaceType(ComInterfaceType.InterfaceIsDual)]
  public interface IScoreGenerator
  {
    IFRMModelResponse Generate(IFRMModelRequest request);
  }
}
