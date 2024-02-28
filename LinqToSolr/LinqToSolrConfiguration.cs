using System;
using System.Collections.Generic;

namespace LinqToSolr
{
    public interface ILinqToSolrConfigurationDefaults
    {
        int Take { get; set; }
        int Skip { get; set; }
        int FacetLimit { get; set; }
        void SetDefaults();
    }
    public class LinqToSolrConfigurationDefaults : ILinqToSolrConfigurationDefaults
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public int FacetLimit { get; set; }

        public LinqToSolrConfigurationDefaults()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Take = 500;
            Skip = 0;
            FacetLimit = 500;
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
            Defaults.FacetLimit = defaultFacetLimit;
        }

        public LinqToSolrConfiguration(ILinqToSolrEndpoint endpoint)
            : this(endpoint, null)
        {
        }
        public LinqToSolrConfiguration(ILinqToSolrEndpoint endpoint, ILinqToSolrConfigurationDefaults defauls)
        {
            Endpoint = endpoint;
            CoreMappings = new Dictionary<Type, string>();
            if (defauls == null)
            {
                Defaults = new LinqToSolrConfigurationDefaults();
                Defaults.SetDefaults();
            }
            else
            {
                Defaults = defauls;
            }
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