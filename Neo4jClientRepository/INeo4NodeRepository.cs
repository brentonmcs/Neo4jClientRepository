using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Neo4jClient;


namespace Neo4jClientRepository
{
    public interface INeo4NodeRepository<TNode> 
        where TNode : class,IDBSearchable, new()
    {
        TNode GetByIndex(string key, object value, Type indexType);
        TNode GetById(long id);
        TNode GetByItemCode(string itemCode);
        TNode GetByTree(Expression<Func<TNode, bool>> filter);
        
        Node<TNode> GetNodeByItemCode(string value);
        Node<TNode> GetNodeByIndex(string key, object value, Type indexType);
        Node<TNode> GetNodeReferenceById(long id);
        Node<TNode> GetNodeByTree(Expression<Func<TNode, bool>> filter);

        IEnumerable<TNode> GetAll();

        Type TargetType { get; }
        Type SourceType { get; }
        

        Node<TNode> UpdateOrInsert(TNode item, NodeReference linkedItem);
        Node<TNode> UpdateOrInsert(TNode item);
        
        void DeleteNode(NodeReference node);
        string ItemCodeIndexName { get;  }

    }
}