using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Neo4jClientRepository
{
    public class Neo4NodeRepository<TRelationship>
        where TRelationship : Relationship
    {
        readonly IGraphClient _graphClient;
        readonly INeo4jRelationshipManager _relationshipManager;

        private Type SourceType { get; set; }
        private Type TargetType { get; set; }
        

        public Neo4NodeRepository(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager)
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            SourceType = _relationshipManager.GetSourceType(typeof(TRelationship));
            TargetType = _relationshipManager.GetTargetType(typeof(TRelationship));
        }
        
        public TResult GetByIndex<TResult>(string key, object value, Type indexType = null) where TResult : class 
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


        public TResult GetById<TResult>(int id)
            where TResult : class 
        {
            return GetByIndex<TResult>("Id", id);
        }

        public Node<TResult> GetNodeReferenceById<TResult>(int id)
            where TResult : class 
        {
            return GetByIndex<Node<TResult>>("Id", id, typeof(TResult));
        }
        
        public TResult GetByTree<TResult>(Expression<Func<TResult, bool>> filter)
        {
            CheckFilter(filter);
   
            return
                _graphClient
                   .RootNode
                   .StartCypher("root")
                   .Match(_relationshipManager.GetMatchStringToRootForSource<TRelationship>())
                   .Where(filter)
                   .Return<TResult>("node")
                   .Results
                   .Single();
        }

        private static void CheckFilter<TResult>(Expression<Func<TResult, bool>> filter)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            if (!filter.Parameters.Any())
                throw new NotSupportedException("No paraments found for Filter");

            if (filter.Parameters.First().Name != "node")
                throw new NotSupportedException("Lambda expression should use 'node' as the Left parameter (node => node.id == 1)");
        }


        public NodeReference UpSert<TNode>(TNode item, NodeReference linkedItem = null)
            where TNode : class, new()
        {            
            var existing = GetNodeReferenceById<TNode>(GetItemId(item));

            if (existing == null)
                return _graphClient.Create(item, new[] { GetReferenceToLinkedItem<TNode>(GetLinkedReference(linkedItem)) }, new[] { GetIndexEntry(item) });
            _graphClient.Update(existing.Reference, node => UpdateNode(item, node), x => new[] { GetIndexEntry(x) });
            return existing.Reference;
        }

        private NodeReference GetLinkedReference(NodeReference linkedItem)
        {
            var linkedReference = linkedItem;
            if (linkedItem == null)
                linkedReference = _graphClient.RootNode;
            return linkedReference;
        }


        private TNode UpdateNode<TNode>(TNode item, TNode existingNode)
        {
            var newValues = GetItemsProperties(item).Select(x => new { value = x.GetValue(item, null), x.Name }).ToList();
            
            foreach (var prop in GetItemsProperties(existingNode))
            {
                prop.SetValue(existingNode, newValues.Single(x => x.Name == prop.Name).value);
            }
            return existingNode;
        }

        private int GetItemId(object item)
        {
            var id = GetItemsProperties(item).SingleOrDefault(x => x.Name == "Id");

            if (id == null)
                return -1;

            return (int)id.GetValue(item, null);         
        }

        private IEnumerable<PropertyInfo> GetItemsProperties(object item)
        {
            return item.GetType().GetProperties();
        }
        private IndexEntry GetIndexEntry(object item)
        {
            
            var indexEntry = new IndexEntry(item.GetType().Name);

            foreach (var prop in GetItemsProperties(item))
            {
                indexEntry.Add(prop.Name, prop.GetValue(item,null));
              
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

    }
}
