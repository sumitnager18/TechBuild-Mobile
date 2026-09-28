using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "Storage_Data", menuName = "PC Technician/Hardware/Storage Data")]
    public class StorageData : HardwareData
    {
        [Header("Storage Specs")]
        [SerializeField] private string formFactor = "M2_2280";
        [SerializeField] private string interfaceProtocol = "NVMe_PCIe4";
        [SerializeField] private int capacityGb = 2000;
        [SerializeField] private int readSpeedMbS = 7000;
        [SerializeField] private int writeSpeedMbS = 6500;

        public string FormFactor => formFactor;
        public string InterfaceProtocol => interfaceProtocol;
        public int CapacityGb => capacityGb;
        public int ReadSpeedMbS => readSpeedMbS;
        public int WriteSpeedMbS => writeSpeedMbS;

        public void SetRuntimeValues(string id, string model, string mfg, string form, int capacity)
        {
            InitializeBase(id, model, mfg, ComponentCategory.Storage);
            formFactor = form;
            capacityGb = capacity;
        }
    }
}
