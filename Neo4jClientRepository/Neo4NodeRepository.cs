﻿using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Neo4jClientRepository.IdGenerator;
using Neo4jClientRepository.RelationshipManager;
using CacheController;

namespace Neo4jClientRepository
{
    public class Neo4NodeRepository<TRelationship> : INeo4NodeRepository where TRelationship : Relationship
    {
        private ICachingService _cachingService;
        private IIDGenerator _idGenerator;
        private IGraphClient _graphClient;
        private INeo4jRelationshipManager _relationshipManager;
        private NodeReference _referenceNode;

        private Type SourceType { get; set; }
        private Type TargetType { get; set; }
        protected Type Relationship { get; set; }

        public Neo4NodeRepository(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, IIDGenerator idGenerator, string indexSerchCode, ICachingService cachingService)
        {
        
            Init(graphClient, relationshipManager, null, indexSerchCode, idGenerator, cachingService);
        }

        public Neo4NodeRepository(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, IIDGenerator idGenerator,  string indexSerchCode, NodeReference referenceNode, ICachingService cachingService)
        {
            Init(graphClient, relationshipManager, referenceNode, indexSerchCode, idGenerator, cachingService);
        }

        private void Init(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, NodeReference referenceNode, string indexSerchCode, IIDGenerator idGenerator, ICachingService cachingService)
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

        public TResult GetByIndex<TResult>(string key, object value, Type indexType) where TResult : class 
        {
            if (!_graphClient.CheckIndexExists(GetIndexName<TResult>(indexType), IndexFor.Node))
                return null;

            return 
                 _graphClient
                 .Cypher
                 .StartWithNodeIndexLookup("node", GetIndexName<TResult>(indexType), key, value)
                 .Return<TResult>("node")
                 .Results
                 .SingleOrDefault();
        }

        private static string GetIndexName<TResult>(Type indexType)
        {
            var indexName = typeof(TResult).Name;
            if (indexType != null)
                indexName = indexType.Name;
            return indexName;
        }


        public TResult GetById<TResult>(long id)
            where TResult : class 
        {
            return GetByIndex<TResult>("Id", id,null);
        }

        public Node<TResult> GetByItemCode<TResult>(string value) where TResult : class
        {
            var cacheKey = CacheKeyName() + "_" + value;
         
            return GetCacheResult( new Func<Node<TResult>>(() => 
                { return GetByIndex<Node<TResult>>(ItemCodeIndexName, value, null);
                }), cacheKey)  as Node<TResult>;           
        }

        private string CacheKeyName()
        {
            return typeof (TRelationship).Name;
        }

        public Node<TResult> GetNodeReferenceById<TResult>(long id) where TResult : class 
        {
            return GetCacheResult(new Func<Node<TResult>>(() =>
            {
                return GetByIndex<Node<TResult>>("Id", id, typeof(TResult));
            })
            , CacheKeyName() + "_" + id.ToString()) as Node<TResult>;
        }

        private object GetCacheResult(Delegate action, string cacheKey)
        {
            if (_cachingService != null)
                return _cachingService.Cache(cacheKey, 1000, action);

            return action.DynamicInvoke();
        }

        public TResult GetByTree<TResult>(Expression<Func<TResult, bool>> filter)
        {
            CheckFilter(filter);
   
            return
                    _referenceNode
                   .StartCypher("root")
                   .Match(_relationshipManager.GetMatchStringToRootForSource(Relationship))
                   .Where(filter)
                   .Return<TResult>("node")
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


        public Node<TNode> UpdateOrInsert<TNode>(TNode item, NodeReference linkedItem) where TNode : class,IDBSearchable, new()
            
        {
            
            var existing = GetByIndex<Node<TNode>>("Id", item.Id, typeof(TNode));
            
            if (existing == null)
            {
                if (item.Id == 0)
                    item.Id = _idGenerator.GetNew(item.GetType().Name);
                var resultNode = _graphClient.Create(item, new[] {GetReferenceToLinkedItem<TNode>(GetLinkedReference(linkedItem))},new[] {GetIndexEntry(item)});
                
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

        public string GetTypeString()
        {
            return _relationshipManager.GetTypeKey(Relationship);
        }
        public IEnumerable<TResult> GetAll<TResult>()
        {
            return GetCacheResult(new Func<IEnumerable<TResult>>(
                () =>
                {
                    return 
                    _referenceNode
                    .StartCypher("root")
                    .Match("root-[:" + GetTypeString() + "]-node")
                    .Return<TResult>("node")
                    .Results;
                }), CacheKeyName() + "_ALL") as IEnumerable<TResult>;
        }

        public string ItemCodeIndexName { get; private set; }


        public Type GetTargetType()
        {
            return TargetType;
        }

        public Type GetSourceType()
        {
            return SourceType;
        }
    }
}
