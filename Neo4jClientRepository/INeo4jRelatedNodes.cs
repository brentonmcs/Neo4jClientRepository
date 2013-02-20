using Neo4jClient;
using SocialGraph.Neo4j.Neo4jUtils;
using System.Collections.Generic;

namespace Neo4jClientRepository
{
    // ReSharper disable InconsistentNaming
    public interface INeo4jRelatedNodes<TNode, TTargetNode>

    // ReSharper restore InconsistentNaming
    {
        void AddRelatedRelationship(string sourceCode, string targetCode);

        void AddRelatedRelationship(Node<TNode> source, Node<TTargetNode> target);

        void AddRelatedRelationship<TData>(Node<TNode> source, Node<TTargetNode> target, TData properties) where TData : class, new();

        void AddRelatedRelationship<TData>(string source, string target, TData properties) where TData : class, new();

        List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(Node<TSourceNode> node) where TSourceNode : IDBSearchable;

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