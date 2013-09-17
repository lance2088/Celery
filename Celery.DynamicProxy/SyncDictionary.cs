using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;

namespace Celery.DynamicProxy
{
    [Serializable]
    public class SyncDictionary<TKey, TValue> : 
        IDictionary<TKey, TValue>, 
        ICollection<KeyValuePair<TKey, TValue>>, 
        IEnumerable<KeyValuePair<TKey, TValue>>, 
        IDictionary, 
        ICollection, 
        IEnumerable, 
        ISerializable, 
        IDeserializationCallback
    {
        private Dictionary<TKey, TValue> _dictionary;

        #region Constructors

        public SyncDictionary() : this(0, null)
        {
        }

        public SyncDictionary(int capacity) : this(capacity, null)
		{
		}

        public SyncDictionary(IEqualityComparer<TKey> comparer) 
            : this(0, comparer)
		{
		}

        public SyncDictionary(int capacity, IEqualityComparer<TKey> comparer)
		{
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }
        #endregion

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            lock (this.SyncRoot)
            {
                if (!this._dictionary.ContainsKey(key))
                {
                    this._dictionary.Add(key, value);
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this._dictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get 
            {
                ICollection<TKey> keys;
                lock (this.SyncRoot)
                {
                    keys = this._dictionary.Keys;
                }
                return keys;
            }
        }

        public bool Remove(TKey key)
        {
            bool removed = false;
            lock (this.SyncRoot)
            {
                removed = this._dictionary.Remove(key);
            }
            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (this.SyncRoot)
            {
                return this._dictionary.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> values;
                lock (this.SyncRoot)
                {
                    values = this._dictionary.Values;
                }
                return values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this._dictionary[key];
            }
            set
            {
                lock (this.SyncRoot)
                {
                    this._dictionary[key] = value;
                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (this.SyncRoot)
            {
                ((IDictionary<TKey, TValue>)this._dictionary).Add(item);
            }
        }

        public void Clear()
        {
            lock (this.SyncRoot)
            {
                this._dictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)this._dictionary)
                .Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (this.SyncRoot)
            {
                ((IDictionary<TKey, TValue>)this._dictionary)
                    .CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get 
            {
                return this._dictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get 
            {
                return 
                    ((IDictionary<TKey, TValue>)this._dictionary).IsReadOnly;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool removed = false;
            lock (this.SyncRoot)
            {
                removed = 
                    ((IDictionary<TKey, TValue>)this._dictionary).Remove(item);
            }
            return removed;
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this._dictionary).GetEnumerator();
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            lock (this.SyncRoot)
            {
                ((ICollection)this._dictionary).CopyTo(array, index);
            }
        }

        public bool IsSynchronized
        {
            get 
            {
                return true;
            }
        }

        public object SyncRoot
        {
            get
            {
                return ((ICollection)this._dictionary).SyncRoot;
            }
        }

        #endregion

        #region ISerializable Members

        public void GetObjectData(
            SerializationInfo info, 
            StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(
                    "Argument \"info\" of type \"" + 
                    typeof(SerializationInfo).FullName + 
                    "\" cannot be null.");
            }
            lock (this.SyncRoot)
            {
                info.AddValue("ParentDictionary", 
                    this._dictionary, 
                    typeof(Dictionary<TKey, TValue>));
            }
        }

        #endregion

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
        }

        #endregion

        #region IDictionary Members

        public void Add(object key, object value)
        {
            lock (this.SyncRoot)
            {
                ((IDictionary)this._dictionary).Add(key, value);
            }
        }

        public bool Contains(object key)
        {
            return ((IDictionary)this._dictionary).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)this._dictionary).GetEnumerator();
        }

        public bool IsFixedSize
        {
            get { return ((IDictionary)this._dictionary).IsFixedSize; }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                ICollection keys;
                lock (this.SyncRoot)
                {
                    keys = ((IDictionary)this._dictionary).Keys;
                }
                return keys;
            }
        }

        public void Remove(object key)
        {
            lock (this.SyncRoot)
            {
                ((IDictionary)this._dictionary).Remove(key);
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                ICollection values;
                lock (this.SyncRoot)
                {
                    values = ((IDictionary)this._dictionary).Values;
                }
                return values;
            }
        }

        public object this[object key]
        {
            get
            {
                return ((IDictionary)this._dictionary)[key];
            }
            set
            {
                lock (this.SyncRoot)
                {
                    ((IDictionary)this._dictionary)[key] = value;
                }
            }
        }

        #endregion
    }
}
