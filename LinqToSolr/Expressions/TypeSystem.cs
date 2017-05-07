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


namespace LinqToSolr.Expressions
{
    internal static class TypeSystem
    {

        internal static Type GetElementType(Type seqType)
        {

            Type ienum = FindIEnumerable(seqType);

            if (ienum == null) return seqType;
#if PORTABLE || NETCORE
            return ienum.GetTypeInfo().IsGenericTypeDefinition
                ? ienum.GetTypeInfo().GenericTypeParameters[0]
                : ienum.GetTypeInfo().GenericTypeArguments[0];
#else
            return ienum.GetGenericArguments()[0];
#endif
        }

        private static Type FindIEnumerable(Type seqType)
        {

            if (seqType == null || seqType == typeof(string))

                return null;

            if (seqType.IsArray)

                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

#if PORTABLE || NETCORE
            if (seqType.GetTypeInfo().IsGenericType)
            {
#else
            if (seqType.IsGenericType)
            {
#endif



#if PORTABLE || NETCORE
                var args =seqType.GetTypeInfo().IsGenericTypeDefinition
                    ? seqType.GetTypeInfo().GenericTypeParameters
                    : seqType.GetTypeInfo().GenericTypeArguments;
                foreach (Type arg in args)

#else
                foreach (Type arg in seqType.GetGenericArguments())

#endif
                {

                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
#if PORTABLE || NETCORE
                    if (ienum.GetTypeInfo().IsAssignableFrom(seqType.GetTypeInfo()))
#else
                    if (ienum.IsAssignableFrom(seqType))
#endif
                    {
                        return ienum;

                    }

                }

            }
#if PORTABLE || NETCORE
            Type[] ifaces = seqType.GetTypeInfo().ImplementedInterfaces.ToArray();
#else
            Type[] ifaces = seqType.GetInterfaces();
#endif

            if (ifaces != null && ifaces.Length > 0)
            {

                foreach (Type iface in ifaces)
                {

                    Type ienum = FindIEnumerable(iface);

                    if (ienum != null) return ienum;

                }

            }

#if PORTABLE || NETCORE
            if (seqType.GetTypeInfo().BaseType != null && seqType.GetTypeInfo().BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.GetTypeInfo().BaseType);
#else
            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
#endif

            }

            return null;

        }


    }
}
