using Neo4jClient;
using System.Collections.Generic;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public interface INeo4jRelatedNodes<TNode, TargetNode, TRelationship>
    {
        void AddRelatedRelationship(string sourceCode, string targetCode);

        void AddRelatedRelationship(Node<TNode> source, Node<TargetNode> target);

        void AddRelatedRelationship<TData>(Node<TNode> source, Node<TargetNode> target, TData properties) where TData : class, new();

        void AddRelatedRelationship<TData>(string source, string target, TData properties) where TData : class, new();

        List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(Node<TSourceNode> node) where TSourceNode : IDBSearchable<TSourceNode>;

        List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(string relatedCode);

        List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(int id);

        List<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(string relatedCode);

        List<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(int id);

        List<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(Node<TSourceNode> node);

        List<TSourceNode> GetRelated<TSourceNode>(int id);
        
        List<TSourceNode> GetRelated<TSourceNode>(string relatedCode);

        List<TSourceNode> GetRelated<TSourceNode>(Node<TSourceNode> node);


        List<RelationshipInstance<TData>> GetRelationships<TData>() where TData : class, new();
    }
}