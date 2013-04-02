using System;
using System.Linq;
using CacheController;
using Neo4jClient;
using Neo4jClientRepository.IdGenerator;
using Neo4jClientRepository.RelationshipManager;

namespace Neo4jClientRepository
{
    public interface INodeRepoCreator
    {
         INeo4NodeRepository CreateNode<TRelationship, TNode>(string itemCode, Type referenceType) 
            where TRelationship : Relationship
            where TNode :   class ,new();

        INeo4NodeRepository CreateNode<TRelationship>(string itemCode)
            where TRelationship : Relationship;

        INeo4jRelatedNodes<TSource, TTarget> CreateRelated<TSource, TTarget, TRelationship>(INeo4NodeRepository sourceRepo, INeo4NodeRepository targetRepo = null)
            where TSource : class, IDBSearchable, new()
            where TTarget : class, IDBSearchable, new()
            where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSource>;
    }

    public class NodeRepoCreator : INodeRepoCreator
    {
        private readonly IGraphClient _graphClient;
        private readonly INeo4jRelationshipManager _relationshipManager;
        private readonly ICachingService _cachingService;
        private readonly IIDGenerator _idGenerator;

        public NodeRepoCreator(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager,
                               ICachingService cachingService, IIDGenerator idGenerator)
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            _cachingService = cachingService;
            _idGenerator = idGenerator;
        }

        public INeo4NodeRepository CreateNode<TRelationship, TNode>(string itemCode, Type referenceType)
            where TRelationship : Relationship
            where TNode :   class ,new()
        {
            return new Neo4NodeRepository<TRelationship>(_graphClient, _relationshipManager, _idGenerator, itemCode, CheckReferenceNode<TNode>(referenceType), _cachingService);
        }

        public INeo4NodeRepository CreateNode<TRelationship>(string itemCode)
            where TRelationship : Relationship
        {
            return new Neo4NodeRepository<TRelationship>(_graphClient, _relationshipManager, _idGenerator, itemCode, null, _cachingService);
        }

        public INeo4jRelatedNodes<TSource, TTarget> CreateRelated<TSource, TTarget, TRelationship>(INeo4NodeRepository sourceRepo, INeo4NodeRepository targetRepo = null)
            where TSource : class, IDBSearchable, new()
            where TTarget : class, IDBSearchable, new()
            where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSource>
        {
            if (sourceRepo.GetSourceType() != typeof(TSource))
                throw new RelationshipNotFoundException("Source Repository does not match Source Type");

            if ((targetRepo ?? sourceRepo).GetSourceType() != typeof(TTarget))
                throw new RelationshipNotFoundException("Target Repository does not match Target Type");

            return new Neo4jRelatedNodes<TSource, TTarget, TRelationship>(_graphClient, _relationshipManager, sourceRepo, targetRepo ?? sourceRepo, _cachingService);
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
            var refNode = _graphClient.RootNode
                        .StartCypher("root")
                        .Match("root-[:" + typeKey + "]-node")
                        .Return<Node<TNode>>("node")
                        .Results
                        .SingleOrDefault();

            if (refNode != null) return refNode.Reference;

            var referenceRelationship = _relationshipManager.GetRelationshipObjectParticipant<TNode>(typeof(TNode), _graphClient.RootNode.GetType(), _graphClient.RootNode);
            return _graphClient.Create(new TNode(), new[] { referenceRelationship });
        }      

    }

}
