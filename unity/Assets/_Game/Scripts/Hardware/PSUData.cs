using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "PSU_Data", menuName = "PC Technician/Hardware/PSU Data")]
    public class PSUData : HardwareData
    {
        [Header("Power Rating")]
        [SerializeField] private int wattage = 750;
        [SerializeField] private int rail12VCapacityWatts = 720;
        [SerializeField] private float transientExcursionPercentage = 120f;
        [SerializeField] private string efficiencyRating = "80_Plus_Bronze";
        [SerializeField] private bool isModular = true;

        [Header("Available Connectors")]
        [SerializeField] private int atx24PinCount = 1;
        [SerializeField] private int cpuEps8PinCount = 2;
        [SerializeField] private int pcie8PinCount = 4;
        [SerializeField] private int sataPowerCount = 6;

        public int Wattage => wattage;
        public int Rail12VCapacityWatts => rail12VCapacityWatts;
        public float TransientExcursionPercentage => transientExcursionPercentage;
        public string EfficiencyRating => efficiencyRating;
        public bool IsModular => isModular;
        public int Atx24PinCount => atx24PinCount;
        public int CpuEps8PinCount => cpuEps8PinCount;
        public int Pcie8PinCount => pcie8PinCount;
        public int SataPowerCount => sataPowerCount;

        public void SetRuntimeValues(string id, string model, string mfg, int watts, string rating, int rail12V = 720)
        {
            InitializeBase(id, model, mfg, ComponentCategory.PSU);
            wattage = watts;
            efficiencyRating = rating;
            rail12VCapacityWatts = rail12V;
        }
    }
}
