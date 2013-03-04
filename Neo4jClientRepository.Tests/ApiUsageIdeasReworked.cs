using Neo4jClient;
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

            // Based on http://wiki.neo4j.org/content/Image:Warehouse.png

            INeo4jRelationshipManager relationshipManager = new Neo4jRelationshipManager();


            var storageLocationService = new Neo4NodeRepository<OwnedBy>(graph, relationshipManager);

            var partsAndProductService = new Neo4NodeRepository<StoredIn>(graph, relationshipManager);



            var frameStore = storageLocationService.UpSert(new StorageLocation {Name = "Frame Store"});

            var mainStore = storageLocationService.UpSert(new StorageLocation {Name = "Main Store"});

            var frame = partsAndProductService.UpSert(new Part {Name = "Frame"}, frameStore);

            partsAndProductService.UpSert(new Product {Name = "Trike", Weight = 2}, mainStore);


        }

    }
}
    