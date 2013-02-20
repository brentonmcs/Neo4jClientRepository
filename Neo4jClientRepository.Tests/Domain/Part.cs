using SocialGraph.Neo4j.Neo4jUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClientRepository.Tests.Domain
{
    public class Part :IDBSearchable
    {
        public string Name { get; set; }
        
        public string ItemSearchCode()
        {
            return Name;
        }

        public int Id { get; set; }
    }
}
