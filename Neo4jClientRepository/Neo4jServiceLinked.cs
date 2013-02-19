using CacheController;
using Neo4jClient;
using Neo4jClient.Gremlin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public abstract class Neo4jServiceLinked<TSourceNode, TLinkedNode, TRootNodeRelationShip, TRelationship> : Neo4jService<TSourceNode>,
                                                                                                               INeo4jServiceLinked<TSourceNode, TLinkedNode, TRootNodeRelationShip, TRelationship>
        where TRootNodeRelationShip : class,new()
        where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSourceNode>
        where TSourceNode : class, IDBSearchable<TSourceNode>, new()
        where TLinkedNode : class, IDBSearchable<TLinkedNode>, new()
    {
        protected NodeReference<TRootNodeRelationShip> RefNode;

        private TSourceNode source;

        public Neo4jServiceLinked(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager)
            : base(graphClient, relationshipManager)
        {
            RefNode = GetOrCreateReferenceNode();
            source = new TSourceNode();
        }

        public void CreateReleationship(Node<TSourceNode> item, Node<TLinkedNode> linkedItem)
        {
            if (!FindRelationship(item, linkedItem))
                graphClient.CreateRelationship(item.Reference, GetLinkedRelationship(linkedItem));
        }

        public new Node<TSourceNode> Get(string code)
        {
            return graphClient
                .RootNode
                .In<TRootNodeRelationShip>(GetRefereanceNodeRelationship())
                .Out<TSourceNode>(GetRootNodeTypeKey(), source.FilterQuery(code))
                .SingleOrDefault();
        }

        public List<Node<TSourceNode>> GetAll()
        {
            return graphClient
                .RootNode
                .In<TRootNodeRelationShip>(GetRefereanceNodeRelationship())
                .Out<TSourceNode>(GetRootNodeKey())
                .ToList();                
        }

        public new Node<TSourceNode> GetCached(string code)
        {
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
            return relationshipManager.GetRelationshipObjectParticipant<TSourceNode>(typeof(TRootNodeRelationShip), typeof(TSourceNode), RefNode);
        }

        protected TRelationship GetLinkedRelationship(Node<TLinkedNode> linkedItem)
        {
            return relationshipManager.GetRelationshipObject<TRelationship>(typeof(TSourceNode), typeof(TLinkedNode), linkedItem.Reference);
        }

        protected NodeReference<TRootNodeRelationShip> GetOrCreateReferenceNode()
        {
            var node = graphClient
                .RootNode
                .In<TRootNodeRelationShip>(GetRefereanceNodeRelationship());

            if (node != null && node.Any())
                return node.Single().Reference;

            var refNode = graphClient.Create<TRootNodeRelationShip>(
                new TRootNodeRelationShip()
                , new[] { GetReferenceNodeRelationShip() as IRelationshipAllowingSourceNode<TRootNodeRelationShip> });
            return refNode;
        }

        protected IRelationshipAllowingSourceNode<TRootNodeRelationShip> GetReferenceNodeRelationShip()
        {
            return relationshipManager.GetRelationshipObjectSource<TRootNodeRelationShip>(typeof(TRootNodeRelationShip), typeof(RootNode), graphClient.RootNode);
        }

        protected new Node<TSourceNode> InsertNew(TSourceNode item)
        {
            graphClient.Create<TSourceNode>(item,
                 new[] { GetItemRelationship() },

                                      new[] { GetIndexEntry(item) });

            var node = Get(item.ItemSearchCode());
            CachingService.UpdateCacheForKey(item.ItemSearchCode(), 10000, node);
            return node;
        }

        private bool FindRelationship(Node<TSourceNode> item, Node<TLinkedNode> linkedItem)
        {
            return item.Out<TLinkedNode>(relationshipManager.GetTypeKey(typeof(TSourceNode), typeof(TLinkedNode)), linkedItem.Data.FilterQuery(linkedItem.Data)).Any();
        }

        private string GetRefereanceNodeRelationship()
        {
            return relationshipManager.GetTypeKey(typeof(TRootNodeRelationShip), typeof(RootNode));
        }

        private string GetRootNodeTypeKey()
        {
            return relationshipManager.GetTypeKey(typeof(TRootNodeRelationShip), typeof(TSourceNode));
        }
        private void UpdateExisting(TSourceNode item, Node<TSourceNode> existing)
        {
            if (!existing.Data.Equals(item))
            {
                graphClient.Update(existing.Reference,
                        u => GetItemUpdateFields(item, u),
                        u => new[]
                            {
                             GetIndexEntry(u)
                            });
            }
        }
    }
}