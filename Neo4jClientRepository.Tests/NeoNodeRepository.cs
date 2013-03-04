using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4jClientRepository.Tests.Relationships;
using Neo4jClientRepository.Tests.Domain;
using Neo4jClient;

namespace Neo4jClientRepository.Tests
{
    [TestClass]
    public class NeoNodeRepository
    {
        private Neo4NodeRepository<OwnedBy> _nodeRepo;

        private INeo4jRelationshipManager relationshipManager = new Neo4jRelationshipManager();
        [TestInitialize]
        public void Init()
        {
            GraphClient graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"));
            graphClient.Connect();


            _nodeRepo = new Neo4NodeRepository<OwnedBy>(graphClient, relationshipManager);
        }

        [TestMethod]
        public void GetNode()
        {
         //   var result = _nodeRepo.GetByIndex<StorageLocation>("Id", 1);


        }

    }
}
