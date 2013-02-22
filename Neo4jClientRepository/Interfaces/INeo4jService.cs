
using Neo4jClient;

namespace Neo4jClientRepository.Interfaces
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
