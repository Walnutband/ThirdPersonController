// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace ARPGDemo.Collections
// {
//     [Serializable]
//     public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
//     {
//         // [SerializeField] private List<TKey> m_Keys = new List<TKey>();
//         [SerializeField] private HashSet<TKey> m_Keys = new HashSet<TKey>();
//         [SerializeField] private List<TValue> m_Values = new List<TValue>();

//         public void Add(TKey key, TValue value)
//         {
            
//         }

//         public bool ContainsKey(TKey key)
//         {
//             throw new NotImplementedException();
//         }

//         public bool Remove(TKey key)
//         {
//             throw new NotImplementedException();
//         }

//         public bool TryGetValue(TKey key, out TValue value)
//         {
//             throw new NotImplementedException();
//         }

//         public TValue this[TKey key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

//         public ICollection<TKey> Keys => throw new NotImplementedException();

//         public ICollection<TValue> Values => throw new NotImplementedException();

//         public void Add(KeyValuePair<TKey, TValue> item)
//         {
//             throw new NotImplementedException();
//         }

//         public void Clear()
//         {
//             throw new NotImplementedException();
//         }

//         public bool Contains(KeyValuePair<TKey, TValue> item)
//         {
//             throw new NotImplementedException();
//         }

//         public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
//         {
//             throw new NotImplementedException();
//         }

//         public bool Remove(KeyValuePair<TKey, TValue> item)
//         {
//             throw new NotImplementedException();
//         }

//         public int Count => throw new NotImplementedException();

//         public bool IsReadOnly => throw new NotImplementedException();

//         public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
//         {
//             throw new NotImplementedException();
//         }

//         IEnumerator IEnumerable.GetEnumerator()
//         {
//             return GetEnumerator();
//         }

//         public void OnBeforeSerialize()
//         {
//             throw new NotImplementedException();
//         }

//         public void OnAfterDeserialize()
//         {
//             throw new NotImplementedException();
//         }
//     }
// }


// // [Serializable]
// // public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
// // {
// //     // 用于 Unity 序列化
// //     [Serializable]
// //     private struct Entry
// //     {
// //         public TKey Key;
// //         public TValue Value;
// //         public Entry(TKey k, TValue v) { Key = k; Value = v; }
// //     }

// //     [SerializeField]
// //     private List<Entry> m_Entries = new List<Entry>();

// //     // 运行时快速索引
// //     private Dictionary<TKey, int> _indexMap;

// //     public SerializableDictionary()
// //     {
// //         RebuildIndex();
// //     }

// //     private void RebuildIndex()
// //     {
// //         _indexMap = new Dictionary<TKey, int>(m_Entries.Count);
// //         for (int i = 0; i < m_Entries.Count; i++)
// //         {
// //             var key = m_Entries[i].Key;
// //             if (_indexMap.ContainsKey(key))
// //                 throw new ArgumentException($"重复的字典键：{key}");
// //             _indexMap[key] = i;
// //         }
// //     }

// //     // 在序列化前将运行时字典写回列表
// //     public void OnBeforeSerialize()
// //     {
// //         // 清单、索引一致时不做额外操作  
// //         // 如果你需要特殊排序或过滤，可在这里调整 _entries
// //     }

// //     // 反序列化后重建索引
// //     public void OnAfterDeserialize()
// //     {
// //         RebuildIndex();
// //     }

// //     #region IDictionary<TKey, TValue> 实现

// //     public TValue this[TKey key]
// //     {
// //         get
// //         {
// //             if (!_indexMap.TryGetValue(key, out var idx))
// //                 throw new KeyNotFoundException(key.ToString());
// //             return m_Entries[idx].Value;
// //         }
// //         set
// //         {
// //             if (_indexMap.TryGetValue(key, out var idx))
// //             {
// //                 m_Entries[idx] = new Entry(key, value);
// //             }
// //             else
// //             {
// //                 m_Entries.Add(new Entry(key, value));
// //                 _indexMap[key] = m_Entries.Count - 1;
// //             }
// //         }
// //     }

// //     public ICollection<TKey> Keys => _indexMap.Keys;
// //     public ICollection<TValue> Values
// //     {
// //         get
// //         {
// //             var vals = new List<TValue>(m_Entries.Count);
// //             foreach (var e in m_Entries) vals.Add(e.Value);
// //             return vals;
// //         }
// //     }

// //     public int Count => m_Entries.Count;
// //     public bool IsReadOnly => false;

// //     public void Add(TKey key, TValue value)
// //     {
// //         if (_indexMap.ContainsKey(key))
// //             throw new ArgumentException($"已存在相同键：{key}");
// //         m_Entries.Add(new Entry(key, value));
// //         _indexMap[key] = m_Entries.Count - 1;
// //     }

// //     public bool ContainsKey(TKey key) => _indexMap.ContainsKey(key);

// //     public bool Remove(TKey key)
// //     {
// //         if (!_indexMap.TryGetValue(key, out var idx))
// //             return false;
// //         // 移除并重建索引
// //         m_Entries.RemoveAt(idx);
// //         RebuildIndex();
// //         return true;
// //     }

// //     public bool TryGetValue(TKey key, out TValue value)
// //     {
// //         if (_indexMap.TryGetValue(key, out var idx))
// //         {
// //             value = m_Entries[idx].Value;
// //             return true;
// //         }
// //         value = default;
// //         return false;
// //     }

// //     public void Clear()
// //     {
// //         m_Entries.Clear();
// //         _indexMap.Clear();
// //     }

// //     public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
// //     public bool Contains(KeyValuePair<TKey, TValue> item)
// //         => ContainsKey(item.Key) && EqualityComparer<TValue>.Default.Equals(this[item.Key], item.Value);
// //     public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
// //     {
// //         for (int i = 0; i < m_Entries.Count; i++)
// //             array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(m_Entries[i].Key, m_Entries[i].Value);
// //     }
// //     public bool Remove(KeyValuePair<TKey, TValue> item)
// //         => Remove(item.Key);
// //     public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
// //     {
// //         foreach (var e in m_Entries)
// //             yield return new KeyValuePair<TKey, TValue>(e.Key, e.Value);
// //     }
// //     IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

// //     #endregion
// // }
