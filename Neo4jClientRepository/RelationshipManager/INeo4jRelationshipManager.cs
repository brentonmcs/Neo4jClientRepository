﻿using Neo4jClient;
using System;

namespace Neo4jClientRepository
{
    public interface INeo4jRelationshipManager
    {
        T GetRelationshipObject<T>(Type source, Type target, NodeReference linkedObject) where T : class;

        T GetRelationshipObject<T, TData>(Type source, Type target, NodeReference linkedObject, TData properties, Type payLoad)
            where T : class
            where TData : class, new();

        IRelationshipAllowingParticipantNode<T> GetRelationshipObjectParticipant<T>(Type source, Type target, NodeReference linkedObject) where T : class;

        IRelationshipAllowingSourceNode<T> GetRelationshipObjectSource<T>(Type source, Type target, NodeReference linkedObject) where T : class;

        string GetTypeKey(Type source, Type target);

        string GetTypeKey(Type source, Type target, Type payLoad);

        Type GetSourceType(Type type);

        Type GetTargetType(Type type);

        Type GetRelationship(Type sourceNode);

        string GetTypeKey(Type currentRelationshipType);
    }
}