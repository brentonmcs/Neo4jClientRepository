namespace Neo4jClientRepository.IdGen
{
    public interface IIdGenerator
    {
        IIDRepoService IidRepoService { get; set; }
        long GetNew(string groupName);
        void LoadGenerator(int cacheSize);
    }
}
