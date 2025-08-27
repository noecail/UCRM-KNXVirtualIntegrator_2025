using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace KNX_Virtual_Integrator.Model.Entities;

/// <summary>
/// Class used only for Value collections,
/// used by the UI to access and modify BigInteger values,
/// which do not raise notifications by default.
/// It is a sort of wrapper
/// </summary>
public class BigIntegerItem : INotifyPropertyChanged
{
    /// <summary>
    /// The visibility of the Value box in TestedElement.
    /// </summary>
    private Visibility? _removeTestButtonVisibility ;
    /// <summary>
    /// Gets or sets the visibility of the Value box in TestedElement.
    /// </summary>
    public Visibility? RemoveTestButtonVisibility
    {
        get => _removeTestButtonVisibility;
        set
        {
            if (_removeTestButtonVisibility == value) return;
            _removeTestButtonVisibility = value;
            OnPropertyChanged();
        }
    }
    /// <summary>
    /// The wrapped BigInteger.
    /// </summary>
    private BigInteger? _bigIntegervalue;
    /// <summary>
    /// Gets or sets the wrapped BigInteger.
    /// </summary>
    public BigInteger? BigIntegerValue
    {
        get => _bigIntegervalue;
        set
        {
            if (_bigIntegervalue != value)
            {
                _bigIntegervalue = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Boolean determining whether the box is enabled or not.
    /// </summary>
    private bool _isEnabled;
    /// <summary>
    /// Gets or sets the boolean determining whether the box is enabled or not.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
    }
    /// <summary>
    /// Initializes the wrapper with a Collapsed visibility and unless the parameter is equal to -1,
    /// the box is enabled.
    /// </summary>
    /// <param name="bi">The <see cref="BigInteger"/> to be wrapped</param>
    public BigIntegerItem(BigInteger bi)
    {
        BigIntegerValue = bi;
        RemoveTestButtonVisibility = Visibility.Collapsed;
        IsEnabled = true;
        if (bi == -1)
        {
            IsEnabled = false;
        }
    }
    /// <summary>
    /// The event that occurs when the BigIntegerItem changes. 
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>
    /// Invokes the event <see cref="PropertyChanged"/> when the BigIntegerItem changes.
    /// </summary>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}