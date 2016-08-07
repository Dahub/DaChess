using System;
using System.Runtime.Serialization;

namespace DaChessV2.Business
{
    public class DaChessException : Exception
    {
        public DaChessException(bool log = false)
        {
            if (log)
                LogHelper.AddLog(LogLevel.ERROR, "Erreur sans message", String.Empty);
        }

        public DaChessException(string message, bool log = false) : base(message)
        {
            if (log)
                LogHelper.AddLog(LogLevel.ERROR, message, String.Empty);
        }

        public DaChessException(string message, Exception innerException, bool log = false) : base(message, innerException)
        {
            if (log)
                LogHelper.AddLog(LogLevel.ERROR, message, innerException.ToString());
        }

        protected DaChessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
