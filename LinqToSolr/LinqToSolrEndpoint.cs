﻿using System;
using System.Linq;
using LinqToSolr.Extensions;

namespace LinqToSolr
{
    public class LinqToSolrEndpoint : ILinqToSolrEndpoint
    {
        const string SOLRINSTANCE = "solr";
        public Uri SolrUri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsProtected => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        public LinqToSolrEndpoint(string solrUrl) : this(solrUrl, null, null)
        { }
        public LinqToSolrEndpoint(string solrUrl, string username, string password)
        {
            Username = username;
            Password = password;
            Uri uri = new Uri(solrUrl);
            string basePath = "/" + (uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? SOLRINSTANCE);
            Uri baseUri = new Uri(uri, basePath);
            string normalizedUri = baseUri.GetLeftPart(UriPartial.Authority) + basePath + "/";
            SolrUri = new Uri(normalizedUri);
        }
    }
}