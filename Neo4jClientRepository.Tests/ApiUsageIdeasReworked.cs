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

            var partsAndProductService = repoCreator.CreateNode<StoredIn, Part>("Name");
            
            
            //var frameStore = storageLocationService.UpdateOrInsert(new StorageLocation {Name = "Frame Store"},null);

            //storageLocationService.UpdateOrInsert(new StorageLocation { Name = "Main Store" }, null);

            //partsAndProductService.UpdateOrInsert(new Part { Name = "Frame" }, frameStore.Reference);

            //partsAndProductService.UpdateOrInsert(new Product { Name = "Trike", Weight = 2 }, mainStore.Reference);


        }


    }
}
    