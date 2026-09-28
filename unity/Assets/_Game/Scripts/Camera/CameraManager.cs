using System;
using System.Collections.Generic;
using UnityEngine;

namespace PCTechnician.Cameras
{
    /// <summary>
    /// Manages smooth transitions between Workshop, Assembly, and Focus Camera zones.
    /// Anchors the player to the workbench without FPS-style navigation.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [Header("Main Camera")]
        [SerializeField] private UnityEngine.Camera controlledCamera;
        [SerializeField] private float transitionDurationSeconds = 0.65f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Registered Zones")]
        [SerializeField] private List<FocusZone> registeredZones = new List<FocusZone>();
        [SerializeField] private FocusZone currentZone;
        [SerializeField] private FocusZone defaultWorkshopZone;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private float startFov;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private float targetFov;

        private float transitionTimer;
        private bool isTransitioning;

        public static CameraManager Instance { get; private set; }

        public event Action<FocusZone> OnFocusZoneChanged;
        public FocusZone CurrentZone => currentZone;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (controlledCamera == null)
            {
                controlledCamera = UnityEngine.Camera.main;
            }
        }

        private void Start()
        {
            if (defaultWorkshopZone != null)
            {
                SwitchToZone(defaultWorkshopZone, instant: true);
            }
        }

        private void Update()
        {
            if (!isTransitioning || controlledCamera == null) return;

            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDurationSeconds);
            float curveValue = transitionCurve.Evaluate(t);

            controlledCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
            controlledCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curveValue);
            controlledCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, curveValue);

            if (t >= 1f)
            {
                isTransitioning = false;
            }
        }

        public void SwitchToZone(FocusZone zone, bool instant = false)
        {
            if (zone == null || controlledCamera == null) return;

            currentZone = zone;
            targetPosition = zone.CameraRigTransform.position;
            targetRotation = zone.CameraRigTransform.rotation;
            targetFov = zone.FieldOfView;

            if (instant)
            {
                controlledCamera.transform.position = targetPosition;
                controlledCamera.transform.rotation = targetRotation;
                controlledCamera.fieldOfView = targetFov;
                isTransitioning = false;
            }
            else
            {
                startPosition = controlledCamera.transform.position;
                startRotation = controlledCamera.transform.rotation;
                startFov = controlledCamera.fieldOfView;
                transitionTimer = 0f;
                isTransitioning = true;
            }

            OnFocusZoneChanged?.Invoke(currentZone);
        }

        public void SwitchToZoneByType(FocusZoneType type)
        {
            var zone = registeredZones.Find(z => z != null && z.ZoneType == type);
            if (zone != null)
            {
                SwitchToZone(zone);
            }
        }

        public void RegisterZone(FocusZone zone)
        {
            if (zone != null && !registeredZones.Contains(zone))
            {
                registeredZones.Add(zone);
            }
        }
    }
}
