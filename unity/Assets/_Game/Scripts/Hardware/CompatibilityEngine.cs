using System;

namespace PCTechnician.Hardware
{
    /// <summary>
    /// Centralized hardware compatibility engine.
    /// Produces structured CompatibilityResult objects without scattering checks across scripts.
    /// </summary>
    public static class CompatibilityEngine
    {
        public const string ERROR_CPU_SOCKET_MISMATCH = "CPU_SOCKET_MISMATCH";
        public const string ERROR_RAM_TYPE_MISMATCH = "RAM_TYPE_MISMATCH";
        public const string ERROR_GPU_INTERFACE_MISMATCH = "GPU_INTERFACE_MISMATCH";
        public const string ERROR_STORAGE_INTERFACE_MISMATCH = "STORAGE_INTERFACE_MISMATCH";
        public const string ERROR_COOLER_SOCKET_MISMATCH = "COOLER_SOCKET_MISMATCH";
        public const string ERROR_PSU_INSUFFICIENT_POWER = "PSU_INSUFFICIENT_POWER";
        public const string ERROR_PSU_MISSING_CONNECTORS = "PSU_MISSING_CONNECTORS";

        public static CompatibilityResult ValidateCPUWithMotherboard(CPUData cpu, MotherboardData mobo)
        {
            if (cpu == null || mobo == null)
            {
                return CompatibilityResult.Fail("NULL_REFERENCE", "CPU or Motherboard specification is null.");
            }

            if (!string.Equals(cpu.SocketType, mobo.SocketType, StringComparison.OrdinalIgnoreCase))
            {
                return CompatibilityResult.Fail(
                    ERROR_CPU_SOCKET_MISMATCH,
                    $"CPU socket '{cpu.SocketType}' does not match motherboard socket '{mobo.SocketType}'."
                );
            }

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidateRAMWithMotherboard(RAMData ram, MotherboardData mobo)
        {
            if (ram == null || mobo == null)
            {
                return CompatibilityResult.Fail("NULL_REFERENCE", "RAM or Motherboard specification is null.");
            }

            if (!string.Equals(ram.MemoryType, mobo.MemoryType, StringComparison.OrdinalIgnoreCase))
            {
                return CompatibilityResult.Fail(
                    ERROR_RAM_TYPE_MISMATCH,
                    $"RAM generation '{ram.MemoryType}' is incompatible with motherboard slot generation '{mobo.MemoryType}'."
                );
            }

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidateGPUWithMotherboard(GPUData gpu, MotherboardData mobo)
        {
            if (gpu == null || mobo == null)
            {
                return CompatibilityResult.Fail("NULL_REFERENCE", "GPU or Motherboard specification is null.");
            }

            if (mobo.PCIe16SlotCount < 1)
            {
                return CompatibilityResult.Fail(
                    ERROR_GPU_INTERFACE_MISMATCH,
                    "Motherboard has no available PCIe x16 slots."
                );
            }

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidateStorageWithMotherboard(StorageData storage, MotherboardData mobo)
        {
            if (storage == null || mobo == null)
            {
                return CompatibilityResult.Fail("NULL_REFERENCE", "Storage or Motherboard specification is null.");
            }

            if (mobo.M2SlotCount < 1)
            {
                return CompatibilityResult.Fail(
                    ERROR_STORAGE_INTERFACE_MISMATCH,
                    "Motherboard does not possess an available M.2 slot."
                );
            }

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidateCoolerWithCPU(CoolerData cooler, CPUData cpu)
        {
            if (cooler == null || cpu == null)
            {
                return CompatibilityResult.Fail("NULL_REFERENCE", "Cooler or CPU specification is null.");
            }

            if (!string.Equals(cooler.SupportedSocket, cpu.SocketType, StringComparison.OrdinalIgnoreCase))
            {
                return CompatibilityResult.Fail(
                    ERROR_COOLER_SOCKET_MISMATCH,
                    $"Cooler mounting bracket '{cooler.SupportedSocket}' does not fit CPU socket '{cpu.SocketType}'."
                );
            }

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidatePSUCapacity(PSUData psu, int totalTdpWatts, int requiredPcieConnectors)
        {
            if (psu == null)
            {
                return CompatibilityResult.Fail("NULL_REFERENCE", "PSU specification is null.");
            }

            if (psu.Wattage < totalTdpWatts)
            {
                return CompatibilityResult.Fail(
                    ERROR_PSU_INSUFFICIENT_POWER,
                    $"PSU wattage ({psu.Wattage}W) is insufficient for estimated system load ({totalTdpWatts}W)."
                );
            }

            if (psu.Pcie8PinCount < requiredPcieConnectors)
            {
                return CompatibilityResult.Fail(
                    ERROR_PSU_MISSING_CONNECTORS,
                    $"PSU does not provide enough PCIe power cables ({psu.Pcie8PinCount} available, {requiredPcieConnectors} required)."
                );
            }

            return CompatibilityResult.Success();
        }
    }
}
