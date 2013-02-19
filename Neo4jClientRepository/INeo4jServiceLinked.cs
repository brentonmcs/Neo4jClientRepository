using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public interface INeo4jServiceLinked<TSourceNode, TLinkedNode, TRootNodeRelationShip, TRelationship> : INeo4jService<TSourceNode>
        where TRootNodeRelationShip : class, new()
        where TRelationship : Relationship, IRelationshipAllowingSourceNode<TSourceNode>
        where TSourceNode : class, IDBSearchable<TSourceNode>, new()
        where TLinkedNode : class, IDBSearchable<TLinkedNode>, new()
    {
        void CreateReleationship(Node<TSourceNode> item, Node<TLinkedNode> linkedItem);

        Node<TSourceNode> UpSert(TSourceNode item, Node<TLinkedNode> linkedItem);
        
        List<Node<TSourceNode>> GetAll();
    }
}
