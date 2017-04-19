using System;
using System.Collections.Generic;
using LinqToSolr.Data;

namespace LinqToSolr.Services
{
    public interface ILinqToSolrService
    {
        ILinqToSolrResponse LastResponse { get; set; }
        Type ElementType { get; set; }
        LinqToSolrRequestConfiguration Configuration { get; set; }
        object Query(Type type, LinqToSolrQuery query = null);
        ICollection<T> Query<T>(LinqToSolrQuery query = null);

        LinqToSolrQuery CurrentQuery { get; set; }

    }
}