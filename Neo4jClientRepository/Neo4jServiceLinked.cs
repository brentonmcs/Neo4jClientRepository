
using System.Linq.Expressions;
using Neo4jClient;
using Neo4jClient.Gremlin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClientRepository
{
    // ReSharper disable InconsistentNaming
    [Obsolete]
    public class Neo4JServiceLinked<TSourceNode, TLinkedNode, TRootNodeRelationShip, TRelationship> : Neo4jService<TSourceNode>, INeo4jServiceLinked<TSourceNode, TLinkedNode>
    // ReSharper restore InconsistentNaming                                                                                                               
        where TRootNodeRelationShip : class,new()
        where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSourceNode>
        where TSourceNode : class, IDBSearchable, new()
        where TLinkedNode : class, IDBSearchable, new()
    {
        protected NodeReference<TRootNodeRelationShip> RefNode;
        
        public Neo4JServiceLinked(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, 
            ICachingService cachingService, Func<TSourceNode, IndexEntry> indexEntry, Action<TSourceNode, TSourceNode> updateFields, string cacheName)
            : base(graphClient, relationshipManager, cachingService,indexEntry, updateFields, cacheName)
        {
            RefNode = GetOrCreateReferenceNode();         
        }

        public void CreateReleationship(Node<TSourceNode> item, Node<TLinkedNode> linkedItem)
        {
            if (!FindRelationship(item, linkedItem))
                GraphClient.CreateRelationship(item.Reference, GetLinkedRelationship(linkedItem));
        }
      

        public new Node<TSourceNode> Get(string code)
        {
            return GraphClient
                .RootNode
                .In<TRootNodeRelationShip>(GetRefereanceNodeRelationship())
                .Out<TSourceNode>(GetRootNodeTypeKey(), x=>x.ItemSearchCode() == code)
                .SingleOrDefault();
        }

        public List<Node<TSourceNode>> GetAll()
        {
            return GraphClient
                .RootNode
                .In<TRootNodeRelationShip>(GetRefereanceNodeRelationship())
                .Out<TSourceNode>(GetRootNodeKey())
                .ToList();
        }

        public new Node<TSourceNode> GetCached(string code)
        {
            if (CachingService == null)
                return Get(code);
            return CachingService.Cache(code, 1000, new Func<string, Node<TSourceNode>>(Get), code) as Node<TSourceNode>;
        }

        public Node<TSourceNode> UpSert(TSourceNode item, Node<TLinkedNode> linkedItem)
        {
            var existing = Get(item.ItemSearchCode());

            if (existing == null)
                existing = InsertNew(item);
            else
                UpdateExisting(item, existing);

            if (linkedItem != null)
                CreateReleationship(existing, linkedItem);

            return existing;
        }

        protected new IRelationshipAllowingParticipantNode<TSourceNode> GetItemRelationship()
        {
            return RelationshipManager.GetRelationshipObjectParticipant<TSourceNode>(typeof(TRootNodeRelationShip), typeof(TSourceNode), RefNode);
        }

        protected TRelationship GetLinkedRelationship(Node<TLinkedNode> linkedItem)
        {
            return RelationshipManager.GetRelationshipObject<TRelationship>(typeof(TSourceNode), typeof(TLinkedNode), linkedItem.Reference);
        }

        protected NodeReference<TRootNodeRelationShip> GetOrCreateReferenceNode()
        {
            var node = GraphClient
                .RootNode
                .In<TRootNodeRelationShip>(GetRefereanceNodeRelationship());

            if (node != null && node.Any())
                return node.Single().Reference;

            return GraphClient.Create( new  TRootNodeRelationShip(), new IRelationshipAllowingParticipantNode<TRootNodeRelationShip>[] {GetReferenceNodeRelationShip()});
        }

        protected IRelationshipAllowingSourceNode<TRootNodeRelationShip> GetReferenceNodeRelationShip()
        {
            return RelationshipManager.GetRelationshipObjectSource<TRootNodeRelationShip>(typeof(TRootNodeRelationShip), typeof(RootNode), GraphClient.RootNode);
        }

        protected new Node<TSourceNode> InsertNew(TSourceNode item)
        {
            //TODO : needs to handle multiple Relationships with Payloads

            GraphClient.Create(item, 
                new [] {GetItemRelationship(), GetItemRelationship()},                                
                new[] { IndexEntry(item) });

            //TODO Search this by the nodes reference
            var node = Get(item.ItemSearchCode());            
            UpdateNodeInCache(CacheType.Id,  node);
            UpdateNodeInCache(CacheType.SearchCode, node);
            return node;
        }

        
        private bool FindRelationship(Node<TSourceNode> item, Node<TLinkedNode> linkedItem)
        {
            //TODO need to test this
            return item.Out(RelationshipManager.GetTypeKey(typeof(TSourceNode), typeof(TLinkedNode)), FilterQuery(linkedItem)).Any();
            


        }

        private static Expression<Func<TSourceNode, bool>> FilterQuery(Node<TLinkedNode> linkedItem)
        {
            return x => x.Id == linkedItem.Data.Id;
        }

        private string GetRefereanceNodeRelationship()
        {
            return RelationshipManager.GetTypeKey(typeof(TRootNodeRelationShip), typeof(RootNode));
        }

        private string GetRootNodeTypeKey()
        {
            return RelationshipManager.GetTypeKey(typeof(TRootNodeRelationShip), typeof(TSourceNode));
        }
        private void UpdateExisting(TSourceNode item, Node<TSourceNode> existing)
        {
            if (!existing.Data.Equals(item))
            {
                GraphClient.Update(existing.Reference,
                        u => UpdateFields(item, u),
                        u => new[]{IndexEntry(u)});
            }
        }
    }
}