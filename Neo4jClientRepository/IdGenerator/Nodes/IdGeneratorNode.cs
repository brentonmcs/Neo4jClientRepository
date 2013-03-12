namespace Neo4jClientRepository.IdGenerator
{
    public class IDGeneratorNode : IDBSearchable
    {
        public string GroupName { get; set; }
        public long CurrentId { get; set; }
       

        public long Id { get; set; }
    }
}