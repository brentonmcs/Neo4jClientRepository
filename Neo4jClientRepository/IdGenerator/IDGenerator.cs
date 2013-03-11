using System.Collections.Generic;
using System.Linq;
using Neo4jClient;
using Neo4jClientRepository.RelationshipManager;

namespace Neo4jClientRepository.IdGenerator
{
    public class  IDGenerator :IIDGenerator
    {
        private readonly IGraphClient _graphClient;
        private readonly int _cacheSize;

        private readonly INeo4NodeRepository IdReferences;
        private readonly INeo4jRelatedNodes<IdReferenceNode, IDGeneratorGroupNodes> IdGroupNames;
        
        public IDGenerator(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, int cacheSize)
        {
            _graphClient = graphClient;
            _cacheSize = cacheSize;
            IdReferences = new Neo4NodeRepository<IDGeneratorReferenceNode>(_graphClient, relationshipManager);
            
            IdGroupNames = new Neo4jRelatedNodes<IdReferenceNode, IDGeneratorGroupNodes, IDGeneratorGroupNodes>(_graphClient, relationshipManager,IdReferences,null,null);

            isLoaded = false;
        }

        public void LoadGenerator()
        {

            GeneratorNode = IdReferences.UpdateOrInsert(new IdReferenceNode(), null);                        
            
            //Update ndoes with current plus cache
            IdList = IdGroupNames.GetAllCachedRelated<IDGeneratorNode>().ToDictionary(x=>x.GroupName, x=> new IdContainer { CurrentValue = x.CurrentId, StartValue = x.CurrentId});


            
            isLoaded = true;
        }

       
        
        private NodeReference GeneratorNode;

        private IDictionary<string,IdContainer> IdList;
        private bool isLoaded;

        public class IdContainer
        {
            public NodeReference GroupNodeReference { get; set; } 
            public long StartValue { get; set; }
            public long CurrentValue { get; set; }
        }

        public long GetNew(string groupName)
        {
            if (!isLoaded)
                throw new NotLoadedException("Call the Load Generator function before getting new Ids");

            lock (groupName)
            {

                if (!IdList.ContainsKey(groupName))
                {
                    var id = new IdContainer {CurrentValue = 1, StartValue = 1};
                    CreateNewNode(groupName, id);
                    return id.CurrentValue;
                }

                var currentId = IdList[groupName];

                if (currentId.CurrentValue - currentId.StartValue > _cacheSize)
                    GetNewIds(groupName);

                return currentId.CurrentValue++;
            }

        }

    }
}