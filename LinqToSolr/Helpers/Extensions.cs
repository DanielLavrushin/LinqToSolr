using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LinqToSolr.Helpers
{
#if !NETSTANDARD1_3
    public class TypeInfo
    {
        private Type type;
        public bool IsGenericType { get { return type.IsGenericType; } }
        public bool IsEnum { get { return type.IsEnum; } }
        public bool IsPrimitive { get { return type.IsPrimitive; } }
        public TypeInfo(Type type)
        {
            this.type = type;
        }

        public bool IsAssignableFrom(TypeInfo typeInfo)
        {
            return true;
        }
    }
#else
    [Flags]
    public enum BindingFlags
    {
        Default = 0x0,
        IgnoreCase = 0x1,
        DeclaredOnly = 0x2,
        Instance = 0x4,
        Static = 0x8,
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
        IgnoreReturn = 0x1000000
    }
#endif

    public static class Extensions
    {
        public static string GetSolrFieldName(this MemberInfo property)
        {
            var format = string.Empty;
            return GetSolrFieldName(property, out format);
        }

        public static string GetSolrFieldName(this MemberInfo property, out string searchFormat)
        {
            searchFormat = null;
            if (property.IsDefined(typeof(SolrFieldAttribute), true))
            {
                var attr = property.GetCustomAttribute<SolrFieldAttribute>();
                searchFormat = attr.SearchFormat;
                return property.GetCustomAttribute<SolrFieldAttribute>()?.PropertyName ?? property.Name;
            }
            return property.Name;
        }

        public static bool IsSolrField(this MemberInfo property)
        {
            return !property.IsDefined(typeof(SolrFieldIgnoreAttribute), true);
        }

#if !NETSTANDARD
        public static TypeInfo GetTypeInfo(this Type type)
        {
            return new TypeInfo(type);
        }
#endif




#if NETSTANDARD1_3
        public static ConstructorInfo GetConstructor(this Type type, Type[] argTypes)
        {
            return type.GetTypeInfo().DeclaredConstructors.FirstOrDefault();
        }
        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }
        public static FieldInfo[] GetFields(this Type type, BindingFlags bindingAttr)
        {
            return type.GetTypeInfo().DeclaredFields.ToArray();
        }
        public static PropertyInfo[] GetProperties(this Type type, BindingFlags bindingAttr)
        {
            return type.GetTypeInfo().DeclaredProperties.ToArray();
        }
           public static Attribute GetCustomAttribute<T>(this Type type)
        {
            return type.GetTypeInfo().GetCustomAttribute(typeof(T));
        }
#endif

#if NET40 || NET35
        public static PropertyInfo GetRuntimeProperty(this Type type, string name)
        {
            return type.GetProperty(name);
        }
        public static T GetCustomAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(member, typeof(T), true);

        }
        public static Attribute GetCustomAttribute<T>(this Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(T), true);

        }

        public static IEnumerable<PropertyInfo> GetRuntimeProperties(this Type type)
        {
            return type.GetProperties();

        }
#endif
    }
}
