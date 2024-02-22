using LinqToSolr.Providers;

namespace LinqToSolr
{
    public class LinqToSolrService : ILinqToSolrService
    {
        public ILinqToSolrConfiguration Configuration { get; }

        public LinqToSolrService(ILinqToSolrConfiguration configuration)
        {
            Configuration = configuration;
        }
        public ILinqToSolrQueriable<TResult> AsQueryable<TResult>()
        {
            return new LinqToSolrQueriable<TResult>(new LinqToSolrProvider(this));
        }
    }
}
