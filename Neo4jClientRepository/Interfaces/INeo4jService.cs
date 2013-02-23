
using Neo4jClient;

// ReSharper disable CheckNamespace
namespace Neo4jClientRepository
// ReSharper restore CheckNamespace
{
    // ReSharper disable InconsistentNaming
    public interface INeo4jService<T>
    // ReSharper restore InconsistentNaming
    {
        Node<T> Get(string code);
        Node<T> Get(int id);
        Node<T> Get(NodeReference<T> node);

        Node<T> GetCached(string code);
        Node<T> GetCached(int id);
        Node<T> UpSert(T item);

        string GetRootNodeKey();
                
    }
}
