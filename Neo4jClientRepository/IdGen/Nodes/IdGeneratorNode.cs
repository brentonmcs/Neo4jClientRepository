namespace Neo4jClientRepository.IdGen
{
    public class IdGeneratorNode : IDBSearchable
    {
        public string GroupName { get; set; }
        public long CurrentId { get; set; }
       

        public long Id { get; set; }
    }
}