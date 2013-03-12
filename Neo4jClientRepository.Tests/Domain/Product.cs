

namespace Neo4jClientRepository.Tests.Domain
{
    public class Product : IDBSearchable
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public long Id { get; set; }

        
    }
}