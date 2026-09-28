using PCTechnician.Assembly;
using PCTechnician.Hardware;
using UnityEngine;

namespace PCTechnician.Tests
{
    /// <summary>
    /// Automated test runner for core simulation logic:
    /// - CompatibilityEngine rules (CPU socket, RAM type, GPU, Cooler, PSU)
    /// - ComponentSlot lifecycle & prerequisites
    /// Can be executed in Unity Test Framework or standalone runtime bootstrap.
    /// </summary>
    public static class CompatibilityAndSlotTests
    {
        public struct TestRunSummary
        {
            public int TotalTests;
            public int PassedTests;
            public int FailedTests;
            public string[] LogMessages;
        }

        public static TestRunSummary RunAllTests()
        {
            var logs = new System.Collections.Generic.List<string>();
            int passed = 0;
            int failed = 0;

            void Assert(bool condition, string testName)
            {
                if (condition)
                {
                    passed++;
                    logs.Add($"[PASS] {testName}");
                }
                else
                {
                    failed++;
                    logs.Add($"[FAIL] {testName}");
                }
            }

            // Test 1: Compatible CPU Socket Validation
            var mobo = ScriptableObject.CreateInstance<MotherboardData>();
            mobo.SetRuntimeValues("mobo_01", "ApexForge A870-M", "ApexForge", "AM5_FICTIONAL", "DDR5");

            var cpuCompatible = ScriptableObject.CreateInstance<CPUData>();
            cpuCompatible.SetRuntimeValues("cpu_01", "NovaCore N7-7600", "NovaCore", "AM5_FICTIONAL", 105);

            var cpuResult = CompatibilityEngine.ValidateCPUWithMotherboard(cpuCompatible, mobo);
            Assert(cpuResult.IsCompatible, "Compatible CPU socket (AM5_FICTIONAL) is accepted by motherboard.");

            // Test 2: Incompatible CPU Socket Rejection
            var cpuIncompatible = ScriptableObject.CreateInstance<CPUData>();
            cpuIncompatible.SetRuntimeValues("cpu_02", "LegacyCore L-3000", "LegacyCore", "LGA1200_FICTIONAL", 65);

            var cpuMismatchResult = CompatibilityEngine.ValidateCPUWithMotherboard(cpuIncompatible, mobo);
            Assert(!cpuMismatchResult.IsCompatible && cpuMismatchResult.ErrorCode == CompatibilityEngine.ERROR_CPU_SOCKET_MISMATCH,
                "Incompatible CPU socket (LGA1200_FICTIONAL) is rejected with ERROR_CPU_SOCKET_MISMATCH.");

            // Test 3: RAM Generation Match
            var ramDdr5 = ScriptableObject.CreateInstance<RAMData>();
            ramDdr5.SetRuntimeValues("ram_01", "TitanRAM DDR5-32", "TitanRAM", "DDR5", 32);

            var ramResult = CompatibilityEngine.ValidateRAMWithMotherboard(ramDdr5, mobo);
            Assert(ramResult.IsCompatible, "Compatible DDR5 RAM is accepted by DDR5 motherboard.");

            // Test 4: RAM Generation Mismatch
            var ramDdr4 = ScriptableObject.CreateInstance<RAMData>();
            ramDdr4.SetRuntimeValues("ram_02", "TitanRAM DDR4-16", "TitanRAM", "DDR4", 16);

            var ramMismatchResult = CompatibilityEngine.ValidateRAMWithMotherboard(ramDdr4, mobo);
            Assert(!ramMismatchResult.IsCompatible && ramMismatchResult.ErrorCode == CompatibilityEngine.ERROR_RAM_TYPE_MISMATCH,
                "Incompatible DDR4 RAM is rejected with ERROR_RAM_TYPE_MISMATCH.");

            // Test 5: GPU PCIe generation and x16 interface
            var gpuGen4 = ScriptableObject.CreateInstance<GPUData>();
            gpuGen4.SetRuntimeValues("gpu_04", "VectorX VX-680", "VectorX", 180, 1, 260, "Gen 4.0", "PCIe_16x");
            var gpuResult = CompatibilityEngine.ValidateGPUWithMotherboard(gpuGen4, mobo);
            Assert(gpuResult.IsCompatible, "PCIe Gen4 x16 GPU is accepted by a Gen5 x16 motherboard.");

            var gpuGen6 = ScriptableObject.CreateInstance<GPUData>();
            gpuGen6.SetRuntimeValues("gpu_06", "VectorX VX-980", "VectorX", 300, 2, 420, "Gen 6.0", "PCIe_16x");
            var gpuGenerationMismatch = CompatibilityEngine.ValidateGPUWithMotherboard(gpuGen6, mobo);
            Assert(!gpuGenerationMismatch.IsCompatible &&
                gpuGenerationMismatch.ErrorCode == CompatibilityEngine.ERROR_GPU_PCIE_GENERATION_MISMATCH,
                "GPU generation beyond the motherboard slot generation is rejected.");

            // Test 6: Storage form factor and protocol
            var nvmeGen4 = ScriptableObject.CreateInstance<StorageData>();
            nvmeGen4.SetRuntimeValues("ssd_04", "HyperDrive 2TB", "HyperDrive", "M2_2280", 2000, "NVMe_PCIe4");
            var storageResult = CompatibilityEngine.ValidateStorageWithMotherboard(nvmeGen4, mobo);
            Assert(storageResult.IsCompatible, "M.2 2280 NVMe Gen4 storage is accepted by a Gen5 M.2 slot.");

            var sataM2 = ScriptableObject.CreateInstance<StorageData>();
            sataM2.SetRuntimeValues("ssd_sata", "Legacy M2 SATA", "Legacy", "M2_2280", 1000, "SATA");
            var storageMismatch = CompatibilityEngine.ValidateStorageWithMotherboard(sataM2, mobo);
            Assert(!storageMismatch.IsCompatible &&
                storageMismatch.ErrorCode == CompatibilityEngine.ERROR_STORAGE_INTERFACE_MISMATCH,
                "Unsupported M.2 storage protocol is rejected.");

            // Test 7: Cooler Socket Fit
            var cooler = ScriptableObject.CreateInstance<CoolerData>();
            cooler.SetRuntimeValues("cooler_01", "ArcticBreeze A-4", "ArcticBreeze", "AM5_FICTIONAL", 180);

            var coolerResult = CompatibilityEngine.ValidateCoolerWithCPU(cooler, cpuCompatible);
            Assert(coolerResult.IsCompatible, "AM5_FICTIONAL cooler bracket matches AM5_FICTIONAL CPU.");

            // Test 8: PSU Power Capacity Check
            var psu = ScriptableObject.CreateInstance<PSUData>();
            psu.SetRuntimeValues("psu_01", "VoltEdge 750 Bronze", "VoltEdge", 750, "80_Plus_Bronze");

            var psuResult = CompatibilityEngine.ValidatePSUCapacity(psu, 450, 2);
            Assert(psuResult.IsCompatible, "750W PSU accepts 450W total system load.");

            var psuOverloadResult = CompatibilityEngine.ValidatePSUCapacity(psu, 850, 2);
            Assert(!psuOverloadResult.IsCompatible && psuOverloadResult.ErrorCode == CompatibilityEngine.ERROR_PSU_INSUFFICIENT_POWER,
                "750W PSU rejects 850W total system load.");

            // Test 9: ComponentSlot Acceptance and Occupancy
            var slotGameObject = new GameObject("TestSlot");
            var slot = slotGameObject.AddComponent<ComponentSlot>();
            slot.Configure("cpu_socket_01", "CPU Socket", ComponentCategory.CPU);

            bool canAcceptCategoryMatch = slot.CanAccept(cpuCompatible, out string reason);
            Assert(canAcceptCategoryMatch, "ComponentSlot accepts valid hardware category (CPU).");

            bool canAcceptCategoryMismatch = slot.CanAccept(ramDdr5, out string mismatchReason);
            Assert(!canAcceptCategoryMismatch, "ComponentSlot rejects incompatible category (RAM into CPU slot).");

            // Install and test occupancy lock
            slot.Install(cpuCompatible, out _);
            Assert(slot.IsOccupied && slot.CurrentState == ComponentState.Locked, "Slot is marked occupied with state Locked.");

            bool rejectSecondInstall = slot.CanAccept(cpuCompatible, out string occupiedReason);
            Assert(!rejectSecondInstall, "Occupied ComponentSlot rejects duplicate component installation.");

            Object.DestroyImmediate(slotGameObject);

            return new TestRunSummary
            {
                TotalTests = passed + failed,
                PassedTests = passed,
                FailedTests = failed,
                LogMessages = logs.ToArray()
            };
        }
    }
}
