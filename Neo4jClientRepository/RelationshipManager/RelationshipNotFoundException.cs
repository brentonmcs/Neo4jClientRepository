using System;
using System.Runtime.Serialization;

namespace Neo4jClientRepository.RelationshipManager
{
    [Serializable]
    public class RelationshipNotFoundException : Exception
    {
            public RelationshipNotFoundException(string message)
            : base(message)
        {

        }

        public RelationshipNotFoundException()
        {
            // Add any type-specific logic, and supply the default message.
        }

        public RelationshipNotFoundException(string message, Exception innerException) :
            base(message, innerException)
        {
            // Add any type-specific logic for inner exceptions.
        }

        protected RelationshipNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }
    }
}
