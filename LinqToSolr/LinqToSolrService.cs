using LinqToSolr.Providers;
using System;
using System.Linq;

namespace LinqToSolr
{
    public class LinqToSolrService : ILinqToSolrService
    {
        internal object LastResponse { get; set; }
        public ILinqToSolrConfiguration Configuration { get; }
        public LinqToSolrService(ILinqToSolrConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IQueryable<TResult> AsQueryable<TResult>()
        {
            return new LinqToSolrQueriable<TResult>(new LinqToSolrProvider(this, typeof(TResult)));
        }

        public TResponse GetLastResponse<TResponse, TResult>() where TResponse : class, ILinqToSolrFinalResponse<TResult>
        {
            return LastResponse as TResponse;
        }
        public void SetLastResponse(object response)
        {
            LastResponse = response;
        }
    }
}
