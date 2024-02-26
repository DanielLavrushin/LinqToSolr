using System;
using System.Collections.Generic;
using System.Net;
#if !NETSTANDARD1_0
using System.Net.Http;
#endif
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace LinqToSolr.Extensions
{
    internal static class NetStandardSupport
    {

#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6
        public static string GetLeftPart(this Uri uri, UriPartial part)
        {
            throw new NotImplementedException();
        }

        public static bool IsAssignableFrom(this Type type, Type? c)
        {
            throw new NotImplementedException();
        }

        public static ConstructorInfo? GetConstructor(this Type type, Type[] types)
        {
            throw new NotImplementedException();
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            throw new NotImplementedException();
        }

        public static MethodInfo[] GetMethods(this Type type, BindingFlags flags)
        {
            throw new NotImplementedException();
        }

        public static PropertyInfo GetProperty(this Type type, string name)
        {
            throw new NotImplementedException();
        }

        public static FieldInfo[] GetFields(this Type type, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public static PropertyInfo[] GetProperties(this Type type, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
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

#if NETSTANDARD1_0
    internal class HttpClient
    {
        public HttpRequestHeaders DefaultRequestHeaders { get; }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            throw new NotImplementedException();
        }
    }
    internal class StringContent : HttpContent
    {
        public StringContent(string content, Encoding encoding, string contentType)
        {

        }
    }
    internal class HttpRequestHeaders
    {
        public AuthenticationHeaderValue Authorization { get; set; }
    }

    internal class AuthenticationHeaderValue
    {
        public AuthenticationHeaderValue(string scheme, string parameter)
        {
        }
    }

    internal class HttpRequestMessage : IDisposable
    {
        public HttpContent Content { get; set; }
        internal HttpRequestMessage(HttpMethod method, Uri requestUri)
        {
        }

        public void Dispose()
        {
        }
    }

    internal class HttpResponseMessage : IDisposable
    {
        public HttpStatusCode StatusCode { get; set; }

        public HttpContent Content { get; set; }

        public void Dispose()
        {
        }
    }
#endif

#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6
    internal class NameValueCollection : Dictionary<string, string>
    {
    }
#endif

#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3 || NETSTANDARD1_6
    internal class HttpUtility
    {
        public static NameValueCollection ParseQueryString(string query)
        {
            throw new NotImplementedException();
        }
    }
#endif

#if NETSTANDARD1_0 || NETSTANDARD1_1
    internal class HttpContent : IDisposable
    {
        public async Task<string> ReadAsStringAsync()
        {
            throw new NotImplementedException();

        }
        public void Dispose()
        {
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

#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3
    [Flags]
    internal enum BindingFlags
    {
        Default = 0,
        IgnoreCase = 1,
        DeclaredOnly = 2,
        Instance = 4,
        Static = 8,
        Public = 0x10,
        NonPublic = 0x20,
        FlattenHierarchy = 0x40,
        InvokeMethod = 0x100,
        CreateInstance = 0x200,
        GetField = 0x400,
        SetField = 0x800,
        GetProperty = 0x1000,
        SetProperty = 0x2000,
        PutDispProperty = 0x4000,
        PutRefDispProperty = 0x8000,
        ExactBinding = 0x10000,
        SuppressChangeType = 0x20000,
        OptionalParamBinding = 0x40000,
        IgnoreReturn = 0x1000000,
        DoNotWrapExceptions = 0x2000000
    }
#endif

#if NETSTANDARD1_0
    public class HttpMethod : IEquatable<HttpMethod>
    {
        private readonly string _method;

        private int _hashcode;

        private static readonly HttpMethod s_getMethod = new HttpMethod("GET");

        private static readonly HttpMethod s_putMethod = new HttpMethod("PUT");

        private static readonly HttpMethod s_postMethod = new HttpMethod("POST");

        private static readonly HttpMethod s_deleteMethod = new HttpMethod("DELETE");

        private static readonly HttpMethod s_headMethod = new HttpMethod("HEAD");

        private static readonly HttpMethod s_optionsMethod = new HttpMethod("OPTIONS");

        private static readonly HttpMethod s_traceMethod = new HttpMethod("TRACE");

        private static readonly HttpMethod s_patchMethod = new HttpMethod("PATCH");

        private static readonly HttpMethod s_connectMethod = new HttpMethod("CONNECT");

        private static readonly Dictionary<HttpMethod, HttpMethod> s_knownMethods = new Dictionary<HttpMethod, HttpMethod>(9)
    {
        { s_getMethod, s_getMethod },
        { s_putMethod, s_putMethod },
        { s_postMethod, s_postMethod },
        { s_deleteMethod, s_deleteMethod },
        { s_headMethod, s_headMethod },
        { s_optionsMethod, s_optionsMethod },
        { s_traceMethod, s_traceMethod },
        { s_patchMethod, s_patchMethod },
        { s_connectMethod, s_connectMethod }
    };

        public static HttpMethod Get => s_getMethod;

        public static HttpMethod Put => s_putMethod;

        public static HttpMethod Post => s_postMethod;

        public static HttpMethod Delete => s_deleteMethod;

        public static HttpMethod Head => s_headMethod;

        public static HttpMethod Options => s_optionsMethod;

        public static HttpMethod Trace => s_traceMethod;

        public static HttpMethod Patch => s_patchMethod;

        internal static HttpMethod Connect => s_connectMethod;

        public string Method => _method;

        internal bool MustHaveRequestBody
        {
            get
            {
                if ((object)this != Get && (object)this != Head && (object)this != Connect && (object)this != Options)
                {
                    return (object)this != Delete;
                }
                return false;
            }
        }

        public HttpMethod(string method)
        {
            _method = method;
        }

        public bool Equals(HttpMethod other)
        {
            if ((object)other == null)
            {
                return false;
            }
            if ((object)_method == other._method)
            {
                return true;
            }
            return string.Equals(_method, other._method, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HttpMethod);
        }

        public override int GetHashCode()
        {
            if (_hashcode == 0)
            {
                _hashcode = StringComparer.OrdinalIgnoreCase.GetHashCode(_method);
            }
            return _hashcode;
        }

        public override string ToString()
        {
            return _method;
        }

        public static bool operator ==(HttpMethod left, HttpMethod right)
        {
            if ((object)left != null && (object)right != null)
            {
                return left.Equals(right);
            }
            return (object)left == right;
        }

        public static bool operator !=(HttpMethod left, HttpMethod right)
        {
            return !(left == right);
        }

        internal static HttpMethod Normalize(HttpMethod method)
        {
            if (!s_knownMethods.TryGetValue(method, out var value))
            {
                return method;
            }
            return value;
        }
    }
#endif
}