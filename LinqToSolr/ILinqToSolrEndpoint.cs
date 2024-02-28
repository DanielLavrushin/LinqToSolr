using System;

namespace LinqToSolr
{
    public interface ILinqToSolrEndpoint
    {
        Uri SolrUri { get; set; }
        string Username { get; }
        string Password { get; }
        bool IsProtected { get; }
    }
}
