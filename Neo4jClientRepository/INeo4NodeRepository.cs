using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Neo4jClient;


namespace Neo4jClientRepository
{
    public interface INeo4NodeRepository
    {
        TResult GetByIndex<TResult>(string key, object value, Type indexType) where TResult : class;
        TResult GetById<TResult>(int id) where TResult : class;
        Node<TResult> GetByItemCode<TResult>(string value) where TResult : class;
        IEnumerable<TResult> GetAll<TResult>();

               
        Node<TResult> GetNodeReferenceById<TResult>(int id) where TResult : class;
        TResult GetByTree<TResult>(Expression<Func<TResult, bool>> filter);

        NodeReference UpdateOrInsert<TNode>(TNode item, NodeReference linkedItem) where TNode : class, IDBSearchable, new();
        void DeleteNode(NodeReference node);

        string ItemCodeIndexName { get;  }

    }
}