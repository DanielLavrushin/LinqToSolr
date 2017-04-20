using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LinqToSolr.Converters
{
    public class LinqToSolrGroupingResponseConverter<TKey, TValue>: JsonConverter
    {
        private Type _documentType;
        private IEnumerable<IGrouping<TKey, TValue>> _groups;

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var rps = new Dictionary<TKey, IEnumerable<TValue>>();
            if (reader.TokenType == JsonToken.StartArray)
            {

            }
            if (reader.TokenType == JsonToken.StartObject)
            {
                if (objectType == typeof(IGrouping<,>))
                {
                }

                var jObject = JObject.Load(reader);
                var properties = jObject.Properties().ToList();
                if (jObject["grouped"] != null)
                {
                    foreach (var g in jObject["grouped"])
                    {
                        var groups = ((JProperty)g).Value["groups"];
                        foreach (var group in groups)
                        {

                            var result = group["doclist"]["docs"].Select(x => x.ToObject<TValue>()).ToList();

                            rps.Add((TKey)Convert.ChangeType(group["groupValue"], typeof(TKey)), result);
                        }



                    }
                }
            }
            else if (reader.TokenType == JsonToken.EndObject)
            {
            }

            return rps.SelectMany(x => x.Value.Select(k => new { x.Key, Value = k }))
                .ToLookup(x => x.Key, x => x.Value);
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }
}
