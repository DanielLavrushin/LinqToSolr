using System;

namespace LinqToSolr
{
    public interface ILinkToSolrEndpoint
    {
        Uri SolrUri { get; set; }
        string Username { get; }
        string Password { get; }
        bool IsProtected { get; }
    }
}
