namespace Neo4jClientRepository.IdGenerator
{
    public interface IIDGenerator
    {
        long GetNew(string groupName);        
    }
}
