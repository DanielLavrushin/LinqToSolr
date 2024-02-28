using LinqToSolr.Providers;
using System.Linq;

namespace LinqToSolr
{
    public class LinqToSolrService : ILinqToSolrService
    {
        public ILinqToSolrConfiguration Configuration { get; }

        public LinqToSolrService(ILinqToSolrConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IQueryable<TResult> AsQueryable<TResult>()
        {
            return new LinqToSolrQueriable<TResult>(new LinqToSolrProvider(this, typeof(TResult)));
        }
    }
}
