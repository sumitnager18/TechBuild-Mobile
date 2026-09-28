using System;
using System.Collections.Generic;
using PCTechnician.Core;
using PCTechnician.Hardware;
using UnityEngine;

namespace PCTechnician.Assembly
{
    public class ComponentSlot : MonoBehaviour
    {
        [Header("Slot Identity")]
        [SerializeField] private string slotId = "Slot_Default";
        [SerializeField] private string slotDisplayName = "Component Slot";
        [SerializeField] private ComponentCategory acceptedCategory;

        [Header("Transform Anchors")]
        [SerializeField] private Transform snapTransform;
        [SerializeField] private Transform alignmentGuideTransform;

        [Header("Prerequisites & Rules")]
        [SerializeField] private List<SlotPrerequisite> prerequisites = new List<SlotPrerequisite>();
        [SerializeField] private bool isLatchOpen = true;

        [Header("Current Occupancy & State")]
        [SerializeField] private HardwareData installedHardwareData;
        [SerializeField] private ComponentState currentState = ComponentState.Uninstalled;

        public event Action<ComponentSlot, ComponentState> OnStateChanged;
        public event Action<ComponentSlot, HardwareData> OnComponentInstalled;
        public event Action<ComponentSlot> OnComponentRemoved;

        public string SlotId => slotId;
        public string SlotDisplayName => slotDisplayName;
        public ComponentCategory AcceptedCategory => acceptedCategory;
        public Transform SnapTransform => snapTransform != null ? snapTransform : transform;
        public bool IsOccupied => installedHardwareData != null && currentState != ComponentState.Uninstalled;
        public HardwareData InstalledHardwareData => installedHardwareData;
        public ComponentState CurrentState => currentState;
        public bool IsLatchOpen => isLatchOpen;

        public void Configure(string id, string displayName, ComponentCategory category)
        {
            slotId = id;
            slotDisplayName = displayName;
            acceptedCategory = category;
        }

        public void SetLatchOpen(bool open)
        {
            isLatchOpen = open;
            if (open && currentState == ComponentState.Locked)
            {
                SetState(ComponentState.Seated);
            }
            else if (!open && currentState == ComponentState.Seated)
            {
                SetState(ComponentState.Locked);
            }
        }

        public bool CanAccept(HardwareData hardware, out string failureReason)
        {
            if (hardware == null)
            {
                failureReason = "Hardware data is null.";
                return false;
            }

            if (IsOccupied)
            {
                failureReason = $"Slot '{slotDisplayName}' is already occupied.";
                return false;
            }

            if (hardware.Category != acceptedCategory)
            {
                failureReason = $"Slot accepts '{acceptedCategory}', but item is '{hardware.Category}'.";
                return false;
            }

            foreach (var prerequisite in prerequisites)
            {
                if (!prerequisite.IsSatisfied)
                {
                    failureReason = prerequisite.GetUnsatisfiedMessage();
                    return false;
                }
            }

            failureReason = string.Empty;
            return true;
        }

        public bool Install(HardwareData hardware, out string failureReason)
        {
            if (!CanAccept(hardware, out failureReason))
            {
                return false;
            }

            installedHardwareData = hardware;
            SetState(isLatchOpen ? ComponentState.Seated : ComponentState.Locked);
            OnComponentInstalled?.Invoke(this, hardware);
            return true;
        }

        public bool Remove(out string failureReason)
        {
            if (!IsOccupied)
            {
                failureReason = "Slot is empty.";
                return false;
            }

            if (!isLatchOpen)
            {
                failureReason = "Release the retention latch before removing this component.";
                return false;
            }

            installedHardwareData = null;
            SetState(ComponentState.Uninstalled);
            OnComponentRemoved?.Invoke(this);
            failureReason = string.Empty;
            return true;
        }

        public void SetState(ComponentState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnStateChanged?.Invoke(this, currentState);
        }

        public void AddPrerequisite(SlotPrerequisite prerequisite)
        {
            if (prerequisite != null)
            {
                prerequisites.Add(prerequisite);
            }
        }
    }
}
