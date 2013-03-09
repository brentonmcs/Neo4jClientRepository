using Neo4jClient;

namespace Neo4jClientRepository.IdGenerator
{
    public class IDGeneratorReferenceNode :
        Relationship,
        IRelationshipAllowingSourceNode<IdReferenceNode>,
        IRelationshipAllowingTargetNode<RootNode>
    {
        public IDGeneratorReferenceNode(NodeReference targetNode)
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