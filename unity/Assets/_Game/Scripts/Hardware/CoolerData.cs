using UnityEngine;

namespace PCTechnician.Hardware
{
    [CreateAssetMenu(fileName = "Cooler_Data", menuName = "PC Technician/Hardware/Cooler Data")]
    public class CoolerData : HardwareData
    {
        [Header("Mounting & Compatibility")]
        [SerializeField] private string supportedSocket = "AM5_FICTIONAL";
        [SerializeField] private int maxTdpDissipationWatts = 180;
        [SerializeField] private int mountingScrewCount = 4;
        [SerializeField] private bool hasPreappliedPaste = false;
        [SerializeField] private string fanHeaderType = "PWM_4Pin";

        public string SupportedSocket => supportedSocket;
        public int MaxTdpDissipationWatts => maxTdpDissipationWatts;
        public int MountingScrewCount => mountingScrewCount;
        public bool HasPreappliedPaste => hasPreappliedPaste;
        public string FanHeaderType => fanHeaderType;

        public void SetRuntimeValues(string id, string model, string mfg, string socket, int maxTdp)
        {
            InitializeBase(id, model, mfg, ComponentCategory.Cooler);
            supportedSocket = socket;
            maxTdpDissipationWatts = maxTdp;
        }
    }
}
