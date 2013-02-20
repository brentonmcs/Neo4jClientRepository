using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;
using Neo4jClientRepository.Tests.Relationships;
using SocialGraph.Neo4j.Neo4jUtils;
using System;


namespace Neo4jClientRepository.Tests
{



    
    class ApiUsageIdeasReworked
    {

        // This is my reworked version of Readify Api Usage Ideas using the Repository Library
        // For the original visit  -    http://hg.readify.net/neo4jclient/src


        void Foo()
        {
            IGraphClient graph = new GraphClient(new Uri(""));

            // Based on http://wiki.neo4j.org/content/Image:Warehouse.png

            INeo4jRelationshipManager relationshipManager = new Neo4jRelationshipManager();
            

            var storageLocationService = new Neo4jService<StorageLocation>(graph, relationshipManager,null, null, null, "StoreageLocation");

            var partsService = new Neo4JServiceLinked<Part, StorageLocation, ReferenceNode, StoredIn>(graph, relationshipManager, null, null, null, "Parts");

            var productService = new Neo4JServiceLinked<Product, StorageLocation, ReferenceNode, StoredIn>(graph, relationshipManager, null, null, null, "Products");
            
          

            var frameStore = storageLocationService.UpSert(new StorageLocation {Name = "Frame Store"});

            var mainStore =  storageLocationService.UpSert(new StorageLocation {Name =  "Main Store"});

            var frame = partsService.UpSert(new Part {Name = "Frame"}, frameStore);
                        
            
            productService.UpSert(new Product { Name =  "Trike", Weight =  2},mainStore);
            

          

        }
    }
}
    