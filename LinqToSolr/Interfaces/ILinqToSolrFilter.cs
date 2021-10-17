using System.Collections.Generic;

namespace LinqToSolr.Interfaces
{
    public interface ILinqToSolrFilter
    {
        string Field { get; set; }
        ICollection<object> Values { get; set; }

    }
}
