using Neo4jClient;
using System;
using Neo4jClientRepository.Tests.Domain;

namespace Neo4jClientRepository.Tests.Relationships
{

    public class StorePurchaseProduct :
               Relationship<StorePurchaseProduct.PayLoad>,
               IRelationshipAllowingSourceNode<StorageLocation>,
               IRelationshipAllowingTargetNode<Product>
    {
        public StorePurchaseProduct(NodeReference targetNode, PayLoad properties)
            : base(targetNode, properties)
        {
        }

        public class PayLoad
        {
            public DateTimeOffset Purchased { get; set; }
        }

        public const string TypeKey = "STORE_PURCHASE_PRODUCT";

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}
