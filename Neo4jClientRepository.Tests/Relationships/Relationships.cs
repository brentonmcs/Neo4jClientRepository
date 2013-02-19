using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClientRepository.Tests.Relationships
{


    public class OwnedBy :
        Relationship,
        IRelationshipAllowingSourceNode<RootNode>,
        IRelationshipAllowingTargetNode<StorageLocation>
    {
        public OwnedBy(NodeReference otherNode)
            : base(otherNode)
        { }

        public override string RelationshipTypeKey
        {
            get { return "OWNED_BY"; }
        }
    }

    public class Requires :
    Relationship<Requires.Payload>,
    IRelationshipAllowingSourceNode<Product>,
    IRelationshipAllowingSourceNode<Part>,
    IRelationshipAllowingTargetNode<Part>
    {
        public Requires(NodeReference otherUser, Payload data)
            : base(otherUser, data)
        { }

        public class Payload
        {
            public int Count { get; set; }
        }

        public override string RelationshipTypeKey
        {
            get { return "REQUIRES"; }
        }
    }

    public class StoredIn :
       Relationship,
       IRelationshipAllowingSourceNode<Part>,
       IRelationshipAllowingSourceNode<Product>,
       IRelationshipAllowingTargetNode<StorageLocation>
    {
        public StoredIn(NodeReference otherNode)
            : base(otherNode)
        { }

        public override string RelationshipTypeKey
        {
            get { return "STORED_IN"; }
        }
    }
}
