using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;

namespace Neo4jClientRepository.Tests.Relationships
{
    public class PartRootNodeRelationship :
      Relationship,
      IRelationshipAllowingSourceNode<RootNode>,
        IRelationshipAllowingTargetNode<Part>
    {
        public PartRootNodeRelationship(NodeReference otherNode)
            : base(otherNode)
        { }

        public const string TypeKey = "HAS_RELATED_NODE";

        public override string RelationshipTypeKey
        {
            get { return TypeKey    ; }
        }
    }
}
