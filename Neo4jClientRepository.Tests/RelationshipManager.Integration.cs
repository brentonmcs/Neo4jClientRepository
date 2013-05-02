
using Neo4jClient;
using Neo4jClientRepository.IdGen;
using Neo4jClientRepository.RelationshipManager;
using Neo4jClientRepository.Tests.Domain;
using Neo4jClientRepository.Tests.Relationships;
using NUnit.Framework;
using System;

namespace Neo4jClientRepository.Tests
{
    [TestFixture]
    [Category("Integration")]
    class RelationshipManagerIntegrationTests
    {
        GraphClient _graphClient;
        Neo4jRelationshipManager _relationshipManager;
        INeo4NodeRepository<StorageLocation> _ownedByService;

        Node<StorageLocation> _initialAddRef;
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
            
            //Chicken and Egg...
            INodeRepoCreator repoCreator = new NodeRepoCreator(_graphClient,_relationshipManager);

            var idGenerator = new IdGenerator(repoCreator);
            idGenerator.LoadGenerator(50);

            _ownedByService = repoCreator.CreateNode<OwnedBy, StorageLocation>("Name");
            
            _initialAddRef = _ownedByService.UpdateOrInsert(new StorageLocation { Id = 1, Name = "Main WH" }, _graphClient.RootNode);
  
        }


        [TestCase]
        public void TestUpdatesNotInsertAnother()
        {
            var reference2 = _ownedByService.UpdateOrInsert(new StorageLocation { Id = 1, Name = "Main WH" }, _graphClient.RootNode);
                      
            Assert.AreEqual(_initialAddRef, reference2);
        }


        [TestCase]
        public void TestGetByIndex()
        {

            var location = _ownedByService.GetByIndex("Name", "Main WH",null);

            Assert.NotNull(location);
            Assert.AreEqual(1, location.Id);
        }

        


        [TestCase]
        public void GetByName()
        {
            var location = _ownedByService.GetById(1);
            Assert.NotNull(location);
            Assert.AreEqual(1, location.Id);

        }

        [TestCase]
        public void InsertThenUpdateRecordAndDelete()
        {
            _ownedByService.UpdateOrInsert(new StorageLocation { Id = 2, Name = "Second Warehouse" }, _graphClient.RootNode);

            _ownedByService.UpdateOrInsert(new StorageLocation { Id = 2, Name = "Second Warehouse 2" }, _graphClient.RootNode);

            var location = _ownedByService.GetById(2);

            Assert.NotNull(location);
            Assert.AreEqual("Second Warehouse 2", location.Name);

            var refNode = _ownedByService.GetNodeReferenceById(2);
            _ownedByService.DeleteNode(refNode.Reference);

            location = _ownedByService.GetById(2);
            Assert.IsNull(location);
            
        }

        [TestCase]
        public void TestId()
        {
            _ownedByService.UpdateOrInsert(new StorageLocation { Id=2, Name = "Second Warehouse" }, _graphClient.RootNode);

            var location = _ownedByService.GetByTree(node => node.Name == "Second Warehouse");


        }

        [TestCase]
        public void GetByTreeSearch()
        {
            var location = _ownedByService.GetByTree(node => node.Name == "Main WH");

           Assert.NotNull(location);
           Assert.AreEqual(1, location.Id);
  
        }

        [TearDown]
        public void Destroy()
        {
            if (_ownedByService == null) return;
            var refNode = _ownedByService.GetNodeReferenceById(1);

            if (refNode != null)
                _ownedByService.DeleteNode(refNode.Reference);

            var warehouse2node = _ownedByService.GetNodeReferenceById(2);

            if (warehouse2node != null)
                _ownedByService.DeleteNode(warehouse2node.Reference);
        }


    }
}
