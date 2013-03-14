namespace Neo4jClientRepository.IdGenerator
{
    public interface IIDGenerator
    {
        IDRepoService IDRepoService { get; set; }
        long GetNew(string groupName);
        void LoadGenerator(int cacheSize);
    }
}
