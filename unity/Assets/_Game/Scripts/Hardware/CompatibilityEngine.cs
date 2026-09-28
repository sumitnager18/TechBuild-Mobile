using System;

namespace PCTechnician.Hardware
{
    public static class CompatibilityEngine
    {
        public const string ERROR_CPU_SOCKET_MISMATCH = "CPU_SOCKET_MISMATCH";
        public const string ERROR_RAM_TYPE_MISMATCH = "RAM_TYPE_MISMATCH";
        public const string ERROR_GPU_INTERFACE_MISMATCH = "GPU_INTERFACE_MISMATCH";
        public const string ERROR_GPU_PCIE_GENERATION_MISMATCH = "GPU_PCIE_GENERATION_MISMATCH";
        public const string ERROR_STORAGE_INTERFACE_MISMATCH = "STORAGE_INTERFACE_MISMATCH";
        public const string ERROR_STORAGE_FORM_FACTOR_MISMATCH = "STORAGE_FORM_FACTOR_MISMATCH";
        public const string ERROR_COOLER_SOCKET_MISMATCH = "COOLER_SOCKET_MISMATCH";
        public const string ERROR_PSU_INSUFFICIENT_POWER = "PSU_INSUFFICIENT_POWER";
        public const string ERROR_PSU_MISSING_CONNECTORS = "PSU_MISSING_CONNECTORS";

        public static CompatibilityResult ValidateCPUWithMotherboard(CPUData cpu, MotherboardData mobo)
        {
            if (cpu == null || mobo == null)
                return CompatibilityResult.Fail("NULL_REFERENCE", "CPU or Motherboard specification is null.");

            if (!string.Equals(cpu.SocketType, mobo.SocketType, StringComparison.OrdinalIgnoreCase))
                return CompatibilityResult.Fail(ERROR_CPU_SOCKET_MISMATCH,
                    $"CPU socket '{cpu.SocketType}' does not match motherboard socket '{mobo.SocketType}'.");

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidateRAMWithMotherboard(RAMData ram, MotherboardData mobo)
        {
            if (ram == null || mobo == null)
                return CompatibilityResult.Fail("NULL_REFERENCE", "RAM or Motherboard specification is null.");

            if (!string.Equals(ram.MemoryType, mobo.MemoryType, StringComparison.OrdinalIgnoreCase))
                return CompatibilityResult.Fail(ERROR_RAM_TYPE_MISMATCH,
                    $"RAM generation '{ram.MemoryType}' is incompatible with motherboard slot generation '{mobo.MemoryType}'.");

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidateGPUWithMotherboard(GPUData gpu, MotherboardData mobo)
        {
            if (gpu == null || mobo == null)
                return CompatibilityResult.Fail("NULL_REFERENCE", "GPU or Motherboard specification is null.");

            if (!string.Equals(gpu.SlotInterface, "PCIe_16x", StringComparison.OrdinalIgnoreCase) ||
                mobo.PCIe16SlotCount < 1)
                return CompatibilityResult.Fail(ERROR_GPU_INTERFACE_MISMATCH,
                    "GPU requires a PCIe x16 slot, but the motherboard does not provide one.");

            if (ComparePcieGeneration(gpu.PcieGeneration, mobo.PcieX16Generation) > 0)
                return CompatibilityResult.Fail(ERROR_GPU_PCIE_GENERATION_MISMATCH,
                    $"GPU PCIe generation '{gpu.PcieGeneration}' exceeds the motherboard slot generation '{mobo.PcieX16Generation}'.");

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidateStorageWithMotherboard(StorageData storage, MotherboardData mobo)
        {
            if (storage == null || mobo == null)
                return CompatibilityResult.Fail("NULL_REFERENCE", "Storage or Motherboard specification is null.");

            if (mobo.M2SlotCount < 1)
                return CompatibilityResult.Fail(ERROR_STORAGE_INTERFACE_MISMATCH,
                    "Motherboard does not possess an available M.2 slot.");

            if (!string.Equals(storage.FormFactor, mobo.M2FormFactor, StringComparison.OrdinalIgnoreCase))
                return CompatibilityResult.Fail(ERROR_STORAGE_FORM_FACTOR_MISMATCH,
                    $"Storage form factor '{storage.FormFactor}' does not match motherboard M.2 support '{mobo.M2FormFactor}'.");

            if (!IsStorageProtocolCompatible(storage.InterfaceProtocol, mobo.M2InterfaceProtocol))
                return CompatibilityResult.Fail(ERROR_STORAGE_INTERFACE_MISMATCH,
                    $"Storage protocol '{storage.InterfaceProtocol}' is incompatible with motherboard M.2 protocol '{mobo.M2InterfaceProtocol}'.");

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidateCoolerWithCPU(CoolerData cooler, CPUData cpu)
        {
            if (cooler == null || cpu == null)
                return CompatibilityResult.Fail("NULL_REFERENCE", "Cooler or CPU specification is null.");

            if (!string.Equals(cooler.SupportedSocket, cpu.SocketType, StringComparison.OrdinalIgnoreCase))
                return CompatibilityResult.Fail(ERROR_COOLER_SOCKET_MISMATCH,
                    $"Cooler mounting bracket '{cooler.SupportedSocket}' does not fit CPU socket '{cpu.SocketType}'.");

            return CompatibilityResult.Success();
        }

        public static CompatibilityResult ValidatePSUCapacity(PSUData psu, int totalTdpWatts, int requiredPcieConnectors)
        {
            if (psu == null)
                return CompatibilityResult.Fail("NULL_REFERENCE", "PSU specification is null.");

            if (psu.Wattage < totalTdpWatts)
                return CompatibilityResult.Fail(ERROR_PSU_INSUFFICIENT_POWER,
                    $"PSU wattage ({psu.Wattage}W) is insufficient for estimated system load ({totalTdpWatts}W).");

            if (psu.Pcie8PinCount < requiredPcieConnectors)
                return CompatibilityResult.Fail(ERROR_PSU_MISSING_CONNECTORS,
                    $"PSU does not provide enough PCIe power cables ({psu.Pcie8PinCount} available, {requiredPcieConnectors} required).");

            return CompatibilityResult.Success();
        }

        private static int ComparePcieGeneration(string deviceGeneration, string slotGeneration)
        {
            return ParseGeneration(deviceGeneration).CompareTo(ParseGeneration(slotGeneration));
        }

        private static int ParseGeneration(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            var normalized = value.Trim().ToLowerInvariant().Replace("gen", "").Replace(" ", "");
            if (float.TryParse(normalized, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var number))
                return (int)Math.Round(number * 10.0);
            return 0;
        }

        private static bool IsStorageProtocolCompatible(string device, string motherboard)
        {
            if (string.Equals(device, motherboard, StringComparison.OrdinalIgnoreCase))
                return true;

            if (device.StartsWith("NVMe_PCIe", StringComparison.OrdinalIgnoreCase) &&
                motherboard.StartsWith("NVMe_PCIe", StringComparison.OrdinalIgnoreCase))
                return ComparePcieGeneration(device.Replace("NVMe_", ""), motherboard.Replace("NVMe_", "")) <= 0;

            return false;
        }
    }
}