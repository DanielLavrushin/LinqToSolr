using System.Linq;
using LinqToSolr.Providers;

namespace LinqToSolr
{
    public interface ILinqToSolrService
    {
        ILinqToSolrConfiguration Configuration { get; }
        IQueryable<TResult> AsQueryable<TResult>();
    }
}
