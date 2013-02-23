using System;
using System.Runtime.Serialization;

namespace Neo4jClientRepository
{
    [Serializable]
    public class CacheDelegateMethodException : Exception
    {
        public CacheDelegateMethodException()
        { }

        protected CacheDelegateMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public CacheDelegateMethodException(string message)
            : base(message)
        {

        }

        public CacheDelegateMethodException(string message, Exception exception)
            : base(message, exception)
        {

        }
    }
}
