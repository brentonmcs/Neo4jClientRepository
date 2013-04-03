using CacheController;
using Neo4jClient;
using Neo4jClientRepository.RelationshipManager;
using System.Collections.Generic;
using System.Linq;

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
        private readonly IGraphClient _graphClient;
        private readonly INeo4jRelationshipManager _relationshipManager;
        private readonly ICachingService _cachingService;
        private Neo4NodeRepository<IdGeneratorNode, IdGeneratorGroupNodes> _idRepo;
        private bool _isLoaded;

        public IidRepoService(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, ICachingService cachingService)            
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            _isLoaded = false;
            _cachingService = cachingService;
        }

        public void InitialiseIdRepoService(IIdGenerator idGenerator )
        {
            _idRepo = new Neo4NodeRepository<IdGeneratorNode,IdGeneratorGroupNodes>(_graphClient, _relationshipManager, idGenerator, string.Empty, GetIdReferenceNode(), _cachingService);
            _isLoaded = true;
        }

        private NodeReference GetIdReferenceNode()
        {
            var result =
                _graphClient
                .RootNode
                .StartCypher("root")
                .Match("root-[:IS_ID_GENERATOR_ROOTNODE]-node")
                .Return<Node<IdReferenceNode>>("node");
                            

            var idReferenceNode = result.Results.SingleOrDefault();            
            return idReferenceNode != null ? idReferenceNode.Reference : _graphClient.Create(new IdReferenceNode(),new IdGeneratorRefNodeRelationship(_graphClient.RootNode));
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
