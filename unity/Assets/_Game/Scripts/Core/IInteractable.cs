using System;
using UnityEngine;

namespace PCTechnician.Core
{
    /// <summary>
    /// Base interface for any object in the workshop that can be interacted with via touch.
    /// </summary>
    public interface IInteractable
    {
        string DisplayName { get; }
        bool CanInteract { get; }
        void OnTouchSelect();
        void OnTouchDeselect();
        void OnTouchHoldStart();
        void OnTouchHoldEnd();
    }
}
