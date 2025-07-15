using Knx.Falcon;

namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IGroupCommunication
{
    Task GroupValueWriteOnAsync();

    Task GroupValueWriteOffAsync();

    Task<GroupValue?> MaGroupValueReadAsync(GroupAddress groupAddress);
    Task<bool> GroupValueWriteAsync(GroupAddress addr, GroupValue value);

}