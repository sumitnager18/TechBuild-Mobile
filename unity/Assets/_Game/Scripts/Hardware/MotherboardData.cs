using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "Motherboard_Data", menuName = "PC Technician/Hardware/Motherboard Data")]
    public class MotherboardData : HardwareData
    {
        [Header("Form Factor & Socket")]
        [SerializeField] private string formFactor = "ATX";
        [SerializeField] private string socketType = "AM5_FICTIONAL";
        [SerializeField] private string chipset = "A870";

        [Header("Memory Slots")]
        [SerializeField] private string memoryType = "DDR5";
        [SerializeField] private int dimmSlotCount = 4;
        [SerializeField] private int maxMemoryCapacityGb = 192;

        [Header("Expansion Slots")]
        [SerializeField] private int pcie16SlotCount = 1;
        [SerializeField] private int m2SlotCount = 2;

        [Header("Power Inputs")]
        [SerializeField] private bool requires24Pin = true;
        [SerializeField] private bool requiresCpuEps8Pin = true;

        public string FormFactor => formFactor;
        public string SocketType => socketType;
        public string Chipset => chipset;
        public string MemoryType => memoryType;
        public int DimmSlotCount => dimmSlotCount;
        public int MaxMemoryCapacityGb => maxMemoryCapacityGb;
        public int PCIe16SlotCount => pcie16SlotCount;
        public int M2SlotCount => m2SlotCount;
        public bool Requires24Pin => requires24Pin;
        public bool RequiresCpuEps8Pin => requiresCpuEps8Pin;

        public void SetRuntimeValues(string id, string model, string mfg, string socket, string ramType)
        {
            InitializeBase(id, model, mfg, ComponentCategory.Motherboard);
            socketType = socket;
            memoryType = ramType;
        }
    }
}
