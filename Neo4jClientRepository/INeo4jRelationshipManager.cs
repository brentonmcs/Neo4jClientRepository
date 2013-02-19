using Neo4jClient;
using System;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public interface INeo4jRelationshipManager
    {
        T GetRelationshipObject<T>(Type Source, Type Target, NodeReference linkedObject) where T : class;

        T GetRelationshipObject<T, TData>(Type Source, Type Target, NodeReference linkedObject, TData properties, Type PayLoad)
            where T : class
            where TData : class, new();

        IRelationshipAllowingParticipantNode<T> GetRelationshipObjectParticipant<T>(Type Source, Type Target, NodeReference linkedObject) where T : class;

        IRelationshipAllowingSourceNode<T> GetRelationshipObjectSource<T>(Type Source, Type Target, NodeReference linkedObject) where T : class;

        string GetTypeKey(Type Source, Type Target);

        string GetTypeKey(Type Source, Type Target, Type PayLoad);
    }
}