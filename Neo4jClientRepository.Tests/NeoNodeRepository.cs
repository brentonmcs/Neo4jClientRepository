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

        [TestMethod]
        public void GetMatchWhenSourceIsRoot()
        {

            var result = _nodeRepo.GetMatch();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            
            Assert.AreEqual("target-[:OWNED_BY]-root", result[0]);
        }

        [TestMethod]
        public void GetMatchWhenSourceIsNotRoot()
        {
            var repo = new Neo4NodeRepository<Requires>(null, relationshipManager);
            var result = repo.GetMatch();

            Assert.IsNotNull(result);
            Assert.AreEqual(2,result.Length);
            Assert.AreEqual("node-[:REQUIRES]-target0", result[0]);
            Assert.AreEqual("target0-[:HAS_RELATED_NODE]-root", result[1]);
        }
    }
}
