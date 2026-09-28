using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "GPU_Data", menuName = "PC Technician/Hardware/GPU Data")]
    public class GPUData : HardwareData
    {
        [Header("Bus & Architecture")]
        [SerializeField] private string slotInterface = "PCIe_16x";
        [SerializeField] private string pcieGeneration = "Gen 5.0";
        [SerializeField] private int vramGb = 16;
        [SerializeField] private int lengthMm = 285;
        [SerializeField] private int slotWidth = 2; // Dual slot
        [SerializeField] private int performanceRating = 910;

        [Header("Power & Thermals")]
        [SerializeField] private int tdpWatts = 220;
        [SerializeField] private int peakTransientWatts = 320;
        [SerializeField] private int thermalOutputWatts = 220;
        [SerializeField] private int requiredPcie8PinCables = 1;

        public string SlotInterface => slotInterface;
        public string PcieGeneration => pcieGeneration;
        public int VramGb => vramGb;
        public int LengthMm => lengthMm;
        public int SlotWidth => slotWidth;
        public int PerformanceRating => performanceRating;
        public int TDPWatts => tdpWatts;
        public int PeakTransientWatts => peakTransientWatts;
        public int ThermalOutputWatts => thermalOutputWatts;
        public int RequiredPcie8PinCables => requiredPcie8PinCables;

        public void SetRuntimeValues(string id, string model, string mfg, int tdp, int pcie8Pins, int peakTransient = 320,
            string generation = "Gen 5.0", string interfaceType = "PCIe_16x")
        {
            InitializeBase(id, model, mfg, ComponentCategory.GPU);
            tdpWatts = tdp;
            requiredPcie8PinCables = pcie8Pins;
            peakTransientWatts = peakTransient;
            thermalOutputWatts = tdp;
            pcieGeneration = generation;
            slotInterface = interfaceType;
        }
    }
}
