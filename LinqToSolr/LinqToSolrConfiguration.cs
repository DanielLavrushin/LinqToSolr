using System;
using System.Collections.Generic;

namespace LinqToSolr
{
    public class LinqToSolrConfiguration : ILinqToSolrConfiguration
    {
        public IDictionary<Type, string> CoreMappings { get; }
        public ILinkToSolrEndpoint Endpoint { get; }

        public LinqToSolrConfiguration(ILinkToSolrEndpoint endpoint)
        {
            Endpoint = endpoint;
            CoreMappings = new Dictionary<Type, string>();
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