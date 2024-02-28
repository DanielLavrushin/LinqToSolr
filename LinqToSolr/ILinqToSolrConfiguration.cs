using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToSolr
{
    public interface ILinqToSolrConfiguration
    {
        IDictionary<Type, string> CoreMappings { get; }
        ILinqToSolrConfigurationDefaults Defaults { get; }
        ILinqToSolrEndpoint Endpoint { get; }
        ILinqToSolrConfiguration MapCoreFor(Type type, string coreName);
        ILinqToSolrConfiguration MapCoreFor<T>(string coreName);
        string GetCore<T>();
        string GetCore(Type type);

    }
}
