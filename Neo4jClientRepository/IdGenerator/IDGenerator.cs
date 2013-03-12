using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Neo4jClientRepository.IdGenerator
{
    public class IDGenerator : IIDGenerator
    {
        private readonly int _cacheSize;
        private readonly IDRepoService _idRepoService;
        private IDictionary<string, IdContainer> _idList;
        
        private bool _isLoaded;

        public class IdContainer
        {            
            public long StartValue { get; set; }
            public long CurrentValue { get; set; }
        }

        public IDGenerator(int cacheSize, IDRepoService idRepoService)
        {
            _cacheSize = cacheSize;
            _idRepoService = idRepoService;
            _isLoaded = false;
        }
        
        public void LoadGenerator()
        {
            //Update ndoes with current plus cache
            _idList = new ConcurrentDictionary<string, IdContainer>();

            foreach (var id in _idRepoService.GetAll())
            {
                _idList.Add(id.GroupName, new IdContainer {CurrentValue = id.CurrentId, StartValue = id.CurrentId});
                _idRepoService.CreateOrUpdateIdNode(id.GroupName,id.CurrentId + _cacheSize);
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
                    _idRepoService.CreateOrUpdateIdNode(groupName, _idList[groupName].CurrentValue + _cacheSize);

                _idList[groupName].CurrentValue++;
                return _idList[groupName].CurrentValue;
            }

            _idRepoService.CreateOrUpdateIdNode(groupName, _cacheSize);
            _idList.Add(groupName, new IdContainer {CurrentValue = 1, StartValue = 1});
            return 1;
        }
    }
}