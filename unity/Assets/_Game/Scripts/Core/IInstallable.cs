using PCTechnician.Assembly;

namespace PCTechnician.Core
{
    /// <summary>
    /// Contract for hardware components that can be installed into a ComponentSlot.
    /// </summary>
    public interface IInstallable
    {
        ComponentState CurrentState { get; }
        bool CanInstallInto(ComponentSlot slot, out string failureReason);
        void BeginInstallation(ComponentSlot slot);
        void CompleteInstallation();
    }
}
