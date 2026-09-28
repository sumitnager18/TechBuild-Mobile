using PCTechnician.Assembly;

namespace PCTechnician.Core
{
    /// <summary>
    /// Contract for hardware components that can be removed from a ComponentSlot.
    /// </summary>
    public interface IRemovable
    {
        bool CanRemove(out string failureReason);
        void BeginRemoval();
        void CompleteRemoval();
    }
}
