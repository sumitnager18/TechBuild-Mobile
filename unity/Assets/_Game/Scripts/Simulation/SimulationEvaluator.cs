using System;
using System.Collections.Generic;
using PCTechnician.Diagnostics;
using PCTechnician.Hardware;
using PCTechnician.Power;
using PCTechnician.Thermal;

namespace PCTechnician.Simulation
{
    /// <summary>
    /// Centralized deterministic simulation evaluator.
    /// Operates purely on Hardware data models and SimulationSnapshot states.
    /// Independent of visual GameObjects, Animators, Cameras, or UI.
    /// </summary>
    public static class SimulationEvaluator
    {
        public const string RAIL_24PIN = "Rail_ATX_24Pin";
        public const string RAIL_CPU_EPS = "Rail_CPU_EPS";
        public const string RAIL_PCIE_GPU = "Rail_PCIe_GPU";

        public static SimulationEvaluationResult Evaluate(
            SimulationSnapshot snapshot,
            MotherboardData mobo,
            CPUData cpu,
            RAMData ram,
            GPUData gpu,
            CoolerData cooler,
            PSUData psu,
            StorageData storage)
        {
            var result = new SimulationEvaluationResult();

            // 1. Physical Assembly Validation
            bool hasMobo = mobo != null;
            bool hasCpu = cpu != null && !string.IsNullOrEmpty(snapshot.CpuId);
            bool hasRam = ram != null && snapshot.RamModuleIds != null && snapshot.RamModuleIds.Count > 0;
            bool hasGpu = gpu != null && !string.IsNullOrEmpty(snapshot.GpuId);
            bool hasCooler = cooler != null && !string.IsNullOrEmpty(snapshot.CoolerId);
            bool hasPsu = psu != null && !string.IsNullOrEmpty(snapshot.PsuId);

            if (!hasMobo)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                    DiagnosticFaultCode.NONE,
                    DiagnosticSubsystem.Motherboard,
                    "Motherboard is not installed in chassis.",
                    "MotherboardTray"
                ));
            }

            if (!hasCpu)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                    DiagnosticFaultCode.CPU_NOT_INSTALLED,
                    DiagnosticSubsystem.CPU,
                    "CPU is not installed in socket.",
                    "CPUSocket"
                ));
            }
            else if (!snapshot.CpuSocketLatched)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Error(
                    DiagnosticFaultCode.CPU_NOT_INSTALLED,
                    DiagnosticSubsystem.CPU,
                    "CPU socket retention arm is not locked.",
                    "CPUSocketLever"
                ));
            }

            if (!hasRam)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                    DiagnosticFaultCode.DRAM_NOT_SEATED,
                    DiagnosticSubsystem.Memory,
                    "No functional DDR memory installed in primary slots.",
                    "RAMSlot_A2"
                ));
            }
            else if (!snapshot.RamLatched)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Error(
                    DiagnosticFaultCode.DRAM_NOT_SEATED,
                    DiagnosticSubsystem.Memory,
                    "RAM module is not fully seated; retention clip is unlatched.",
                    "RAMSlot_A2"
                ));
            }

            // Cooler check
            if (!hasCooler)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                    DiagnosticFaultCode.COOLER_NOT_MOUNTED,
                    DiagnosticSubsystem.Thermal,
                    "CPU cooler is not mounted. System will overheat rapidly.",
                    "CPUCoolerMount"
                ));
            }
            else if (snapshot.CoolerScrewsTightened < cooler.MountingScrewCount)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Warning(
                    DiagnosticFaultCode.COOLER_LOOSE_SCREWS,
                    DiagnosticSubsystem.Thermal,
                    $"Cooler mounting screws are incomplete ({snapshot.CoolerScrewsTightened}/{cooler.MountingScrewCount} tightened).",
                    "CoolerScrews"
                ));
            }

            if (hasCooler && !snapshot.CoolerFanConnected)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Warning(
                    DiagnosticFaultCode.FAN_HEADER_DISCONNECTED,
                    DiagnosticSubsystem.Thermal,
                    "CPU cooler PWM fan header is disconnected.",
                    "CPU_FAN_Header"
                ));
            }

            result.IsAssembledCorrectly = result.DiagnosticFaults.Count == 0;

            // 2. Power Graph & Connector Validation
            var powerGraph = new PowerGraph();
            powerGraph.AddNode(new PowerNode(RAIL_24PIN, "PSU", "Motherboard", PowerConnectorType.ATX24Pin, 300, 75));
            powerGraph.AddNode(new PowerNode(RAIL_CPU_EPS, "PSU", "CPU_VRM", PowerConnectorType.CPUEPS8Pin, 280, cpu != null ? cpu.TDPWatts : 105));
            if (hasGpu)
            {
                powerGraph.AddNode(new PowerNode(RAIL_PCIE_GPU, "PSU", "GPU_VRM", PowerConnectorType.PCIe8Pin, 300, gpu.TDPWatts));
            }

            // Apply connections from snapshot
            if (snapshot.ConnectedPowerRails != null)
            {
                foreach (var rail in snapshot.ConnectedPowerRails)
                {
                    if (rail == RAIL_24PIN) powerGraph.ConnectRail(RAIL_24PIN, PowerConnectorType.ATX24Pin, out _);
                    if (rail == RAIL_CPU_EPS) powerGraph.ConnectRail(RAIL_CPU_EPS, PowerConnectorType.CPUEPS8Pin, out _);
                    if (rail == RAIL_PCIE_GPU) powerGraph.ConnectRail(RAIL_PCIE_GPU, PowerConnectorType.PCIe8Pin, out _);
                }
            }

            // Power Rail Invariants
            bool pwr24Connected = powerGraph.IsRailConnected(RAIL_24PIN);
            bool pwrCpuConnected = powerGraph.IsRailConnected(RAIL_CPU_EPS);
            bool pwrGpuConnected = !hasGpu || powerGraph.IsRailConnected(RAIL_PCIE_GPU);

            if (!pwr24Connected)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                    DiagnosticFaultCode.ATX_24PIN_DISCONNECTED,
                    DiagnosticSubsystem.Power,
                    "Main ATX 24-Pin power cable is disconnected. Motherboard cannot power on.",
                    "ATX_24Pin"
                ));
            }

            if (hasCpu && !pwrCpuConnected)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                    DiagnosticFaultCode.CPU_POWER_MISSING,
                    DiagnosticSubsystem.Power,
                    "CPU EPS 12V 8-Pin power cable is disconnected. CPU VRM has no power.",
                    "CPU_EPS"
                ));
            }

            if (hasGpu && !pwrGpuConnected)
            {
                result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                    DiagnosticFaultCode.GPU_POWER_MISSING,
                    DiagnosticSubsystem.Power,
                    "GPU PCIe power cable is disconnected. Graphics card cannot initialize.",
                    "PCIe_Power"
                ));
            }

            result.IsGpuPowered = pwrGpuConnected;
            result.IsPoweredCorrectly = pwr24Connected && pwrCpuConnected && pwrGpuConnected;

            // PSU Capacity Calculation
            int cpuTdp = cpu != null ? cpu.TDPWatts : 105;
            int cpuPeak = cpu != null ? cpu.PeakPowerWatts : 140;
            int gpuTdp = gpu != null ? gpu.TDPWatts : 0;
            int gpuTransient = gpu != null ? gpu.PeakTransientWatts : 0;

            result.EstimatedLoad = PowerLoadProfile.Calculate(cpuTdp, cpuPeak, gpuTdp, gpuTransient);
            if (psu != null)
            {
                result.PSUStatusResult = powerGraph.EvaluatePSU(
                    psu.Wattage,
                    result.EstimatedLoad,
                    psu.Pcie8PinCount,
                    gpu != null ? gpu.RequiredPcie8PinCables : 0
                );

                if (result.PSUStatusResult.Status == PSUStatus.Insufficient)
                {
                    result.DiagnosticFaults.Add(DiagnosticResult.Error(
                        DiagnosticFaultCode.PSU_INSUFFICIENT_WATTAGE,
                        DiagnosticSubsystem.Power,
                        result.PSUStatusResult.Message,
                        "PSU"
                    ));
                }
            }

            // 3. Thermal Simulation
            if (hasCpu)
            {
                result.CPUThermalResult = ThermalModel.CalculateCPUThermals(
                    cpuPowerWatts: cpuTdp,
                    coolerInstalled: hasCooler,
                    coolerTdpRating: cooler != null ? cooler.MaxTdpDissipationWatts : 0,
                    coolerScrewsTightened: snapshot.CoolerScrewsTightened,
                    totalScrewsRequired: cooler != null ? cooler.MountingScrewCount : 4,
                    fanConnected: snapshot.CoolerFanConnected,
                    pasteAmount: snapshot.ThermalPasteAmount,
                    caseAirflowFactor: 1.0f,
                    ambientTemp: 22.0f
                );

                result.IsCpuThermallySafe = !result.CPUThermalResult.IsEmergencyShutdown;

                if (result.CPUThermalResult.IsEmergencyShutdown)
                {
                    result.DiagnosticFaults.Add(DiagnosticResult.Critical(
                        DiagnosticFaultCode.THERMAL_RUNAWAY,
                        DiagnosticSubsystem.Thermal,
                        result.CPUThermalResult.DiagnosticMessage,
                        "CPUCooler"
                    ));
                }
                else if (result.CPUThermalResult.State == ThermalState.Throttling)
                {
                    result.DiagnosticFaults.Add(DiagnosticResult.Warning(
                        DiagnosticFaultCode.THERMAL_PASTE_DEGRADED,
                        DiagnosticSubsystem.Thermal,
                        result.CPUThermalResult.DiagnosticMessage,
                        "ThermalPaste"
                    ));
                }
            }

            // 4. Overall POST Feasibility
            result.CanPostSucceed = result.IsAssembledCorrectly 
                && result.IsPoweredCorrectly 
                && result.IsCpuThermallySafe 
                && (result.PSUStatusResult.CanSafelyPowerContinuous);

            return result;
        }
    }
}
