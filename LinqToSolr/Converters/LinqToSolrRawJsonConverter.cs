using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LinqToSolr.Data;
using LinqToSolr.Services;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;
using System.Security;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace LinqToSolr.Converters
{
    public class LinqToSolrRawJsonConverter : JsonConverter
    {
        private PropertyInfo[] _transformerProperties;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            serializer.Error += Serializer_Error;
            var obj = Activator.CreateInstance(objectType);
           serializer.Populate(reader, obj);
            var json = JObject.Load(reader);
            if (reader.TokenType == JsonToken.EndObject)
            {
                foreach (var prop in _transformerProperties)
                {
                    if (json[prop.Name] != null)
                    {
                        prop.SetValue(obj, JsonConvert.DeserializeObject(json[prop.Name].Value<string>(), prop.PropertyType), null);
                    }
                }
            }

            return obj;
        }
        private void Serializer_Error(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }
        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanRead
        {
            get { return true; }
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {

#if NETCORE || PORTABLE
                    var members =
                   objectType.GetTypeInfo().DeclaredProperties.Where(x=> x.GetCustomAttributes(typeof(LinqToSolrJsonPropertyAttribute), true).Any()).ToList();
#else
            var members =
                objectType.GetProperties().Where(x=> x.GetCustomAttributes(typeof(LinqToSolrJsonPropertyAttribute), true).Any()).ToList();
#endif

            if (members.Any())
            {
                _transformerProperties = members.ToArray();
                return true;
            }
            return false;
        }
    }
}
