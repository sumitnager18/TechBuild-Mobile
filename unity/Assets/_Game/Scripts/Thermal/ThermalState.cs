namespace PCTechnician.Thermal
{
    public enum ThermalState
    {
        Cold,       // < 35°C (Idle / room temp)
        Normal,     // 35°C – 65°C (Healthy operating range)
        Warm,       // 65°C – 78°C (Moderate gaming / workload)
        Hot,        // 78°C – 88°C (High sustained load)
        Throttling, // 88°C – 98°C (Clock throttling, performance reduced)
        Critical    // > 98°C (Emergency thermal trip / shutdown)
    }
}
