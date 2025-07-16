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
    
    public List<GroupValue?> Value { get; } // Value to send or expected to be read

    private string _address = "0/1/1";

    public string Address
    {
        get => _address;
        set => _address = value;
    }
    
    //Constructors
    public DataPointType(int type)
    {
        Type = type;
        Value = [new GroupValue(true)];
        GetSizeOf();
    }

    public DataPointType(int type, string address)
    {
        Type = type;
        Value = [new GroupValue(true)];
        GetSizeOf();
        Address = address;
    }

    public DataPointType(int type, string  address, List<GroupValue?> values)
    {
        Type = type;
        Address = address;
        GetSizeOf();
        Value = [];
        for (int i = 0; i < values.Count; i++)
        {
            Value.Add(values[i]);
        }
    }
    public DataPointType(DataPointType dpt)
    {
        Type = dpt.Type;
        Value = [];
        Address = dpt.Address;
        GetSizeOf();
        for (int i = 0; i < dpt.Value.Count; i++)
        {
            Value[i] = dpt.Value[i];
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
            case 236:
            case 238:
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
            case 234:
            case 237:
            case 239:
            case 244:
            case 246:
                _size = 16;
                break;
            case 10:
            case 11:
            case 30:
            case 203:
            case 205:
            case 206:
            case 209:
            case 223:
            case 225:
            case 232:
            case 240:
            case 250:
            case 254:
                _size = 24;
                break;
            case 215:
            case 216:
            case 218:
            case 248:
            case 252:
                _size = 40;
                break;
            case 245:
                _size = 44;
                break;
            case 212:
            case 221:
            case 222:
            case 224:
            case 229:
            case 235:
            case 242:
            case 251:
            case 257:
            case 274:
                _size = 48;
                break;
            case 271: 
            case 272:
                _size = 56;
                break;
            case 19:
            case 29:
            case 213:
            case 219:
            case 230:
            case 243:
            case 255:
            case 273:
                _size = 64;
                break;
            case 265:
                _size = 65;
                break;
            case 267:
                _size = 66;
                break;
            case 268:
                _size = 72;
                break;
            case 247:
            case 266:
                _size = 96;
                break;
            case 16:
            case 269:
            case 277:
            case 278:
            case 279:  
            case 280:
            case 281:
            case 282:
            case 283:
            case 284:
                _size = 112;
                break;
            case 256:
                _size = 128;
                break;
            case 270:
                _size = 160;
                break;
            default:
                _size = 32;
                break;
        }
    }
    
    
    /// <summary>
    /// This method compares the number of addresses of 2 DPT.
    /// </summary>
    /// <returns>Returns true when both DPTs have the same number of addresses.</returns>
    public bool CompareValuesLength(DataPointType dpt)
    {
        return Value.Count == dpt.Value.Count;
                
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
        var res = true;
        foreach (var value in Value)
        {
            res = res && Convert.ToUInt64(value) < max;
        }
        return res;
    }

    /// <summary>
    /// This method adds a value to a DPT.
    /// </summary>
    public void AddValue(GroupValue? value)
    {
        Value.Add(value);
    }
    
    /// <summary>
    /// This method Replaces a value with another.
    /// </summary>
    public void ReplaceValue(int index, GroupValue? value)
    {
        RemoveValue(index);
        AddValue(value);
    }
    
    /// <summary>
    /// This method deletes an address to a DPT.
    /// </summary>
    public void RemoveValue(int index)
    {
        Value.RemoveAt(index);
    }

    /// <summary>
    /// This method checks if the group value of the DPT is the same as the one in parameter.
    /// <returns>Returns true when the read(in parameter) and expected values are the same</returns>
    /// </summary>
    public bool CompareGroupValue(GroupValue value, int index)
    {
        return value.Equals(Value[index]);
    }
    
}

