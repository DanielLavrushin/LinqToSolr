using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Services;
using LinqToSolr.Query;
using LinqToSolr.Data;
using LinqToSolr.Interfaces;
using System.Linq.Expressions;

namespace LinqToSolr.Tests.Models
{
    public class SolrServiceFactory
    {

        public ILinqToSolrService solr;
        ILinqToSolrQueriable<TestCoreDoc> quaryable;
        int docsNum = 10;
        public SolrServiceFactory(ILinqToSolrService solr)
        {
            this.solr = solr;
            quaryable = (ILinqToSolrQueriable<TestCoreDoc>)solr.AsQueryable<TestCoreDoc>();
        }

        internal SolrServiceFactory Limit(int docsNum)
        {
            this.docsNum = docsNum;
            return this;
        }

        public TestCoreDoc GenerateDoc()
        {
            return GenerateDocs(1).First();
        }

        public TestCoreDoc[] GenerateDocs(int number)
        {
            var parentId = Guid.NewGuid();
            var docs = new List<TestCoreDoc>();
            for (int i = 0; i < number; i++)
            {
                docs.Add(new TestCoreDoc
                {
                    Id = Guid.NewGuid(),
                    ParentId = parentId,
                    Time = DateTime.Now,
                    Name = "TestCoreDoc " + i
                });
            }
            return docs.ToArray();
        }

        public SolrServiceFactory Reset()
        {
            docsNum = 10;
            quaryable = (ILinqToSolrQueriable<TestCoreDoc>)solr.AsQueryable<TestCoreDoc>();
            return this;
        }
        public ILinqToSolrQueriable<TestCoreDoc> Queriable()
        {
            return quaryable;
        }
        public IEnumerable<TestCoreDoc> AddOrUpdate(params TestCoreDoc[] docs)
        {
            solr.AddOrUpdate(docs);
            return docs;
        }

        public void DeleteAll()
        {
            solr.DeleteAll<TestCoreDoc>();
        }
        public IEnumerable<TestCoreDoc> Query(Expression<Func<TestCoreDoc, bool>> query)
        {
            return quaryable.Where(query).Take(docsNum).ToList();
        }

        public void Delete(Expression<Func<TestCoreDoc, bool>> query)
        {
            quaryable.Delete(query);
        }

        public void Delete(params Guid[] ids)
        {
            var objs = ids.Cast<object>().ToArray();
            quaryable.Delete(objs);
        }
    }
}
