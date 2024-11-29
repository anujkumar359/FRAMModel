using EasyJet.FRAMModel.Engine.Calculator;
using EasyJet.FRAMModel.Engine.Exceptions;
using EasyJet.FRAMModel.Engine.ExternalContract;
using EasyJet.FRAMModel.SleepWake.Calculator;
using EasyJet.FRAMModel.Utilities;
using System;
using System.Runtime.InteropServices;


namespace EasyJet.FRAMModel.Engine
{
  [ComVisible(true)]
  [Guid("9D316225-FE4F-4CF2-9F6A-58775229DF81")]
  [ClassInterface(ClassInterfaceType.None)]
  [ProgId("EasyJet.FRAMModel.ScoreGenerator")]
  public class ScoreGenerator : IScoreGenerator
  {
    public IFRMModelResponse Generate(IFRMModelRequest request)
    {
      IFRMModelResponse frmModelResponse = (IFRMModelResponse) new FRMModelResponse();
      try
      {
        EntityMapper entityMapper = new EntityMapper();
        frmModelResponse = entityMapper.GetScoreArray(new ScoreCalculator().Calculate(entityMapper.GetDutyBlockList(request)));

        ProcessSleepWake processData = new ProcessSleepWake();
        frmModelResponse.FRMScore = processData.Calculate(request, frmModelResponse.FRMScore);
      }
      catch (ArgumentNullException ex)
      {
        frmModelResponse.ErrorNumber = 1001;
        frmModelResponse.ErrorDescription = ex.ParamName;
        frmModelResponse.FRMScore = (string[]) null;
      }
      catch (InvalidDataValueException ex)
      {
        frmModelResponse.ErrorNumber = ex.ErrorNumber;
        frmModelResponse.ErrorDescription = "InvalidDataValueException for parameter " + ex.FieldName + " at Index " + (object) ex.Index + " contains Value " + ex.FieldValue;
        frmModelResponse.FRMScore = (string[]) null;
      }
      catch (InvalidDataFormatException ex)
      {
        frmModelResponse.ErrorNumber = ex.ErrorNumber;
        frmModelResponse.ErrorDescription = ex.Message;
        frmModelResponse.FRMScore = (string[]) null;
      }
      catch (Exception ex)
      {
        frmModelResponse.ErrorNumber = 1004;
        frmModelResponse.ErrorDescription = ex.Message;
        frmModelResponse.FRMScore = (string[]) null;
      }
      return frmModelResponse;
    }
  }
}
