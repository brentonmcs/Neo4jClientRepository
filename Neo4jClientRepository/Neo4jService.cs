using System.Linq;
using System.Linq.Expressions;
using Neo4jClient;
using Neo4jClient.Gremlin;
using Neo4jClientRepository.Caching;
using Neo4jClientRepository.Interfaces;
using Neo4jClientRepository.RelationshipManager;
using SocialGraph.Neo4j.Neo4jUtils;
using System;



namespace Neo4jClientRepository
{

    // ReSharper disable InconsistentNaming
    public class Neo4jService<T> : INeo4jService<T> where T : class , IDBSearchable, new()
    // ReSharper restore InconsistentNaming
    {
        protected readonly IGraphClient GraphClient;
        protected readonly INeo4jRelationshipManager RelationshipManager;
        protected readonly ICachingService CachingService;

        private readonly string _cacheName;
        protected readonly T Source = new T();


        public Neo4jService(IGraphClient graphClient, INeo4jRelationshipManager relationshipManager, ICachingService cachingService, Func<T, IndexEntry> indexEntry, Action<T, T> updateFields, string cacheName)
        {
            GraphClient = graphClient;
            RelationshipManager = relationshipManager;
            IndexEntry = indexEntry;
            UpdateFields = updateFields;
            _cacheName = cacheName;
            CachingService = cachingService;
        }

        public Node<T> Get(string code)
        {
            var result = GetQuery(x => x.ItemSearchCode() == code );
            UpdateNodeInCache(CacheType.SearchCode, result);
            return result;
        }

        public Node<T> Get(NodeReference<T> node)
        {
            try
            {
                return node.StartCypher("node").Return<Node<T>>("node").Results.Single();
            }
            catch (InvalidOperationException)
            {

                throw new ObjectNotFoundException();
            }

        }

        private Node<T> GetQuery(Expression<Func<T, bool>> filterQuery)
        {
            try
            {
                return GraphClient
                            .RootNode
                            .In(GetRootNodeKey(), filterQuery)
                            .Single();                                
            }
            catch (InvalidOperationException)
            {
                throw new ObjectNotFoundException();
            }
        }

        protected string GetCacheKey(CacheType type, dynamic value)
        {
            if (type == CacheType.Id)
                return _cacheName + value;
            return _cacheName + value;
        }

        public Node<T> Get(int id)
        {
            return GetQuery(x => x.Id == id);
        }

        public Node<T> GetCached(string code)
        {
            if (CachingService == null)
                return Get(code);

            var result = CachingService.Cache(GetCacheKey(CacheType.SearchCode, code), 1000, new Func<string, Node<T>>(Get), code) as Node<T>;
            if (result == null)
                return null;

            UpdateNodeInCache(CacheType.Id, result);
            return result;
        }

        

        public Node<T> GetCached(int id)
        {
            if (CachingService == null)
                return Get(id);

            var result = CachingService.Cache(GetCacheKey(CacheType.Id, id), 1000, new Func<string, Node<T>>(Get), id) as Node<T>;
            if (result == null)
                return null;

            UpdateNodeInCache(CacheType.SearchCode, result);
            return result;
        }

        public string GetRootNodeKey()
        {
            return RelationshipManager.GetTypeKey(typeof(T), typeof(RootNode));
        }

        public Node<T> UpSert(T item)
        {
            var existing = Get(GetSearchCode(item));

            if (existing == null)
            {
                return InsertNew(item);
            }
            if (!existing.Data.Equals(item))
            {
                GraphClient.Update(existing.Reference,
                                   u => UpdateFields(item, u),
                                   u => new[] { IndexEntry(u) });
            }
            return existing;
        }

        protected Func<T, IndexEntry> IndexEntry;

    

        protected void UpdateNodeInCache(CacheType type, Node<T> result)
        {
            if (CachingService != null)
                CachingService.UpdateCacheForKey(GetCacheKey(type, result.Data.Id), 10000, result);
        }

        protected IRelationshipAllowingSourceNode<T> GetItemRelationship()
        {
            return RelationshipManager.GetRelationshipObjectSource<T>(typeof(T), typeof(RootNode), GraphClient.RootNode);
        }

        protected Action<T, T> UpdateFields;

        protected string GetSearchCode(T item)
        {
            return item.ItemSearchCode();
        }

        protected Node<T> InsertNew(T item)
        {
            if (item.Id == 0)
                item.Id = (new Random()).Next();

            GraphClient.Create(item,
                                  new[] { GetItemRelationship() },
                                  new[] { IndexEntry == null ? null : IndexEntry(item) });

            var node = Get(GetSearchCode(item));
            return node;
        }
    }
}
