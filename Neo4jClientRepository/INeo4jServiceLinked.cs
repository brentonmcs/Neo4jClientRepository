using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4jClientRepository;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public interface INeo4jServiceLinked<TSourceNode, TLinkedNode> : INeo4jService<TSourceNode> where TSourceNode : class, IDBSearchable, new()
        where TLinkedNode : class, IDBSearchable, new()
    {
        void CreateReleationship(Node<TSourceNode> item, Node<TLinkedNode> linkedItem);

        Node<TSourceNode> UpSert(TSourceNode item, Node<TLinkedNode> linkedItem);
        
        List<Node<TSourceNode>> GetAll();
    }
}
