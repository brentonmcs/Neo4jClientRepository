using System;

// ReSharper disable CheckNamespace
namespace Neo4jClientRepository.IdGen
// ReSharper restore CheckNamespace
{
    [Serializable]
    public class NotLoadedException : Exception
    {
        public NotLoadedException(string message)
            :base(message)
        {
            
        }
    }
}
