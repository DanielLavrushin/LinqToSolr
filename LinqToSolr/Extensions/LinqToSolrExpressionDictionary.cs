using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LinqToSolr.Extensions;
namespace LinqToSolr.Extensions
{
    public class ExpressionKeyComparer<T> : IEqualityComparer<Expression<Func<T, object>>>
    {
        public bool Equals(Expression<Func<T, object>> x, Expression<Func<T, object>> y)
        {
            return x.ToString() == y.ToString();
        }

        public int GetHashCode(Expression<Func<T, object>> obj)
        {
            return obj.ToString().GetHashCode();
        }
    }

    public class LinqToSolrFacetDictionary<T> : IDictionary<Expression<Func<T, object>>, IDictionary<object, int>>
    {
        private readonly Dictionary<Expression<Func<T, object>>, IDictionary<object, int>> _innerDictionary = new Dictionary<Expression<Func<T, object>>, IDictionary<object, int>>(new ExpressionKeyComparer<T>());
        public IDictionary<string, IDictionary<object, int>> RawFacetFields { get; set; }
        public IDictionary<object, int> this[Expression<Func<T, object>> key] { get => _innerDictionary[key]; set => _innerDictionary[key] = value; }
        IDictionary<object, int> IDictionary<Expression<Func<T, object>>, IDictionary<object, int>>.this[Expression<Func<T, object>> key]
        {
            get
            {
                return _innerDictionary[key];
            }
            set
            {
                _innerDictionary[key] = value;
            }
        }

        public ICollection<Expression<Func<T, object>>> Keys => _innerDictionary.Keys;

        public ICollection<IDictionary<object, int>> Values => _innerDictionary.Values;

        public int Count => _innerDictionary.Count;

        public bool IsReadOnly => false;

        ICollection<IDictionary<object, int>> IDictionary<Expression<Func<T, object>>, IDictionary<object, int>>.Values { get; }

        public void Add(Expression<Func<T, object>> key, IDictionary<object, int> value)
        {
            _innerDictionary.Add(key, value);
        }

        public void Add(KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>> item)
        {
            _innerDictionary.Add(item.Key, item.Value);
        }

        public void Add(Expression<Func<T, object>> key, IDictionary<object, int>[] value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>[]> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }


        public bool Contains(KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(Expression<Func<T, object>> key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<Expression<Func<T, object>>, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<Expression<Func<T, object>>, object>>)_innerDictionary).CopyTo(array, arrayIndex);
        }

        public void CopyTo(KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        public bool Remove(Expression<Func<T, object>> key)
        {
            return _innerDictionary.Remove(key);
        }

        public bool Remove(KeyValuePair<Expression<Func<T, object>>, object> item)
        {
            return _innerDictionary.Remove(item.Key);
        }

        public bool Remove(KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Expression<Func<T, object>> key, out IDictionary<object, int> value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }
        public IDictionary<TOut, int> GetFacet<TKey, TOut>(Expression<Func<T, TKey>> key)
        {
            Expression correctExpr = null;
            if (key.Body is MemberExpression memberExpr)
            {
                if (!typeof(IEnumerable).IsAssignableFrom(memberExpr.Type))
                {
                    correctExpr = Expression.Lambda<Func<T, object>>(Expression.Convert(memberExpr, typeof(object)), key.Parameters);
                }
            }

            var keyAsString = (correctExpr ?? key).ToString();
            var match = _innerDictionary.FirstOrDefault(k => k.Key.ToString() == keyAsString);
            if (!match.Equals(default(KeyValuePair<Expression<Func<T, TOut>>, IDictionary<TOut, int>>)))
            {
                var result = new Dictionary<TOut, int>();
                foreach (var pair in match.Value)
                {
                    TOut keyConverted = (TOut)Convert.ChangeType(pair.Key, typeof(TOut));
                    result[keyConverted] = pair.Value;
                }
                return result;
            }
            throw new KeyNotFoundException("The key was not found in the dictionary.");
        }
        public IDictionary<TKey, int> GetFacet<TKey>(Expression<Func<T, TKey>> key)
        {
            Expression correctExpr = null;
            if (key.Body is MemberExpression memberExpr)
            {
                correctExpr = Expression.Lambda<Func<T, object>>(Expression.Convert(memberExpr, typeof(object)), key.Parameters);
            }

            var keyAsString = (correctExpr ?? key).ToString();
            var match = _innerDictionary.FirstOrDefault(k => k.Key.ToString() == keyAsString);
            if (!match.Equals(default(KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>>)))
            {
                return match.Value as IDictionary<TKey, int>;
            }
            throw new KeyNotFoundException("The key was not found in the dictionary.");
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        IEnumerator<KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>>> IEnumerable<KeyValuePair<Expression<Func<T, object>>, IDictionary<object, int>>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }


    }
}
