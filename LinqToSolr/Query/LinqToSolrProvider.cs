using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToSolr.Expressions;
using LinqToSolr.Services;
using LinqToSolr.Models;
using System.Collections.Generic;
using LinqToSolr.Interfaces;
using LinqToSolr.Helpers.Json;

namespace LinqToSolr.Query
{
    public interface ILinqToSolrProvider : IQueryProvider
    {
        ILinqToSolrService Service { get; }
        SolrResponse<TResult> ExecuteQuery<TResult>(ILinqToSolrQueriable<TResult> quariable);
        void DeleteAll<TResult>();
        void Delete<TResult>(params object[] id);
        void Delete<TResult>(ILinqToSolrQueriable<TResult> quariable);
        IEnumerable<TResult> AddOrUpdate<TResult>(IEnumerable<TResult> document, bool softCommit = false);
    }
    public class LinqToSolrProvider : ILinqToSolrProvider
    {
        public ILinqToSolrService Service { get; }
        internal bool IsEnumerable;
        internal bool IsGroupping;
        IQueryable query;
        SolrWebClient Client;
        Type returnExecuteType;
        object result;

        public LinqToSolrProvider(ILinqToSolrService service)
        {
            Service = service;
            Client = new SolrWebClient(Service.Configuration.EndPoint.ToString());
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(((MethodCallExpression)expression).Arguments[0].Type);
            query = query ?? (IQueryable)Activator.CreateInstance(typeof(LinqToSolrQueriable<>).MakeGenericType(elementType), new object[] { this, expression });
            return query;
        }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            query = new LinqToSolrQueriable<TElement>(this, expression);
            return CreateQuery(expression) as LinqToSolrQueriable<TElement>;
        }

        public object Execute(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(((MethodCallExpression)expression).Arguments[0].Type);
            var providerQuery = GetType().GetMethod("ExecuteQuery").MakeGenericMethod(new[] { elementType });

            query = returnExecuteType == elementType ? query : (IQueryable)Activator.CreateInstance(typeof(LinqToSolrQueriable<>).MakeGenericType(elementType), new object[] { this, expression });
            providerQuery.Invoke(this, new[] { query });
            return IsEnumerable ? result : ((IEnumerable)result).Cast<object>().FirstOrDefault();
        }


        public TResult Execute<TResult>(Expression expression)
        {
            result = null;
            IsEnumerable = typeof(TResult).Name == "IEnumerable`1";
            returnExecuteType = query.GetType().GetGenericArguments()[0];
            IsGroupping = returnExecuteType.Name == "IGrouping`2";
            return (TResult)Execute(expression);
        }

        private SolrWebRequest PrepareQueryRequest<TResult>(ILinqToSolrQuery query)
        {
            var config = Service.Configuration;
            string path = string.Format("/{0}/select", config.GetIndex(typeof(TResult)));
            var request = new SolrWebRequest(path);

            request.AddParameter("q", "*");
            request.AddParameter("wt", "json");
            request.AddParameter("indent", "false");
            request.AddParameter("rows", (query.Take > 0 ? query.Take : config.Take).ToString());
            request.AddParameter("start", (query.Start > 0 ? query.Start : config.Start).ToString());

            if (query.JoinFields.Any())
            {
                foreach (var joiner in query.JoinFields)
                {
                    var joinCore = config.GetIndex(joiner.PropertyRealType);
                    var joinstr = string.Format("from={0} to={1} fromIndex={2}", joiner.ForeignKey, joiner.FieldKey, joinCore);
                    request.AddParameter("q", "{!join " + joinstr + "}");
                }
            }

            if (query.GroupFields.Any())
            {
                request.AddParameter("group", "true");
                request.AddParameter("group.limit", (query.Take > 0 ? query.Take : config.Take).ToString());
                request.AddParameter("group.offset", (query.Start > 0 ? query.Start : config.Start).ToString());
                foreach (var groupField in query.GroupFields)
                {
                    request.AddParameter("group.field", groupField.Field);
                }
            }

            if (query.Filters.Any())
            {
                foreach (var filter in query.Filters)
                {
                    if (filter.Values == null)
                    {
                        request.AddParameter("fq", filter.Field);
                    }
                    else
                    {
                        request.AddParameter("fq", string.Format("{0}: ({1})", filter.Field,
                            string.Join(" OR ", filter.Values.Select(x => string.Format("\"{0}\"", x)).ToArray()
                            )));
                    }
                }
            }

            if (query.Sortings.Any())
            {
                request.AddParameter("sort", string.Join(", ", query.Sortings.Reverse().Select(x =>
                        string.Format("{0} {1}", x.Field, x.Order == SolrSortTypes.Desc ? "DESC" : "ASC")).ToArray()));
            }


            if (query.Facets.Any())
            {
                request.AddParameter("facet", "true");
                request.AddParameter("facet.mincount", "1");

                if (config.FacetsLimit > 0)
                {
                    request.AddParameter("facet.limit", config.FacetsLimit.ToString());
                }

                var ignoredFacets = query.FacetsToIgnore.Select(x => x.Field).ToList();
                foreach (var facet in query.Facets)
                {
                    request.AddParameter("facet.field", ignoredFacets.Any(x => x == facet.Field)
                        ? string.Format("{{!ex={0}}}{0}", facet.Field)
                        : facet.Field);
                }
            }

            if (query.Select != null)
            {
                request.AddParameter("fl", query.Select.GetSelectFields());
            }

            return request;
        }
        private SolrWebRequest PrepareAddOrUpdateRequest<TResult>(IEnumerable<TResult> documents, bool softCommit = false)
        {
            var config = Service.Configuration;
            var index = config.GetIndex(typeof(TResult));

            string path = string.Format("/{0}/update", index);
            var request = new SolrWebRequest(path, SolrWebMethod.POST);
            request.AddParameter("wt", "json");
            request.AddParameter("commit", "true");
            if (softCommit)
            {
                request.AddParameter("softCommit", "true");
            }
            if (documents != null && documents.Any())
            {
                request.Body = documents.ToJson();
            }
            return request;
        }
        private SolrWebRequest PrepareDeleteRequest<TResult>(ILinqToSolrQuery query, object[] deleteDocIds = null, bool softCommit = false)
        {
            var config = Service.Configuration;
            var index = config.GetIndex(typeof(TResult));

            string path = string.Format("/{0}/update", index);
            var request = new SolrWebRequest(path, SolrWebMethod.POST);

            request.AddParameter("wt", "json");
            request.AddParameter("commit", "true");
            if (softCommit)
            {
                request.AddParameter("softCommit", "true");
            }

            if (deleteDocIds != null && deleteDocIds.Any())
            {
                request.Body = new { delete = deleteDocIds }.ToJson();
            }
            else if (query != null)
            {
                var deleteByQuery = new List<string>();
                if (query.Filters.Any())
                {
                    foreach (var filter in query.Filters)
                    {
                        if (filter.Values == null)
                        {
                            deleteByQuery.Add(filter.Field);
                        }
                        else
                        {
                            deleteByQuery.Add($"{filter.Field}: ({string.Join(" OR ", filter.Values.Select(x => $"\"{{x}}\"").ToArray())})");
                        }
                    }
                    request.Body = new { delete = new { query = string.Join(" AND ", deleteByQuery.ToArray()) } }.ToJson();
                }
            }
            return request;
        }
        public virtual SolrResponse<TResult> ExecuteQuery<TResult>(ILinqToSolrQueriable<TResult> quariable)
        {
            quariable = quariable ?? new LinqToSolrQueriable<TResult>(this, null);
            var solrQuery = quariable.Translate();
            var request = PrepareQueryRequest<TResult>(solrQuery);
            var response = Client.Execute(request);

            Service.LastResponseUrl = response.ResponseUri;
            var current = response.Content.FromJson<SolrResponse<TResult>>();
            Finalize(current, quariable);

            return current;
        }
        public void DeleteAll<TResult>()
        {
            var quariable = new LinqToSolrQueriable<TResult>(this, null);
            var solrQuery = quariable.Translate();

            solrQuery.Filters.Clear();
            solrQuery.Filters.Add(LinqToSolrFilter.Create("*:*"));
            var request = PrepareDeleteRequest<TResult>(solrQuery);
            var response = Client.Execute(request);
            var current = response.Content.FromJson<SolrResponse<TResult>>();
            Finalize(current, quariable);
        }
        public void Delete<TResult>(ILinqToSolrQueriable<TResult> quariable)
        {
            quariable = quariable ?? new LinqToSolrQueriable<TResult>(this, null);
            var solrQuery = quariable.Translate();

            var request = PrepareDeleteRequest<TResult>(solrQuery);
            var response = Client.Execute(request);
            var current = response.Content.FromJson<SolrResponse<TResult>>();
            Finalize(current, quariable);
        }
        public void Delete<TResult>(params object[] id)
        {
            var request = PrepareDeleteRequest<TResult>(null, id);
            var response = Client.Execute(request);
            var current = response.Content.FromJson<SolrResponse<TResult>>();
            Finalize(current);

        }

        public IEnumerable<TResult> AddOrUpdate<TResult>(IEnumerable<TResult> document, bool softCommit = false)
        {
            var request = PrepareAddOrUpdateRequest(document, softCommit);
            var response = Client.Execute(request);
            var current = response.Content.FromJson<SolrResponse<TResult>>();
            Finalize(current);
            return document;
        }

        private ICollection<TResult> Finalize<TResult>(SolrResponse<TResult> response, ILinqToSolrQueriable<TResult> quariable = null)
        {
            if (response.Error != null)
            {
                throw new Exception(response.Error.Message);
            }


            if (IsGroupping && response.Groups != null)
            {
                var group = quariable.SolrQuery.GroupFields.First();
                var func = ((LambdaExpression)group.Expression).Compile();
                var groupByMethod = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.GroupBy) && x.GetGenericArguments().Count() == 2).First();
                var allDocs = response.Groups.SelectMany(x => x.Value.Groups.Select(s => s.Documents)).SelectMany(x => x.Documents).Distinct().ToList();
                result = groupByMethod.MakeGenericMethod(typeof(TResult), group.Type).Invoke(null, new object[] { allDocs, func });
            }

            if (quariable != null && response.Response?.Documents != null && quariable.SolrQuery.Select != null)
            {
                var func = ((LambdaExpression)quariable.SolrQuery.Select.Expression).Compile();
                var selectMethod = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Select) && x.GetGenericArguments().Count() == 2).First();
                result = selectMethod.MakeGenericMethod(typeof(TResult), returnExecuteType).Invoke(null, new object[] { response.Response.Documents, func });
            }

            if (result == null)
            {
                result = response.Response?.Documents;
            }
            return response.Response?.Documents;
        }
    }
}
