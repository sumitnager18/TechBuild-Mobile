using System.Collections.Generic;
using PCTechnician.Core;
using UnityEngine;

namespace PCTechnician.Cameras
{
    /// <summary>
    /// Reusable FocusZone component. Defines exact camera framing, allowable interactions,
    /// and contextual action sets for mobile touch assembly.
    /// </summary>
    public class FocusZone : MonoBehaviour
    {
        [Header("Zone Identification")]
        [SerializeField] private FocusZoneType zoneType = FocusZoneType.Assembly;
        [SerializeField] private string zoneDisplayName = "Assembly View";

        [Header("Camera Framing")]
        [SerializeField] private Transform focusTarget;
        [SerializeField] private Transform cameraRigTransform;
        [SerializeField] private float cameraDistance = 0.45f;
        [SerializeField] private float fieldOfView = 50f;

        [Header("Contextual Actions Allowed in this Zone")]
        [SerializeField] private List<string> allowedActionIds = new List<string>();

        public FocusZoneType ZoneType => zoneType;
        public string ZoneDisplayName => zoneDisplayName;
        public Transform FocusTarget => focusTarget != null ? focusTarget : transform;
        public Transform CameraRigTransform => cameraRigTransform != null ? cameraRigTransform : transform;
        public float CameraDistance => cameraDistance;
        public float FieldOfView => fieldOfView;
        public IReadOnlyList<string> AllowedActionIds => allowedActionIds;

        public void Configure(FocusZoneType type, string name, float fov, float dist)
        {
            zoneType = type;
            zoneDisplayName = name;
            fieldOfView = fov;
            cameraDistance = dist;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.05f);
            if (cameraRigTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(cameraRigTransform.position, transform.position);
                Gizmos.DrawWireCube(cameraRigTransform.position, new Vector3(0.08f, 0.06f, 0.12f));
            }
        }
    }
}
