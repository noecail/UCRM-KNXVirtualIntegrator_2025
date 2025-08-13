//#pragma warning disable

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;


namespace KNX_Virtual_Integrator.Model.Entities;

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged where TValue : notnull where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dictionary;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
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
        _dictionary = new Dictionary<TKey, TValue>(dictionary);
    }

    /// <summary>Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class. </summary>
    /// <param name="comparer">The comparer. </param>
    public ObservableDictionary(IEqualityComparer<TKey> comparer)
    {
        _dictionary = new Dictionary<TKey, TValue>(comparer);
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>Called when the collection has changed.</summary>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        CollectionChanged?.Invoke(this, args);
    }

    private void NotifyAdd(TKey key, TValue value)
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            new KeyValuePair<TKey, TValue>(key, value)
        ));
    }

    private void NotifyRemove(TKey key, TValue value)
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        NotifyReset();
    }

    private void NotifyReset()
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset
        ));
    }

    public void Add(TKey key, TValue value)
    {
        _dictionary.Add(key, value);
        NotifyAdd(key, value);
    }

    public bool Remove(TKey key)
    {
        if (_dictionary.TryGetValue(key, out var value) && _dictionary.Remove(key))
        {
            NotifyRemove(key, value);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        if (_dictionary.Count > 0)
        {
            _dictionary.Clear();
            NotifyReset();
        }
    }

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value!);

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

    public ICollection<TKey> Keys => _dictionary.Keys;
    public ICollection<TValue> Values => _dictionary.Values;
    public int Count => _dictionary.Count;
    public bool IsReadOnly => false;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);
}


//#pragma warning restore