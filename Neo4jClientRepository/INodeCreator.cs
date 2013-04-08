using System;
using System.Linq;
using CacheController;
using Neo4jClient;
using Neo4jClientRepository.IdGen;
using Neo4jClientRepository.RelationshipManager;

namespace Neo4jClientRepository
{
    public interface INodeRepoCreator
    {
        IIdGenerator IDGenerator { get; set; }
        ICachingService CachingService { get; set; }

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

    public class NodeRepoCreator : INodeRepoCreator
    {
        private readonly IGraphClient _graphClient;
        private readonly INeo4jRelationshipManager _relationshipManager;

        public  ICachingService CachingService { get; set; }        
        public  IIdGenerator IDGenerator { get; set; }

        public NodeRepoCreator(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager)
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;                        
        }

        

        public INeo4NodeRepository<TModel> CreateNode<TRelationship, TModel>(string itemCode)
            where TRelationship : Relationship
            where TModel : class,IDBSearchable, new()
        {
            return new Neo4NodeRepository<TModel, TRelationship>(_graphClient, _relationshipManager, IDGenerator, itemCode, null, CachingService);
        }
      

        private NodeReference CheckReferenceNode<TNode>(Type referenceType) where TNode : class ,new()
        {
            NodeReference nodeReference = null;
            if (referenceType != null)
                nodeReference = GetReferenceNode<TNode>(referenceType);
            return nodeReference;
        }

        private NodeReference GetReferenceNode<TNode>(Type relationship) where TNode :   class ,new()
        {
            if (relationship == null) throw new ArgumentNullException("relationship");

            var typeKey = _relationshipManager.GetTypeKey(relationship);
            var refNode = _graphClient
                        .Cypher
                        .Start(new { root = _graphClient.RootNode })
                        .Match("root-[:" + typeKey + "]-node")
                        .Return<Node<TNode>>("node")
                        .Results
                        .SingleOrDefault();

            if (refNode != null) return refNode.Reference;

            var referenceRelationship = _relationshipManager.GetRelationshipObjectParticipant<TNode>(typeof(TNode), _graphClient.RootNode.GetType(), _graphClient.RootNode);
            return _graphClient.Create(new TNode(), new[] { referenceRelationship });
        }

        public INeo4NodeRepository<TModel> CreateNode<TRelationship, TNode, TModel>(string itemCode, Type referenceType) 
            where TRelationship : Relationship where TNode : class, new() 
            where TModel : class, IDBSearchable, new()
        {
            return new Neo4NodeRepository<TModel, TRelationship>(_graphClient, _relationshipManager, IDGenerator, itemCode, CheckReferenceNode<TNode>(referenceType), CachingService);
        }
        

        public INeo4jRelatedNodes<TSource, TTarget> CreateRelated<TSource, TTarget, TRelationship>(
            INeo4NodeRepository<TSource> sourceRepo, INeo4NodeRepository<TTarget> targetRepo)
            where TSource : class, IDBSearchable, new() 
                where TTarget : class, IDBSearchable, new() 
                where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSource>                 
        {
            if (sourceRepo == null) throw new ArgumentNullException("sourceRepo");
            if (targetRepo == null) throw new ArgumentNullException("targetRepo");
            return new Neo4jRelatedNodes<TSource, TTarget, TRelationship>(_graphClient, _relationshipManager, sourceRepo, targetRepo, CachingService);
        }

     
    }

}
