using EasyJet.FRAMModel.Engine.Entities;


namespace EasyJet.FRAMModel.Engine.Calculator
{
  internal class Stage1Calculator
  {
    public Stage1Response Calculate(DutyBlock dutyBlock)
    {
      return new Stage1Response() { DutyBlock = dutyBlock };
    }
  }
}
