using System;

namespace LinqToSolr
{
    public interface ILinkToSolrEndpoint
    {
        Uri SolrUri { get; set; }
    }
}
