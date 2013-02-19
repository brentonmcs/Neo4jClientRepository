using Neo4jClient;
using Neo4jClient.Gremlin;
using SocialGraph.Neo4j.Neo4jUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClientRepository
{
  

    public  class Neo4jServiceV2<T> : INeo4jService<T> where T : class , SocialGraph.Neo4j.Neo4jUtils.IDBSearchable<T>, new()
    {
        protected readonly IGraphClient graphClient;
        protected readonly INeo4jRelationshipManager relationshipManager;

        protected abstract string CacheName { get;  }

        private readonly T source = new T();

       
        public Neo4jServiceV2(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, Func<T,IndexEntry> IndexEntry, Action<T,T> UpdateFields  )
        {
            this.graphClient = graphClient;
            this.relationshipManager = relationshipManager;
            _IndexEntry = IndexEntry;
            _UpdateFields = UpdateFields;
        }

        public Node<T> Get(string code)
        {
            try
            {
                return graphClient
                      .RootNode
                      .In<T>(GetRootNodeKey(), source.FilterQuery(code))
                      .Single();
            }
            catch (InvalidOperationException)
            {
                throw new ObjectNotFoundException();
            }
        }

        public Node<T> Get(int Id)
        {
            try
            {
                return graphClient
                   .RootNode
                   .In<T>(GetRootNodeKey(), x => x.Id == Id)
                   .Single();
            }
            catch (InvalidOperationException)
            {
                throw new ObjectNotFoundException();
            }
        }

        public Node<T> GetCached(string code)
        {
            var result =  CachingService.Cache(CacheName +  code, 1000, new Func<string, Node<T>>(Get), code) as Node<T>;
            CachingService.UpdateCacheForKey(CacheName + result.Data.Id, 1000, result);
            return result;
        }

        public Node<T> GetCached(int  Id)
        {
            var result = CachingService.Cache(CacheName + Id, 1000, new Func<string, Node<T>>(Get), Id) as Node<T>;
            CachingService.UpdateCacheForKey(CacheName + result.Data.ItemSearchCode(), 1000, result);
            return result;
        }

        public string GetRootNodeKey()
        {
            return relationshipManager.GetTypeKey(typeof(T), typeof(RootNode));
        }

        public Node<T> UpSert(T item)
        {
            var existing = Get(GetSearchCode(item));

            if (existing == null)
            {
                return InsertNew(item);
            }
            else
            {
                if (!existing.Data.Equals(item))
                {
                    graphClient.Update(existing.Reference,
                            u => _UpdateFields(item, u),
                            u => new[]{_IndexEntry(u)});
                }
                return existing;
            }
        }

        protected Func<T, IndexEntry> _IndexEntry;

        protected IRelationshipAllowingSourceNode<T> GetItemRelationship()
        {
            return relationshipManager.GetRelationshipObjectSource<T>(typeof(T), typeof(RootNode), graphClient.RootNode) as IRelationshipAllowingSourceNode<T>;
        }

        protected Action<T, T> _UpdateFields;
        protected string GetSearchCode(T item)
        {
            return item.ItemSearchCode();
        }

        protected Node<T> InsertNew(T item)
        {
            if (item.Id == 0)
                item.Id = (new Random()).Next();

            graphClient.Create<T>(item,
                                  new[] { GetItemRelationship() },
                                  new[] { _IndexEntry  == null ? null : _IndexEntry(item) });

            var node = Get(GetSearchCode(item));
  //          CachingService.UpdateCacheForKey(GetSearchCode(item), 10000, node);
            return node;
        }
    }
}
