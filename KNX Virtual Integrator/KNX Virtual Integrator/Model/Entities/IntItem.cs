using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace KNX_Virtual_Integrator.Model.Entities;

/// <summary>
/// Int wrapper
/// Used to have a setter
/// <see cref="PropertyChanged"/> and interface visibility handling.
/// </summary>
public class IntItem : INotifyPropertyChanged
{
    /// <summary>
    /// Value of the DPT
    /// </summary>
    private int _value;
    /// <summary>
    /// Gets or sets the value of the DPT
    /// </summary>
    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }
    
    /// <summary>
    /// Visibility of the button used to remove a DPT in <see cref="View.Windows.StructureEditWindow"/>
    /// </summary>
    private Visibility? _removeDptButtonVisibility;
    /// <summary>
    /// Gets or sets the visibility of the button used to
    /// remove a DPT in <see cref="View.Windows.StructureEditWindow"/>
    /// </summary>
    public Visibility? RemoveDptButtonVisibility
    {
        get => _removeDptButtonVisibility;
        set
        {
            if (_removeDptButtonVisibility == value) return;
            _removeDptButtonVisibility = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// Used to "quicken" search of the value since using the item will output directly its value.
    /// </summary>
    /// <param name="item">The item whose value will be returned.</param>
    /// <returns>The value of the item.</returns>
    public static implicit operator int(IntItem item) => item.Value;
    /// <summary>
    /// To print only the <see cref="Value"/> of the item.
    /// </summary>
    /// <returns>The value as a string</returns>
    public override string ToString()
    {
        return Value.ToString();
    }
    /// <summary>
    /// Constructs the class with hidden visibility. 
    /// </summary>
    /// <param name="value">the value to which <see cref="Value"/> will be initialised.</param>
    public IntItem(int value)
    {
        Value = value;
        RemoveDptButtonVisibility = Visibility.Hidden;
    }
    /// <summary>
    /// Event that occurs when the IntItem changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Invokes <see cref="PropertyChanged"/> when called.
    /// </summary>
    /// <param name="name">The name of the property that was changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}