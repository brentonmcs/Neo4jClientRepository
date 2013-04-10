using System;
using CacheController;
using Neo4jClient;
using Neo4jClientRepository.IdGen;

namespace Neo4jClientRepository
{
    public interface INodeRepoCreator
    {
        IIdGenerator IDGenerator { get; set; }
        //ICachingService CachingService { get; set; }

        INeo4NodeRepository<TModel> CreateNode<TRelationship, TNode, TModel>(string itemCode, Type referenceType) 
            where TRelationship : Relationship
            where TNode :   class ,new()
            where TModel :class,IDBSearchable, new();

        INeo4NodeRepository<TModel> CreateNode<TRelationship, TModel>(string itemCode)
            where TRelationship : Relationship
            where TModel : class,IDBSearchable, new();

        INeo4jRelatedNodes<TSource, TTarget> CreateRelated<TSource, TTarget, TRelationship>(
            INeo4NodeRepository<TSource> sourceRepo, INeo4NodeRepository<TTarget> targetRepo)
            where TSource : class, IDBSearchable, new()
            where TTarget : class, IDBSearchable, new()
            where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSource>;

    }
}