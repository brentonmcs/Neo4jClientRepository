using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClientRepository
{
    public class Neo4NodeRepository<TRelationship>
        where TRelationship : Relationship
    {
        readonly IGraphClient _graphClient;
        readonly INeo4jRelationshipManager _relationshipManager;

        public Type SourceType { get { return _source; } }
        public Type TargetType { get { return _target; } }
        public string RelationshipTypeKey { get { return _typeKey; } }
     
        private Type _source;
        private Type _target;
        private string _typeKey;

        public Neo4NodeRepository(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager)
        {
            this._graphClient = graphClient;
            this._relationshipManager = relationshipManager;

            _source = _relationshipManager.GetSourceType(typeof(TRelationship));
            _target = _relationshipManager.GetTargetType(typeof(TRelationship));
            _typeKey = _relationshipManager.GetTypeKey(typeof(TRelationship));
        }
        
        public TResult GetByIndex<TResult>(string key, object value, Type indexType = null)
        {
            var result=
                 _graphClient
                 .Cypher
                 .StartWithNodeIndexLookup("node", GetIndexName<TResult>(indexType), key, value)
                 .Return<TResult>("node");

           var query =  result.Query.QueryText;
           var p = result.Query.QueryParameters.ToString();

           return result.Results
                .SingleOrDefault();
        }

        private static string GetIndexName<TResult>(Type indexType)
        {
            var indexName = typeof(TResult).Name;
            if (indexType != null)
                indexName = indexType.Name;
            return indexName;
        }


        public TResult GetById<TResult>(int Id)
        {
            return GetByIndex<TResult>("Id", Id);
        }

        public Node<TResult> GetNodeReferenceById<TResult>(int Id)
        {
            return GetByIndex<Node<TResult>>("Id", Id, typeof(TResult));
        }
        
        public TResult GetByTree<TResult>(Expression<Func<TResult, bool>> Filter)
        {

            var filter = Filter.Parameters.First();

            if (filter.Name != "node")
                throw new NotSupportedException("Lambda expression should use 'node' as the Left parameter (node => node.id == 1)");
    //        filter.Body.
            var result = 
                _graphClient
                   .RootNode
                   .StartCypher("root")
                   .Match(GetMatch())
                   .Where(Filter)
                   .Return<TResult>("node");

            var query = result.Query.QueryText;
            return
                   result
                   .Results
                   .Single();
        }

        public string[] GetMatch()
        {
            var result = new List<string>();
            
            var currentRelationshipType = typeof(TRelationship);
            var currentSource = SourceType;
            var count = 0;

            var typeKey = _relationshipManager.GetTypeKey(currentRelationshipType);
            
            while (currentSource != typeof(RootNode))
            {   
                result.Add(string.Format("node{2}-[:{0}]-target{1}", typeKey, count, count == 0 ? "" : count.ToString()));
                currentRelationshipType = _relationshipManager.GetRelationship(_relationshipManager.GetTargetType(currentRelationshipType));
                currentSource = _relationshipManager.GetSourceType(currentRelationshipType);
                typeKey = _relationshipManager.GetTypeKey(currentRelationshipType);
                count++;
            }

            count--;
            result.Add(string.Format("node{1}-[:{0}]-root", typeKey,count < 0? "":  count.ToString()));
            return result.ToArray();
        }

        public NodeReference UpSert<TNode>(TNode item, NodeReference linkedItem)
            where TNode : class, new()
        {
            var existing = GetNodeReferenceById<TNode>(GetItemId(item));

            return (existing == null ? InsertNode(item, linkedItem) : UpdateNode<TNode>(item, existing.Reference));
        }

        private NodeReference UpdateNode<TNode>(TNode item, NodeReference<TNode> existingItem)
        {
            //Todo - need to fix this.
            _graphClient.Update<TNode>(existingItem, node => node = item);
            return existingItem;
        }

        private NodeReference InsertNode<TNode>(TNode item, NodeReference linkedItem) where TNode : class, new()
        {
            var linkRef = new[] { GetReferenceToLinkedItem<TNode>(linkedItem) };
            var indexEntry = GetIndexEntry(item);

            return    _graphClient.Create(item, linkRef, indexEntry);

        }

        private int GetItemId(object item)
        {
            var id = GetItemsProperties(item).SingleOrDefault(x => x.Name == "Id");

            if (id == null)
                return -1;

            return (int)id.GetValue(item, null);         
        }

        private PropertyInfo[] GetItemsProperties(object item)
        {
            return item.GetType().GetProperties();
        }
        private IEnumerable<IndexEntry> GetIndexEntry(object item)
        {
            
            var indexEntry = new IndexEntry(item.GetType().Name);

            foreach (var prop in GetItemsProperties(item))
            {
                indexEntry.Add(prop.Name, prop.GetValue(item,null));
              
            }
            return new [] { indexEntry};
        }

        private IRelationshipAllowingParticipantNode<TNode> GetReferenceToLinkedItem<TNode>(NodeReference linkedItem)
            where TNode: class, new()
        {
            return _relationshipManager.GetRelationshipObjectParticipant<TNode>(SourceType, TargetType, linkedItem);
        }
      


    }
}
