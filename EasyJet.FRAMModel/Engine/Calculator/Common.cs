using System;


namespace EasyJet.FRAMModel.Engine.Calculator
{
  internal class Common
  {
    public Decimal GetTimeInDecimalFormat(string time)
    {
      string[] strArray = time.Split(':');
      return (Decimal) new TimeSpan(int.Parse(strArray[0]), int.Parse(strArray[1]), 0).TotalHours / 24M;
    }

    public Decimal GetMinTimeFactorWithCubicPower(
      Decimal minValue1,
      Decimal timeInDecimal,
      Decimal cubicValue,
      Decimal squareValue,
      Decimal singleValue,
      Decimal defaultValue)
    {
      Decimal num1 = timeInDecimal * timeInDecimal * timeInDecimal;
      Decimal num2 = timeInDecimal * timeInDecimal;
      Decimal val2 = cubicValue * num1 + squareValue * num2 + singleValue * timeInDecimal + defaultValue;
      return Math.Min(minValue1, val2);
    }

    public Decimal GetMinTimeFactorWithSquarePower(
      Decimal minValue1,
      Decimal timeInDecimal,
      Decimal squareValue,
      Decimal singleValue,
      Decimal defaultValue)
    {
      Decimal num = timeInDecimal * timeInDecimal;
      Decimal val2 = squareValue * num + singleValue * timeInDecimal + defaultValue;
      return Math.Min(minValue1, val2);
    }
  }
}
