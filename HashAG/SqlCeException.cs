using System;
using System.Runtime.Serialization;

namespace HashAG
{
    [Serializable]
    internal class SqlCeException : Exception
    {
        public SqlCeException()
        {
        }

        public SqlCeException(string message) : base(message)
        {
        }

        public SqlCeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SqlCeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}