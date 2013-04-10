using Neo4jClient;
using Neo4jClientRepository.IdGen;
using Neo4jClientRepository.RelationshipManager;
using Neo4jClientRepository.Tests.Domain;
using Neo4jClientRepository.Tests.Relationships;
using System;


namespace Neo4jClientRepository.Tests
{
    internal class ApiUsageIdeasReworked
    {

        // This is my reworked version of Readify Api Usage Ideas using the Repository Library
        // For the original visit  -    http://hg.readify.net/neo4jclient/src


        private void Foo()
        {

            IGraphClient graph = new GraphClient(new Uri(""));
            INeo4jRelationshipManager relationshipManager = new Neo4jRelationshipManager();
            INodeRepoCreator repoCreator = new NodeRepoCreator(graph, relationshipManager);

            var idGenerator = new IdGenerator(repoCreator);           
            idGenerator.LoadGenerator(5);
            

            var storageLocationService = repoCreator.CreateNode<OwnedBy, StorageLocation>("Name");

            var partsService = repoCreator.CreateNode<StoredIn, Part>("Name");
            var productService = repoCreator.CreateNode<StoredIn, Product>("Name");
            
            var frameStore = storageLocationService.UpdateOrInsert(new StorageLocation {Name = "Frame Store"});
            var mainStore = storageLocationService.UpdateOrInsert(new StorageLocation { Name = "Main Store" });

            storageLocationService.UpdateOrInsert(new StorageLocation { Name = "Main Store" });

            partsService.UpdateOrInsert(new Part { Name = "Frame" }, frameStore.Reference);

            productService.UpdateOrInsert(new Product { Name = "Trike", Weight = 2 }, mainStore.Reference);


            var customerNode = repoCreator.CreateNode<IsCustomerRelationship, Customer>("Name");

            customerNode.UpdateOrInsert(new Customer {Name = "Fred", DOB = new DateTime (1, 1, 1970)});

            var customerPurchaseService = repoCreator.CreateRelated<Customer, Product, CustomerPurchaseProduct>(customerNode, productService);

            customerPurchaseService.AddRelatedRelationship("Fred", "Trike", new CustomerPurchaseProduct.PayLoad { Purchased = DateTime.UtcNow});




        }


    }
}
    