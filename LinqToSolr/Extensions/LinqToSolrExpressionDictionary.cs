using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

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

    internal class LinqToSolrExpressionDictionary<T> : IDictionary<Expression<Func<T, object>>, object[]>
    {
        private readonly Dictionary<Expression<Func<T, object>>, object[]> _innerDictionary = new Dictionary<Expression<Func<T, object>>, object[]>(new ExpressionKeyComparer<T>());

        public object[] this[Expression<Func<T, object>> key] { get => _innerDictionary[key]; set => _innerDictionary[key] = value; }
        public IDictionary<string, IDictionary<object, int>> Raw { get; set; }
        object[] IDictionary<Expression<Func<T, object>>, object[]>.this[Expression<Func<T, object>> key]
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

        public ICollection<object[]> Values => _innerDictionary.Values;

        public int Count => _innerDictionary.Count;

        public bool IsReadOnly => false;

        ICollection<object[]> IDictionary<Expression<Func<T, object>>, object[]>.Values { get; }

        public void Add(Expression<Func<T, object>> key, object[] value)
        {
            _innerDictionary.Add(key, value);
        }

        public void Add(KeyValuePair<Expression<Func<T, object>>, object[]> item)
        {
            _innerDictionary.Add(item.Key, item.Value);
        }

        public void Add(Expression<Func<T, object>> key, object[][] value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<Expression<Func<T, object>>, object[][]> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }


        public bool Contains(KeyValuePair<Expression<Func<T, object>>, object[]> item)
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

        public void CopyTo(KeyValuePair<Expression<Func<T, object>>, object[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<Expression<Func<T, object>>, object[]>> GetEnumerator()
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

        public bool Remove(KeyValuePair<Expression<Func<T, object>>, object[]> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(Expression<Func<T, object>> key, out object[] value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        IEnumerator<KeyValuePair<Expression<Func<T, object>>, object[]>> IEnumerable<KeyValuePair<Expression<Func<T, object>>, object[]>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }


    }
}
