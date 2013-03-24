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
            AddRelatedRelationship(_sourceDataSource.GetByItemCode<TNode>(sourceCode), _targetDataSource.GetByItemCode<TTargetNode>(targetCode));
        }

       
        public void AddRelatedRelationship(Node<TNode> source, Node<TTargetNode> target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");

            if (MultiplesCheck(source, target)) return;

            _graphClient.CreateRelationship(source.Reference, GetRelatedRelationship(target));

            //TODO need to change this to update the cache not delete?
            if (_cachingService != null)
                _cachingService.DeleteCache(GetCacheKey(_sourceDataSource));
        }

        
        public void AddRelatedRelationship<TData>(Node<TNode> source, Node<TTargetNode> target, TData properties) where TData : class, new()
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            if (MultiplesCheck<TData>(source, target)) return;

            _graphClient.CreateRelationship(source.Reference, GetRelatedRelationship(target, properties));

            if (_cachingService != null)
                _cachingService.DeleteCache(GetCacheKey(_sourceDataSource));
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

            return GetRelated<Node<TSourceNode>>(node.Reference);
        }

        public IEnumerable<Node<TSourceNode>> GetCachedRelated<TSourceNode>(string relatedCode, bool searchSource) where TSourceNode : class, IDBSearchable, new()
        {
            return _cachingService.Cache(GetCacheKey(relatedCode), 1000, new Func<string, bool, IEnumerable<Node<TSourceNode>>>(GetRelatedNodes<TSourceNode>), relatedCode, searchSource) as IEnumerable<Node<TSourceNode>>;
        }

        public IEnumerable<Node<TSourceNode>> GetCachedRelated<TSourceNode>(long id, bool searchSource) where TSourceNode : class, IDBSearchable, new()                                    
        {
            //return
            //    _cachingService.Cache(GetCacheKey(id), 1000, new Func<int, IEnumerable<Node<TSourceNode>>>(GetRelatedNodes<TSourceNode>), id) as IEnumerable<Node<TSourceNode>>

            return GetRelatedNodes<TSourceNode>(id, searchSource);
        }

        public IEnumerable<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(string relatedCode, bool searchSource) where TSourceNode : class, IDBSearchable, new()
        {
            var searchNode = GetNode(relatedCode, searchSource);
            return searchNode == null ? null : GetRelated<Node<TSourceNode>>(searchNode);
        }

        public IEnumerable<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(long id, bool searchSource) where TSourceNode : class, IDBSearchable, new()
        {
            var searchNode = GetNode(id, searchSource);
            return searchNode == null ? null : GetRelated<Node<TSourceNode>>(searchNode);
        }


        //This will find Friends Of Friends, What other products users purchased 
        public IEnumerable<TSourceNode> FindOtherRelated<TSourceNode>(Node<TSourceNode> startingNode) where TSourceNode :IDBSearchable
        {
            return startingNode
                .StartCypher("startNode")
                .Match("startNode-[:" + GetRootTypeKey() + "]-othernode-[:" + GetRootTypeKey() + "]-otherStartNodes")
                .Where<TSourceNode>(otherStartNodes => otherStartNodes.Id != startingNode.Data.Id)
                .Return<TSourceNode>("otherStartNodes")
                .Results;
        }
        
        private NodeReference GetNode( object identifier, bool searchSource)
        {
            var identifierStr = string.Empty;
            if (identifier is string)
                identifierStr = identifier.ToString();

            var idenitiferLong = -1000L;
            if (identifier is long)
                idenitiferLong = long.Parse(identifier.ToString());

            if (searchSource)
            {
                if (!string.IsNullOrEmpty( identifierStr))
                    return _sourceDataSource.GetByItemCode<TNode>(identifierStr).Reference;
                if (idenitiferLong >= 0)
                    return _sourceDataSource.GetNodeReferenceById<TNode>(idenitiferLong).Reference;
                throw new InvalidSourceNodeException();
            }

            if (!string.IsNullOrEmpty(identifierStr))
                return _targetDataSource.GetByItemCode<TTargetNode>(identifierStr).Reference;
            if (idenitiferLong >= 0)
                return _targetDataSource.GetNodeReferenceById<TTargetNode>(idenitiferLong).Reference;
            throw new InvalidSourceNodeException();

        }


        public IEnumerable<TSourceNode> GetRelated<TSourceNode>(long id, bool searchSource) 
            where TSourceNode : class, IDBSearchable, new()
        {
            var node = GetNode(id, searchSource);
            return GetRelated<TSourceNode>(node);
        }

        public IEnumerable<TSourceNode> GetRelated<TSourceNode>(string relatedCode, bool searchSource) where TSourceNode : class, IDBSearchable, new()
        {
            return GetRelated<TSourceNode>(GetNode(relatedCode, searchSource));           
        }

        public IEnumerable<TResult> GetRelated<TResult>(NodeReference node)
        {
            if (node == null) throw new ArgumentNullException("node");
            var matchText = string.Format("source-[:{0}]-targets", TypeKeyRelatingNodes());            
            var results = node.StartCypher("source").Match(matchText).ReturnDistinct<TResult>("targets");

            var query = results.Query.QueryText;
            return results.Results.ToList();
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

        private string GetCacheKey(INeo4NodeRepository source)
        {
            return GetCacheKey(source.ItemCodeIndexName);
        }

        private string GetCacheKey(string searchCode)
        {
            return searchCode + GetRootTypeKey();
        }

        private string GetCacheKey(long id)
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
            return _relationshipManager.GetTypeKey(typeof(TNode), typeof(TTargetNode), payload);
        }


        public IEnumerable<TSourceNode> GetAllCachedRelated<TSourceNode>()
        {
            var matchText = _relationshipManager.GetMatchStringToRootForSource(typeof (TSourceNode));
            return _graphClient.RootNode.StartCypher("root").Match(matchText).ReturnDistinct<TSourceNode>("targets").Results;
        }




       
    }
}