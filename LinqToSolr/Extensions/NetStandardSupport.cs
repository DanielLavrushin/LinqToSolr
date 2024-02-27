using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
#if !NETSTANDARD1_0
using System.Net.Http;
#endif
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
namespace LinqToSolr.Extensions
{
    internal static class NetStandardSupport
    {
        public static string ParseQueryString(this LinqToSolrRequest request)
        {
#if NET45 || NET46 || NETSTANDARD1_0 || NETSTANDARD1_3 || NETSTANDARD1_6
            var parameters = new List<string>();
            foreach (var key in request.QueryParameters.AllKeys)
            {
                foreach (var value in request.QueryParameters.GetValues(key))
                {
                    parameters.Add(key + "=" + WebUtility.UrlEncode(value));
                }
            }
            return string.Join("&", parameters);
#else
      return request.QueryParameters.ToString();
#endif

        }

#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6
        public static readonly string SchemeDelimiter = "://";

        public static string GetLeftPart(this Uri uri, UriPartial part)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            switch (part)
            {
                case UriPartial.Scheme:
                    return uri.Scheme + SchemeDelimiter;

                case UriPartial.Authority:
                    return uri.Scheme + SchemeDelimiter + uri.Authority;

                case UriPartial.Path:
                    return uri.Scheme + SchemeDelimiter + uri.Authority + uri.AbsolutePath;

                case UriPartial.Query:
                    return uri.Scheme + SchemeDelimiter + uri.Authority + uri.AbsolutePath + "?" + uri.Query;

                default:
                    throw new ArgumentOutOfRangeException(nameof(part), "Invalid UriPartial value.");
            }
        }

#endif

#if NETSTANDARD1_6
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition
                   ? type.GetTypeInfo().GenericTypeParameters
                   : type.GetTypeInfo().GenericTypeArguments;
        }

        public static bool IsAssignableFrom(this Type type, Type c)
        {
            return type.GetTypeInfo().IsAssignableFrom(c.GetTypeInfo());
        }

        public static ConstructorInfo GetConstructor(this Type type, Type[] types)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.DeclaredConstructors.FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                if (parameters.Length != types.Length) return false;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != types[i]) return false;
                }
                return true;
            });
        }
        public static FieldInfo[] GetFields(this Type type, BindingFlags bindingAttr)
        {
            return type.GetRuntimeFields().ToArray();
        }
        public static PropertyInfo[] GetProperties(this Type type, BindingFlags bindingAttr)
        {
            return type.GetRuntimeProperties().ToArray();
        }
        public static MethodInfo[] GetMethods(this Type type, BindingFlags bindingAttr)
        {
            return type.GetRuntimeMethods().ToArray();
        }
        public static PropertyInfo GetProperty(this Type type, string propertyName)
        {
            return type.GetRuntimeProperty(propertyName);
        }
#endif

        public static bool IsPrimitive(this Type type)
        {
#if NETSTANDARD2_0_OR_GREATER
            return type.IsPrimitive;
#else
            return type == typeof(bool) ||
                   type == typeof(byte) ||
                   type == typeof(sbyte) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(long) ||
                   type == typeof(ulong) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(char);
#endif
        }

        public static bool IsEnum(this Type type)
        {
#if NETSTANDARD2_0_OR_GREATER
            return type.IsEnum;
#else
            return type.GetTypeInfo().IsEnum;
#endif
        }

        public static bool IsGenericType(this Type type)
        {
#if NETSTANDARD2_0_OR_GREATER
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static bool IsInterface(this Type type)
        {
#if NETSTANDARD2_0_OR_GREATER
            return type.IsInterface;
#else
            return type.GetTypeInfo().IsInterface;
#endif
        }

        public static byte[] GetAsciiBytes(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            byte[] bytes = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c <= 0x7f) bytes[i] = (byte)c;
                else throw new Exception("Non-ASCII character encountered");
            }
            return bytes;
        }
    }


#if NET45 || NET46 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6
    internal static class HttpUtility
    {
        public static NameValueCollection ParseQueryString(string queryString)
        {
            var queryParameters = new NameValueCollection();
            // Remove the '?' from the beginning of the string if it exists
            if (queryString.StartsWith("?"))
            {
                queryString = queryString.Substring(1);
            }

            var pairs = queryString.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = Uri.UnescapeDataString(keyValue[0]);
                    var value = Uri.UnescapeDataString(keyValue[1]);
                    queryParameters.Add(key, value);
                }
            }
            return queryParameters;
        }
    }
#endif


#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6
    internal enum UriPartial
    {
        Scheme,
        Authority,
        Path,
        Query
    }
#endif

}