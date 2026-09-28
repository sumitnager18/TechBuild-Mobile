using System;
using PCTechnician.Core;
using UnityEngine;

namespace PCTechnician.Assembly
{
    [Serializable]
    public class SlotPrerequisite : IPrerequisite
    {
        [SerializeField] private string prerequisiteDescription;
        [SerializeField] private bool requiresSlotOpen;
        [SerializeField] private ComponentSlot dependentSlot;
        [SerializeField] private ComponentState requiredState;

        public string PrerequisiteDescription => prerequisiteDescription;

        public bool IsSatisfied
        {
            get
            {
                if (dependentSlot == null) return true;
                return dependentSlot.CurrentState == requiredState;
            }
        }

        public SlotPrerequisite(string description, ComponentSlot slot, ComponentState state)
        {
            prerequisiteDescription = description;
            dependentSlot = slot;
            requiredState = state;
        }

        public string GetUnsatisfiedMessage()
        {
            return $"Prerequisite not met: {prerequisiteDescription}";
        }
    }
}
