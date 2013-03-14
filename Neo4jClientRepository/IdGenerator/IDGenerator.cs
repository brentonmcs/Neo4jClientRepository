using System.Linq;
using Ninject;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Neo4jClientRepository.IdGenerator
{
    public class IDGenerator : IIDGenerator
    {
        private int _cacheSize;

        [Inject]
        public IDRepoService IDRepoService { get; set; }
        private IDictionary<string, IdContainer> _idList;
        
        private bool _isLoaded;

        public class IdContainer
        {            
            public long StartValue { get; set; }
            public long CurrentValue { get; set; }
        }

        public IDGenerator()
        {            
            _isLoaded = false;
        }

        public void LoadGenerator(int cacheSize)
        {
            IDRepoService.InitialiseIdRepoService(this);
            _cacheSize = cacheSize;
            //Update ndoes with current plus cache
            _idList = new ConcurrentDictionary<string, IdContainer>();

            var allIds = IDRepoService.GetAll().ToList();

            if (!allIds.Any())
                _idList.Add("IDGeneratorNode", new IdContainer {CurrentValue =  1, StartValue = 1});


            foreach (var id in allIds)
            {
                _idList.Add(id.GroupName, new IdContainer {CurrentValue = id.CurrentId, StartValue = id.CurrentId});
                IDRepoService.CreateOrUpdateIdNode(id.GroupName,id.CurrentId + _cacheSize);
            }

            _isLoaded = true;
        }

        public long GetNew(string groupName)
        {
            if (!_isLoaded)
                throw new NotLoadedException("Call the Load Generator function before getting new Ids");
           
            if (_idList.ContainsKey(groupName))
            {                
                if (_idList[groupName].CurrentValue - _idList[groupName].StartValue >= _cacheSize)
                    IDRepoService.CreateOrUpdateIdNode(groupName, _idList[groupName].CurrentValue + _cacheSize);

                _idList[groupName].CurrentValue++;
                return _idList[groupName].CurrentValue;
            }

            IDRepoService.CreateOrUpdateIdNode(groupName, _cacheSize);
            _idList.Add(groupName, new IdContainer {CurrentValue = 1, StartValue = 1});
            return 1;
        }
    }
}