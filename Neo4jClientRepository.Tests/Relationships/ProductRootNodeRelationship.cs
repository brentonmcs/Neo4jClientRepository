using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;

namespace Neo4jClientRepository.Tests.Relationships
{
    public class ProductRootNodeRelationship :
        Relationship,
        IRelationshipAllowingSourceNode<RootNode>,
        IRelationshipAllowingTargetNode<Part>
    {
        public ProductRootNodeRelationship(NodeReference otherNode)
            : base(otherNode)
        { }

        public const string TypeKey = "HAS_RELATED_NODE"; 
        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}