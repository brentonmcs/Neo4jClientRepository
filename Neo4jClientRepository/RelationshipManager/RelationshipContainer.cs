using System;
using System.Collections.Generic;

namespace Neo4jClientRepository.RelationshipManager
{
    public class RelationshipContainer
    {
        public RelationshipContainer(List<Type> target, List<Type> source, Type payload)
        {
            Payload = payload;
            Source = source;
            Target = target;
        }

        public Type Payload { get; private set; }

        public List<Type> Source { get; private set; }

        public List<Type> Target { get; private set; }
    }
}