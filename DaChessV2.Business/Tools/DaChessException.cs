using System;
using System.Runtime.Serialization;

namespace DaChessV2.Business
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

        /// <summary>
        /// En release, on ne retourne pas toute l'exception (qui sera inscrite dans la console)
        /// mais uniquement le message
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
#if DEBUG
            return base.ToString();
#else
            return this.Message;
#endif
        }

        protected DaChessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
