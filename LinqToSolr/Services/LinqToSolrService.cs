using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using LinqToSolr.Converters;
using LinqToSolr.Data;
using LinqToSolr.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace LinqToSolr.Services
{
    public class LinqToSolrService: ILinqToSolrService
    {
        public LinqToSolrRequestConfiguration Configuration { get; set; }
        public ILinqToSolrResponse LastResponse { get; set; }
        public Type ElementType { get; set; }
        public string CurrentFilterUrl { get; set; }
        protected IRestClient Client;
        public LinqToSolrQuery CurrentQuery { get; set; }

        protected LinqToSolrService()
        {
            Configuration = LinqToSolrRequestConfiguration._instance;

            if (Configuration.Take <= 0)
            {
                Configuration.Take = 10;
            }
            if (Configuration == null || string.IsNullOrEmpty(Configuration.EndPoint))
                throw new Exception("Solr Endpoint was not provided. Specify Solr server in SolrRequestConfiguration.");
            Client = new RestClient(Configuration.EndPoint);
        }

        protected LinqToSolrService(LinqToSolrRequestConfiguration configuration)
        {
            Configuration = configuration;
            if (Configuration.Take <= 0)
            {
                Configuration.Take = 10;
            }
            if (string.IsNullOrEmpty(Configuration.EndPoint))
                throw new Exception("Solr Endpoint was not provided. Specify Solr server in SolrRequestConfiguration.");
            Client = new RestClient(Configuration.EndPoint);

        }


        public LinqToSolrQueriable<T> AsQueryable<T>()
        {
            CurrentQuery = new LinqToSolrQuery() { CurrentType = typeof(T) };
            return new LinqToSolrQueriable<T>(this);
        }

        private RestRequest PrepareRequest(string index)
        {


            string path = string.Format("/solr/{0}/select", index);
            var request = new RestRequest(path, Method.GET);


            request.AddQueryParameter("q", "*");
            request.AddQueryParameter("wt", "json");
            request.AddQueryParameter("indent", "true");
            request.AddQueryParameter("rows", Configuration.Take.ToString());
            request.AddQueryParameter("start", Configuration.Start.ToString());


            if (CurrentQuery.IsGroupEnabled)
            {
                request.AddQueryParameter("group", "true");
                request.AddQueryParameter("group.limit", Configuration.Take.ToString());
                request.AddQueryParameter("group.offset", Configuration.Start.ToString());
                foreach (var groupField in CurrentQuery.GroupFields)
                {
                    request.AddQueryParameter("group.field", groupField);
                }

            }


            if (CurrentQuery.Filters.Any())
            {
                foreach (var filter in CurrentQuery.Filters)
                {
                    request.AddQueryParameter("fq", string.Format("{0}: ({1})", filter.Name,
                        string.Join(" OR ", filter.Values.Select(x => string.Format("\"{0}\"", x))
                        )));
                }
            }


            if (!string.IsNullOrEmpty(CurrentQuery.FilterUrl))
            {
                foreach (var fstring in CurrentQuery.FilterUrl.Split(new[] { "&fq=" }, StringSplitOptions.None))
                {
                    if (!string.IsNullOrEmpty(fstring))
                    {
                        request.AddQueryParameter("fq", fstring);
                    }
                }
            }

            if (CurrentQuery.Sortings.Any())
            {
                request.AddQueryParameter("sort", string.Join(", ", CurrentQuery.Sortings.Select(x =>
                        string.Format("{0} {1}", x.Name, x.Order == SolrSortTypes.Desc ? "DESC" : "ASC"))));
            }


            if (CurrentQuery.Facets.Any())
            {
                request.AddQueryParameter("facet", "true");
                request.AddQueryParameter("facet.mincount", "1");

                foreach (var facet in CurrentQuery.Facets)
                {
                    request.AddQueryParameter("facet.field", facet.SolrName);
                }
            }

            if (CurrentQuery.Select != null)
            {
                request.AddQueryParameter("fl", CurrentQuery.Select.GetSelectFields());

            }


            return request;
        }




        public ICollection<T> Query<T>(LinqToSolrQuery query = null)
        {
            return Query(typeof(T), query) as ICollection<T>;

        }

        public object Query(Type elementType, LinqToSolrQuery query = null)
        {
            if (query != null)
            {
                CurrentQuery = query;
            }
            var index = Configuration.GetIndex(elementType);

            if (string.IsNullOrEmpty(index))
            {
                throw new ArgumentNullException(nameof(index),
                    string.Format(
                        "The type '{0}' is not assigned for any Solr Index. Register this type in a service configuration (SolrRequestConfiguration.MapIndexFor)",
                        ElementType.Name));
            }

            if (!string.IsNullOrEmpty(Configuration.SolrLogin) && !string.IsNullOrEmpty(Configuration.SolrPassword))
            {
                Client.Authenticator = new HttpBasicAuthenticator(Configuration.SolrLogin, Configuration.SolrPassword);
            }

            var response = Client.Execute(PrepareRequest(index));

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new EntryPointNotFoundException(
                    "Server returned 404 - Not Found. Check if solr url is correct or index name was correctly provided.");
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "Solr is protected by password. Setup login and passowrd in the configuration class.");
            }

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }


            LastResponse = JsonConvert.DeserializeObject<LinqToSolrResponse>(response.Content);
            LastResponse.LastServiceUri = response.ResponseUri;
            if (LastResponse.Header.Status == 0)
            {
                if (LastResponse.Body != null)
                {

                    LastResponse.FoundDocuments = LastResponse.Body?.Count ?? 0;

                    var listMethod =
                        typeof(List<>).MakeGenericType(CurrentQuery.Select != null && !CurrentQuery.Select.IsSingleField
                            ? CurrentQuery.Select.Type
                            : elementType);

                    var genList = JsonConvert.DeserializeObject(
                        JsonConvert.SerializeObject(LastResponse.Body.Documents), listMethod);



                    if (LastResponse.Facets != null)
                    {

                        foreach (var facet in CurrentQuery.Facets)
                        {
                            var content = JsonConvert.DeserializeObject<dynamic>(response.Content).facet_counts;
                            var groups = content.facet_fields[facet.SolrName] as JArray;

                            LastResponse.Facets.Add(facet.SolrName,
                                groups.Where((x, i) => i % 2 == 0).Select(x => ((JValue)x).Value).ToArray());
                        }
                    }

                    if (CurrentQuery.Select?.IsSingleField == true)
                    {
                        var fieldDelegate = ((LambdaExpression)CurrentQuery.Select.Expression).Compile();
                        var selectMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                            .First(m => m.Name == "Select" && m.GetParameters().Count() == 2);

                        var fieldList = selectMethod.MakeGenericMethod(elementType, CurrentQuery.Select.Type)
                            .Invoke(genList, new[] { genList, fieldDelegate });
                        CurrentQuery = null;
                        return fieldList;
                    }

                    CurrentQuery = null;
                    return genList;
                }


                if (CurrentQuery.IsGroupEnabled)
                {
                    var _keyType = ElementType.GetGenericArguments()[0];
                    var _valueType = ElementType.GetGenericArguments()[1];

                    var solrConverterType = typeof(LinqToSolrGroupingResponseConverter<,>).MakeGenericType(_keyType, _valueType);
                    var converterInstance = Activator.CreateInstance(solrConverterType);

                    var groupResult = JsonConvert.DeserializeObject(response.Content, solrConverterType,
                      converterInstance as JsonConverter);
                    return groupResult;
                }


                CurrentQuery = null;
                return null;
            }
            throw new MissingFieldException(LastResponse.Error.Message);
        }
    }
}
