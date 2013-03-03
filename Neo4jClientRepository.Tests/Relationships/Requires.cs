using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;

namespace Neo4jClientRepository.Tests.Relationships
{
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

        public const string TypeKey = "REQUIRES";

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}