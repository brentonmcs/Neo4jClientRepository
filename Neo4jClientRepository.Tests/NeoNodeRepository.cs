using System;
using System.Collections.Generic;
using System.Linq;
using CacheController;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4jClientRepository.RelationshipManager;
using Neo4jClientRepository.Tests.Relationships;

using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;
using NUnit.Framework;

namespace Neo4jClientRepository.Tests
{
    [TestClass]
    public class NeoNodeRepository
    {        
        private readonly INeo4jRelationshipManager _relationshipManager = new Neo4jRelationshipManager();

        private IGraphClient _graphClient;

        private INeo4jRelatedNodes<Part, Part> _relatedProductService;
        private INeo4NodeRepository<Part> _partRepository;
        private ICachingService _cachingService;

        [TestInitialize]
        public void Init()
        {
            _graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"));
            ((GraphClient) _graphClient).Connect();

            _cachingService = new CachingService();

            _partRepository = new Neo4NodeRepository<Part,PartRootNodeRelationship>(_graphClient, _relationshipManager, null, "Code", null);
            _relatedProductService = new Neo4jRelatedNodes<Part, Part, RelatedParts>(_graphClient, _relationshipManager, _partRepository, _partRepository, _cachingService);
            
        }

        [TestMethod]
        public void TestFindRelated()
        {
            var node = _partRepository.UpdateOrInsert(new Part
                {
                    Id = 1,
                    Name = "Test Part"
                }, null);

            
            FindRelatedProducts(node);
        }

        public void FindRelatedProducts(Node<Part> part )
        {

            var result =
                GetOtherRelated(part, "STORE_PURCHASE_PRODUCT");
            //.GroupBy(x => x.Id)
            //    .Select(x => new { id = x.Key, count = x.Count() })
            //    .OrderByDescending(x => x.count);

            //  var currentlyRelated = RelatedProductService.GetCachedRelated<Product>(id).Select(x => x.Data.Id);

        }


        private string GetOtherRelated<TNode>(Node<TNode> product, string typeKey)  where TNode : IDBSearchable
        {
            var result = 
                product
                    .StartCypher("startProduct")
                    .Match("product-[" + typeKey + ":]-users-[:" + typeKey + "]-otherproducts")
                    .Where<TNode, TNode>((otherproducts, startProduct) => otherproducts.Id != startProduct.Id)
                  //  .Return<TNode>("otherproducts")
                    .Query;
            
            return result.QueryText;
        }


        [TearDown]
        public void TearDown()
        {
            var node = _partRepository.GetNodeReferenceById(1);

            if (node !=null)
                 _partRepository.DeleteNode(node.Reference);
        }
    }
}
