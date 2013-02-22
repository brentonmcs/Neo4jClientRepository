using Neo4jClientRepository.Interfaces;

namespace Neo4jClientRepository.Tests.Domain
{
    public class Part :IDBSearchable
    {
        public string Name { get; set; }
        
        public string ItemSearchCode()
        {
            return Name;
        }

        public int Id { get; set; }
    }
}
