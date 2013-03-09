using System.Linq.Expressions;
using Neo4jClient;
using Neo4jClient.Gremlin;
using System;
using System.Collections.Generic;
using System.Linq;
using CacheController;
using Neo4jClientRepository.RelationshipManager;

namespace Neo4jClientRepository
{

    public class Neo4jRelatedNodes<TNode, TTargetNode, TRelationship> : INeo4jRelatedNodes<TNode, TTargetNode>

        where TNode : class, IDBSearchable, new()
        where TTargetNode : class, IDBSearchable, new()
        where TRelationship : Relationship, IRelationshipAllowingSourceNode<TNode>
    {
        protected bool AllowMultipleRelationships { get; set; }

        private readonly IGraphClient _graphClient;
        private readonly INeo4jRelationshipManager _relationshipManager;
        private readonly INeo4NodeRepository _sourceDataSource;
        private readonly INeo4NodeRepository _targetDataSource;
        private readonly ICachingService _cachingService;

        public Neo4jRelatedNodes(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager,
                                 INeo4NodeRepository sourceDataSource, INeo4NodeRepository targetDataSource,
                                 ICachingService cachingService)
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            _sourceDataSource = sourceDataSource;
            _targetDataSource = targetDataSource;
            _cachingService = cachingService;
            AllowMultipleRelationships = false;
        }

        public void AddRelatedRelationship(string sourceCode, string targetCode)
        {
            AddRelatedRelationship(_sourceDataSource.GetByItemCode<TNode>(sourceCode),
                                   _targetDataSource.GetByItemCode<TTargetNode>(targetCode));
        }

        public void AddRelatedRelationship(Node<TNode> source, Node<TTargetNode> target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            if (MultiplesCheck(source, target)) return;

            _graphClient.CreateRelationship(source.Reference, GetRelatedRelationship(target));

            //TODO need to change this to update the cache not delete?
            if (_cachingService != null)
                _cachingService.DeleteCache(GetCacheKey(source));
        }

        public void AddRelatedRelationship<TData>(Node<TNode> source, Node<TTargetNode> target, TData properties) where TData : class, new()
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            if (MultiplesCheck<TData>(source, target)) return;

            _graphClient.CreateRelationship(source.Reference, GetRelatedRelationship(target, properties));

            if (_cachingService != null)
                _cachingService.DeleteCache(GetCacheKey(source));
        }

        public void AddRelatedRelationship<TData>(string source, string target, TData properties)
            where TData : class, new()
        {
            AddRelatedRelationship(_sourceDataSource.GetByItemCode<TNode>(source),
                                   _targetDataSource.GetByItemCode<TTargetNode>(target), properties);
        }

        public IEnumerable<Node<TSourceNode>> GetCachedRelated<TSourceNode>(Node<TSourceNode> node) where TSourceNode : class, IDBSearchable, new()
        {
            if (node == null) throw new ArgumentNullException("node");
            
            return GetCachedRelated<TSourceNode>(node.Data.ItemSearchCode());
        }

        public IEnumerable<Node<TSourceNode>> GetCachedRelated<TSourceNode>(string relatedCode) where TSourceNode : class, IDBSearchable, new()
        {
            return _cachingService.Cache(GetCacheKey(relatedCode), 1000, new Func<string, IEnumerable<Node<TSourceNode>>>(GetRelatedNodes<TSourceNode>), relatedCode) as IEnumerable<Node<TSourceNode>>;
        }

        public IEnumerable<Node<TSourceNode>> GetCachedRelated<TSourceNode>(int id) where TSourceNode : class, IDBSearchable, new()                                    
        {
            return
                _cachingService.Cache(GetCacheKey(id), 1000, new Func<int, IEnumerable<Node<TSourceNode>>>(GetRelatedNodes<TSourceNode>), id) as IEnumerable<Node<TSourceNode>>;
        }

        public IEnumerable<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(string relatedCode) where TSourceNode : class, IDBSearchable, new()                                    
        {
            return GetRelated<Node<TSourceNode>,TSourceNode>(GetNode<TSourceNode>(relatedCode));
        }

        public IEnumerable<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(int id) where TSourceNode : class, IDBSearchable, new()
        {            
            return GetRelated<Node<TSourceNode>,TSourceNode>(GetNode<TSourceNode>(id));
        }

        private Node<TResult> GetNode<TResult>( object identifier) where TResult : class, IDBSearchable, new()
        {
            var identifierStr = string.Empty;
            if (identifier is string)
                identifierStr = identifier.ToString();

            var idenitiferInt = -1000;
            if (identifier is int)
                idenitiferInt = (int)identifier;

            if (typeof(TResult) == typeof(TNode))
            {
                if (string.IsNullOrEmpty( identifierStr))
                    return _sourceDataSource.GetByItemCode<TResult>(identifierStr);
                if (idenitiferInt >= 0)
                    return _sourceDataSource.GetNodeReferenceById<TResult>(idenitiferInt);
                throw new InvalidSourceNodeException();
            }

            if (string.IsNullOrEmpty(identifierStr))
                return _targetDataSource.GetByItemCode<TResult>(identifierStr);
            if (idenitiferInt >= 0)
                return _targetDataSource.GetNodeReferenceById<TResult>(idenitiferInt);
            throw new InvalidSourceNodeException();

        }


        public IEnumerable<TSourceNode> GetRelated<TSourceNode>(int id) 
            where TSourceNode : class, IDBSearchable, new()
        {
            var node = GetNode<TSourceNode>(id);
            return GetRelated<TSourceNode,TSourceNode>(node);
        }

        public IEnumerable<TSourceNode> GetRelated<TSourceNode>(string relatedCode) where TSourceNode : class, IDBSearchable, new()
        {
            return GetRelated<TSourceNode, TSourceNode>(GetNode<TSourceNode>(relatedCode));           
        }

        public IEnumerable<TResult> GetRelated<TResult, TSourceNode>(Node<TSourceNode> node)            
            where TSourceNode : class, new()
        {
            if (node == null) throw new ArgumentNullException("node");
            var matchText = string.Format("source-[:{0}]-targets", TypeKeyRelatingNodes());

            return node.StartCypher("source").Match(matchText).ReturnDistinct<TResult>("targets").Results;
        }

        public IEnumerable<RelationshipInstance<TData>> GetRelationships<TData>() where TData : class, new()
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
            return GetCacheKey(source.Data.ItemSearchCode());
        }

        private string GetCacheKey(string searchCode)
        {
            return searchCode + GetRootTypeKey();
        }

        private string GetCacheKey(int id)
        {
            return id + GetRootTypeKey();
        }

        private TRelationship GetRelatedRelationship(Node<TTargetNode> target)
        {
            return _relationshipManager.GetRelationshipObject<TRelationship>(typeof(TNode), typeof(TTargetNode), target.Reference);
        }

        private TRelationship GetRelatedRelationship<TData>(Node<TTargetNode> target, TData properties) where TData : class,new()
        {
            return _relationshipManager.GetRelationshipObject<TRelationship, TData>(typeof(TNode), typeof(TTargetNode), target.Reference, properties, typeof(TData));
        }

        private string GetRootTypeKey()
        {
            return _relationshipManager.GetTypeKey(typeof(TNode), typeof(TTargetNode));
        }

        private static string GetSourceRootKey()
        {
            //return _relationshipManager.GetTypeKey()
            return "";
            //TODO Need to fix this...
        }

        private bool MultiplesCheck(Node<TNode> source, Node<TTargetNode> target)
        {
            return (!AllowMultipleRelationships) && (RelationshipExists(source.Reference, target.Data));
        }

        private bool MultiplesCheck<TData>(Node<TNode> source, Node<TTargetNode> target) where TData : class, new()
        {
            return (!AllowMultipleRelationships) && (RelationshipExists(source.Reference, target.Data, typeof(TData)));
        }

        private bool RelationshipExists(NodeReference<TNode> node, TTargetNode target, Type payload = null)
        {
            return node                
                   .Out(TypeKeyRelatingNodes(payload),FilterQuery(target))
                   .GremlinCount() > 0;
        }

        private static Expression<Func<TTargetNode, bool>> FilterQuery(TTargetNode target)
        {
            return x=>x.Id ==  target.Id;
        }
      
        private string TypeKeyRelatingNodes(Type payload = null)
        {
            if (payload == null) throw new ArgumentNullException("payload");
            return _relationshipManager.GetTypeKey(typeof(TNode), typeof(TTargetNode), payload);
        }
    }
}