using System;

namespace Neo4jClientRepository.IdGenerator
{
    public class NotLoadedException : Exception
    {
        public NotLoadedException(string message)
            :base(message)
        {
            
        }
    }
}
