using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "CPU_Data", menuName = "PC Technician/Hardware/CPU Data")]
    public class CPUData : HardwareData
    {
        [Header("Socket & Architecture")]
        [SerializeField] private string socketType = "AM5_FICTIONAL";
        [SerializeField] private int coreCount = 8;
        [SerializeField] private int threadCount = 16;
        [SerializeField] private float baseClockGhz = 3.8f;
        [SerializeField] private float boostClockGhz = 5.2f;

        [Header("Thermals & Power")]
        [SerializeField] private int tdpWatts = 105;
        [SerializeField] private int peakPowerWatts = 142;
        [SerializeField] private float thermalDesignLimitCelsius = 95f;
        [SerializeField] private float maxSafeTempCelsius = 95f;
        [SerializeField] private int performanceRating = 820;
        [SerializeField] private bool hasIntegratedGraphics = true;

        [Header("Alignment Features")]
        [SerializeField] private int notchIndex = 0; // Alignment triangle corner

        public string SocketType => socketType;
        public int CoreCount => coreCount;
        public int ThreadCount => threadCount;
        public float BaseClockGhz => baseClockGhz;
        public float BoostClockGhz => boostClockGhz;
        public int TDPWatts => tdpWatts;
        public int PeakPowerWatts => peakPowerWatts;
        public float ThermalDesignLimitCelsius => thermalDesignLimitCelsius;
        public float MaxSafeTempCelsius => maxSafeTempCelsius;
        public int PerformanceRating => performanceRating;
        public bool HasIntegratedGraphics => hasIntegratedGraphics;
        public int NotchIndex => notchIndex;

        public void SetRuntimeValues(string id, string model, string mfg, string socket, int tdp, int peakWatts = 142)
        {
            InitializeBase(id, model, mfg, ComponentCategory.CPU);
            socketType = socket;
            tdpWatts = tdp;
            peakPowerWatts = peakWatts;
        }
    }
}
