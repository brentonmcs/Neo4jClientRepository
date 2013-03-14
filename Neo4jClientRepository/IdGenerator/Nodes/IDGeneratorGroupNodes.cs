using Neo4jClient;

namespace Neo4jClientRepository.IdGenerator
{
    public class IDGeneratorGroupNodes :
        Relationship,
        IRelationshipAllowingSourceNode<IdReferenceNode>,
        IRelationshipAllowingTargetNode<IDGeneratorNode>
    {
        public IDGeneratorGroupNodes(NodeReference targetNode)
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