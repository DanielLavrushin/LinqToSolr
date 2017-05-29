using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinqToSolr.Data
{
    public class LinqToSolrJoiner
    {
        public string Field { get; set; }
        public string ForeignKey { get; set; }
        public string FieldKey { get; set; }

        public Type ObjectType { get; set; }
        public PropertyInfo FieldProperty { get; set; }
        public Type PropertyRealType { get; set; }

        public LinqToSolrJoiner(string field, Type objectType)
        {
            Field = field;
            ObjectType = objectType;
#if PORTABLE || NETSTANDARD1_6 || NETSTANDARD2_0

            FieldProperty = objectType.GetRuntimeProperty(field);
#else
            FieldProperty = objectType.GetProperty(field);
#endif
            PropertyRealType = GetRealPropertyType();

            ForeignKey = GetForeignKey();
            FieldKey = GetFieldKey();

        }


        public Type GetRealPropertyType()
        {

#if PORTABLE || NETCORE
            var isGenericArray =  FieldProperty.PropertyType.IsArray && FieldProperty.PropertyType.GetTypeInfo().IsGenericTypeDefinition;
            return isGenericArray ? FieldProperty.PropertyType.GetTypeInfo().GenericTypeParameters[0] : FieldProperty.PropertyType;
#else
            var isGenericArray =  FieldProperty.PropertyType.IsArray && FieldProperty.PropertyType.IsGenericType;
            return isGenericArray ? FieldProperty.PropertyType.GetGenericArguments()[0] : FieldProperty.PropertyType;
#endif






        }
        public string GetForeignKey()
        {
            var foreignKeyAttr = FieldProperty.GetCustomAttributes(typeof(LinqToSolrForeignKeyAttribute), false).ToArray()[0] as LinqToSolrForeignKeyAttribute;
            return foreignKeyAttr.ForeignKey;
        }

        public string GetFieldKey()
        {
#if PORTABLE || NETSTANDARD1_6 || NETSTANDARD2_0
            var props = PropertyRealType.GetRuntimeProperties().ToList();
#else
            var props = PropertyRealType.GetProperties().ToList();
#endif
            foreach (var p in props)
            {
                var keyAttr = p.GetCustomAttributes(typeof(LinqToSolrKeyAttribute), false).ToArray();
                if (keyAttr.Any())
                {
                    return ((LinqToSolrKeyAttribute)keyAttr[0]).Key;
                }
            }

            return null;
        }
    }
}
