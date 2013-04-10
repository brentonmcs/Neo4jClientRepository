using System;
using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;

namespace Neo4jClientRepository.Tests.Relationships
{
    public class CustomerPurchaseProduct :
        Relationship<CustomerPurchaseProduct.PayLoad>,
        IRelationshipAllowingSourceNode<Customer>,
        IRelationshipAllowingTargetNode<Product>
    {
        public CustomerPurchaseProduct(NodeReference targetNode, PayLoad properties)
            : base(targetNode, properties)
        {
        }

        public class PayLoad :IPayload
        {
            public DateTimeOffset Purchased { get; set; }

            public string CompareName()
            {
                return "Purchased";
            }

            public string CompareValue()
            {
                return Purchased.ToString();
            }
        }

        public const string TypeKey = "CUSTOMER_PURCHASE_PRODUCT";

        public override string RelationshipTypeKey
        {
            get { return TypeKey; }
        }
    }
}