namespace LinqToSolr
{
    public class LinqToSolrConfiguration : ILinqToSolrConfiguration
    {
        public ILinkToSolrEndpoint Endpoint { get; }

        public LinqToSolrConfiguration(ILinkToSolrEndpoint endpoint)
        {
            Endpoint = endpoint;
        }
    }
}