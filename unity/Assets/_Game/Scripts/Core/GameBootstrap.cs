using PCTechnician.Cameras;
using PCTechnician.Interaction;
using UnityEngine;

namespace PCTechnician.Core
{
    /// <summary>
    /// GameBootstrap: Coordinates clean, deterministic initialization of modular sub-systems.
    /// Does NOT act as a monolithic god manager containing all game logic.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Sub-System References")]
        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private InteractionManager interactionManager;

        [Header("Boot Configuration")]
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private int targetFrameRate = 60; // Mobile battery & thermal optimization

        public static bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (initializeOnAwake && !IsInitialized)
            {
                InitializeGame();
            }
        }

        public void InitializeGame()
        {
            // Mobile environment configuration
            Application.targetFrameRate = targetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Debug.Log("[PC Technician Simulator] Initializing Game Systems (Prototype 0.1.0)...");

            // Verify or locate sub-systems
            if (cameraManager == null)
            {
                cameraManager = FindFirstObjectByType<CameraManager>();
            }

            if (interactionManager == null)
            {
                interactionManager = FindFirstObjectByType<InteractionManager>();
            }

            IsInitialized = true;
            Debug.Log("[PC Technician Simulator] Core Architecture Initialized Successfully.");
        }
    }
}
