
using Neo4jClient;
using Neo4jClientRepository.IdGenerator;
using Neo4jClientRepository.RelationshipManager;
using Neo4jClientRepository.Tests.Domain;
using Neo4jClientRepository.Tests.Relationships;
using NUnit.Framework;
using System;

namespace Neo4jClientRepository.Tests
{
    [TestFixture]
    class RelationshipManagerIntegrationTests
    {
        GraphClient _graphClient;
        Neo4jRelationshipManager _relationshipManager;
        Neo4NodeRepository<OwnedBy> _ownedByService;

        NodeReference _initialAddRef;
        [SetUp]
        public void Init()
        {
            try
            {

            
            _graphClient = new GraphClient(new Uri("http://localhost:7474/db/data"));
            _graphClient.Connect();

            }
            catch (Exception)
            {

                throw new Exception("Not Connected To Neo4j Server");
            }

            _relationshipManager = new Neo4jRelationshipManager();
            var idRepoService = new IdRepoService(_graphClient, _relationshipManager);
            var idGenerator = new IDGenerator(50, idRepoService);
            
            _ownedByService = new Neo4NodeRepository<OwnedBy>(_graphClient, _relationshipManager,idGenerator, "Name");

            _initialAddRef = _ownedByService.UpdateOrInsert(new StorageLocation { Id = 0, Name = "Main WH" }, _graphClient.RootNode);
  
        }


        [TestCase]
        public void TestUpdatesNotInsertAnother()
        {
            var reference2 = _ownedByService.UpdateOrInsert(new StorageLocation { Id = 0, Name = "Main WH" }, _graphClient.RootNode);
                      
            Assert.AreEqual(_initialAddRef, reference2);
        }


        [TestCase]
        public void TestGetByIndex()
        {

            var location = _ownedByService.GetByIndex<StorageLocation>("Name", "Main WH",null);

            Assert.NotNull(location);
            Assert.AreEqual(0, location.Id);
        }

        


        [TestCase]
        public void GetByName()
        {
            var location = _ownedByService.GetById<StorageLocation>(0);
            Assert.NotNull(location);
            Assert.AreEqual(0, location.Id);

        }

        [TestCase]
        public void InsertThenUpdateRecordAndDelete()
        {
            _ownedByService.UpdateOrInsert(new StorageLocation { Id = 2, Name = "Second Warehouse" }, _graphClient.RootNode);

            _ownedByService.UpdateOrInsert(new StorageLocation { Id = 2, Name = "Second Warehouse 2" }, _graphClient.RootNode);

            var location = _ownedByService.GetById<StorageLocation>(2);

            Assert.NotNull(location);
            Assert.AreEqual("Second Warehouse 2", location.Name);

            var refNode = _ownedByService.GetNodeReferenceById<StorageLocation>(2);
            _ownedByService.DeleteNode(refNode.Reference);

            location = _ownedByService.GetById<StorageLocation>(2);
            Assert.IsNull(location);
            
        }


        [TestCase]
        public void GetByTreeSearch()
        {
            var location = _ownedByService.GetByTree<StorageLocation>(node => node.Name == "Main WH");

           Assert.NotNull(location);
           Assert.AreEqual(0, location.Id);
  
        }

        [TearDown]
        public void Destroy()
        {
            var refNode = _ownedByService.GetNodeReferenceById<StorageLocation>(0);

            if (refNode != null)
                _ownedByService.DeleteNode(refNode.Reference);
        }


    }
}
