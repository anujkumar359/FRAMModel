using System;
using System.Runtime.Serialization;


namespace EasyJet.FRAMModel.Engine.Exceptions
{
  internal class InvalidDataFormatException : ApplicationException
  {
    public int ErrorNumber => 1003;

    public InvalidDataFormatException()
    {
    }

    public InvalidDataFormatException(string message)
      : base(message)
    {
    }

    public InvalidDataFormatException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    public InvalidDataFormatException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override Exception GetBaseException() => base.GetBaseException();

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
    }

    public override string ToString() => base.ToString();

    public override bool Equals(object obj) => base.Equals(obj);

    public override int GetHashCode() => base.GetHashCode();
  }
}
