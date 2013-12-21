using System;
using System.Runtime.Serialization;

namespace Lycus.Satori
{
    [Serializable]
    public class LoaderException : Exception
    {
        public LoaderException()
        {
        }

        public LoaderException(string message)
            : base(message)
        {
        }

        public LoaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected LoaderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
