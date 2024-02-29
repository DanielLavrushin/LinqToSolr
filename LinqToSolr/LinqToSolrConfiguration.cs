using System;
using System.Collections.Generic;

namespace LinqToSolr
{
    public interface ILinqToSolrConfigurationDefaults
    {
        int Take { get; set; }
        int Skip { get; set; }
    }
    public class LinqToSolrConfigurationDefaults : ILinqToSolrConfigurationDefaults
    {
        public int Take { get; set; } = 500;
        public int Skip { get; set; } = 0;

        public LinqToSolrConfigurationDefaults()
        {
        }
        public LinqToSolrConfigurationDefaults(ILinqToSolrConfigurationDefaults defaults)
        {
            if (defaults != null)
            {
                Take = defaults.Take;
                Skip = defaults.Skip;
            }
        }
    }
    public class LinqToSolrConfiguration : ILinqToSolrConfiguration
    {
        public ILinqToSolrConfigurationDefaults Defaults { get; }
        public IDictionary<Type, string> CoreMappings { get; }
        public ILinqToSolrEndpoint Endpoint { get; }

        public LinqToSolrConfiguration(string solrUrl, int defaultTake, int defaultSkip, int defaultFacetLimit)
            : this(new LinqToSolrEndpoint(solrUrl), new LinqToSolrConfigurationDefaults())
        {
            Defaults.Take = defaultTake;
            Defaults.Skip = defaultSkip;
        }

        public LinqToSolrConfiguration(ILinqToSolrEndpoint endpoint)
            : this(endpoint, null)
        {
        }
        public LinqToSolrConfiguration(ILinqToSolrEndpoint endpoint, ILinqToSolrConfigurationDefaults defauls)
        {
            Endpoint = endpoint;
            CoreMappings = new Dictionary<Type, string>();
            Defaults = defauls;
        }

        public ILinqToSolrConfiguration MapCoreFor(Type type, string coreName)
        {
            CoreMappings.Add(type, coreName);
            return this;
        }

        public ILinqToSolrConfiguration MapCoreFor<T>(string coreName)
        {
            return MapCoreFor(typeof(T), coreName);
        }

        public string GetCore<T>()
        {
            return GetCore(typeof(T));
        }

        public string GetCore(Type type)
        {
            if (CoreMappings.ContainsKey(type))
            {
                return CoreMappings[type];
            }

            throw new NullReferenceException($"None of the solr cores are mapped to the type {type.Name}");

        }

    }
}