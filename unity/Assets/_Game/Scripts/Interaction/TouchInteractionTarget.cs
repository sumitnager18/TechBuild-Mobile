using System;
using PCTechnician.Core;
using UnityEngine;

namespace PCTechnician.Interaction
{
    /// <summary>
    /// Decoupled touch interaction zone. Provides expanded touch margins for mobile fingers
    /// without distorting the visual mesh collider bounds.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TouchInteractionTarget : MonoBehaviour, IInteractable
    {
        [Header("Target Information")]
        [SerializeField] private string targetDisplayName = "Hardware Target";
        [SerializeField] private bool isInteractionEnabled = true;
        [SerializeField] private InteractionType primaryInteractionType = InteractionType.TapSelect;

        [Header("Expanded Touch Volume")]
        [SerializeField] private Vector3 mobileTouchPadding = new Vector3(0.04f, 0.04f, 0.04f); // 40mm mobile touch buffer

        public event Action OnSelected;
        public event Action OnDeselected;
        public event Action OnHoldStarted;
        public event Action OnHoldCompleted;

        public string DisplayName => targetDisplayName;
        public bool CanInteract => isInteractionEnabled && gameObject.activeInHierarchy;
        public InteractionType PrimaryInteractionType => primaryInteractionType;

        public void SetInteractionEnabled(bool enabled)
        {
            isInteractionEnabled = enabled;
        }

        public void OnTouchSelect()
        {
            if (!CanInteract) return;
            OnSelected?.Invoke();
        }

        public void OnTouchDeselect()
        {
            OnDeselected?.Invoke();
        }

        public void OnTouchHoldStart()
        {
            if (!CanInteract) return;
            OnHoldStarted?.Invoke();
        }

        public void OnTouchHoldEnd()
        {
            OnHoldCompleted?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one + mobileTouchPadding * 2f);
        }
    }
}
