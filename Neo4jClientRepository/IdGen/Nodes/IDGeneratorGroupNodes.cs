using Neo4jClient;

namespace Neo4jClientRepository.IdGen
{
    public class IdGeneratorGroupNodes :
        Relationship,
        IRelationshipAllowingSourceNode<IdReferenceNode>,
        IRelationshipAllowingTargetNode<IdGeneratorNode>
    {
        public IdGeneratorGroupNodes(NodeReference targetNode)
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