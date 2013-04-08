namespace Neo4jClientRepository.IdGen
{
    public interface IIdGenerator
    {
        long GetNew(string groupName);
        void LoadGenerator(int cacheSize);
    }
}
