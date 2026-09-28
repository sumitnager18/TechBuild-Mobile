namespace PCTechnician.Cameras
{
    public enum FocusZoneType
    {
        Workshop,       // Overview of workbench and entire chassis
        Assembly,       // Working distance side view of opened case
        CPUSocket,      // LGA socket, retention arm, load plate closeup
        RAMSlots,       // DIMM slots closeup (A2/B2 priority)
        PCIeSlot,       // Expansion slot & GPU area
        M2Slot,         // M.2 standoff and NVMe drive slot
        PowerArea,      // Motherboard ATX 24-Pin and CPU EPS area
        PSUBasement     // Power supply shroud and modular cable routing
    }
}
