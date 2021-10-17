using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

using LinqToSolr.Data;
using LinqToSolr.Query;
using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Services
{
    public class LinqToSolrService : ILinqToSolrService
    {
        public ILinqToSolrProvider Provider { get; }
        public ILinqToSolrConfiguration Configuration { get; }

        internal ILinqToSolrQuery CurrentQuery { get; set; }
        public Uri LastResponseUrl { get; set; }
        public LinqToSolrService() : this(LinqToSolrConfiguration._instance)
        {
        }

        public LinqToSolrService(ILinqToSolrConfiguration configuration)
        {
            Configuration = configuration;
            if (Configuration.Take <= 0)
            {
                Configuration.Take = 10;
            }
            if (Configuration.EndPoint == null)
                throw new Exception("Solr Endpoint was not provided. Specify Solr server in SolrRequestConfiguration.");

            Provider = new LinqToSolrProvider(this);
        }

        public IQueryable<T> AsQueryable<T>()
        {
            return new LinqToSolrQueriable<T>(Provider, null);
        }


        private SolrWebRequest PrepareUpdateOrDeleteRequest<T>(T[] documentsToUpdate, object[] deleteDocIds, string deleteByQuery, bool softCommit = false)
        {

            var index = Configuration.GetIndex(typeof(T));

            if (string.IsNullOrEmpty(index))
            {
                throw new ArgumentNullException(nameof(index),
                    string.Format(
                        "The type '{0}' is not assigned for any Solr Index. Register this type in a service configuration (SolrRequestConfiguration.MapIndexFor)",
                       typeof(T).Name));
            }

            string path = string.Format("/{0}/update", index);
            var request = new SolrWebRequest(path, SolrWebMethod.POST);


            var updateDocs = documentsToUpdate.ToJson();
            request.AddParameter("wt", "json");
            request.AddParameter("commit", "true");
            if (softCommit)
            {
                request.AddParameter("softCommit", "true");

            }
            if (documentsToUpdate != null && documentsToUpdate.Any())
            {
                request.Body = updateDocs;
            }
            else if (deleteDocIds != null && deleteDocIds.Any())
            {
                request.Body = new { delete = deleteDocIds }.ToJson();
            }
            else if (!string.IsNullOrEmpty(deleteByQuery))
            {
                request.Body = new { delete = new { query = deleteByQuery } }.ToJson();
            }




            return request;
        }

        //private void PerformUpdate<T>(T[] documentsToUpdate, bool softCommit = false)
        //{
        //    var request = PrepareUpdateOrDeleteRequest(documentsToUpdate, null, null, softCommit);
        //    FinalizeResponse(request);


        //}



        //private void FinalizeResponse(SolrWebRequest request)
        //{
        //    var responce = Client.Execute(request);
        //    if (responce.StatusCode == HttpStatusCode.OK || responce.StatusCode == HttpStatusCode.NoContent)
        //    {
        //        LastResponse = new SolrResponse { LastServiceUri = responce.ResponseUri };
        //        return;
        //    }

        //    if (!string.IsNullOrEmpty(responce.Content))
        //    {
        //        LastResponse = responce.Content.FromJson<SolrResponse>();
        //        LastResponse.LastServiceUri = responce.ResponseUri;
        //        LastResponse.Content = responce.Content;
        //    }

        //    if (LastResponse.Error != null)
        //        throw new Exception("Oops! SOLR Says: " + LastResponse.Error.Message);
        //}

        //public void AddOrUpdate<T>(T[] document, bool softCommit = false)
        //{
        //    if (document == null)
        //        throw new ArgumentNullException(nameof(document));

        //    PerformUpdate(document, softCommit);
        //}
        //public void AddOrUpdate<T>(T document, bool softCommit = false)
        //{
        //    AddOrUpdate<T>(new[] { document }, softCommit);
        //}

        //public void Delete<T>(object documentId, bool softCommit = false)
        //{
        //    Delete<T>(new object[] { documentId }, softCommit);
        //}

        //public void Delete<T>(object[] documentId, bool softCommit = false)
        //{
        //    if (documentId == null)
        //        throw new ArgumentNullException(nameof(documentId));

        //    PerformDelete<T>(documentId, softCommit);
        //}
        //public void Delete<T>(Expression<Func<T, bool>> query, bool softCommit = false)
        //{
        //    if (query == null)
        //        throw new ArgumentNullException(nameof(query));

        //    var translator = new LinqToSolrQueryTranslator(this, typeof(T));
        //    var q = Evaluator.PartialEval(query);
        //    var queryToStr = translator.Translate(BooleanVisitor.Process(q));
        //    PerformDelete<T>(queryToStr, softCommit);
        //}





    }
}
