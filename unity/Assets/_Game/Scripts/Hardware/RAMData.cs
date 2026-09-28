using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "RAM_Data", menuName = "PC Technician/Hardware/RAM Data")]
    public class RAMData : HardwareData
    {
        [Header("Memory Spec")]
        [SerializeField] private string memoryType = "DDR5";
        [SerializeField] private int capacityGb = 32;
        [SerializeField] private int speedMhz = 6000;
        [SerializeField] private int casLatency = 30;

        [Header("Physical Notch")]
        [SerializeField] private float notchOffsetRatio = 0.55f; // Asymmetric key

        public string MemoryType => memoryType;
        public int CapacityGb => capacityGb;
        public int SpeedMhz => speedMhz;
        public int CasLatency => casLatency;
        public float NotchOffsetRatio => notchOffsetRatio;

        public void SetRuntimeValues(string id, string model, string mfg, string type, int capacity)
        {
            InitializeBase(id, model, mfg, ComponentCategory.RAM);
            memoryType = type;
            capacityGb = capacity;
        }
    }
}
