using EasyJet.FRAMModel.Engine.Entities;
using System.Collections.Generic;


namespace EasyJet.FRAMModel.Engine.Calculator
{
  internal class ScoreCalculator
  {
    public ScoreList Calculate(List<DutyBlock> dutyBlockList)
    {
      ScoreList scoreList = new ScoreList();
      List<DutyBlockScore> dutyBlockScoreList = new List<DutyBlockScore>();
      foreach (DutyBlock dutyBlock in dutyBlockList)
      {
        DutyBlockScore dutyBlockScore = new Stage6Calculator()
                    .Calculate(new Stage5Calculator()
                    .Calculate(new Stage4Calculator()
                    .Calculate(new Stage3Calculator()
                    .Calculate(new Stage2Calculator()
                    .Calculate(new Stage1Calculator()
                    .Calculate(dutyBlock))))));
        dutyBlockScoreList.Add(dutyBlockScore);
      }
      scoreList.DutyBlockScoreList = dutyBlockScoreList;
      return scoreList;
    }
  }
}
