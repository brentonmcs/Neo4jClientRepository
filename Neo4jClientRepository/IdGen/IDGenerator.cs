using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ninject;

namespace Neo4jClientRepository.IdGen
{
    public class IdGenerator : IIdGenerator
    {
        private int _cacheSize;

        [Inject]
        public IIDRepoService IidRepoService { get; set; }
        private IDictionary<string, IdContainer> _idList;

        private const string IdNodeName = "IdGeneratorNode";
        
        private bool _isLoaded;

        class IdContainer
        {            
            public long StartValue { get; set; }
            public long CurrentValue { get; set; }
            public long Id { get; set; }
        }

        public IdGenerator()
        {            
            _isLoaded = false;
        }

        public void LoadGenerator(int cacheSize)
        {
            IidRepoService.InitialiseIdRepoService(this);
            _cacheSize = cacheSize;
            //Update ndoes with current plus cache
            _idList = new ConcurrentDictionary<string, IdContainer>();

            var allIds = IidRepoService.GetAll().ToList();

            

            _isLoaded = true;

            foreach (var id in allIds)
            {
                _idList.Add(id.GroupName, new IdContainer {CurrentValue = id.CurrentId, StartValue = id.CurrentId, Id = id.Id});
                IidRepoService.CreateOrUpdateIdNode(id.GroupName,id.CurrentId + _cacheSize, id.Id);
            }


            CreateIdNodeToKeepTrackOfIdsForIds(allIds);
        }

        private void CreateIdNodeToKeepTrackOfIdsForIds(IReadOnlyCollection<IdGeneratorNode> allIds)
        {
            if (allIds.Any(x => x.GroupName == IdNodeName)) return;

            var idNodeIndexId = allIds.Any() ? 1 : allIds.Count + 1;
            _idList.Add(IdNodeName, new IdContainer {CurrentValue = 1, StartValue = 1, Id = idNodeIndexId});
            IidRepoService.CreateOrUpdateIdNode(IdNodeName, _cacheSize, idNodeIndexId);
        }

        public long GetNew(string groupName)
        {
            if (!_isLoaded)
                throw new NotLoadedException("Call the Load Generator function before getting new Ids");
           
            if (_idList.ContainsKey(groupName))
            {
                var currentContainer = _idList[groupName];
                if (currentContainer.CurrentValue - currentContainer.StartValue >= _cacheSize)
                    IidRepoService.CreateOrUpdateIdNode(groupName, currentContainer.CurrentValue + _cacheSize,currentContainer.Id);

                currentContainer.CurrentValue++;
                return currentContainer.CurrentValue;
            }

            IidRepoService.CreateOrUpdateIdNode(groupName, _cacheSize, GetNew(IdNodeName));
            _idList.Add(groupName, new IdContainer {CurrentValue = 1, StartValue = 1});
            return 1;
        }
    }
}