using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4jClientRepository.RelationshipManager;
using Neo4jClientRepository.Tests.Relationships;

using Neo4jClient;

namespace Neo4jClientRepository.Tests
{
    [TestClass]
    public class NeoNodeRepository
    {
        private INeo4NodeRepository _nodeRepo;

        private readonly INeo4jRelationshipManager relationshipManager = new Neo4jRelationshipManager();
        [TestInitialize]
        public void Init()
        {
            var graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"));
            graphClient.Connect();


            //_nodeRepo = new Neo4NodeRepository<OwnedBy>(graphClient, relationshipManager,"Name");
        }

        [TestMethod]
        public void GetNode()
        {
         //   var result = _nodeRepo.GetByIndex<StorageLocation>("Id", 1);


        }

    }
}
