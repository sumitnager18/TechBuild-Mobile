using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "Case_Data", menuName = "PC Technician/Hardware/Case Data")]
    public class CaseData : HardwareData
    {
        [Header("Chassis Specs")]
        [SerializeField] private string supportedMotherboardFormFactor = "ATX";
        [SerializeField] private float maxGpuLengthMm = 360f;
        [SerializeField] private float maxCpuCoolerHeightMm = 165f;
        [SerializeField] private int expansionSlotCount = 7;
        [SerializeField] private bool hasSideGlassPanel = true;

        public string SupportedMotherboardFormFactor => supportedMotherboardFormFactor;
        public float MaxGpuLengthMm => maxGpuLengthMm;
        public float MaxCpuCoolerHeightMm => maxCpuCoolerHeightMm;
        public int ExpansionSlotCount => expansionSlotCount;
        public bool HasSideGlassPanel => hasSideGlassPanel;

        public void SetRuntimeValues(string id, string model, string mfg, string formFactor)
        {
            InitializeBase(id, model, mfg, ComponentCategory.Chassis);
            supportedMotherboardFormFactor = formFactor;
        }
    }
}
