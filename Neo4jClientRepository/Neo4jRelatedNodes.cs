using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using CacheController;
using Neo4jClientRepository.RelationshipManager;
using Neo4jClient.Cypher;

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
        private readonly INeo4NodeRepository<TNode> _sourceDataSource;
        private readonly INeo4NodeRepository<TTargetNode> _targetDataSource;
        private readonly ICachingService _cachingService;

        private readonly Type _payload;

        public Neo4jRelatedNodes(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager,
                                 INeo4NodeRepository<TNode> sourceDataSource, INeo4NodeRepository<TTargetNode> targetDataSource,
                                 ICachingService cachingService)
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            _sourceDataSource = sourceDataSource;
            _targetDataSource = targetDataSource;
            _cachingService = cachingService;
            AllowMultipleRelationships = false;

            _payload = _relationshipManager.GetPayloadType(typeof(TRelationship));
        }

        
        public void AddRelatedRelationship(string sourceCode, string targetCode)
        {
            AddRelatedRelationship(_sourceDataSource.GetNodeByItemCode(sourceCode), _targetDataSource.GetNodeByItemCode(targetCode));
        }


        public void AddRelatedRelationship(long sourceId, long targetId)
        {
            var sourceNode = _sourceDataSource.GetNodeReferenceById(sourceId);
            var targetNode = _targetDataSource.GetNodeReferenceById(targetId);
            AddRelatedRelationship(sourceNode, targetNode);
        }

        public void AddRelatedRelationship<TData>(long sourceId, long targetId, TData properties) where TData : class, IPayload, new()
        {
            CheckPayload(properties);
                
            var sourceNode = _sourceDataSource.GetNodeReferenceById(sourceId);
            var targetNode = _targetDataSource.GetNodeReferenceById(targetId);
            AddRelatedRelationship(sourceNode, targetNode, properties);
        }

        private void CheckPayload<TData>(TData properties) where TData : class, new()
        {
            if (_payload != null && properties == null) 
                throw new PayloadMissingException();
        }

        private void CheckPayload()
        {
            if (_payload != null) throw new PayloadMissingException();
        }
        public void AddRelatedRelationship(Node<TNode> source, Node<TTargetNode> target)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            CheckPayload();

            if (MultiplesCheck(source, target)) return;

            _graphClient.CreateRelationship(source.Reference, GetRelatedRelationship(target));

            //TODO need to change this to update the cache not delete?
            if (_cachingService != null)
                _cachingService.DeleteCache(GetCacheKey(_sourceDataSource));
        }
        
        public void AddRelatedRelationship<TData>(Node<TNode> source, Node<TTargetNode> target, TData properties) 
                        where TData : class,IPayload, new()
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            CheckPayload(properties);

            if (MultiplesCheck(source, target, properties)) return;

            _graphClient.CreateRelationship(source.Reference, GetRelatedRelationship(target, properties));

            if (_cachingService != null)
                _cachingService.DeleteCache(GetCacheKey(_sourceDataSource));
        }

        public void AddRelatedRelationship<TData>(string source, string target, TData properties)
            where TData : class, IPayload, new()
        {
            AddRelatedRelationship(_sourceDataSource.GetNodeByItemCode(source),
                                   _targetDataSource.GetNodeByItemCode(target), properties);
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
            return
                _cachingService.Cache(GetCacheKey(id), 1000,
                                      new Func<long, bool, IEnumerable<Node<TSourceNode>>>(GetRelatedNodes<TSourceNode>),
                                      id, searchSource) as IEnumerable<Node<TSourceNode>>;
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
        public IEnumerable<TSourceNode> FindOtherRelated<TSourceNode>(Node<TSourceNode> startingNode, string typeKey) where TSourceNode :IDBSearchable
        {
            var matchQuery = string.Format("startNodes-[:{0}]-othernode-[:{0}]-otherStartNodes", typeKey);

            if (startingNode == null)
                throw new ArgumentNullException("startingNode");

            return 
                    startingNode
                        .StartCypher("startNodes")
                        .Match(matchQuery)
                        .Where<TSourceNode, TSourceNode>((startNodes, otherStartNodes) => startNodes.Id != otherStartNodes.Id)                                    
                        .Return<TSourceNode>("otherStartNodes").Results;
        }

        private NodeReference GetNode( object identifier, bool searchSource)
        {
            
            var defaultIdentifiers = SetDefaultIdentiferValues(identifier);

            if (searchSource)
            {
                if (!string.IsNullOrEmpty(defaultIdentifiers.Item1))
                    return _sourceDataSource.GetNodeByItemCode(defaultIdentifiers.Item1).Reference;
                if (defaultIdentifiers.Item2 >= 0)
                    return _sourceDataSource.GetNodeReferenceById(defaultIdentifiers.Item2).Reference;
                throw new InvalidSourceNodeException();
            }

            if (!string.IsNullOrEmpty(defaultIdentifiers.Item1))
                return _targetDataSource.GetNodeByItemCode(defaultIdentifiers.Item1).Reference;
            if (defaultIdentifiers.Item2 > 0)
                return _targetDataSource.GetNodeReferenceById(defaultIdentifiers.Item2).Reference;

            throw new InvalidSourceNodeException();

        }

        private static Tuple<string, long> SetDefaultIdentiferValues(object identifier)
        {
            var identifierStr = string.Empty;
            if (identifier is string)
                identifierStr = identifier.ToString();

            var idenitiferLong = -1000L;
            if (identifier is long)
                idenitiferLong = long.Parse(identifier.ToString());
            return new Tuple<string,long>(identifierStr, idenitiferLong);
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
            return node.StartCypher("source").Match(matchText).ReturnDistinct<TResult>("targets").Results.ToList();                        
        }

        public IEnumerable<RelationshipInstance<TData>> GetRelationships<TData>() where TData : class, IPayload, new()
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

        private string GetCacheKey(INeo4NodeRepository<TNode> source)
        {
            return GetCacheKey(source.ItemCodeIndexName);
        }

        private string GetCacheKey(string searchCode)
        {
            return searchCode + GetRootTypeKey();
        }


        private string GetCacheKey(long searchCode)
        {
            return searchCode + GetRootTypeKey();
        }
      
        private TRelationship GetRelatedRelationship(Node<TTargetNode> target)
        {
            return _relationshipManager.GetRelationshipObject<TRelationship>(typeof(TNode), typeof(TTargetNode), target.Reference);
        }

        private TRelationship GetRelatedRelationship<TData>(Node<TTargetNode> target, TData properties) where TData : class,new()
        {
            CheckPayload(properties);
            return _relationshipManager.GetRelationshipObject<TRelationship, TData>(typeof(TNode), typeof(TTargetNode), target.Reference, properties, typeof(TData));
        }

        public string GetRootTypeKey()
        {            
            return _relationshipManager.GetTypeKey(typeof(TNode), typeof(TTargetNode), _payload);
        }

        private  string GetSourceRootKey()
        {
            //TODO Need to fix this to work with Source Nodes not tied to the root
            return _relationshipManager.GetTypeKey(typeof(TNode), typeof( RootNode));
        }

        private bool MultiplesCheck(Node<TNode> source, Node<TTargetNode> target)
        {
            return (!AllowMultipleRelationships) && (GetMultipleCount(GetMultipesQuery(source.Reference, target.Data)));
        }
        private bool MultiplesCheck<TData>(Node<TNode> source, Node<TTargetNode> target, TData payload = null) where TData : class,IPayload, new()
        {
            return (!AllowMultipleRelationships) && (RelationshipExists(source.Reference, target.Data, payload));
        }

        private bool RelationshipExists<TData>(NodeReference node, TTargetNode target, TData payload = null) where TData : class,IPayload, new()
        {
            var result = GetMultipesQuery(node, target, typeof(TData));

            if (payload != null)
            {
                var payloadStr = string.Format("r.{0} = '{1}'", payload.CompareName(), payload.CompareValue());
                result = result.AndWhere(payloadStr);
            }

            return GetMultipleCount(result);
        }

        private static bool GetMultipleCount(ICypherFluentQuery query)
        {
            return  query.Return(() => All.Count()).Results.SingleOrDefault() != 0;            
        }

        private ICypherFluentQuery GetMultipesQuery(NodeReference node, TTargetNode target, Type payloadType = null )
        {
            var match = string.Format("n-[r:{0}]-p", TypeKeyRelatingNodes(payloadType));
            var where = string.Format("p.Id = {0}", target.Id);
            return node.StartCypher("n").Match(match).Where(where);            
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