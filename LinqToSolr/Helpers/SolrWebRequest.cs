using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;


namespace LinqToSolr
{
    public enum SolrWebMethod
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public class SolrWebParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public SolrWebParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    public class SolrWebRequest
    {

        public SolrWebMethod Method { get; set; }
        public string ActionUrl { get; set; }
        public string Body { get; set; }
        public ICollection<SolrWebParameter> Parameters { get; set; }

        public SolrWebRequest(string action, SolrWebMethod method = SolrWebMethod.GET)
        {
            ActionUrl = action;
            Method = method;
            Parameters = new List<SolrWebParameter>();
        }

        public void AddParameter(string name, object value)
        {
            Parameters.Add(new SolrWebParameter(name, value));
        }

    }

    public class SolrWebResponse
    {
        public string Content { get; set; }
        public Uri ResponseUri { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public Exception ErrorException { get; set; }
    }

    public class SolrWebClient
    {
        private string _endpoint;
        public SolrWebClient(string endPoint)
        {
            //    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | (SecurityProtocolType)3072;


            _endpoint = endPoint;
        }

        public SolrWebResponse Execute(SolrWebRequest request)
        {

            var paramStr = string.Join("&", request.Parameters.Select(x => string.Format("{0}={1}", x.Name, x.Value)).ToArray());
            _endpoint = _endpoint.EndsWith("/") ? _endpoint.TrimEnd('/') : _endpoint;
            request.ActionUrl = request.ActionUrl.StartsWith("/") ? request.ActionUrl.TrimStart('/') : request.ActionUrl;
            paramStr = string.Format("{0}/{1}?{2}", _endpoint, request.ActionUrl, paramStr.Substring(0, paramStr.Length));
            var response = new SolrWebResponse();
            response.ResponseUri = new Uri(paramStr);


#if NETSTANDARD
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            if (request.Method == SolrWebMethod.GET)
            {
                var webResponse = client.GetAsync(paramStr).Result;
                response.Content = webResponse.Content.ReadAsStringAsync().Result;
                response.StatusCode = webResponse.StatusCode;
            }
            else
            {
                var content = new System.Net.Http.StringContent(request.Body, Encoding.UTF8, "application/json");
                var webResponse = client.PostAsync(paramStr, content).Result;
                //var webResponse = client.GetAsync(paramStr).Result;
                response.Content = webResponse.Content.ReadAsStringAsync().Result;
                response.StatusCode = webResponse.StatusCode;
            }
#else


            var webRequest = (HttpWebRequest)WebRequest.Create(paramStr);
            if (request.Method == SolrWebMethod.GET)
            {
                try
                {
                    using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                    {
                        var encoding = Encoding.GetEncoding(webResponse.CharacterSet);

                        using (var responseStream = webResponse.GetResponseStream())
                        {
                            using (var reader = new System.IO.StreamReader(responseStream, encoding))
                            {
                                response.Content = reader.ReadToEnd();
                                response.StatusCode = webResponse.StatusCode;
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    using (WebResponse res = e.Response)
                    {
                        var webResponse = (HttpWebResponse)res;
                        Console.WriteLine("Error code: {0}", webResponse.StatusCode);
                        using (Stream d = res.GetResponseStream())
                        using (var reader = new StreamReader(d))
                        {
                            string text = reader.ReadToEnd();
                            response.Content = text;
                            response.StatusCode = webResponse.StatusCode;
                            Console.WriteLine(response.Content);
                        }
                    }
                }
            }
            else
            {
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.ContentLength = request.Body.Length;
                var data = Encoding.UTF8.GetBytes(request.Body);
                using (var stream = webRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                try
                {
                    var webResponse = (HttpWebResponse)webRequest.GetResponse();
                    response.Content = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                    response.StatusCode = webResponse.StatusCode;
                }
                catch (WebException e)
                {
                    using (WebResponse res = e.Response)
                    {
                        var webResponse = (HttpWebResponse)res;
                        Console.WriteLine("Error code: {0}", webResponse.StatusCode);
                        using (Stream d = res.GetResponseStream())
                        using (var reader = new StreamReader(d))
                        {
                            string text = reader.ReadToEnd();
                            response.Content = text;
                            response.StatusCode = webResponse.StatusCode;
                            Console.WriteLine(response.Content);
                        }
                    }
                }
            }
#endif
            return response;
        }
    }
}
