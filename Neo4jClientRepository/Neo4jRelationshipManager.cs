using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public class Neo4jRelationshipManager : INeo4jRelationshipManager
    {
        private Dictionary<RelationshipContainer, Type> Relationships;

        public Neo4jRelationshipManager()
        {
            RelationshipLocator();
        }

        public T GetRelationshipObject<T>(Type Source, Type Target, NodeReference linkedObject) where T : class
        {
            var type = GetType(Source, Target);
            var constructor = type.GetConstructor(new[] { typeof(NodeReference) });
            var test = constructor.Invoke(new object[] { linkedObject }) as T;
            return test;
        }

        public T GetRelationshipObject<T, TData>(Type Source, Type Target, NodeReference linkedObject, TData properties, Type Payload)
            where T : class
            where TData : class, new()
        {
            var constructor = GetType(Source, Target, Payload).GetConstructor(new[] { typeof(NodeReference), typeof(TData) });
            var test = constructor.Invoke(new object[] { linkedObject, properties }) as T;
            return test;
        }

        public IRelationshipAllowingParticipantNode<T> GetRelationshipObjectParticipant<T>(Type Source, Type Target, NodeReference linkedObject) where T : class
        {
            var constructor = GetType(Source, Target).GetConstructor(new[] { typeof(NodeReference) });
            var test = constructor.Invoke(new object[] { linkedObject }) as IRelationshipAllowingParticipantNode<T>;
            return test;
        }

        public IRelationshipAllowingSourceNode<T> GetRelationshipObjectSource<T>(Type Source, Type Target, NodeReference linkedObject) where T : class
        {
            var constructor = GetType(Source, Target).GetConstructor(new[] { typeof(NodeReference) });
            var test = constructor.Invoke(new object[] { linkedObject }) as IRelationshipAllowingSourceNode<T>;
            return test;
        }

        public string GetTypeKey(Type Source, Type Target)
        {
            return GetTypeKey(Source, Target, null);
        }

        public string GetTypeKey(Type Source, Type Target, Type PayLoad)
        {
            return GetType(Source, Target, PayLoad).GetFields().Where(x => x.Name == "TypeKey").Single().GetRawConstantValue().ToString();
        }

        private Type GetGenericType(Type i)
        {
            if (i.GetGenericArguments().Any())
                return i.GetGenericArguments().First();
            return null;
        }

        private Type GetType(Type Source, Type Target, Type Payload = null)
        {
            return Relationships
                    .Where(x => x.Key.Source == Source)
                    .Where(x => x.Key.Target == Target)
                    .Where(x => x.Key.Payload == Payload)
                    .Single()
                    .Value;
        }

        private void RelationshipLocator()
        {
            Relationships = new Dictionary<RelationshipContainer, Type>();

            var types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass).Where(x => x.IsSubclassOf(typeof(Relationship)));

            foreach (var t in types)
            {
                var interfaces = t.GetInterfaces();
                var container = new RelationshipContainer();
                foreach (var i in interfaces)
                {
                    if (i.GetGenericTypeDefinition() == typeof(IRelationshipAllowingTargetNode<>))
                        container.Target = GetGenericType(i);

                    if (i.GetGenericTypeDefinition() == typeof(IRelationshipAllowingSourceNode<>))
                        container.Source = GetGenericType(i);
                }

                if ((t.BaseType.GetGenericArguments().Any()) &&
                    (t.BaseType.GetGenericTypeDefinition() == typeof(Relationship<>)))
                    container.Payload = GetGenericType(t.BaseType);

                if (container.Target == null || container.Source == null)
                    throw new Exception("Container not setup correctly");

                Relationships.Add(container, t);
            }
        }

        private class RelationshipContainer
        {
            public Type Payload { get; set; }

            public Type Source { get; set; }

            public Type Target { get; set; }
        }
    }
}