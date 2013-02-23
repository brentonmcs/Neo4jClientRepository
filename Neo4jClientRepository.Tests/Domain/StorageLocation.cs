
namespace Neo4jClientRepository.Tests.Domain
{
    public class StorageLocation : IDBSearchable
    {
        public string Name { get; set; }
        
        public string ItemSearchCode()
        {
            return Name;
        }

        public int Id { get; set; }
    }
}