
using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;
using Neo4jClientRepository.Tests.Relationships;
using NUnit.Framework;
using System;

namespace Neo4jClientRepository.Tests
{
    [TestFixture]
    class RelationshipManagerIntegrationTests
    {
        GraphClient graphClient;
        Neo4jRelationshipManager relationshipManager;
        Neo4NodeRepository<OwnedBy> ownedByService;

        NodeReference initialAddRef;
        [SetUp]
        public void Init()
        {
            graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"));
            graphClient.Connect();

            relationshipManager = new Neo4jRelationshipManager();

            ownedByService = new Neo4NodeRepository<OwnedBy>(graphClient, relationshipManager);

            initialAddRef = ownedByService.UpSert(new StorageLocation { Id = 0, Name = "Main WH" }, graphClient.RootNode);
  
        }


        [TestCase]
        public void TestUpdatesNotInsertAnother()
        {
            var reference2 = ownedByService.UpSert(new StorageLocation { Id = 0, Name = "Main WH" }, graphClient.RootNode);
                      
            Assert.AreEqual(initialAddRef, reference2);
        }


        [TestCase]
        public void TestGetByIndex()
        {

            var location = ownedByService.GetByIndex<StorageLocation>("Name", "Main WH");

            Assert.NotNull(location);
            Assert.AreEqual(0, location.Id);
        }

        


        [TestCase]
        public void GetByName()
        {
            var location = ownedByService.GetById<StorageLocation>(0);
            Assert.NotNull(location);
            Assert.AreEqual(0, location.Id);

        }

        [TestCase]
        public void InsertThenUpdateRecordAndDelete()
        {
            ownedByService.UpSert(new StorageLocation { Id = 2, Name = "Second Warehouse" }, graphClient.RootNode);

            ownedByService.UpSert(new StorageLocation { Id = 2, Name = "Second Warehouse 2" }, graphClient.RootNode);

            var location = ownedByService.GetById<StorageLocation>(2);

            Assert.NotNull(location);
            Assert.AreEqual("Second Warehouse 2", location.Name);

            var refNode = ownedByService.GetNodeReferenceById<StorageLocation>(2);
            ownedByService.DeleteNode(refNode.Reference);

            location = ownedByService.GetById<StorageLocation>(2);
            Assert.IsNull(location);
            
        }


        [TestCase]
        public void GetByTreeSearch()
        {
            var location = ownedByService.GetByTree<StorageLocation>(node => node.Name == "Main WH");

           Assert.NotNull(location);
           Assert.AreEqual(0, location.Id);
  
        }

        [TearDown]
        public void Destroy()
        {
            var refNode = ownedByService.GetNodeReferenceById<StorageLocation>(0);
            ownedByService.DeleteNode(refNode.Reference);
        }


    }
}
