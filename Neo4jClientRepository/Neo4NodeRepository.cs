using System.Globalization;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClient.Cypher;
using Neo4jClientRepository.IdGen;
using Neo4jClientRepository.RelationshipManager;
using CacheController;

namespace Neo4jClientRepository
{
    public class Neo4NodeRepository<TModel, TRelationship> : INeo4NodeRepository<TModel> 
        where TRelationship : Relationship
        where TModel : class,IDBSearchable, new()
    {
        private ICachingService _cachingService;
        private IIdGenerator _idGenerator;
        private IGraphClient _graphClient;
        private INeo4jRelationshipManager _relationshipManager;
        private NodeReference _referenceNode;

        protected Type Relationship { get; set; }

        public Neo4NodeRepository(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, IIdGenerator idGenerator, string indexSearchCode, ICachingService cachingService)
        {
        
            Init(graphClient, relationshipManager, null, indexSearchCode, idGenerator, cachingService);
        }

        public Neo4NodeRepository(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, IIdGenerator idGenerator,  string indexSearchCode, NodeReference referenceNode, ICachingService cachingService)
        {
            Init(graphClient, relationshipManager, referenceNode, indexSearchCode, idGenerator, cachingService);
        }

        private void Init(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, NodeReference referenceNode, string indexSerchCode, IIdGenerator idGenerator, ICachingService cachingService)
        {
            
            if (graphClient == null) throw new ArgumentNullException("graphClient");
            if (relationshipManager == null) throw new ArgumentNullException("relationshipManager");

            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            ItemCodeIndexName = indexSerchCode;
            _idGenerator = idGenerator;
            _cachingService = cachingService;
            SourceType = _relationshipManager.GetSourceType(typeof(TRelationship));
            TargetType = _relationshipManager.GetTargetType(typeof(TRelationship));

            Relationship = typeof (TRelationship);

            _referenceNode = referenceNode;
            if (referenceNode == null)
                _referenceNode = graphClient.RootNode;
            
        }

        public TModel GetByIndex(string key, object value, Type indexType)
        {
            return GetByIndexGeneric<TModel>(key,value,indexType);
        }

        private TResult GetByIndexGeneric<TResult>(string key, object value, Type indexType)
            where TResult : class
        {
            if (!_graphClient.CheckIndexExists(GetIndexName(indexType), IndexFor.Node))
                return null;

            return
                 _graphClient
                 .Cypher
                 //.StartWithNodeIndexLookup("node", GetIndexName(indexType), key, value)
                 .Start(new { node = Node.ByIndexLookup(GetIndexName(indexType),key,value) })
                 .Return<TResult>("node")
                 .Results
                 .SingleOrDefault();

        }

        private static string GetIndexName(Type indexType)
        {
            var indexName = typeof(TModel).Name;
            if (indexType != null)
                indexName = indexType.Name;
            return indexName;
        }


        public TModel GetById(long id)
        {
            return GetByIndex("Id", id,null);
        }

        public TModel GetByItemCode(string value)
        {
            var cacheKey = CacheKeyName() + "_" + value;

            return GetCacheResult(new Func<TModel>(() => GetByIndex(ItemCodeIndexName, value, null)), cacheKey) as TModel;           
        }

        public Node<TModel> GetNodeByItemCode(string value)
        {
            var cacheKey = CacheKeyName() + "_" + value;

            return GetCacheResult(new Func<Node<TModel>>(() => GetByIndexGeneric<Node<TModel>>(ItemCodeIndexName, value, null)), cacheKey) as Node<TModel>;           
        }

        public Node<TModel> GetNodeByIndex(string key, object value, Type indexType)
        {
            var cacheKey = CacheKeyName() + "_" + key + "_" + value;

            return GetCacheResult(new Func<Node<TModel>>(() => GetByIndexGeneric<Node<TModel>>(key, value, indexType)), cacheKey) as Node<TModel>;           
        }

        public Node<TModel> GetNodeReferenceById(long id)
        {
            return GetCacheResult(new Func<Node<TModel>>(() => GetByIndexGeneric<Node<TModel>>("Id", id, typeof(TModel))), CacheKeyName() + "_" + id.ToString(CultureInfo.InvariantCulture)) as Node<TModel>;
        }

        private static string CacheKeyName()
        {
            return typeof (TRelationship).Name;
        }

        
        private object GetCacheResult(Delegate action, string cacheKey)
        {
            return _cachingService != null ? _cachingService.Cache(cacheKey, 1000, action) : action.DynamicInvoke();
        }

        public TModel GetByTree(Expression<Func<TModel, bool>> filter)
        {
            CheckFilter(filter);
   
            return
                    _referenceNode
                   .StartCypher("root")
                   .Match(_relationshipManager.GetMatchStringToRootForSource(Relationship))
                   .Where(filter)
                   .Return<TModel>("node")
                   .Results
                   .Single();
        }

        private static void CheckFilter<TResult>(Expression<Func<TResult, bool>> filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            if (!filter.Parameters.Any())
                throw new NotSupportedException("No Parameters found for Filter");

            if (filter.Parameters.First().Name != "node")
                throw new NotSupportedException("Lambda expression should use 'node' as the Left parameter (node => node.id == 1)");
        }


        public Node<TModel> UpdateOrInsert(TModel item, NodeReference linkedItem) 
            
        {
            
            var existing = GetNodeReferenceById(item.Id);
            
            if (existing == null)
            {
                if (item.Id == 0)
                    item.Id = _idGenerator.GetNew(item.GetType().Name);
                var resultNode = _graphClient.Create(item, new[] { GetReferenceToLinkedItem<TModel>(GetLinkedReference(linkedItem)) }, new[] { GetIndexEntry(item) });
                
                return _graphClient.Get(resultNode);

            }
            _graphClient.Update(existing.Reference, node => UpdateNode(item, node), x => new[] { GetIndexEntry(x) });
            return existing;
        }
      

        private NodeReference GetLinkedReference(NodeReference linkedItem)
        {
            var linkedReference = linkedItem;
            if (linkedItem == null)
                linkedReference = _referenceNode;
            return linkedReference;
        }


        private static TNode UpdateNode<TNode>(TNode item, TNode existingNode)
        {
            var newValues = GetItemsProperties(item).Select(x => new { value = x.GetValue(item, null), x.Name }).ToList();
            
            foreach (var prop in GetItemsProperties(existingNode))
            {
                prop.SetValue(existingNode, newValues.Single(x => x.Name == prop.Name).value);
            }
            return existingNode;
        }
     
       
        private static IEnumerable<PropertyInfo> GetItemsProperties(object item)
        {
            return item.GetType().GetProperties();
        }

        private static IndexEntry GetIndexEntry(object item)
        {
            
            var indexEntry = new IndexEntry(item.GetType().Name);

            foreach (var prop in GetItemsProperties(item))
            {
                var value = prop.GetValue(item, null);
                if (value != null)
                    indexEntry.Add(prop.Name, value);
            }
            return indexEntry;
        }

        private IRelationshipAllowingParticipantNode<TNode> GetReferenceToLinkedItem<TNode>(NodeReference linkedItem)
            where TNode: class, new()
        {
            return _relationshipManager.GetRelationshipObjectParticipant<TNode>(SourceType, TargetType, linkedItem);
        }

        public void DeleteNode(NodeReference node)
        {
            _graphClient.Delete(node, DeleteMode.NodeAndRelationships);
        }

        private string GetTypeString()
        {
            return _relationshipManager.GetTypeKey(Relationship);
        }
        public IEnumerable<TModel> GetAll()
        {
            return GetCacheResult(new Func<IEnumerable<TModel>>(
                () => _referenceNode
                          .StartCypher("root")
                          .Match("root-[:" + GetTypeString() + "]-node")
                          .Return<TModel>("node")
                          .Results), CacheKeyName() + "_ALL") as IEnumerable<TModel>;
        }

        public string ItemCodeIndexName { get; private set; }


        public Type TargetType { get; private set; }
        public Type SourceType { get; private set; }


        






        
    }
}
