using System.Collections.Generic;
using FakeItEasy;
using NUnit.Framework;

using Neo4jClientRepository.IdGen;

namespace Neo4jClientRepository.Tests
{
    [TestFixture]
    public class IdGeneratorTests
    {
        private IIdGenerator _idGenerator;
        private INodeRepoCreator _repoCreator;
        private INeo4NodeRepository<IdGeneratorNode> _idRepoService;
      
        [SetUp]
        public void Init()
        {
            _repoCreator = A.Fake<INodeRepoCreator>();
            _idGenerator = new IdGenerator (_repoCreator);
            _idRepoService= A.Fake<INeo4NodeRepository<IdGeneratorNode>>();
            A.CallTo(() => _repoCreator.CreateNode<IdGroupNodeRelationship, IdReferenceNode, IdGeneratorNode>("",typeof(IdGeneratorRefNodeRelationship))).Returns(_idRepoService);
        }

        [Test]
        [ExpectedException(typeof(NotLoadedException))]
        public void TestFailWhenNotLoaded()
        {
            _idGenerator.GetNew("Test");
            Assert.AreNotEqual(null,null);
        }

        [Test]
        public void TestGetOneForNewGroup()
        {
            _idGenerator.LoadGenerator(3);
            A.CallTo(() => _idRepoService.GetAll()).Returns(new List<IdGeneratorNode>());
            var result = _idGenerator.GetNew("Test");
            Assert.AreEqual(1, result);
        }

        [Test]
        public void TestGetTwoForNewGroup()
        {


            var idList = new List<IdGeneratorNode>
                {
                    new IdGeneratorNode{ CurrentId = 1, GroupName = "Test"}
                };
            A.CallTo(() => _idRepoService.GetAll()).Returns(idList);
            _idGenerator.LoadGenerator(3);
            var result = _idGenerator.GetNew("Test");
            Assert.AreEqual(2, result);
        }


        [Test]
        public void TestTheSecondTimeItGivesNextNumber()
        {
            var idList = new List<IdGeneratorNode>
                {
                    new IdGeneratorNode{ CurrentId = 1, GroupName = "Test"}
                };
            A.CallTo(() => _idRepoService.GetAll()).Returns(idList);
            _idGenerator.LoadGenerator(3);
            var result = _idGenerator.GetNew("Test");
            Assert.AreEqual(2, result);
             result = _idGenerator.GetNew("Test");
            Assert.AreEqual(3, result);
        }

        [Test]
        public void TestNewGroupGetsStartingValue()
        {
            var idList = new List<IdGeneratorNode>
                {
                    new IdGeneratorNode{ CurrentId = 1, GroupName = "Test"}
                };
            A.CallTo(() => _idRepoService.GetAll()).Returns(idList);
            _idGenerator.LoadGenerator(3);           
            var result = _idGenerator.GetNew("Test");
            Assert.AreEqual(2, result);
            result = _idGenerator.GetNew("Test2");
            Assert.AreEqual(1, result);
            result = _idGenerator.GetNew("Test");
            Assert.AreEqual(3, result);
        }

        [Test]
        public void TestAfterThreeItGetsNewFromCache()
        {
            var idList = new List<IdGeneratorNode>
                {
                    new IdGeneratorNode{ CurrentId = 1, GroupName = "Test"}
                };
            A.CallTo(() => _idRepoService.GetAll()).Returns(idList);
            _idGenerator.LoadGenerator(3);
            var result = _idGenerator.GetNew("Test");
            Assert.AreEqual(2, result);
            result = _idGenerator.GetNew("Test");
            Assert.AreEqual(3, result);
            result = _idGenerator.GetNew("Test");
            Assert.AreEqual(4, result);
            result = _idGenerator.GetNew("Test");
            Assert.AreEqual(5, result);


            A.CallTo(() => _idRepoService.UpdateOrInsert(GetFakeIdNode2(4, "Test"), null)).MustHaveHappened();
            
            A.CallTo(() => _idRepoService.UpdateOrInsert(GetFakeIdNode2(7, "Test"), null)).MustHaveHappened();
        }


        [Test]
        public void TestReservesCacheInDb()
        {
            var idList = new List<IdGeneratorNode>
                {

                    new IdGeneratorNode{ CurrentId = 1, GroupName = "Test"}
                };
            A.CallTo(() => _idRepoService.GetAll()).Returns(idList);
            
            
            _idGenerator.LoadGenerator(3);

            A.CallTo(() => _idRepoService.UpdateOrInsert(GetFakeIdNode2(4,"Test"), null)).MustHaveHappened();
        //    A.CallTo(() => _idRepoService.UpdateOrInsert(, null)).MustHaveHappened();

            var genNode = new IdGeneratorNode
            {
                GroupName = "IdGeneratorNode",
                Id = 1,
                CurrentId = 3
            };
        //    A.CallTo(() => _idRepoService.UpdateOrInsert(genNode, null)).MustHaveHappened();
        
        }

        private static IdGeneratorNode GetFakeIdNode2(long currentId,string GroupName)
        {
            return A<IdGeneratorNode>.That.Matches(x => x.CurrentId == currentId && x.GroupName == GroupName);
        }

    }
}
