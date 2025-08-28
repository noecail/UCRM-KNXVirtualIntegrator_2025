using System.Collections.ObjectModel;
using KNX_Virtual_Integrator.Model.Implementations;
using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Interfaces;
/// <summary>
/// Represents the communication with the bus in terms of group addresses and values.
/// Allows writing and reading information from the bus while checking its state.
/// </summary>
public interface IGroupCommunication
{
    /// <summary>
    /// Gets or sets the address with which the program will communicate with the bus.
    /// It specifies which Group Address will make use or read the <see cref="GroupValue"/>.
    /// </summary>
    GroupAddress GroupAddress { get; set; }

    /// <summary>
    /// Gets the group value to send to the bus.
    /// It represents which state the object will be in.
    /// </summary>
    GroupValue? GroupValue { get; set; }

    /// <summary>
    /// Dev method. Sends asynchronously the value "on" to a specified address.
    /// First verifies the bus state before sending the value.
    /// Logs an error if it fails. 
    /// </summary>
    /// <returns>A task representing the completion of the writing.</returns>
    Task GroupValueWriteOnAsync();

    /// <summary>
    /// Dev method. Sends asynchronously the value "off" to a specified address.
    /// First verifies the bus state before sending the value.
    /// Logs an error if it fails. 
    /// </summary>
    /// <returns>A task representing the completion of the writing.</returns>
    Task GroupValueWriteOffAsync();

    /// <summary>
    /// Sends asynchronously a value to a specified address.
    /// First verifies the bus state before sending the value.
    /// Logs an error if it fails.
    /// </summary>
    /// <param name="addr">The address at which the value is sent.</param>
    /// <param name="value">The value to send.</param>
    /// <returns>A task representing the completion of the writing.</returns>
    Task<bool> GroupValueWriteAsync(GroupAddress addr, GroupValue value);

    ///<summary>
    /// Converts a uLong value to a byte table to write on the bus
    ///</summary>
    /// <param name="toSend">The value to send.</param>
    /// <param name="groupValue">The table to fill before writing.</param>
    void ConvertToGroupValue(ulong toSend, byte[] groupValue);

    /// <summary>
    /// Reads asynchronously values from a group address.
    /// Verifies the bus connection state before sending the request.
    /// uses a <see cref="TaskCompletionSource{T}"/> to capture the read value.
    /// </summary>
    /// <param name="groupAddress">The group address at which the value should be read.</param>
    /// <returns>A task representing the completion of the task, containing the received messages.</returns>
    Task<GroupValue?> MaGroupValueReadAsync(GroupAddress groupAddress);

    /// <summary>
    /// Reads asynchronously values from a group address until the timer runs out.
    /// Verifies the bus connection state before sending the request.
    /// uses a <see cref="TaskCompletionSource{T}"/> to capture the read value.
    /// </summary>
    /// <param name="groupAddress">The group address at which the value should be read.</param>
    ///  <param name="timerDuration">Timer in ms under which the message should be received.</param>
    /// <returns>A task representing the completion of the task, containing the received messages..</returns>
    Task<List<GroupCommunication.GroupMessage>> GroupValuesWithinTimerAsync(GroupAddress groupAddress,
        int timerDuration);

    /// <summary>
    /// Reads asynchronously values from a group address until a Write is received or the timer runs out.
    /// Verifies the bus connection state before sending the request.
    /// uses a <see cref="TaskCompletionSource{T}"/> to capture the read value.
    /// </summary>
    /// <param name="groupAddress">The group address at which the value should be read.</param>
    ///  <param name="timerDuration">Timer in ms under which the message should be received.</param>
    /// <returns>A task representing the completion of the task, containing the received messages.</returns>
    Task<List<GroupCommunication.GroupMessage>> GroupValuesTimerOrRecievedAWriteAsync(GroupAddress groupAddress, int timerDuration);
    
    /// <summary>
    /// Gets the collection of group events args.
    /// </summary>
    public ObservableCollection<GroupEventArgs> GroupEvents { get; }


}