namespace KNX_Virtual_Integrator.Model.Interfaces;

public interface IGroupCommunication
{
    Task GroupValueWriteOnAsync();

    Task GroupValueWriteOffAsync();
}