using System;
using System.Runtime.Serialization;

namespace Neo4jClientRepository
{
    [Serializable]
    public class ObjectNotFoundException : Exception
    {
          public ObjectNotFoundException(string message)
            : base(message)
        {

        }

        public ObjectNotFoundException()
        {
            // Add any type-specific logic, and supply the default message.
        }

        public ObjectNotFoundException(string message, Exception innerException) :
            base(message, innerException)
        {
            // Add any type-specific logic for inner exceptions.
        }

        protected ObjectNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }
    }
}
