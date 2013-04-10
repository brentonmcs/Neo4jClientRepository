using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;

namespace Neo4jClientRepository.Tests.Relationships
{
    public class IsCustomerRelationship :
        Relationship,
        IRelationshipAllowingSourceNode<Customer>,
        IRelationshipAllowingTargetNode<RootNode>
    {
        public IsCustomerRelationship(NodeReference targetNode)
            : base(targetNode)
        {
        }


        public const string TypeKey = "IS_CUSTOMER";

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}