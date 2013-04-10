using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo4jClientRepository.Tests.Domain
{
    public class Customer :IDBSearchable
    {
        public string Name { get; set; }
        public DateTimeOffset DOB { get; set; }

        public long Id { get; set; }
    }
}
