using Neo4jClient;
using Neo4jClientRepository.RelationshipManager;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClientRepository.IdGenerator
{
    public interface IDRepoService
    {
        IEnumerable<IDGeneratorNode> GetAll();
        void CreateOrUpdateIdNode(string groupName, long newCacheSize);
    }

    public class IdRepoService : IDRepoService
    {
        private readonly IGraphClient _graphClient;
        private readonly INeo4jRelationshipManager _relationshipManager;
        private readonly Neo4NodeRepository<IDGeneratorGroupNodes> _idRepo;

        public IdRepoService(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager)            
        {
            _graphClient = graphClient;
            _relationshipManager = relationshipManager;
            _idRepo = new Neo4NodeRepository<IDGeneratorGroupNodes>(_graphClient, _relationshipManager,string.Empty, GetIdReferenceNode());
        }

        private NodeReference GetIdReferenceNode()
        {
            var idReferenceNode =
                _graphClient.RootNode.StartCypher("root")
                            .Match("root-[:IS_ID_GENERATOR_ROOTNODE]-node")
                            .Return<Node<IDGeneratorReferenceNode>>("node")
                            .Results.SingleOrDefault();
            
            if (idReferenceNode != null)
                return idReferenceNode.Reference;
            
            return _graphClient.Create(new IdReferenceNode(),new IDGeneratorReferenceNode(_graphClient.RootNode));            
        }

        public IEnumerable<IDGeneratorNode> GetAll()
        {
            return _idRepo.GetAll<IDGeneratorNode>();
        }

        public void CreateOrUpdateIdNode(string groupName, long newCacheSize)
        {
            _idRepo.UpdateOrInsert(new IDGeneratorNode { CurrentId = newCacheSize, GroupName = groupName }, null);
        }
        
    }
}
