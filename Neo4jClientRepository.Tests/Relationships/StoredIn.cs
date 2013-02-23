using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;

namespace Neo4jClientRepository.Tests.Relationships
{
    public class StoredIn :
       Relationship,
       IRelationshipAllowingSourceNode<Part>,
       IRelationshipAllowingSourceNode<Product>,
       IRelationshipAllowingTargetNode<StorageLocation>
    {
        public StoredIn(NodeReference otherNode)
            : base(otherNode)
        { }

        public override string RelationshipTypeKey
        {
            get { return "STORED_IN"; }
        }
    }
}
