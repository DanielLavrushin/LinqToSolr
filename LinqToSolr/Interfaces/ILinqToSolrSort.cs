using LinqToSolr.Data;

namespace LinqToSolr.Interfaces
{

    public interface ILinqToSolrSort
    {

        string Field { get; set; }
        SolrSortTypes Order { get; set; }

    }
}
