namespace Neo4jClientRepository
{
    public interface IPayload
    {
        string CompareName();

        string CompareValue();
    }
}