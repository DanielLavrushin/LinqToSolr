using LinqToSolr.Providers;

namespace LinqToSolr
{
    public class LinqToSolrService : ILinqToSolrService
    {
        public ILinqToSolrConfiguration Configuration { get; }

        public LinqToSolrService()
        {
            Configuration = new LinqToSolrConfiguration();
        }
        public ILinqToSolrQueriable<TResult> AsQueryable<TResult>()
        {
            return new LinqToSolrQueriable<TResult>(new LinqToSolrProvider());
        }
    }
}
