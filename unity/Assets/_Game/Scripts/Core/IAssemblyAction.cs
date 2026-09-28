using System;

namespace PCTechnician.Core
{
    /// <summary>
    /// Contextual assembly action exposed to the mobile Action Drawer.
    /// </summary>
    public interface IAssemblyAction
    {
        string ActionId { get; }
        string ActionLabel { get; }
        string IconName { get; }
        bool IsAvailable { get; }
        void Execute();
    }
}
