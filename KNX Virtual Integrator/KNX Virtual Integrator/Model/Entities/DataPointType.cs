using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Entities;
/// <summary>
/// Represents a DataPointType with : its type code, size, address and values expected to be sent or read
/// </summary>
public class DataPointType
{
    private int _type;
    public int Type // DPT code
    {
        get => _type; 
        set
        {
            _type = value;
            GetSizeOf();
        }
    }

    private int _size; // Size of the DPT
    
    public GroupValue? Value; // Value to send or expected to be read

    public List<int[]> Address = [[]];
    
    //Constructors
    public DataPointType(int type)
    {
        Type = type;
        Value = new GroupValue(true);
        GetSizeOf();
    }
    public DataPointType(int type, List<int[]>  addresses)
    {
        Type = type;
        Value = new GroupValue(true);
        GetSizeOf();
        for (int i = 0; i < addresses.Count; i++)
        {
            for (int j = 0; j < addresses[i].Length; j++)
            {
                Address[i][j] = addresses[i][j];
            }
        }
    }
    
    public DataPointType(int type, List<int[]>  addresses, GroupValue? value)
    {
        Type = type;
        Value = value;
        GetSizeOf();
        for (int i = 0; i < addresses.Count; i++)
        {
            for (int j = 0; j < addresses[i].Length; j++)
            {
                Address[i][j] = addresses[i][j];
            }
        }
    }
    public DataPointType(DataPointType dpt)
    {
        Type = dpt.Type;
        Value = dpt.Value;
        GetSizeOf();
        for (int i = 0; i < dpt.Address.Count; i++)
        {
            for (int j = 0; j < dpt.Address[i].Length; j++)
            {
                Address[i][j] = dpt.Address[i][j];
            }
        }
    }
    
    
    //Methods
    
    /// <summary>
    /// This method computes the size of a DPT from its code. Only the most used types are implemented(under 222). Check
    /// https://support.knx.org/hc/fr/article_attachments/15392631105682 to add here the new formats if not yet implemented.
    /// </summary>
    private void GetSizeOf()
    {
        switch (Type)
        {
            case 1:
                _size = 1;
                break;
            case 2:
            case 23:
            case 24:
            case 28:
                _size = 2;
                break;
            case 31:
                _size = 3;
                break;
            case 3:
                _size = 4;
                break;
            case 4:
            case 5:
            case 6:
            case 17:
            case 18:
            case 20:
            case 21:
            case 25:
            case 26:
                _size = 8;
                break;
            case 200:
                _size = 9;
                break;
            case 7:
            case 8:
            case 9:
            case 22:
            case 201:
            case 202:
            case 204:
            case 207:
            case 211:
            case 217:
                _size = 16;
                break;
            case 10:
            case 11:
            case 30:
            case 203:
            case 205:
            case 206:
            case 209:
                _size = 24;
                break;
            case 215:
            case 216:
            case 218:
                _size = 40;
                break;
            case 212:
            case 221:
            case 222:
                _size = 48;
                break;
            case 19:
            case 29:
            case 213:
            case 219:
                _size = 64;
                break;
            case 16:
                _size = 112;
                break;
            default:
                _size = 32;
                break;
        }
    }
    
    /// <summary>
    /// This method processes the number of addresses in a DPT.
    /// </summary>
    /// <returns>Returns the number of addresses in a DPT.</returns>
    public int GetAddressLength()
    {
        return Address.Count;
                
    }
    /// <summary>
    /// This method compares the number of addresses of 2 DPT.
    /// </summary>
    /// <returns>Returns true when both DPTs have the same number of addresses.</returns>
    public bool CompareAddressLength(DataPointType dpt)
    {
        return Address.Count == dpt.Address.Count;
                
    }
    
    /// <summary>
    /// This method checks if two DPTs have the same format.
    /// </summary>
    /// <param name="dpt">The DPT that we want to compare with. </param>
    /// <returns>Returns a boolean corresponding to if the DPTs are of the same type.</returns>
    public bool IsEqual(DataPointType dpt)
    {
        return dpt.Type == Type;
                
    }
    
    
    /// <summary>
    /// This method checks whether the selected values to send and to read can fit in the selected size.
    /// <returns>Returns a boolean acknowledging whether the test is possible or not</returns>
    /// </summary>
    public bool IsPossible()
    {
        var max = Convert.ToUInt64(1 << _size);
        return  Convert.ToUInt64(Value) < max;
    }

    /// <summary>
    /// This method adds an address to a DPT.
    /// </summary>
    public void AddAddress(int[] address)
    {
        Address.Add(address);
    }
    
    /// <summary>
    /// This method deletes an address to a DPT.
    /// </summary>
    public void RemoveAddress(int index)
    {
        Address.RemoveAt(index);
    }
    
    
}

