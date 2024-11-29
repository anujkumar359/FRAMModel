using EasyJet.FRAMModel.Engine.Entities;
using System;
using System.Collections.Generic;

//#nullable disable
namespace EasyJet.FRAMModel.Engine.Calculator
{
    internal class Stage6Calculator
    {
        //This code is integrated with part2.
        Dictionary<string, decimal> p = new Dictionary<string, decimal>
                {
                    { "b", 1.042m }, // Morning to Evening Transition Contribution
                    { "c", 0.989m }, // Long Duties Contribution
                    { "d", 1.046m }, // Sector Contribution
                    { "e", 1.157m }, // Ineffective Contextual Rest Contribution
                    { "f", 1.0m },   // Early To Night Contribution
                    { "g", 1.0m },   // Night to Early Contribution
                    { "h", 1.0m },   // Suboptimal Night Contribution
                    { "i", 1.038m }  // Preliminary FRAM Score
                };


        public DutyBlockScore Calculate(Stage5Response stage5Response)
        {
            return new DutyBlockScore()
            {
                DutyPeriodScoreList = this.GetFinalFRAMScore(stage5Response)
            };
        }

        public List<DutyPeriodScore> GetFinalFRAMScore(Stage5Response stage5Response)
        {
            List<DutyPeriodScore> finalFramScore = new List<DutyPeriodScore>();
            for (int index = 0; index < stage5Response.DutyBlock.DutyBlockDutyCount; ++index)
            {
                DutyPeriodScore dutyPeriodScore = new DutyPeriodScore();
                Decimal val2 = (stage5Response.MorningToEveningTransitionContributionList[index] * p["b"])
                            + (stage5Response.LongDutiesContributionList[index] * p["c"])
                            + (stage5Response.SectorContributionList[index] * p["d"])
                            + (stage5Response.IneffectiveContextualRestPartList[index] * p["e"])
                            + (stage5Response.EarlyToNightContributionList[index] * p["f"])
                            + (stage5Response.NightToEarlyContributionList[index] * p["g"])
                            + (stage5Response.SuboptimalNightContributionList[index] * p["h"]);

                Decimal Total = (val2 + (stage5Response.PreliminaryFRAMScoreList[index] * p["i"]));

                Decimal num = index != 0 ?
                                ((Total < 0M) ?
                                    stage5Response.PreliminaryFRAMScoreList[index]
                                    : Total)
                                : Math.Max(0M, val2);
                dutyPeriodScore.Score = num;
                finalFramScore.Add(dutyPeriodScore);
            }
            return finalFramScore;
        }
    }
}
