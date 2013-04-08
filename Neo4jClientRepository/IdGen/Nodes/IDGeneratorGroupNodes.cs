using Neo4jClient;

namespace Neo4jClientRepository.IdGen
{
    public class IdGroupNodeRelationship :
        Relationship,
        IRelationshipAllowingSourceNode<IdReferenceNode>,
        IRelationshipAllowingTargetNode<IdGeneratorNode>
    {
        public IdGroupNodeRelationship(NodeReference targetNode)
            : base(targetNode)
        {

        }

        public const string TypeKey = "ID_GROUP_NODE_REFERENCED_TO";

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}