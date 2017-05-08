using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using System.Text;
using System.Threading;
#if DNXCORE50
using Xunit;
using XAssert = Xunit.Assert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Utilities;
using System.Collections;
#if !(NET20 || NET35 || NET40 || PORTABLE40)
using System.Threading.Tasks;
#endif
using System.Linq;


namespace LinqToSolr.Tests
{
    #if DNXCORE50
    public class TestFixtureAttribute: Attribute
    {
      
    }
    public class OneTimeSetUpAttribute: Attribute
    {

    }
    public class OneTimeTearDownAttribute: Attribute
    {

    }

    public class XUnitAssert
    {
        public static void IsInstanceOf(Type expectedType, object o)
        {
            XAssert.IsType(expectedType, o);
        }

        public static void AreEqual(double expected, double actual, double r)
        {
            XAssert.Equal(expected, actual, 5); // hack
        }

        public static void AreEqual(object expected, object actual, string message = null)
        {
            XAssert.Equal(expected, actual);
        }

        public static void AreEqual<T>(T expected, T actual, string message = null)
        {
            XAssert.Equal(expected, actual);
        }

        public static void AreNotEqual(object expected, object actual, string message = null)
        {
            XAssert.NotEqual(expected, actual);
        }

        public static void AreNotEqual<T>(T expected, T actual, string message = null)
        {
            XAssert.NotEqual(expected, actual);
        }

        public static void Fail(string message = null, params object[] args)
        {
            if (message != null)
            {
            //    message = message.FormatWith(CultureInfo.InvariantCulture, args);
            }

            XAssert.True(false, message);
        }

        public static void Pass()
        {
        }

        public static void IsTrue(bool condition, string message = null)
        {
            XAssert.True(condition);
        }

        public static void IsFalse(bool condition)
        {
            XAssert.False(condition);
        }

        public static void IsNull(object o)
        {
            XAssert.Null(o);
        }

        public static void IsNotNull(object o)
        {
            XAssert.NotNull(o);
        }

        public static void AreNotSame(object expected, object actual)
        {
            XAssert.NotSame(expected, actual);
        }

        public static void AreSame(object expected, object actual)
        {
            XAssert.Same(expected, actual);
        }
    }

    public class CollectionAssert
    {
        public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            XAssert.Equal(expected, actual);
        }

        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            XAssert.Equal(expected, actual);
        }
    }

#endif
}
