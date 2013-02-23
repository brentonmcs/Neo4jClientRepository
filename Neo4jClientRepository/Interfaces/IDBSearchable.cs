// ReSharper disable CheckNamespace
namespace Neo4jClientRepository
// ReSharper restore CheckNamespace
{
    public interface IDBSearchable
    {        
        string ItemSearchCode();

        int Id { get; set; }
    }

}
