using System;
using System.Collections.Generic;
using System.Linq;

using LinqToSolr.Interfaces;

namespace LinqToSolr.Models
{
    public class LinqToSolrConfiguration : ILinqToSolrConfiguration
    {
        internal static ILinqToSolrConfiguration _instance;
        public ICollection<ILinqToSolrIndexMapping> IndexMappings { get; set; }

        public int Take { get; set; }
        public int Start { get; set; }
        public Uri EndPoint { get; set; }
        public int FacetsLimit { get; set; }


        public LinqToSolrConfiguration(string endPoint) : this(endPoint, 10)
        {

        }

        public LinqToSolrConfiguration(string endPoint, int maxDocuments) : this(endPoint, maxDocuments, 0)
        {
        }
        public LinqToSolrConfiguration(string endPoint, int rows, int startFrom)
        {
            if (!endPoint.EndsWith("/"))
                endPoint = endPoint + "/";

            EndPoint = new Uri(endPoint);

            Start = startFrom;
            Take = rows;
            IndexMappings = new List<ILinqToSolrIndexMapping>();
        }
        public LinqToSolrConfiguration SetFacetsLimit(int limit)
        {
            FacetsLimit = limit;
            return this;
        }
        public ILinqToSolrConfiguration MapIndexFor(Type type, string index)
        {
            IndexMappings.Add(new LinqToSolrIndexMapping(type, index));
            return this;
        }

        public ILinqToSolrConfiguration MapIndexFor<T>(string index)
        {
            return MapIndexFor(typeof(T), index);
        }

        public string GetIndex<T>()
        {
            if (IndexMappings.Any())
            {
                var index = IndexMappings.FirstOrDefault(x => x.Type == typeof(T));
                if (index != null)
                {
                    return index.Index;
                }
            }
            return null;
        }

        public string GetIndex(Type type)
        {
            if (IndexMappings.Any())
            {
                var index = IndexMappings.FirstOrDefault(x => x.Type == type);
                if (index != null)
                {
                    return index.Index;
                }
            }

            throw new NullReferenceException($"None of the indexes are mapped to the type {type.Name}");

        }

        public static ILinqToSolrConfiguration Instance(string endpoint)
        {
            _instance = _instance ?? new LinqToSolrConfiguration(endpoint);
            return _instance;
        }
    }
}
