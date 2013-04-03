using Neo4jClient;

namespace Neo4jClientRepository.IdGen
{
    public class IdGeneratorRefNodeRelationship :
        Relationship,
        IRelationshipAllowingSourceNode<RootNode>,
        IRelationshipAllowingTargetNode<IdReferenceNode>
    {
        public IdGeneratorRefNodeRelationship(NodeReference targetNode)
            : base(targetNode)
        {

        }

        public const string TypeKey = "IS_ID_GENERATOR_ROOTNODE";

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}