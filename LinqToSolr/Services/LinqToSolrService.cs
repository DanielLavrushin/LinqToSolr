using System;
using System.Collections.Generic;
using System.Linq;
using LinqToSolr.Data;

namespace LinqToSolr.Services
{
    public class LinqToSolrService: ILinqToSolrService
    {
        public ILinqToSolrResponse LastResponse { get; set; }
        public Type ElementType { get; set; }
        public LinqToSolrRequestConfiguration Configuration { get; set; }
        public object Query(Type type, LinqToSolrQuery query = default(LinqToSolrQuery))
        {
            throw new NotImplementedException();
        }

        public ICollection<T> Query<T>(LinqToSolrQuery query = default(LinqToSolrQuery))
        {
            throw new NotImplementedException();
        }

        public LinqToSolrQuery CurrentQuery { get; set; }
    }
}
