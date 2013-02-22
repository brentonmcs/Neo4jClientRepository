using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4jClientRepository.Caching;


namespace Neo4jClientRepository.Tests
{
    
    [TestClass]
    public class CacheTests
    {
        private ICachingService _cachingService;
        

        [TestInitialize]
        public void Init()
        {
            _cachingService = new CachingService();
            _cachingService.DeleteAll();
        }

        [TestMethod]
        public void CacheReturnsSameDateTime()
        {
            var result = _cachingService.Cache("test", 10, new Func<DateTime>(TestFunction));

            var result2 = _cachingService.Cache("test", 10, new Func<DateTime>(TestFunction));

            Assert.AreEqual(result, result2);

        }

        [TestMethod]
        public void CacheDoesntReturnSameDateTimeAfterCachePeriod()
        {
            var result = _cachingService.Cache("test", 10, new Func<DateTime>(TestFunction));

            System.Threading.Thread.Sleep(15);

            var result2 = _cachingService.Cache("test", 10, new Func<DateTime>(TestFunction));

            Assert.AreNotEqual(result, result2);

        }

        [TestMethod]
        public void CachingReturnsDifferentResultForDifferentKeys()
        {
            var result = _cachingService.Cache("test", 10, new Func<DateTime>(TestFunction));

            System.Threading.Thread.Sleep(5);
            var result2 = _cachingService.Cache("test1", 10, new Func<DateTime>(TestFunction));

            Assert.AreNotEqual(result, result2);

        }

        [TestMethod]
        public void CachingReturnsDifferentResultForDifferentKeys2()
        {
            var result = _cachingService.Cache("test", 10, new Func<DateTime>(TestFunction));


            var result2 = _cachingService.Cache("test1", 10, new Func<int>(TestFunction2));

            Assert.AreNotEqual(result, result2);
        }

        [TestMethod]
        public void CacheReturnsDifferentAmountAfterCacheKeyDeleted()
        {
            var result = _cachingService.Cache("test", 100, new Func<DateTime>(TestFunction));

            _cachingService.DeleteCache("test");
            System.Threading.Thread.Sleep(5);
            var result2 = _cachingService.Cache("test", 100, new Func<DateTime>(TestFunction));

            Assert.AreNotEqual(result, result2);

        }

        [TestMethod]
        public void CacheReturnsDifferentAmountAfterCacheCompletelyDeleted()
        {
            var result = _cachingService.Cache("test", 30, new Func<DateTime>(TestFunction));
            _cachingService.DeleteAll();
            System.Threading.Thread.Sleep(5);
            var result2 = _cachingService.Cache("test", 30, new Func<DateTime>(TestFunction));

            Assert.AreNotEqual(result, result2);

        }

        [TestMethod]
        public void CacheResultIsUpdatedWithNewResult()
        {
            var result = _cachingService.Cache("test", 10, new Func<int>(TestFunction2));


            _cachingService.UpdateCacheForKey("test", 20, 1);
            var result2 = _cachingService.Cache("test", 10, new Func<int>(TestFunction2));

            Assert.AreNotEqual(result, result2);
            Assert.AreEqual(1,result2);
        }


        [TestMethod]
        public void CacheReturnNullIfDelegateIsNull()
        {
            var result = _cachingService.Cache("test7", 10, null);


                        
            Assert.AreEqual(null,result);
        }


        [TestMethod]
        [ExpectedException(typeof(CacheDelegateMethodException))]
        public void CacheThrowsExceptionIfDelegateDoes()
        {
            var result = _cachingService.Cache("test", 10, new Func<int>(Test3));



            Assert.AreEqual(null, result);
        }

        public DateTime TestFunction()
        {

            return DateTime.Now;
        }

        public int TestFunction2()
        {
            return 0;
        }

        public int Test3()
        {
            throw new NullReferenceException();

        }
    }
}
