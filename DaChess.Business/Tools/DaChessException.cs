using System;
using System.Runtime.Serialization;

namespace DaChess.Business
{
    public class DaChessException : Exception
    {
        public DaChessException()
        {
        }

        public DaChessException(string message) : base(message)
        {
        }

        public DaChessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DaChessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
