using System;
using System.Collections.Generic;

namespace PCTechnician.Simulation
{
    /// <summary>
    /// Pure C# serializable data snapshot of a PC build.
    /// Completely decoupled from Unity GameObjects, instances, and scene memory addresses.
    /// </summary>
    [Serializable]
    public class SimulationSnapshot
    {
        public int Version { get; set; } = 1;
        public long Timestamp { get; set; }

        // Component Identifiers
        public string MotherboardId { get; set; }
        public string CpuId { get; set; }
        public string CoolerId { get; set; }
        public string GpuId { get; set; }
        public string PsuId { get; set; }
        public string StorageId { get; set; }
        public List<string> RamModuleIds { get; set; } = new List<string>();

        // Physical Assembly States
        public bool SidePanelOpen { get; set; }
        public bool CpuSocketLatched { get; set; }
        public float ThermalPasteAmount { get; set; }
        public int CoolerScrewsTightened { get; set; }
        public bool CoolerFanConnected { get; set; }
        public bool RamLatched { get; set; }
        public bool GpuPcieLatched { get; set; }
        public bool GpuBracketScrewed { get; set; }
        public bool StorageM2Screwed { get; set; }

        // Power Cable Connections (Connected Rails)
        public List<string> ConnectedPowerRails { get; set; } = new List<string>();

        // Runtime Power Switch State
        public bool PowerSwitchOn { get; set; }
    }
}
