using System;
using System.Collections.Generic;
using Neo4jClient;

namespace Neo4jClientRepository
{
   
    public interface INeo4jRelatedNodes<TNode, TTargetNode>   
    {
        void AddRelatedRelationship(long sourceId, long targetId);
        void AddRelatedRelationship(string sourceCode, string targetCode);
        void AddRelatedRelationship(Node<TNode> source, Node<TTargetNode> target);

        void AddRelatedRelationship<TData>(long sourceId, long targetId, TData properties) where TData : class, IPayload, new();         
        void AddRelatedRelationship<TData>(Node<TNode> source, Node<TTargetNode> target, TData properties) where TData : class, IPayload, new();         
        void AddRelatedRelationship<TData>(string source, string target, TData properties) where TData : class, IPayload, new();

        string GetRootTypeKey(Type payload = null);

        IEnumerable<Node<TSourceNode>> GetCachedRelated<TSourceNode>(string relatedCode, bool searchSource) where TSourceNode : class, IDBSearchable, new();
        IEnumerable<Node<TSourceNode>> GetCachedRelated<TSourceNode>(long id, bool searchSource) where TSourceNode : class, IDBSearchable, new();
        IEnumerable<Node<TSourceNode>> GetCachedRelated<TSourceNode>(Node<TSourceNode> node) where TSourceNode : class, IDBSearchable, new();

        IEnumerable<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(string relatedCode, bool searchSource) where TSourceNode : class, IDBSearchable, new();
        IEnumerable<Node<TSourceNode>> GetRelatedNodes<TSourceNode>(long id, bool searchSource) where TSourceNode : class, IDBSearchable, new();

        IEnumerable<TSourceNode> GetRelated<TSourceNode>(long id, bool searchSource) where TSourceNode : class, IDBSearchable, new();
        IEnumerable<TSourceNode> GetRelated<TSourceNode>(string relatedCode, bool searchSource) where TSourceNode : class, IDBSearchable, new();
        IEnumerable<TResult> GetRelated<TResult>(NodeReference node);

        IEnumerable<RelationshipInstance<TData>> GetRelationships<TData>()  where TData : class, IPayload, new();

        IEnumerable<TSourceNode> GetAllCachedRelated<TSourceNode>();

        IEnumerable<TSourceNode> FindOtherRelated<TSourceNode>(Node<TSourceNode> startingNode, string typeKey) where TSourceNode : IDBSearchable;
    }
}