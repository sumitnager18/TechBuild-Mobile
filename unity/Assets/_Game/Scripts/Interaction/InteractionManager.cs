using System;
using PCTechnician.Core;
using UnityEngine;

namespace PCTechnician.Interaction
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("Touch Raycasting")]
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private LayerMask interactableLayerMask = ~0;
        [SerializeField] private float requiredHoldDurationSeconds = 0.8f;

        [Header("Selection State")]
        [SerializeField] private TouchInteractionTarget currentTarget;

        private float currentHoldTimer;
        private bool isHolding;

        public static InteractionManager Instance { get; private set; }

        public event Action<TouchInteractionTarget> OnTargetSelected;
        public event Action<TouchInteractionTarget> OnTargetDeselected;
        public event Action<TouchInteractionTarget, float> OnHoldProgress; // 0.0 to 1.0

        public TouchInteractionTarget CurrentTarget => currentTarget;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (targetCamera == null)
            {
                targetCamera = UnityEngine.Camera.main;
            }
        }

        private void Update()
        {
            HandleTouchInput();
            HandleHoldTimer();
        }

        private void HandleTouchInput()
        {
            // Mobile touch or desktop fallback
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    PerformRaycast(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    EndHold();
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                PerformRaycast(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndHold();
            }
        }

        public void PerformRaycast(Vector2 screenPosition)
        {
            if (targetCamera == null) return;

            Ray ray = targetCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 20f, interactableLayerMask))
            {
                var target = hit.collider.GetComponentInParent<TouchInteractionTarget>();
                if (target != null && target.CanInteract)
                {
                    SelectTarget(target);
                    StartHold();
                    return;
                }
            }

            DeselectCurrent();
        }

        public void SelectTarget(TouchInteractionTarget target)
        {
            if (currentTarget == target) return;

            if (currentTarget != null)
            {
                currentTarget.OnTouchDeselect();
                OnTargetDeselected?.Invoke(currentTarget);
            }

            currentTarget = target;
            if (currentTarget != null)
            {
                currentTarget.OnTouchSelect();
                OnTargetSelected?.Invoke(currentTarget);
            }
        }

        public void DeselectCurrent()
        {
            if (currentTarget != null)
            {
                currentTarget.OnTouchDeselect();
                OnTargetDeselected?.Invoke(currentTarget);
                currentTarget = null;
            }
            EndHold();
        }

        private void StartHold()
        {
            if (currentTarget == null) return;
            isHolding = true;
            currentHoldTimer = 0f;
            currentTarget.OnTouchHoldStart();
        }

        private void HandleHoldTimer()
        {
            if (!isHolding || currentTarget == null) return;

            currentHoldTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(currentHoldTimer / requiredHoldDurationSeconds);
            OnHoldProgress?.Invoke(currentTarget, progress);

            if (progress >= 1f)
            {
                currentTarget.OnTouchHoldEnd();
                isHolding = false;
            }
        }

        private void EndHold()
        {
            if (!isHolding) return;
            isHolding = false;
            currentHoldTimer = 0f;
            if (currentTarget != null)
            {
                OnHoldProgress?.Invoke(currentTarget, 0f);
            }
        }
    }
}
