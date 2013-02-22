﻿using Neo4jClient;
using System.Collections.Generic;

namespace Neo4jClientRepository.Interfaces
{
// ReSharper disable InconsistentNaming
    public interface INeo4jServiceLinked<TSourceNode, TLinkedNode> : INeo4jService<TSourceNode> where TSourceNode : class, IDBSearchable, new()
// ReSharper restore InconsistentNaming
        where TLinkedNode : class, IDBSearchable, new()
    {
        void CreateReleationship(Node<TSourceNode> item, Node<TLinkedNode> linkedItem);

        Node<TSourceNode> UpSert(TSourceNode item, Node<TLinkedNode> linkedItem);
        
        List<Node<TSourceNode>> GetAll();
    }
}