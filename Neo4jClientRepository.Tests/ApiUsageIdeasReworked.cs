using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;
using Neo4jClientRepository.Tests.Relationships;
using Neo4jClientRepository.Tests.Services;
using SocialGraph.Neo4j.Neo4jUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var storageLocationService = new StorageLocationService(graph, relationshipManager);

            // Can create nodes from POCOs
            var frameStore = graph.Create(
                new StorageLocation { Name = "Frame Store" });
            var mainStore = graph.Create(
                new StorageLocation { Name = "Main Store" });

            // Can create a node with outgoing relationships
            var frame = graph.Create(
                new Part { Name = "Frame" },
                new StoredIn(frameStore));

            // Can create multiple outgoing relationships and relationships with payloads
            graph.Create(
                new Product { Name = "Trike", Weight = 2 },
                new StoredIn(mainStore),
                new Requires(frame, new Requires.Payload { Count = 1 }));

            // Can create relationships in both directions
            graph.Create(
                new Part { Name = "Pedal" },
                new StoredIn(frameStore),
                new Requires(frame, new Requires.Payload { Count = 2 })
                    { Direction = RelationshipDirection.Incoming });

            var wheel = graph.Create(
                 new Part { Name = "Wheel" },
                 new Requires(frame, new Requires.Payload { Count = 2 })
                    { Direction = RelationshipDirection.Incoming });

            // Can create implicit incoming relationships
            graph.Create(
                new StorageLocation { Name = "Wheel Store" },
                new StoredIn(wheel));

            // Can create relationships against the root node
            graph.Create(
                new StorageLocation {Name = "Auxillary Store"},
                new StoredIn(wheel),
                new OwnedBy(graph.RootNode));
        
    }
    }
}
    