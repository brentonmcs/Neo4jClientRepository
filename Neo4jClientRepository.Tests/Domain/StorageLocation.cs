
namespace Neo4jClientRepository.Tests.Domain
{
    public class StorageLocation : IDBSearchable
    {
        public string Name { get; set; }
        
        public int Id { get; set; }
    }
}