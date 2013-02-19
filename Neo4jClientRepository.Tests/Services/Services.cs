using Neo4jClient;
using Neo4jClientRepository.Tests.Domain;
using SocialGraph.Neo4j.Neo4jUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClientRepository.Tests.Services
{
    public class StorageLocationService : Neo4jService<StorageLocation>
    {

        public StorageLocationService(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager)
            : base(graphClient, relationshipManager)
        {

        }

        protected override string CacheName
        {
            get
            {
                return "StoreageLocation";
            }
        }

        protected override IndexEntry GetIndexEntry(StorageLocation item)
        {
            return new IndexEntry(CacheName)
            {
                {"Name", item.Name}
            };
        }

        protected override void GetItemUpdateFields(StorageLocation newItem, StorageLocation oldItem)
        {
            //Not Required
        }
    }
}
