using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable CheckNamespace
namespace Neo4jClientRepository
// ReSharper restore CheckNamespace
{
    // ReSharper disable InconsistentNaming
    public class Neo4jRelationshipManager : INeo4jRelationshipManager
    // ReSharper restore InconsistentNaming
    {
        private Dictionary<RelationshipContainer, Type> _relationships;

        public Neo4jRelationshipManager()
        {
            RelationshipLocator();
        }

        public T GetRelationshipObject<T>(Type source, Type target, NodeReference linkedObject) where T : class
        {
            return GetConstructor(source, target).Invoke(new object[] { linkedObject }) as T;
        }

        public T GetRelationshipObject<T, TData>(Type source, Type target, NodeReference linkedObject, TData properties, Type payLoad)
            where T : class
            where TData : class, new()
        {            

            var constructor = GetConstructor(source, target, new[] {typeof (NodeReference), typeof (TData)}, payLoad);

            return constructor.Invoke(new object[] { linkedObject, properties }) as T;                        
        }

        public IRelationshipAllowingParticipantNode<T> GetRelationshipObjectParticipant<T>(Type source, Type target, NodeReference linkedObject) where T : class
        {
            return GetConstructor(source, target).Invoke(new object[] { linkedObject }) as IRelationshipAllowingParticipantNode<T>;
        }

        private ConstructorInfo GetConstructor(Type source, Type target, Type[] contstructorParams = null , Type payload = null)
        {
            if (contstructorParams == null)
                contstructorParams = new[] { typeof(NodeReference) };
            var constructor = GetType(source, target, payload).GetConstructor(contstructorParams);

            if (constructor == null)
                throw new RelationshipNotFoundException();
            return constructor;
        }

        public IRelationshipAllowingSourceNode<T> GetRelationshipObjectSource<T>(Type source, Type target, NodeReference linkedObject) where T : class
        {
            return GetConstructor(source, target).Invoke(new object[] { linkedObject }) as IRelationshipAllowingSourceNode<T>;
        }

        public string GetTypeKey(Type source, Type target)
        {
            return GetTypeKey(source, target, null);
        }

        public string GetTypeKey(Type source, Type target, Type payLoad)
        {
            try
            {
                return GetTypeKeyFromContainer(GetType(source, target, payLoad));
                       
            }
            catch (InvalidOperationException)
            {
                
                throw new RelationshipTypeKeyNotFound();
            }
            
        }

        private string GetTypeKeyFromContainer(Type Relationship)
        {
            return
            Relationship
            .GetFields()
                       .Where(x => x.Name == "TypeKey").Single()
                       .GetRawConstantValue()
                       .ToString();
        }

        private static Type GetGenericType(Type i)
        {
            return i.GetGenericArguments().Any() ? i.GetGenericArguments().First() : null;
        }

        private Type GetType(Type source, Type target, Type payload = null)
        {
            try
            {
                var sourceTypeRealtionships = _relationships
                    .Where(x => x.Key.Source.Contains(source))
                    .Where(x => x.Key.Target.Contains(target));

                if (payload != null)
                    sourceTypeRealtionships= sourceTypeRealtionships.Where(x => x.Key.Payload == payload);

                return sourceTypeRealtionships.Single().Value;
            }
            catch (InvalidOperationException)
            {                
                throw new RelationshipNotFoundException();
            }
            
        }

        private void RelationshipLocator()
        {
            _relationships = new Dictionary<RelationshipContainer, Type>();


            var types = AppDomain.CurrentDomain.GetAssemblies().ToList()
                                 .SelectMany(s => s.GetTypes())
                                 .Where(x => x.IsClass)
                                 .Where(x => x.IsSubclassOf(typeof (Relationship)))
                                 .Where(x => x != typeof (Relationship)); //We don't want the actual Relationship class

            
            foreach (var t in types)
            {

                var source = new List<Type>();
                var target = new List<Type>();
                Type payload = null;

                var interfaces = t.GetInterfaces();

                AddFindDataTypesForSourceAndTarget(interfaces,  source,  target);

                if (t.BaseType != null && ((t.BaseType.GetGenericArguments().Any()) &&
                                           (t.BaseType.GetGenericTypeDefinition() == typeof(Relationship<>))))
                    payload = GetGenericType(t.BaseType);


                if (!target.Any() || !source.Any())
                    continue;
                
                _relationships.Add(new RelationshipContainer(target, source, payload), t);
            }
        }

        private static void AddFindDataTypesForSourceAndTarget(IEnumerable<Type> interfaces, List<Type> source, List<Type> target)
        {
            foreach (var i in interfaces)
            {
                FindAttributeType(source, i, typeof (IRelationshipAllowingSourceNode<>));
                    
                FindAttributeType(target, i, typeof(IRelationshipAllowingTargetNode<>));
            }
        }

        private static void FindAttributeType(List<Type> returnList, Type i, Type attributeToFind)
        {            
            if (i.GetGenericTypeDefinition() == attributeToFind)
                returnList.Add( GetGenericType(i));            
        }

        private class RelationshipContainer
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




        public Type GetSourceType(Type type)
        {
          
            return GetRelationshipContainer(type).Source.First(); 

        }

        private RelationshipContainer GetRelationshipContainer(Type type)
        {
            return  _relationships.Where(x => x.Value == type).FirstOrDefault().Key;
        }


        public Type GetTargetType(Type type)
        {
            return GetRelationshipContainer(type).Target.First();
        }


        public Type GetRelationship(Type sourceNode)
        {
            var results = _relationships.Where(x => x.Key.Target.Contains(sourceNode));

            var rootNodeRelationships = results.Where(x => x.Key.Source.Contains(typeof(RootNode)));

            if (rootNodeRelationships.Any())
                return rootNodeRelationships.First().Value;

            return results.Any() ? results.First().Value : null;              
        }


        public string GetTypeKey(Type currentRelationshipType)
        {
            return GetTypeKeyFromContainer(currentRelationshipType);
        }
    }
}