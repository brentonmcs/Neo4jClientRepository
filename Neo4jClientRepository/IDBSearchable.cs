namespace Neo4jClientRepository
{
    public interface IDBSearchable
    {                
        long Id { get; set; }
    }

    public interface IPayload
    {
        bool Compare(object payload);
    }

}
