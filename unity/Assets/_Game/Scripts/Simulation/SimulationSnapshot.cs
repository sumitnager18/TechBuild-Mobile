using System;
using System.Collections.Generic;
using PCTechnician.Power;

namespace PCTechnician.Simulation
{
    [Serializable]
    public class SimulationSnapshot
    {
        public const int CURRENT_VERSION = 2;

        public int Version = CURRENT_VERSION;
        public long Timestamp;

        public string MotherboardId;
        public string CpuId;
        public string CoolerId;
        public string GpuId;
        public string PsuId;
        public string StorageId;
        public List<string> RamModuleIds = new List<string>();

        public bool SidePanelOpen;
        public bool CpuSocketLatched;
        public float ThermalPasteAmount;
        public int CoolerScrewsTightened;
        public bool CoolerFanConnected;
        public bool RamLatched;
        public bool GpuPcieLatched;
        public bool GpuBracketScrewed;
        public bool StorageM2Screwed;

        // Legacy rail IDs retained for backwards compatibility.
        public List<string> ConnectedPowerRails = new List<string>();

        // Preferred v2 representation: exact PSU source connector identity.
        public List<PowerConnectionState> ConnectedPowerConnections = new List<PowerConnectionState>();

        public bool PowerSwitchOn;
    }
}