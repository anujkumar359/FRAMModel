using System;
using System.Runtime.Serialization;


namespace EasyJet.FRAMModel.Engine.Exceptions
{
  internal class InvalidDataValueException : ApplicationException
  {
    public int ErrorNumber => 1002;

    public InvalidDataValueException(string fieldName, int index, string fieldValue)
    {
      this.FieldName = fieldName;
      this.Index = index;
      this.FieldValue = fieldValue;
    }

    public InvalidDataValueException(
      string message,
      string fieldName,
      int index,
      string fieldValue)
      : base(message)
    {
      this.FieldName = fieldName;
      this.Index = index;
      this.FieldValue = fieldValue;
    }

    public InvalidDataValueException(
      string message,
      Exception innerException,
      string fieldName,
      int index,
      string fieldValue)
      : base(message, innerException)
    {
      this.FieldName = fieldName;
      this.Index = index;
      this.FieldValue = fieldValue;
    }

    public InvalidDataValueException(
      SerializationInfo info,
      StreamingContext context,
      string fieldName,
      int index,
      string fieldValue)
      : base(info, context)
    {
      this.FieldName = fieldName;
      this.Index = index;
      this.FieldValue = fieldValue;
    }

    public int Index { get; set; }

    public string FieldName { get; set; }

    public string FieldValue { get; set; }
  }
}
