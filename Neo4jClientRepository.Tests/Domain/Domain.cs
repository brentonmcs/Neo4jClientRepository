using SocialGraph.Neo4j.Neo4jUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClientRepository.Tests.Domain
{
    public class Product
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public int Id { get; set; }
    }

    public class StorageLocation : IDBSearchable<StorageLocation>
    {
        public string Name { get; set; }

        public Expression<Func<StorageLocation, bool>> FilterQuery(StorageLocation item)
        {
            return FilterQuery(item.ItemSearchCode());
        }

        public Expression<Func<StorageLocation, bool>> FilterQuery(string code)
        {
            return x => x.ItemSearchCode() == code;
        }

        public string ItemSearchCode()
        {
            return Name;
        }

        public int Id
        {
            get
            {
                return Id;
            }
            set
            {
                Id = value;
            }
        }
    }

    public class Part
    {
        public string Name { get; set; }
    }
}
