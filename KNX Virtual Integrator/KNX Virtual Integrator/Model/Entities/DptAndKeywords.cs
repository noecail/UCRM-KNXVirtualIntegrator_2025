using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KNX_Virtual_Integrator.Model.Entities;

/// <summary>
/// The class holding defining a DPT and the keywords associated with it.
/// </summary>
public class DptAndKeywords : INotifyPropertyChanged
{
    /// <summary>
    /// Key/Number of the instance.
    /// </summary>
    public int Key;
    
    /// <summary>
    /// List of keywords associated with the <see cref="Dpt"/>.
    /// </summary>
    private List<string> _keywords = [];
    
    /// <summary>
    /// Gets or sets the list of keywords associated with the <see cref="Dpt"/>.
    /// </summary>
    public List<string> Keywords
    {
        get => _keywords;
        set
        {
            if (_keywords == value) return;
            _keywords = value;
            OnPropertyChanged();
            UpdateKeywordList();
        }
    }
    
    /// <summary>
    /// String of all the keywords associated with the <see cref="Dpt"/>.
    /// </summary>
    private string _allKeywords = "";
    /// <summary>
    /// Gets or sets the string of all the keywords associated with the <see cref="Dpt"/>.
    /// </summary>
    public string AllKeywords
    {
        get => _allKeywords;
        set
        {
            if (_allKeywords == value) return;
            _allKeywords = value;
            OnPropertyChanged();
            UpdateKeywords();
        }
        
    }
    /// <summary>
    /// The DPT associated with the <see cref="Keywords"/>.
    /// </summary>
    public DataPointType Dpt { get; set; } = new ();

    /// <summary>
    /// Takes a string, and puts all the keywords inside it into the keywords associated
    /// </summary>
    private void UpdateKeywords()
    {
        _keywords.Clear();
        foreach (var kw in AllKeywords.Split(',').ToList())
        {
            _keywords.Add(kw);
            OnPropertyChanged(nameof(Keywords));
        }
    }
    
    /// <summary>
    /// Takes all the keywords associated to a dpt and group them, separating them with commas
    /// </summary>
    public void UpdateKeywordList()
    {
        if (Keywords == null || Keywords.Count == 0)
            return;
        _allKeywords = string.Join(',', Keywords);
        OnPropertyChanged(nameof(AllKeywords));
    }
    
    /// <summary>
    /// Empty constructor since everything is already initialized.
    /// </summary>
    public DptAndKeywords() { }
    
    /// <summary>
    /// Copies a DptAndKeywords.
    /// </summary>
    /// <param name="other">The DptAndKeywords to copy</param>
    public DptAndKeywords(DptAndKeywords other)
    {
        Key = other.Key;
        _keywords = other._keywords != null ? new List<string>(other._keywords) : new List<string>();
        _allKeywords = other._allKeywords ?? string.Empty;
        Dpt = new DataPointType(other.Dpt);

        PropertyChanged = null; // les handlers ne doivent pas être copiés
    }

    /// <summary>
    /// Event that occurs when the DptAndKeywords changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Invokes <see cref="PropertyChanged"/> when called.
    /// </summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
}