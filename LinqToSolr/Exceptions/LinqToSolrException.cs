using LinqToSolr.Extensions;
using System;

namespace LinqToSolr
{
    public class LinqToSolrException : Exception
    {
        public LinqToSolrResponseError ResponseError { get; private set; }

        private LinqToSolrException(string message, LinqToSolrResponseError responseError)
            : base(message)
        {
            ResponseError = responseError;
        }

        public LinqToSolrException() : base("Unknow solr error occured")
        {
        }

        public LinqToSolrException(string message) : base(message)
        {
        }

        public LinqToSolrException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public static LinqToSolrException ParseSolrErrorResponse(string solrResponse)
        {
            var responseError = JsonParser.FromJson<LinqToSolrResponseError>(solrResponse);
            var message = responseError?.Error?.Mesasge ?? "An error occurred with the Solr response, but no specific message was provided.";
            return new LinqToSolrException(message, responseError);
        }
    }
}
