using CacheController;
using Neo4jClient;
using Neo4jClient.Gremlin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public abstract class Neo4jRelatedNodes<TNode, TargetNode, TRelationship> : INeo4jRelatedNodes<TNode, TargetNode, TRelationship>
        where TNode : class,IDBSearchable<TNode>, new()
        where TargetNode : class,IDBSearchable<TargetNode>, new()
        where TRelationship : Relationship, IRelationshipAllowingSourceNode<TNode>
    {
        protected bool AllowMultipleRelationships = false;

        private readonly IGraphClient _graphClient;
        private readonly INeo4jRelationshipManager _relationshipManager;
        private readonly INeo4jService<TNode> _sourceDataSource;
        private readonly INeo4jService<TargetNode> _targetDataSource;

        protected Neo4jRelatedNodes(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, INeo4jService<TNode> sourceDataSource, INeo4jService<TargetNode> targetDataSource)
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            _sourceDataSource = sourceDataSource;
            _targetDataSource = targetDataSource;
        }

        public void AddRelatedRelationship(string sourceCode, string targetCode)
        {
            AddRelatedRelationship(_sourceDataSource.GetCached(sourceCode), _targetDataSource.GetCached(targetCode));
        }

        public void AddRelatedRelationship(Node<TNode> source, Node<TargetNode> target)
        {
            if (MultiplesCheck(source, target))
                return;
            _graphClient.CreateRelationship(source.Reference, GetRelatedRelationship(target));
            CachingService.DeleteCache(GetCacheKey(source));
        }

        public void AddRelatedRelationship<TData>(Node<TNode> source, Node<TargetNode> target, TData properties) where TData : class, new()
        {
            if (MultiplesCheck<TData>(source, target))
                return;

            _graphClient.CreateRelationship(source.Reference, GetRelatedRelationship(target, properties));
            CachingService.DeleteCache(GetCacheKey(source));
        }

        public void AddRelatedRelationship<TData>(string source, string target, TData properties) where TData : class, new()
        {
            AddRelatedRelationship(_sourceDataSource.Get(source), _targetDataSource.Get(target), properties);
        }

        public List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(Node<TSourceNode> node)
            where TSourceNode : IDBSearchable<TSourceNode>
        {
            return GetCachedRelated<TSourceNode>(node.Data.ItemSearchCode());
        }

        public List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(string relatedCode)
        {
            return CachingService.Cache(GetCacheKey(relatedCode), 1000, new Func<string, List<Node<TSourceNode>>>(GetRelatedNodes<TSourceNode>), relatedCode) as List<Node<TSourceNode>>;
        }

        public List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(int id)
        {
            return CachingService.Cache(GetCacheKey(id), 1000, new Func<int, List<Node<TSourceNode>>>(GetRelatedNodes<TSourceNode>), id) as List<Node<TSourceNode>>;
        }

        public List<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(Node<TSourceNode> node)
        {
            var matchText = string.Format("source-[:{0}]-targets", TypeKeyRelatingNodes());

            return
                node
                .StartCypher("source")
                .Match(matchText)
                   .ReturnDistinct<Node<TSourceNode>>("targets")
                   .Results
                   .ToList();
        }

        public List<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(string relatedCode)
        {
            if (typeof(TSourceNode) == typeof(TNode))
            // ReSharper disable SuspiciousTypeConversion.Global
                return GetRelatedNodes(_sourceDataSource.Get(relatedCode)) as List<Node<TSourceNode>>;
            // ReSharper restore SuspiciousTypeConversion.Global
            if (typeof(TSourceNode) == typeof(TargetNode))
            // ReSharper disable SuspiciousTypeConversion.Global
                return GetRelatedNodes(_targetDataSource.Get(relatedCode)) as List<Node<TSourceNode>>;
            // ReSharper restore SuspiciousTypeConversion.Global

            throw new InvalidSourceNodeException();
        }

        public List<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(int id)
        {
            if (typeof(TSourceNode) == typeof(TNode))
            // ReSharper disable SuspiciousTypeConversion.Global
                return GetRelatedNodes(_sourceDataSource.Get(id)) as List<Node<TSourceNode>>;
            // ReSharper restore SuspiciousTypeConversion.Global
            if (typeof(TSourceNode) == typeof(TargetNode))
            // ReSharper disable SuspiciousTypeConversion.Global
                return GetRelatedNodes(_targetDataSource.Get(id)) as List<Node<TSourceNode>>;
            // ReSharper restore SuspiciousTypeConversion.Global

            throw new InvalidSourceNodeException();
        }
        
        public List<TSourceNode> GetRelated<TSourceNode>(int id)
        {
            if (typeof (TSourceNode) == typeof (TNode))
                // ReSharper disable SuspiciousTypeConversion.Global
                return GetRelated(_sourceDataSource.Get(id)) as List<TSourceNode>;
            // ReSharper restore SuspiciousTypeConversion.Global
            if (typeof(TSourceNode) == typeof(TargetNode))
                // ReSharper disable SuspiciousTypeConversion.Global
                return GetRelated(_targetDataSource.Get(id)) as List<TSourceNode>;
            // ReSharper restore SuspiciousTypeConversion.Global

            throw new InvalidSourceNodeException();
        }

        public List<TSourceNode> GetRelated<TSourceNode>(string relatedCode)
        {
            
            if (typeof(TSourceNode) == typeof(TNode))
                // ReSharper disable SuspiciousTypeConversion.Global
                return GetRelated(_sourceDataSource.Get(relatedCode)) as List<TSourceNode>;
            // ReSharper restore SuspiciousTypeConversion.Global
            if (typeof(TSourceNode) == typeof(TargetNode))
                // ReSharper disable SuspiciousTypeConversion.Global
                return GetRelated(_targetDataSource.Get(relatedCode)) as List<TSourceNode>;
            // ReSharper restore SuspiciousTypeConversion.Global

            throw new InvalidSourceNodeException();
        }

        public List<TSourceNode> GetRelated<TSourceNode>(Node<TSourceNode> node)
        {
            var matchText = string.Format("source-[:{0}]-targets", TypeKeyRelatingNodes());

            return
                node
                .StartCypher("source")
                .Match(matchText)
                   .ReturnDistinct<TSourceNode>("targets")
                   .Results
                   .ToList();
        }

        public List<RelationshipInstance<TData>> GetRelationships<TData>() where TData : class, new()
        {
            var relatedTypeKey = TypeKeyRelatingNodes(typeof(TData));

            var matchSource = string.Format("root-[:{0}]-source-[r:{1}]-targets", GetSourceRootKey(), relatedTypeKey);

            return
                _graphClient
                .RootNode
                .StartCypher("root")
                .Match(matchSource)
                .Return<RelationshipInstance<TData>>("r")
                .Results
                .ToList();
        }

        private string GetCacheKey(Node<TNode> source)
        {
            return source.Data.ItemSearchCode() + GetRootTypeKey();
        }

        private string GetCacheKey(string searchCode)
        {
            return searchCode + GetRootTypeKey();
        }

        private string GetCacheKey(int id)
        {
            return id + GetRootTypeKey();
        }

        private TRelationship GetRelatedRelationship(Node<TargetNode> target)
        {
            return _relationshipManager.GetRelationshipObject<TRelationship>(typeof(TNode), typeof(TargetNode), target.Reference);
        }

        private TRelationship GetRelatedRelationship<TData>(Node<TargetNode> target, TData properties) where TData : class,new()
        {
            return _relationshipManager.GetRelationshipObject<TRelationship, TData>(typeof(TNode), typeof(TargetNode), target.Reference, properties, typeof(TData));
        }

        private string GetRootTypeKey()
        {
            return _relationshipManager.GetTypeKey(typeof(TNode), typeof(TargetNode));
        }

        private string GetSourceRootKey()
        {
            return _sourceDataSource.GetRootNodeKey();
        }

        private bool MultiplesCheck(Node<TNode> source, Node<TargetNode> target)
        {
            return (!AllowMultipleRelationships) && (RelationshipExists(source.Reference, target.Data));
        }

        private bool MultiplesCheck<TData>(Node<TNode> source, Node<TargetNode> target) where TData : class, new()
        {
            return (!AllowMultipleRelationships) && (RelationshipExists(source.Reference, target.Data, typeof(TData)));
        }

        private bool RelationshipExists(NodeReference<TNode> node, TargetNode target, Type payload = null)
        {
            return node
                   .Out(TypeKeyRelatingNodes(payload), FilterQuery(node))
                   .GremlinCount() > 0;
        }

        private object FilterQuery(NodeReference node)
        {
            throw new NotImplementedException();
        }

        private string TypeKeyRelatingNodes(Type payload = null)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return _relationshipManager.GetTypeKey(typeof(TNode), typeof(TargetNode), payload);
        }
    }
}