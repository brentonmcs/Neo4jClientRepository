using Neo4jClient;
using Neo4jClientRepository.RelationshipManager;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClientRepository.IdGenerator
{
    public interface IDRepoService
    {
        void InitialiseIdRepoService(IIDGenerator idGenerator);
        IEnumerable<IDGeneratorNode> GetAll();
        void CreateOrUpdateIdNode(string groupName, long newCacheSize);
    }

    public class IdRepoService : IDRepoService
    {
        private readonly IGraphClient _graphClient;
        private readonly INeo4jRelationshipManager _relationshipManager;
        private Neo4NodeRepository<IDGeneratorGroupNodes> _idRepo;
        private bool _isLoaded;

        public IdRepoService(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager)            
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            _isLoaded = false;
        }

        public void InitialiseIdRepoService(IIDGenerator idGenerator )
        {
            _idRepo = new Neo4NodeRepository<IDGeneratorGroupNodes>(_graphClient, _relationshipManager, idGenerator,string.Empty, GetIdReferenceNode());
            _isLoaded = true;
        }

        private NodeReference GetIdReferenceNode()
        {
            var result =
                _graphClient.RootNode.StartCypher("root")
                            .Match("root-[:IS_ID_GENERATOR_ROOTNODE]-node")
                            .Return<Node<IdReferenceNode>>("node");
                            

            var idReferenceNode = result.Results.SingleOrDefault();
            
            if (idReferenceNode != null)
                return idReferenceNode.Reference;
            
            return _graphClient.Create(new IdReferenceNode(),new IDGeneratorRefNodeRelationship(_graphClient.RootNode));            
        }

        public IEnumerable<IDGeneratorNode> GetAll()
        {
            if (!_isLoaded)
                throw new NotLoadedException("Run the InitialiseIdRepoService for the IdRepoService");
            return _idRepo.GetAll<IDGeneratorNode>();
        }

        public void CreateOrUpdateIdNode(string groupName, long newCacheSize)
        {
            if (!_isLoaded)
                throw new NotLoadedException("Run the InitialiseIdRepoService for the IdRepoService");
            _idRepo.UpdateOrInsert(new IDGeneratorNode { CurrentId = newCacheSize, GroupName = groupName }, null);
        }
        
    }
}
