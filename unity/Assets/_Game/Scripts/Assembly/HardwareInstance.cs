using PCTechnician.Core;
using PCTechnician.Hardware;
using UnityEngine;

namespace PCTechnician.Assembly
{
    /// <summary>
    /// Presentation component attached to physical 3D GameObjects in the scene.
    /// Bridges visual GameObjects to data-driven HardwareData without tightly coupling simulation logic.
    /// </summary>
    public class HardwareInstance : MonoBehaviour, IInspectable, IInstallable, IRemovable
    {
        [Header("Data Definition")]
        [SerializeField] private HardwareData hardwareData;

        [Header("State Tracking")]
        [SerializeField] private ComponentSlot currentSlot;
        [SerializeField] private ComponentState currentState = ComponentState.Uninstalled;

        [Header("Inspection Transforms")]
        [SerializeField] private Transform inspectionAnchor;

        public HardwareData Data => hardwareData;
        public ComponentSlot CurrentSlot => currentSlot;
        public ComponentState CurrentState => currentState;

        public void BindData(HardwareData data)
        {
            hardwareData = data;
        }

        #region IInspectable
        public string GetInspectionTitle() => hardwareData != null ? hardwareData.ModelName : name;
        public string GetTechnicalSummary() => hardwareData != null ? $"{hardwareData.ManufacturerName} • {hardwareData.Category}" : "Unknown Hardware";
        public Transform GetInspectionAnchor() => inspectionAnchor != null ? inspectionAnchor : transform;
        public void BeginInspection() { }
        public void EndInspection() { }
        #endregion

        #region IInstallable
        public bool CanInstallInto(ComponentSlot slot, out string failureReason)
        {
            if (slot == null)
            {
                failureReason = "Target slot is null.";
                return false;
            }
            return slot.CanAccept(hardwareData, out failureReason);
        }

        public void BeginInstallation(ComponentSlot slot)
        {
            currentSlot = slot;
            currentState = ComponentState.Aligned;
        }

        public void CompleteInstallation()
        {
            if (currentSlot != null)
            {
                currentState = currentSlot.IsLatchOpen ? ComponentState.Seated : ComponentState.Locked;
                transform.position = currentSlot.SnapTransform.position;
                transform.rotation = currentSlot.SnapTransform.rotation;
            }
        }
        #endregion

        #region IRemovable
        public bool CanRemove(out string failureReason)
        {
            if (currentSlot == null)
            {
                failureReason = "Component is not currently installed in a slot.";
                return false;
            }
            return currentSlot.CanRemove(out failureReason);
        }

        public void BeginRemoval()
        {
            currentState = ComponentState.PickedUp;
        }

        public void CompleteRemoval()
        {
            currentSlot = null;
            currentState = ComponentState.Uninstalled;
        }
        #endregion
    }
}
