using Neo4jClient;
using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Neo4jClientRepository
// ReSharper restore CheckNamespace
{
    // ReSharper disable InconsistentNaming
    public interface INeo4jRelatedNodes<TNode, TTargetNode>

    // ReSharper restore InconsistentNaming
    {
        void AddRelatedRelationship(string sourceCode, string targetCode);

        void AddRelatedRelationship(Node<TNode> source, Node<TTargetNode> target);

        void AddRelatedRelationship<TData>(Node<TNode> source, Node<TTargetNode> target, TData properties) where TData : class, new();

        void AddRelatedRelationship<TData>(string source, string target, TData properties) where TData : class, new();

        List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(string relatedCode) where TSourceNode : class, IDBSearchable, new();        
        List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(int id) where TSourceNode : class, IDBSearchable, new();
        List<Node<TSourceNode>> GetCachedRelated<TSourceNode>(Node<TSourceNode> node) where TSourceNode : class, IDBSearchable, new();

        List<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(string relatedCode) where TSourceNode : class, IDBSearchable, new();
        List<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(int id) where TSourceNode : class, IDBSearchable, new();        
        List<TSourceNode> GetRelated<TSourceNode>(int id) where TSourceNode : class, IDBSearchable, new();
        List<TSourceNode> GetRelated<TSourceNode>(string relatedCode) where TSourceNode : class, IDBSearchable, new();
        
        List<TResult> GetRelated<TResult, TSourceNode>(Node<TSourceNode> node) where TSourceNode : class, new();


        List<RelationshipInstance<TData>> GetRelationships<TData>() where TData : class, new();
    }
}