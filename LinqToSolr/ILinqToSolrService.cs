using System.Linq;
using LinqToSolr.Providers;

namespace LinqToSolr
{
    public interface ILinqToSolrService
    {
        ILinqToSolrConfiguration Configuration { get; }
        ILinqToSolrQueriable<TResult> AsQueryable<TResult>();
    }
}
