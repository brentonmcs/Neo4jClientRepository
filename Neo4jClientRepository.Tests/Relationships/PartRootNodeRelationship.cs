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

        public override string RelationshipTypeKey
        {
            get { return "HAS_RELATED_NODE"; }
        }
    }

    public class ProductRootNodeRelationship :
      Relationship,
        IRelationshipAllowingSourceNode<RootNode>,
        IRelationshipAllowingTargetNode<Part>
    {
        public ProductRootNodeRelationship(NodeReference otherNode)
            : base(otherNode)
        { }

        public override string RelationshipTypeKey
        {
            get { return "HAS_RELATED_NODE"; }
        }
    }
}
