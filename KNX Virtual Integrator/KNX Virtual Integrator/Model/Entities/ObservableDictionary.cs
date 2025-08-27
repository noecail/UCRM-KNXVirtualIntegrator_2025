//#pragma warning disable

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;


namespace KNX_Virtual_Integrator.Model.Entities;
/// <summary>
/// Class wrapper for IDictionary to handle more notifications when properties change.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary</typeparam>
public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged where TValue : notnull where TKey : notnull
{
    /// <summary>
    /// The instance of the dictionary.
    /// </summary>
    private readonly Dictionary<TKey, TValue> _dictionary;
    /// <summary>
    /// Invoked when a collection has changed.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    /// <summary>
    /// Invoked when a property has changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class. </summary>
    public ObservableDictionary()
    {
        _dictionary = new Dictionary<TKey, TValue>();
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class. </summary>
    /// <param name="dictionary">The dictionary to initialize this dictionary. </param>
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _dictionary = new Dictionary<TKey, TValue>();

        // copie profonde du dico
        foreach (var kvp in dictionary)
        {
            TValue value;

            if (typeof(TValue) == typeof(FunctionalModelStructure.DptAndKeywords))
            {
                // On appelle explicitement le constructeur de copie
                var original = (FunctionalModelStructure.DptAndKeywords)(object)kvp.Value;
                value = (TValue)(object)new FunctionalModelStructure.DptAndKeywords(original);
            }
            else
            {
                // Fallback : copie directe (shallow)
                value = kvp.Value;
            }

            _dictionary.Add(kvp.Key, value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="comparer">The comparer. </param>
    public ObservableDictionary(IEqualityComparer<TKey> comparer)
    {
        _dictionary = new Dictionary<TKey, TValue>(comparer);
    }
    /// <summary>
    /// Called when the collection has changed.
    /// </summary>
    /// <param name="propertyName">The name of the changed property/attribute.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    /// <summary>
    /// Called when the collection has changed.
    /// </summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        CollectionChanged?.Invoke(this, args);
    }
    /// <summary>
    /// nvokes the change of Count, Item[] and a Add with the key and value
    /// </summary>
    /// <param name="key">The key of the added item</param>
    /// <param name="value">The value of the added item</param>
    private void NotifyAdd(TKey key, TValue value)
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
    }
    /// <summary>
    /// Invokes the change of Count, Item[] and a Remove with the key,index and value
    /// </summary>
    /// <param name="key">The key of the removed item</param>
    /// <param name="value">The value of the removed item</param>
    /// <param name="index">the index of the removed item</param>
    private void NotifyRemove(TKey key, TValue value, int index)
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove,
            new KeyValuePair<TKey, TValue>(key, value),
            index
        ));
    }
    /// <summary>
    /// Invokes the change of Count, Item[] and a Reset
    /// </summary>
    private void NotifyReset()
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset
        ));
    }
    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
    /// <exception cref="ArgumentNullException">key is null</exception>
    /// <exception cref="ArgumentException">An element with the same key already exists in the Dictionary.</exception>
    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        NotifyAdd(key, value);
    }
    /// <summary>
    /// Removes the value with the specified key from the Dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>true if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the Dictionary.</returns>
    /// <exception cref="ArgumentNullException">key is null</exception>
    public bool Remove(TKey key)
    {
        var keysList = new List<TKey>(Keys);
        keysList.Sort();
        var index = keysList.IndexOf(key);
        if (_dictionary.TryGetValue(key, out var value) && _dictionary.Remove(key))
        {
            NotifyRemove(key, value, index);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Removes all keys and values from the Dictionary.
    /// </summary>
    public void Clear()
    {
        if (_dictionary.Count > 0)
        {
            _dictionary.Clear();
            NotifyReset();
        }
    }
    /// <summary>
    /// Determines whether the Dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the Dictionary.</param>
    /// <returns>true if the Dictionary contains an element with the specified key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">key is null</exception>
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);
    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <returns>true if the Dictionary contains an element with the specified key; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">key is null</exception>
    public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value!);
    /// <summary>
    /// Gets/sets the value in the dictionary associated with the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="ArgumentNullException">key is null</exception>
    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            if (_dictionary.TryGetValue(key, out TValue? oldValue))
            {
                _dictionary[key] = value;
                OnPropertyChanged("Item[]");
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>(key, value),
                    new KeyValuePair<TKey, TValue>(key, oldValue)
                ));
            }
            else
            {
                _dictionary[key] = value;
                NotifyAdd(key, value);
            }
        }
    }
    /// <summary>
    /// Gets an ICollection containing the keys of the dictionary.
    /// </summary>
    public ICollection<TKey> Keys => _dictionary.Keys;
    /// <summary>
    /// Gets an ICollectio containing the values of the dictionary.
    /// </summary>
    public ICollection<TValue> Values => _dictionary.Values;
    /// <summary>
    /// Gets the number of elements contained in the dictionary.
    /// </summary>
    public int Count => _dictionary.Count;
    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only and returns false.
    /// </summary>
    public bool IsReadOnly => false;
    /// <summary>
    /// Returns an enumerator that iterates through the Dictionary.
    /// </summary>
    /// <returns>An Enumerator structure for the Dictionary</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    /// <summary>
    /// Adds a KeyValuePair with the parameter Key and Value
    /// </summary>
    /// <param name="item">The KeyValuePair to copy.</param>
    /// <exception cref="ArgumentNullException">key is null</exception>
    /// <exception cref="ArgumentException"> An element with the same key already exists in the IDictionary</exception>
    /// <exception cref="NotSupportedException">The IDictionary is read-only</exception>
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    /// <summary>
    /// Determines whether a sequence contains a specified element by using the default equality comparer.
    /// </summary>
    /// <param name="item">the KeyValuePair to compare with.</param>
    /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">The source is null</exception>
    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);
    /// <summary>
    /// Copies the elements of the ICollection to an Array, starting at a particular Array index.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination of the elements copied from ICollection. The Array must have zero-based indexing.</param>
    /// <param name="arrayIndex"> The zero-based index in array at which copying begins</param>
    /// <exception cref="ArgumentNullException">The source is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
    /// <exception cref="ArgumentException">The number of elements in the source ICollection is greater than the available space from arrayIndex to the end of the destination array</exception>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
    }
    /// <summary>
    /// <see cref="Remove(TKey)"/>
    /// </summary>
    /// <param name="item">the item to remove</param>
    /// <returns>true if the removal was successful.</returns>
    /// <exception cref="ArgumentNullException">key is null</exception>
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}


//#pragma warning restore