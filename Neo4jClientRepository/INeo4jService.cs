using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public interface INeo4jService<T>
    {
        Node<T> Get(string code);
        Node<T> Get(int Id);
        Node<T> GetCached(string code);
        Node<T> GetCached(int id);
        Node<T> UpSert(T item);

        string GetRootNodeKey();
    }
}
