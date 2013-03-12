

namespace Neo4jClientRepository.Tests.Domain
{
    public class Product : IDBSearchable
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public int Id { get; set; }

        public string ItemSearchCodeIndexName()
        {
            return Name;
        }
    }
}