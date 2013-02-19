using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialGraph.Neo4j.Neo4jUtils
{
    public interface IDBSearchable<T> 
    {
        Expression<Func<T, bool>> FilterQuery(T item);
        Expression<Func<T, bool>> FilterQuery(string code);
        string ItemSearchCode();

        int Id { get; set; }
    }

}
