using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToSolr.Data
{
    public class LinqToSolrRequestConfiguration
    {
        internal static LinqToSolrRequestConfiguration _instance;
        public string SolrLogin { get; set; }
        public string SolrPassword { get; set; }
        public int Take { get; set; }
        public int Start { get; set; }
        public string EndPoint { get; set; }

        public ICollection<LinqToSolrIndexMapping> IndexMappings { get; set; }

        public LinqToSolrRequestConfiguration(string endPoint)
        {
            Initiate(endPoint, 0, 0);
        }

        public LinqToSolrRequestConfiguration(string endPoint, int rows)
        {
            Initiate(endPoint, rows, 0);
        }
        public LinqToSolrRequestConfiguration(string endPoint, int rows, int startFrom)
        {
            Initiate(endPoint, rows, startFrom);
        }

        private void Initiate(string endPoint, int rows, int startFrom)
        {
            EndPoint = endPoint;
            Start = startFrom;
            Take = rows;
            IndexMappings = new List<LinqToSolrIndexMapping>();
        }

        public LinqToSolrRequestConfiguration MapIndexFor(Type type, string index)
        {
            IndexMappings.Add(new LinqToSolrIndexMapping(type, index));
            return this;
        }

        public LinqToSolrRequestConfiguration MapIndexFor<T>(string index)
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
            return null;
        }

        public static LinqToSolrRequestConfiguration Instance(string endpoint)
        {

            _instance = _instance ?? new LinqToSolrRequestConfiguration(endpoint);

            return _instance;
        }


    }
}
