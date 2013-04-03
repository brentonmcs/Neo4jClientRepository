using System.Collections.Generic;
using FakeItEasy;
using NUnit.Framework;

using Neo4jClientRepository.IdGen;

namespace Neo4jClientRepository.Tests
{
    [TestFixture]
    public class IdGeneratorTests
    {
        private IIDRepoService _repoService;
        private IIdGenerator _idGenerator;
        [SetUp]
        public void Init()
        {
            _repoService = A.Fake<IIDRepoService>();

            _idGenerator = new IdGenerator {IidRepoService = _repoService};
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
            A.CallTo(() => _repoService.GetAll()).Returns(new List<IdGeneratorNode>());
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
            A.CallTo(() => _repoService.GetAll()).Returns(idList);
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
            A.CallTo(() => _repoService.GetAll()).Returns(idList);
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
            A.CallTo(() => _repoService.GetAll()).Returns(idList);
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
            A.CallTo(() => _repoService.GetAll()).Returns(idList);
            A.CallTo(() => _repoService.InitialiseIdRepoService(A<IIdGenerator>.Ignored)).DoesNothing();
            _idGenerator.LoadGenerator(3);
            var result = _idGenerator.GetNew("Test");
            Assert.AreEqual(2, result);
            result = _idGenerator.GetNew("Test");
            Assert.AreEqual(3, result);
            result = _idGenerator.GetNew("Test");
            Assert.AreEqual(4, result);
            result = _idGenerator.GetNew("Test");
            Assert.AreEqual(5, result);
            A.CallTo(() => _repoService.CreateOrUpdateIdNode("Test",7,A<long>.Ignored)).MustHaveHappened();
        }

        [Test]
        public void TestReservesCacheInDb()
        {
            var idList = new List<IdGeneratorNode>
                {
                    new IdGeneratorNode{ CurrentId = 1, GroupName = "Test"}
                };
            A.CallTo(() => _repoService.GetAll()).Returns(idList);

            _idGenerator.LoadGenerator(3);
            A.CallTo(() => _repoService.CreateOrUpdateIdNode("Test", 4, A<long>.Ignored)).MustHaveHappened();
        }

    }
}
