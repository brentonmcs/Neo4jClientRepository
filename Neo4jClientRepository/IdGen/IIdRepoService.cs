using System.Collections.Generic;

namespace Neo4jClientRepository.IdGen
{
    public interface IIDRepoService
    {
        void InitialiseIdRepoService(IIdGenerator idGenerator);
        IEnumerable<IdGeneratorNode> GetAll();
        void CreateOrUpdateIdNode(string groupName, long newCacheSize, long id);
    }

    public class IidRepoService : IIDRepoService
    {
        private readonly INodeRepoCreator _repoCreator;
        private INeo4NodeRepository<IdGeneratorNode> _idRepo;
        private bool _isLoaded;

        public IidRepoService(INodeRepoCreator repoCreator)            
        {
            _repoCreator = repoCreator;            
            _isLoaded = false;
            
        }

        public void InitialiseIdRepoService(IIdGenerator idGenerator )
        {                        
            _idRepo = _repoCreator.CreateNode<IdGeneratorGroupNodes, IdReferenceNode, IdGeneratorNode>("",typeof (IdGeneratorRefNodeRelationship));                                    
            _isLoaded = true;
        }
    

        public IEnumerable<IdGeneratorNode> GetAll()
        {            
                if (!_isLoaded)
                    throw new NotLoadedException("Run the InitialiseIdRepoService for the IidRepoService");
                return _idRepo.GetAll();
            
        }

        public void CreateOrUpdateIdNode(string groupName, long newCacheSize, long id)
        {
            if (!_isLoaded)
                throw new NotLoadedException("Run the InitialiseIdRepoService for the IidRepoService");
            _idRepo.UpdateOrInsert(new IdGeneratorNode { Id= id, CurrentId = newCacheSize, GroupName = groupName }, null);
        }
        
    }
}
