using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;

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
}