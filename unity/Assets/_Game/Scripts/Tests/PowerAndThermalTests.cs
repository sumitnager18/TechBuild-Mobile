using System;
using System.Collections.Generic;
using PCTechnician.Diagnostics;
using PCTechnician.Hardware;
using PCTechnician.Power;
using PCTechnician.Simulation;
using PCTechnician.Thermal;

namespace PCTechnician.Tests
{
    /// <summary>
    /// Automated test suite validating Stage 2 Power Graph, Thermal Model, and Determinism.
    /// Pure C# test suite executing without UnityEngine GameObject or scene dependencies.
    /// </summary>
    public static class PowerAndThermalTests
    {
        public struct TestRunSummary
        {
            public int TotalTests;
            public int PassedTests;
            public int FailedTests;
            public string[] LogMessages;
        }

        public static TestRunSummary RunAllStage2Tests()
        {
            var logs = new List<string>();
            int passed = 0;
            int failed = 0;

            void Assert(bool condition, string testId, string testName)
            {
                if (condition)
                {
                    passed++;
                    logs.Add($"[PASS] {testId}: {testName}");
                }
                else
                {
                    failed++;
                    logs.Add($"[FAIL] {testId}: {testName}");
                }
            }

            // ==========================================
            // POWER TESTS (TC-11 to TC-15)
            // ==========================================

            // TC-11: Valid 24-pin connection
            var powerGraph1 = new PowerGraph();
            powerGraph1.AddNode(new PowerNode("Mobo_24Pin", "PSU", "Motherboard", PowerConnectorType.ATX24Pin, 300, 75));
            bool connect24Ok = powerGraph1.ConnectRail("Mobo_24Pin", PowerConnectorType.ATX24Pin, out string err11);
            Assert(connect24Ok && powerGraph1.IsRailConnected("Mobo_24Pin"), "TC-11", "Valid ATX 24-Pin connector connects successfully.");

            // TC-12: Invalid connector rejected
            var powerGraph2 = new PowerGraph();
            powerGraph2.AddNode(new PowerNode("GPU_PCIe", "PSU", "GPU", PowerConnectorType.PCIe8Pin, 300, 220));
            bool invalidConnect = powerGraph2.ConnectRail("GPU_PCIe", PowerConnectorType.CPUEPS8Pin, out string err12);
            Assert(!invalidConnect && !string.IsNullOrEmpty(err12), "TC-12", "CPUEPS8Pin into PCIe8Pin port is rejected with error.");

            // TC-13: Missing CPU EPS produces CPU power fault
            var snapshot13 = new SimulationSnapshot
            {
                CpuId = "NovaCore_N7_7600",
                MotherboardId = "ApexForge_A870",
                CoolerId = "ArcticBreeze_A4",
                CpuSocketLatched = true,
                RamModuleIds = new List<string> { "TitanRAM_32" },
                RamLatched = true,
                ConnectedPowerRails = new List<string> { SimulationEvaluator.RAIL_24PIN } // Missing CPU EPS!
            };
            var eval13 = SimulationEvaluator.Evaluate(snapshot13, null, null, null, null, null, null, null);
            bool hasCpuPowerFault = eval13.DiagnosticFaults.Exists(f => f.FaultCode == DiagnosticFaultCode.CPU_POWER_MISSING);
            Assert(hasCpuPowerFault && !eval13.CanPostSucceed, "TC-13", "Missing CPU EPS rail causes CPU_POWER_MISSING fault and POST failure.");

            // TC-14: Missing GPU power produces GPU power fault
            var gpuData14 = UnityEngine.ScriptableObject.CreateInstance<GPUData>();
            gpuData14.SetRuntimeValues("gpu_01", "VectorX VX-780", "VectorX", 220, 1);
            var snapshot14 = new SimulationSnapshot
            {
                CpuId = "NovaCore_N7_7600",
                GpuId = "VectorX_VX_780",
                CpuSocketLatched = true,
                CoolerId = "ArcticBreeze_A4",
                RamModuleIds = new List<string> { "TitanRAM_32" },
                RamLatched = true,
                ConnectedPowerRails = new List<string> { SimulationEvaluator.RAIL_24PIN, SimulationEvaluator.RAIL_CPU_EPS } // Missing PCIe!
            };
            var eval14 = SimulationEvaluator.Evaluate(snapshot14, null, null, null, gpuData14, null, null, null);
            bool hasGpuPowerFault = eval14.DiagnosticFaults.Exists(f => f.FaultCode == DiagnosticFaultCode.GPU_POWER_MISSING);
            Assert(hasGpuPowerFault && !eval14.IsGpuPowered, "TC-14", "Missing PCIe power to dedicated GPU causes GPU_POWER_MISSING fault.");

            // TC-15: Insufficient PSU produces insufficient-power status
            var psuWeak = UnityEngine.ScriptableObject.CreateInstance<PSUData>();
            psuWeak.SetRuntimeValues("psu_weak", "VoltEdge 300", "VoltEdge", 300, "Standard");
            var eval15 = SimulationEvaluator.Evaluate(snapshot14, null, null, null, gpuData14, null, psuWeak, null);
            bool isPsuInsufficient = eval15.PSUStatusResult.Status == PSUStatus.Insufficient;
            Assert(isPsuInsufficient, "TC-15", "300W PSU evaluated as Insufficient for 385W system load.");

            // ==========================================
            // THERMAL TESTS (TC-16 to TC-20)
            // ==========================================

            // TC-16: Correct cooler produces safe temperature
            var thermal16 = ThermalModel.CalculateCPUThermals(
                cpuPowerWatts: 105,
                coolerInstalled: true,
                coolerTdpRating: 180,
                coolerScrewsTightened: 4,
                totalScrewsRequired: 4,
                fanConnected: true,
                pasteAmount: 0.55f, // optimal
                caseAirflowFactor: 1.0f,
                ambientTemp: 22.0f
            );
            Assert(thermal16.State == ThermalState.Normal || thermal16.State == ThermalState.Warm, "TC-16",
                $"Fully mounted cooler with optimal paste yields healthy temperature ({thermal16.TemperatureCelsius:F1}°C).");

            // TC-17: No cooler produces critical thermal state
            var thermal17 = ThermalModel.CalculateCPUThermals(
                cpuPowerWatts: 105,
                coolerInstalled: false, // NO COOLER!
                coolerTdpRating: 0,
                coolerScrewsTightened: 0,
                totalScrewsRequired: 4,
                fanConnected: false,
                pasteAmount: 0f
            );
            Assert(thermal17.State == ThermalState.Critical && thermal17.IsEmergencyShutdown, "TC-17",
                "Operating CPU without cooler immediately triggers Critical Emergency Shutdown.");

            // TC-18: Poor thermal paste produces worse temperature than optimal paste
            var thermal18Poor = ThermalModel.CalculateCPUThermals(105, true, 180, 4, 4, true, 0.15f); // insufficient paste
            Assert(thermal18Poor.TemperatureCelsius > thermal16.TemperatureCelsius + 10f, "TC-18",
                $"Poor paste ({thermal18Poor.TemperatureCelsius:F1}°C) runs >10°C hotter than optimal paste ({thermal16.TemperatureCelsius:F1}°C).");

            // TC-19: Excessive thermal paste produces degraded thermal performance
            var thermal19Excess = ThermalModel.CalculateCPUThermals(105, true, 180, 4, 4, true, 1.35f); // excessive paste
            Assert(thermal19Excess.TemperatureCelsius > thermal16.TemperatureCelsius + 4f, "TC-19",
                $"Excessive paste thickness degrades conductance ({thermal19Excess.TemperatureCelsius:F1}°C vs {thermal16.TemperatureCelsius:F1}°C).");

            // TC-20: High CPU load increases temperature
            var thermal20HighLoad = ThermalModel.CalculateCPUThermals(170, true, 180, 4, 4, true, 0.55f);
            Assert(thermal20HighLoad.TemperatureCelsius > thermal16.TemperatureCelsius, "TC-20",
                $"170W CPU load ({thermal20HighLoad.TemperatureCelsius:F1}°C) runs hotter than 105W load ({thermal16.TemperatureCelsius:F1}°C).");

            // ==========================================
            // DETERMINISM TESTS (TC-21 & TC-22)
            // ==========================================

            // TC-21: Identical simulation state produces identical evaluation
            var eval21A = SimulationEvaluator.Evaluate(snapshot14, null, null, null, gpuData14, null, psuWeak, null);
            var eval21B = SimulationEvaluator.Evaluate(snapshot14, null, null, null, gpuData14, null, psuWeak, null);
            bool determinismMatches = eval21A.CanPostSucceed == eval21B.CanPostSucceed
                && eval21A.EstimatedLoad.ContinuousLoadWatts == eval21B.EstimatedLoad.ContinuousLoadWatts
                && eval21A.PSUStatusResult.Status == eval21B.PSUStatusResult.Status;
            Assert(determinismMatches, "TC-21", "Identical simulation inputs produce identical deterministic output.");

            // TC-22: Save/restore produces identical evaluation
            string jsonSnapshot = UnityEngine.JsonUtility.ToJson(snapshot14);
            var rehydrated = UnityEngine.JsonUtility.FromJson<SimulationSnapshot>(jsonSnapshot);
            var eval22 = SimulationEvaluator.Evaluate(rehydrated, null, null, null, gpuData14, null, psuWeak, null);
            bool rehydrateMatches = eval22.CanPostSucceed == eval21A.CanPostSucceed 
                && eval22.DiagnosticFaults.Count == eval21A.DiagnosticFaults.Count;
            Assert(rehydrateMatches, "TC-22", "Rehydrated JSON snapshot produces identical evaluation results.");

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
