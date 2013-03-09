using System;
using System.Runtime.Serialization;

namespace Neo4jClientRepository
{
    [Serializable]
    public class RelationshipTypeKeyNotFoundException : Exception
    {
             public RelationshipTypeKeyNotFoundException(string message)
            : base(message)
        {

        }

        public RelationshipTypeKeyNotFoundException()
        {
            // Add any type-specific logic, and supply the default message.
        }

        public RelationshipTypeKeyNotFoundException(string message, Exception innerException) :
            base(message, innerException)
        {
            // Add any type-specific logic for inner exceptions.
        }

        protected RelationshipTypeKeyNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Implement type-specific serialization constructor logic.
        }
    }
}
