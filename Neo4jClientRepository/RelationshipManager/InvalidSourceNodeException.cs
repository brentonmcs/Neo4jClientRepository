using System;
using System.Runtime.Serialization;

namespace Neo4jClientRepository.RelationshipManager

{
    [Serializable]
    public class InvalidSourceNodeException : Exception
    {
        public InvalidSourceNodeException(string message)
            : base(message)
        {

        }

        public InvalidSourceNodeException()
        {
            // Add any type-specific logic, and supply the default message.
        }

        public InvalidSourceNodeException(string message, Exception innerException) :
            base(message, innerException)
        {
            // Add any type-specific logic for inner exceptions.
        }

        protected InvalidSourceNodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }
    }

}
