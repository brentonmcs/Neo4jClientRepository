using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4jClientRepository.RelationshipManager;
using Neo4jClientRepository.Tests.Relationships;
using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;


namespace Neo4jClientRepository.Tests
{
    [TestClass]    
    public class RelationshipManagerTests
    {
        readonly INeo4jRelationshipManager _relationshipManager = new Neo4jRelationshipManager();

        [TestMethod]
        public void RelationshipManagerGetTypeSet()
        {
            //Setup
            var source = typeof (RootNode);
            var target = typeof (StorageLocation);
            
            //Act
            var type = _relationshipManager.GetTypeKey(source, target);

            Assert.AreEqual("OWNED_BY", type);
            
        }

        [TestMethod]
        [ExpectedExceptionAttribute(typeof(RelationshipNotFoundException))]
        public void RelationshipNotFound()
        {
            //Setup
            var source = typeof(String);
            var target = typeof(StorageLocation);

            //Act
            _relationshipManager.GetTypeKey(source, target);
                        
        }

        [TestMethod]
        [ExpectedExceptionAttribute(typeof(RelationshipTypeKeyNotFoundException))]
        public void RelationshipTypeKeyFieldNotSet()
        {
            //Setup
            var source = typeof(Product);
            var target = typeof(StorageLocation);

            //Act
            _relationshipManager.GetTypeKey(source, target);


        }

        [TestMethod]        
        public void RelationshipGetRelationshipObject()
        {
            //Setup
            var source = typeof(Product);
            var target = typeof(StorageLocation);

            //Act
            var result = _relationshipManager.GetRelationshipObject<Relationship>(source, target, 0 );

            //Assert
            Assert.AreEqual(result.GetType(), typeof(StoredIn));
            
        }

        [TestMethod]
        public void RelationshipGetRelationshipObjectParticipant()
        {
            //Setup
            var source = typeof(Product);
            var target = typeof(StorageLocation);

            //Act
            var result = _relationshipManager.GetRelationshipObjectParticipant<Product>(source, target, 0);

            //Assert
            Assert.AreEqual(result.GetType(), typeof(StoredIn));
        }

        [TestMethod]
        public void RelationshipGetRelationshipObjectSource()
        {
            //Setup
            var source = typeof(Product);
            var target = typeof(StorageLocation);

            //Act
            var result = _relationshipManager.GetRelationshipObjectSource<Product>(source, target, 0);

            //Assert
            Assert.AreEqual(result.GetType(), typeof(StoredIn));
        }


        [TestMethod]
        public void RelationshipGetRelationshipObjectWithPayload()
        {
            //Setup
            var source = typeof(StorageLocation);
            var target = typeof(Product);
            var payload = typeof (StorePurchaseProduct.PayLoad);
            //Act
            var result = _relationshipManager.GetRelationshipObject<StorePurchaseProduct, StorePurchaseProduct.PayLoad>(source, target, 0, 
                new StorePurchaseProduct.PayLoad { Purchased = DateTime.UtcNow}, payload);

            //Assert
            Assert.AreEqual(result.GetType(), typeof(StorePurchaseProduct));
        }

        [TestMethod]
        [ExpectedExceptionAttribute(typeof(RelationshipNotFoundException))]
        public void HandleMultipleSourceTypesNoPayLoadDoesntFind()
        {
            var source = typeof(StorageLocation);
            var target = typeof(StorageLocation);

            _relationshipManager.GetRelationshipObjectSource<StorageLocation>(source, target, 0);            
        }

        [TestMethod]
        
        public void HandleMultipleSourceTypes()
        {
            var source = typeof(Part);
            var target = typeof(Part);
            var payload = typeof(Requires.Payload);
            var result = _relationshipManager.GetRelationshipObject<Relationship, Requires.Payload>(source, target, 0, new Requires.Payload { Count = 0 }, payload);

            Assert.AreEqual(typeof(Requires), result.GetType());

        }

        [TestMethod]
        public void TestGetSourceType()
        {
            var resut = _relationshipManager.GetSourceType(typeof(StorePurchaseProduct));
            Assert.AreEqual(typeof(StorageLocation), resut);
        }

        [TestMethod]
        public void TestGetTargetType()
        {
            var resut = _relationshipManager.GetTargetType(typeof(StorePurchaseProduct));
            Assert.AreEqual(typeof(Product), resut);
        }

        [TestMethod]
        public void FindLinkedRelationshipsBasedOnSource()
        {
            var result = _relationshipManager.GetRelationship(typeof(Product));

            Assert.AreEqual(_relationshipManager.GetTargetType(result), typeof (Product));
        }


        [TestMethod]
        public void GetMatchWhenSourceIsRoot()
        {

            var result = _relationshipManager.GetMatchStringToRootForSource(typeof(OwnedBy));

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);

            Assert.AreEqual("node-[:OWNED_BY]-root", result[0]);
        }

        [TestMethod]
        public void GetMatchWhenSourceIsNotRoot()
        {
            var result = _relationshipManager.GetMatchStringToRootForSource(typeof(Requires));

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("node-[:REQUIRES]-target0", result[0]);
            Assert.AreEqual("target0-[:HAS_RELATED_NODE]-root", result[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestForExceptionIfTargetAndSourceMatch()
        {
            _relationshipManager.GetMatchStringToRootForSource(typeof(RelatedProduct));            
        }


    }
}
